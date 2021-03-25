# cpix-validator

Web app for validating [CPIX 2.3 documents](http://dashif.org/guidelines/).

CPIX documents are used to carry content protection information in digital media processing workflows. They contain data such as:

* encryption keys
* signaling information for DRM system activation
* rules for mapping encryption keys to different types of tracks
* key rotation schedules

Latest build hosted at https://cpix-validator.azurewebsites.net

# Feature set

Internally, the [Axinom CPIX library](https://github.com/Axinom/cpix) is used. Accordingly, only the CPIX features supported by that library are inspected.

Contributions for extending the feature set are welcome!

# System requirements

* .NET Framework 4.7
