using Axinom.Cpix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Axinom.CpixValidator
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Validate_Click(object sender, EventArgs e)
        {
            // If not provided, the file will be empty but not null.
            var cpixFile = Request.Files["cpixFile"];
            var pfxFile = Request.Files["pfxFile"];

            var pfxPassword = Request.Form["pfxPassword"];

            // Form results (validation did not take place if any errors here).
            var formErrors = new List<string>();

            // Validation results.
            var errors = new List<string>();
            var warnings = new List<string>();
            var messages = new List<string>();

            // If form errors occur, we do not show validation results.
            resultsPanel.Visible = false;

            X509Certificate2 recipientCertificate = null;

            try
            {
                if (pfxFile.ContentLength != 0)
                {
                    // If a PFX file is provided, a PFX password must be provided.
                    if (string.IsNullOrWhiteSpace("pfxPassword"))
                    {
                        formErrors.Add("You must provide the PFX password if you provide a PFX file.");
                        return;
                    }

                    byte[] pfxBytes;

                    using (var reader = new BinaryReader(pfxFile.InputStream))
                        pfxBytes = reader.ReadBytes(pfxFile.ContentLength);

                    try
                    {
                        recipientCertificate = new X509Certificate2(pfxBytes, pfxPassword);
                    }
                    catch (Exception ex)
                    {
                        formErrors.Add("Unable to open the PFX file: " + ex.Message);
                        return;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(pfxPassword))
                {
                    // You cannot provide only a PFX password with no PFX file - something went wrong.
                    formErrors.Add("You provided a PFX password but no PFX file.");
                    return;
                }

                // If we got this far, we got to the validation step and can show results.
                resultsPanel.Visible = true;
                positiveResultPanel.Visible = false;
                negativeResultPanel.Visible = false;

                CpixDocument cpix;

                try
                {
                    // This can throw for a large number of reasons (most validation failures will come from here).
                    // It is slightly annoying because we just get the first error but this is also minimum-effort solution.

                    if (recipientCertificate != null)
                    {
                        cpix = CpixDocument.Load(cpixFile.InputStream, recipientCertificate);
                    }
                    else
                    {
                        cpix = CpixDocument.Load(cpixFile.InputStream);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    warnings.Add("Validation could not be completed. Fix the above error and try again to perform a full validation.");
                    return;
                }

                // If it loaded, let's do some more checks for logical consistency.

                if (cpix.ContentKeys.Any())
                {
                    if (cpix.ContentKeysAreReadable)
                    {
                        if (recipientCertificate == null)
                        {
                            warnings.Add("Content keys in the document are not encrypted. This CPIX document should not be used in production scenarios.");
                        }
                        else
                        {
                            messages.Add("Content keys in the document are encrypted. Decryption was successful.");
                        }
                    }
                    else
                    {
                        if (recipientCertificate == null)
                        {
                            // Keys are not readable and we did not have the key. This is fine but we cannot validate
                            // the key data like this, so emit a warning.
                            warnings.Add("Encrypted content keys are present in the document but cannot be validated without the recipient private key. Provide the PFX file with the recipient's key pair to validate encrypted data.");
                        }
                        else
                        {
                            // We had a certificate but still could not read the content keys? Wrong certificate!
                            errors.Add("The certificate in the PFX file was not defined as a valid recipient by the CPIX document.");
                        }
                    }
                }

                if (cpix.UsageRules.Any(r => r.ContainsUnsupportedFilters))
                {
                    messages.Add("The document includes usage rule filters that are not supported by this validator. These filters were skipped during inspection.");
                }

                // And finally, just dump some useful data.
                if (cpix.ContentKeys.Any())
                {
                    var keyIds = cpix.ContentKeys.Select(k => k.Id.ToString()).OrderBy(k => k);

                    messages.Add($"The document contains {cpix.ContentKeys.Count} content keys:{Environment.NewLine}{string.Join(Environment.NewLine, keyIds)}.");
                }
                else
                {
                    messages.Add("The document does not contain any content keys.");
                }
                
                if (cpix.Recipients.Any())
                {
                    var recipientNames = cpix.Recipients.Select(r => r.Certificate.Subject).OrderBy(r => r);

                    messages.Add($"The document defines {cpix.Recipients.Count} recipients:{Environment.NewLine}{string.Join(Environment.NewLine, recipientNames)}");
                }
                else
                {
                    messages.Add("The document does not define any valid recipients.");
                }

                if (cpix.UsageRules.Any())
                {
                    messages.Add($"The document defines {cpix.UsageRules.Count} usage rules.");
                }
                else
                {
                    messages.Add("The document does not define any usage rules.");
                }

                if (cpix.ContentKeys.SignedBy != null)
                {
                    var signerNames = cpix.ContentKeys.SignedBy.Select(s => s.Subject).OrderBy(s => s);

                    messages.Add($"The set of content keys is digitally signed by:{Environment.NewLine}{string.Join(Environment.NewLine, signerNames)}");
                }
                else if (cpix.ContentKeys.Any())
                {
                    messages.Add("The set of content keys is not digitally signed.");
                }

                if (cpix.UsageRules.SignedBy != null)
                {
                    var signerNames = cpix.UsageRules.SignedBy.Select(s => s.Subject).OrderBy(s => s);

                    messages.Add($"The set of usage rules is digitally signed by:{Environment.NewLine}{string.Join(Environment.NewLine, signerNames)}");
                }
                else if (cpix.UsageRules.Any())
                {
                    messages.Add("The set of usage rules is not digitally signed.");
                }

                if (cpix.Recipients.SignedBy != null)
                {
                    var signerNames = cpix.Recipients.SignedBy.Select(s => s.Subject).OrderBy(s => s);

                    messages.Add($"The set of valid recipients is digitally signed by:{Environment.NewLine}{string.Join(Environment.NewLine, signerNames)}");
                }
                else if (cpix.Recipients.Any())
                {
                    messages.Add("The set of valid recipients is not digitally signed.");
                }

                if (cpix.SignedBy != null)
                {
                    messages.Add($"The document as a whole is signed by:{Environment.NewLine}{cpix.SignedBy.Subject}");
                }
                else
                {
                    messages.Add("The document as a whole is not digitally signed.");
                }
            }
            catch (Exception ex)
            {
                formErrors.Add("Unexpected processing error. Please report this on GitHub! " + ex.Message);
            }
            finally
            {
                if (recipientCertificate != null)
                    recipientCertificate.Dispose();

                formErrorsList.Visible = formErrors.Any();

                if (formErrors.Any())
                {
                    formErrorsList.DataSource = formErrors;
                    formErrorsList.DataBind();
                }

                errorList.DataSource = errors;
                errorList.DataBind();

                warningList.DataSource = warnings;
                warningList.DataBind();

                messageList.DataSource = messages;
                messageList.DataBind();

                if (errors.Any())
                {
                    negativeResultPanel.Visible = true;
                }
                else
                {
                    positiveResultPanel.Visible = true;
                }
            }
        }
    }
}