Slimsy v2
============
**Effortless Responsive Images with LazySizes and Umbraco**

# Slimsy v2 is not compatible with Slimsy v1 at all, if you upgrade you will have to refactor all of your code! #

![](Slimsy.png)

__Release Downloads__ 

NuGet Package: [![NuGet release](https://img.shields.io/nuget/vpre/Our.Umbraco.Slimsy.svg)](https://www.nuget.org/packages/Our.Umbraco.Slimsy/)

Umbraco Package: [![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/website-utilities/slimsy) 

__Prerelease Downloads__ 

NuGet Package: [![MyGet build](https://img.shields.io/myget/slimsy-v2/vpre/Our.Umbraco.Slimsy.svg)](https://www.myget.org/gallery/slimsy)

Umbraco Package (zip file): [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/JeavonLeopold/slimsy/build/artifacts) 

[![Build status](https://ci.appveyor.com/api/projects/status/a7rxrfkxc5dx8cuo?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/slimsy)

**Note** Slimsy v2.0.0+ requires Umbraco v7.6.0+

LazySizes.js used in conjunction with ImageProcessor.Web and the built-in Umbraco Image Cropper will make your responsive websites images both adaptive and "retina" quality (if supported by the client browser).

Slimsy includes lazysizes.min.js and picturefill.min.js and some helper methods.


## Implementing post package installation

### 1. Add lazysizes.min.js & picturefill.min.js to your pages

In your master template add the  Javascript files

```
    <script src="/scripts/picturefill.min.js"></script>
    <script src="/scripts/lazysizes.min.js" async=""></script>
```

You can of course bundle these together. If you don't already have JavaScript bundling in place you should take a look at the [Optimus](http://our.umbraco.org/projects/developer-tools/optimus) package, it will allow you to bundle them together in minutes.

### 2. Ensure all img elements are set to `display: block` or `display: inline-block;`

e.g. 

	img {
	  display:block;
	}

### 3. Adjust your image elements, adding `data-srcset`, `data-src`, `sizes="auto"` & `class="lazyload"` attributes

Use the `GetSrcSetUrls` UrlHelper extension method to generate your `data-srcset` attributes. For these methods to function correctly your image property types should use the built-in **Image Cropper**.

#### `Url.GetSrcSetUrls(publishedContent, int width, int height)`
Use this method for setting the crop dimensions in your Razor code, assumes your image cropper property alias is "umbracoFile"

e.g. An initial image size of 270 x 161. This example is looping pages, each page has a media picker with property alias "Image"

    @foreach (var feature in featuredPages)
    {
        var featureImage = Umbraco.TypedMedia(feature.GetPropertyValue<int>("image"));
        <div class="3u">
            <!-- Feature -->
            <section class="is-feature">
            	<img src="@Url.GetCropUrl(featureImage, 270, 161, quality:30, furtherOptions:"&format=auto")" data-srcset="@Url.GetSrcSetUrls(featureImage, 270, 161)" data-src="@Url.GetCropUrl(featureImage, 270, 161)" sizes="auto" class="lazyload" />
            </section>
        </div>
    }
</div>

This example uses the LQIP (low quality image place holder) technique.

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, int quality)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, string propertyAlias)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, AspectRatio aspectRatio)`

Slimsy 2 allows you to define a predefined ratio for your image so you don't need to work out the math associated with it, first you instantiate a new built in class of AspectRatio and pass in two integer values, this will crop the image(s) to the desired ration.

    @foreach (var feature in featuredPages)
    {
        var ratio = new AspectRatio(16, 9);
        <div class="3u">
            <section class="is-feature">
                @if (feature.HasValue("image"))
                {
                    var featureImage = Umbraco.TypedMedia(feature.GetPropertyValue<int>("image"));
                    <a href="@feature.Url" class="image image-full">
                        <img data-srcset="@Url.GetSrcSetUrls(featureImage, 1920, 0, ratio)" data-src="@Url.GetCropUrl(featureImage, 270, 161)" sizes="auto" class="lazyload" />
                    </a>
                }
                <h3><a href="@feature.Url">@feature.Name</a>
                </h3>
                @Umbraco.Truncate(feature.GetPropertyValue<string>("bodyText"), 100)
            </section>
        </div>
    }

#### `Url.GetSrcSetUrls(publishedContent, cropAlias)`

#### `Url.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias)`

Use this method when you want to use a predefined crop, assumes your image cropper property alias is "umbracoFile".

    @foreach (var feature in featuredPages)
    {
        <div class="3u">
            <section class="is-feature">
                @if (feature.HasValue("image"))
                {
                    var featureImage = Umbraco.TypedMedia(feature.GetPropertyValue<int>("image"));
                    <a href="@feature.Url" class="image image-full">
                        <img data-srcset="@Url.GetSrcSetUrls(featureImage, "home", "umbracoFile")" data-src="@Url.GetCropUrl(featureImage, "umbracoFile", "home")" sizes="auto" class="lazyload"/>
                    </a>
                }
                <h3><a href="@feature.Url">@feature.Name</a>
                </h3>
                @Umbraco.Truncate(feature.GetPropertyValue<string>("bodyText"), 100)
            </section>
        </div>
    }

#### `Url.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, string outputFormat)`

# Using `<picture>` element

Below is an example of how to use the `<picture>` element to provide automated WebP versions of your images using the [ImageProcessor WebP plugin](http://imageprocessor.org/imageprocessor/plugins/#webp).

    @foreach (var feature in featuredPages)
    {
        var featureImage = Umbraco.TypedMedia(feature.GetPropertyValue<int>("image"));
        <div class="3u">
            <!-- Feature -->
            <section class="is-feature">

                <picture>
                    <!--[if IE 9]><video style="display: none"><![endif]-->
                    <source data-srcset="@Url.GetSrcSetUrls(featureImage, 270, 161, "umbracoFile", "webp")" type="image/webp" data-sizes="auto"/>
                    <source data-srcset="@Url.GetSrcSetUrls(featureImage, 270, 161)" type="image/jpeg" data-sizes="auto"/>
                    <!--[if IE 9]></video><![endif]-->
                    <img
                        src="data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw=="
                        data-src="@Url.GetCropUrl(featureImage, 270, 161)"
                        class="lazyload"
                        alt="image with artdirection"
                        data-sizes="auto"/>
                </picture>
            </section>
        </div>
    }

# Advanced Options

You can specify the default output format for all images

    <add key="Slimsy:Format" value="jpg"/>

You can specify the default background color by added another appsetting to web.config. As an example this setting is used if ImageProcessor is converting a png to a jpg and it as some transparency

	<add key="Slimsy:BGColor" value="fff"/>

# Lazysizes.js

Lazysizes.js is awesome and it's what makes Slimsy v2 so easy to implement. If you need to find out more information about it or how to hook into it's Javascript events be sure to check out it's [GitHub](https://github.com/aFarkas/lazysizes#combine-data-srcset-with-data-src) 

# Test Site & Source Code

A test site is included in the solution, the username and password for Umbraco are admin/password.
By default the test site is configured to use full IIS (due to IIS Express SQL CE persistence issue) on the domain slimsy.local, you can change it to use IIS Express if you prefer.

Visual Studio 2015 is required for compiling the source code

# Credits and references

This project includes [LazySizes](https://github.com/aFarkas/lazysizes) and [Picturefill](https://github.com/scottjehl/picturefill) Both projects are MIT licensed.

Many thanks to Douglas Robar for naming Slimsy.

# Change log

[Here](Changelog.md)