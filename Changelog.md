Slimsy Change log
============
- 0.1.0-beta - the beginning
- 0.1.1-beta - switched to build of SlimResponse so the Umbraco installer doesn't display a warning
- 0.1.2-beta - added quality parameter for Slimmage to adjust
- 0.1.3-beta - all URLs will now be lowercase for SEO best practice
- 0.1.4-beta - add some null checks before converting to lowercase
- 1.0.0 - release
- 1.1.0 - all ImageProcessor.Web URLs now have upscale=false and by default format=jpg, this can be disabled by adding web.config appsetting. Also fixed package uninstall action 
- 1.1.1 - added option to overload the output format for a specific image
- 1.1.2 - added option to set default background color for use with output format
- 1.1.3 - removed upscale=false as it stops the image being cropped to correct ratio for oversized requests
- 1.1.4 - updated slimmage.js to v0.4.1 and add NuGet package
- 1.1.5 - Umbraco package only release without HtmlAgilityPack Assembly
- 1.1.6 - Umbraco v7.3.0 upgrade & focal point preference fix