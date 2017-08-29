<%@ page language="C#" autoeventwireup="true" codebehind="Default.aspx.cs" inherits="Axinom.CpixValidator.Default" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>CPIX validator</title>

    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta/css/bootstrap.min.css" integrity="sha384-/Y6pD6FV/Vv2HJnA6t+vslU6fwYXjCFtcEpHbNJ0lyAFsXTsjBbfaDjzALeQsN6M" crossorigin="anonymous">
</head>
<body>
    <div class="container">
        <h1>CPIX document validator</h1>
        <p>
            This page validates the conformance of XML documents to the <a href="http://dashif.org/guidelines/">Content Protection Information Exchange (CPIX) 2.0</a> specification.
        </p>

        <asp:Panel runat="server" ID="resultsPanel" Visible="false">
            <h2>Result</h2>

            <asp:Panel runat="server" ID="negativeResultPanel" Visible="false">
                <p>
                    The CPIX document is <span class="badge badge-danger">INVALID</span>
                </p>
            </asp:Panel>

            <asp:Panel runat="server" ID="positiveResultPanel" Visible="false">
                <p>
                    The CPIX document is <span class="badge badge-primary">potentially valid</span>
                </p>
                <p>
                    Not every aspect of the CPIX specification is checked by this validator, so a definitive statement of validity is impossible.
                </p>
            </asp:Panel>

            <h3>Details</h3>

            <asp:Repeater runat="server" ID="errorList" ItemType="System.String">
                <ItemTemplate>
                    <div class="alert alert-danger"><%# Item %></div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Repeater runat="server" ID="warningList" ItemType="System.String">
                <ItemTemplate>
                    <div class="alert alert-warning"><%# Item %></div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Repeater runat="server" ID="messageList" ItemType="System.String">
                <ItemTemplate>
                    <div class="alert alert-info"><%# Item %></div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <h2>Input data</h2>
        <asp:Repeater runat="server" ID="formErrorsList" ItemType="System.String">
            <ItemTemplate>
                <div class="alert alert-danger"><%# Item %></div>
            </ItemTemplate>
        </asp:Repeater>

        <form id="form1" runat="server">
            <div class="form-group row">
                <label for="cpixFile" class="col-form-label col-sm-2">Select CPIX file</label>
                <div class="col-sm-10">
                    <input runat="server" type="file" accept=".xml" class="form-control" id="cpixFile" required />
                </div>
            </div>

            <div class="form-group row">
                <span class="col-form-label col-sm-2">Recipient private key (optional)</span>

                <div class="col-sm-10">
                    <p>
                        The validator will attempt to decrypt any encrypted content keys.
                    </p>

                    <p>
                        <button type="button" class="btn btn-sm btn-info" data-toggle="collapse" href="#moreinfo">More info...</button>
                    </p>

                    <div id="moreinfo" class="collapse">
                        <div class="card card-body">
                            <p>This verifies that the cryptographic parameters are valid and the encrypted data can be decrypted successfully. Without decryption these elements cannot be thoroughly inspected.</p>
                            <p>The CPIX document must contain a DeliveryData element identifying the subject of the certificate in the PFX file as a valid recipient.</p>
                            <p>Do not upload any sensitive production data!</p>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="pfxFile" class="form-control-label">PFX file with certificate and private key</label>
                        <input runat="server" type="file" accept=".pfx" class="form-control" id="pfxFile" />

                        <label for="pfxPassword" class="form-control-label">PFX password</label>
                        <input runat="server" type="password" class="form-control" id="pfxPassword" />
                    </div>
                </div>
            </div>

            <p>
                <asp:Button runat="server" CssClass="btn btn-primary" Text="Validate" OnClick="Validate_Click" />
            </p>
        </form>
    </div>

    <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.11.0/umd/popper.min.js" integrity="sha384-b/U6ypiBEHpOf/4+1nzFpr53nxSS+GLCkfwBdFNTxtclqqenISfwAzpKaMNFNmj4" crossorigin="anonymous"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta/js/bootstrap.min.js" integrity="sha384-h0AbiXch4ZDo7tp9hKZ4TsHbi047NrKGLO3SEJAg45jXxnGIfYzk4Si90RDIqNm1" crossorigin="anonymous"></script>

    <script>
        $(function () {
            $('[data-toggle="popover"]').popover()
        })
    </script>
</body>
</html>
