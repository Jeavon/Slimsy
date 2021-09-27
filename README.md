Slimsy v2
============
**Effortless Responsive & Lazy Images with LazySizes and Umbraco**

# Slimsy v2 is not compatible with Slimsy v1 at all, if you upgrade you will have to refactor all of your code! #

![](Slimsy.png)

__Release Downloads__

NuGet Package: [![NuGet release](https://img.shields.io/nuget/vpre/Our.Umbraco.Slimsy.svg)](https://www.nuget.org/packages/Our.Umbraco.Slimsy/)

Umbraco Package: [![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/website-utilities/slimsy)

__Prerelease Downloads__

NuGet Package: [![MyGet build](https://img.shields.io/myget/slimsy-v2/vpre/Our.Umbraco.Slimsy.svg)](https://www.myget.org/gallery/slimsy-v2)

Umbraco Package (zip file): [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/CrumpledDog/slimsy/build/artifacts)

[![Build status](https://ci.appveyor.com/api/projects/status/a7rxrfkxc5dx8cuo?svg=true)](https://ci.appveyor.com/project/CrumpledDog/slimsy)

**Note** Slimsy v2.0.0+ requires Umbraco v7.6.0+

LazySizes.js used in conjunction with ImageProcessor.Web and the built-in Umbraco Image Cropper will make your responsive websites images both adaptive and "retina" quality (if supported by the client browser), the images are also be lazy loaded.

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

```
img {
    display:block;
}
```

### 3. Adjust your image elements, adding `data-srcset`, `data-src`, `sizes="auto"` & `class="lazyload"` attributes

Use the `GetSrcSetUrls` UrlHelper extension method to generate your `data-srcset` attributes. For these methods to function correctly your image property types should use the built-in **Image Cropper**.

#### `Url.GetSrcSetUrls(publishedContent, int width, int height)`
Use this method for setting the crop dimensions in your Razor code, assumes your image cropper property alias is "umbracoFile"

e.g. An initial image size of 270 x 161. This example is looping pages, each page has a media picker with property alias "Image"

```
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
```

This example uses the LQIP (low quality image place holder) technique.

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, int quality)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, string propertyAlias)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality)`

#### `Url.GetSrcSetUrls(publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat)`

#### `Url.GetSrcSetUrls(publishedContent, AspectRatio aspectRatio)`

Slimsy v2 allows you to define a predefined ratio for your image so you don't need to work out the math associated with it, first you instantiate a new built in class of AspectRatio and pass in two integer values, this will crop the image(s) to the desired ratio.

```
@foreach (var feature in featuredPages)
{
    var ratio = new AspectRatio(16, 9);
    <div class="3u">
        <section class="is-feature">
            @if (feature.HasValue("image"))
            {
                var featureImage = Umbraco.TypedMedia(feature.GetPropertyValue<int>("image"));
                <a href="@feature.Url" class="image image-full">
                    <img data-srcset="@Url.GetSrcSetUrls(featureImage, ratio)" data-src="@Url.GetCropUrl(featureImage, 270, 161)" sizes="auto" class="lazyload" />
                </a>
            }
            <h3><a href="@feature.Url">@feature.Name</a></h3>
            @Umbraco.Truncate(feature.GetPropertyValue<string>("bodyText"), 100)
        </section>
    </div>
}
```

#### `Url.GetSrcSetUrls(publishedContent, string cropAlias)`

#### `Url.GetSrcSetUrls(publishedContent, string cropAlias, string propertyAlias)`

Use this method when you want to use a predefined crop, assumes your image cropper property alias is "umbracoFile".

```
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
            <h3><a href="@feature.Url">@feature.Name</a></h3>
            @Umbraco.Truncate(feature.GetPropertyValue<string>("bodyText"), 100)
        </section>
    </div>
}
```

#### `Url.GetSrcSetUrls(publishedContent, string cropAlias, string propertyAlias, string outputFormat)`

### 4 (optional). Adjust the rendering of your TinyMce Richtext editors

#### `Html.ConvertImgToSrcSet(IPublishedContent publishedContent, string propertyAlias, bool generateLqip, bool removeStyleAttribute, bool roundWidthHeight)`

Use this method to convert images entered into TinyMce Rich Text editors to use img source set using generated paths

```
@Html.ConvertImgToSrcSet(Model.Content, "bodyText", true, true)

```

#### `Html.ConvertImgToSrcSet(string html, bool generateLqip, bool removeStyleAttribute, bool removeUdiAttribute)`

#### `Html.ConvertImgToSrcSet(IHtmlString html, bool generateLqip, bool removeStyleAttribute, bool removeUdiAttribute)`

# Using `<picture>` element

Below is an example of how to use the `<picture>` element to provide automated WebP versions of your images using the [ImageProcessor WebP plugin](http://imageprocessor.org/imageprocessor/plugins/#webp), this example also implements a optional LQIP image.

```
foreach (var caseStudyImage in caseStudyImagesCollection)
{
    var imgSrcSet = Url.GetSrcSetUrls(caseStudyImage, 650, 0);
    var imgSrcSetWebP = Url.GetSrcSetUrls(caseStudyImage, 650, 0, Constants.Conventions.Media.File, "webp", quality:70);

    var imgSrc = Url.GetCropUrl(caseStudyImage, 650, 0);
    var imgLqip = Url.GetCropUrl(caseStudyImage, 650, 0, quality: 30, furtherOptions: "&format=auto");

    <div class="picture-box">
        <picture>
            <!--[if IE 9]><video style="display: none"><![endif]-->
            <source data-srcset="@imgSrcSetWebP" srcset="@imgLqip" type="image/webp" data-sizes="auto"/>
            <source data-srcset="@imgSrcSet" srcset="@imgLqip" type="image/jpeg" data-sizes="auto"/>
            <!--[if IE 9]></video><![endif]-->
            <img
                src="@imgLqip"
                data-src="@imgSrc"
                class="lazyload"
                data-sizes="auto"
                alt="@caseStudyImage.Name" />
        </picture>
    </div>
}
```

# Advanced Options

You can specify the default output format for all images.

  `<add key="Slimsy:Format" value="jpg"/>`

You can specify the default background color by added another appsetting to web.config. As an example this setting is used if ImageProcessor is converting a png to a jpg and it as some transparency.

  `<add key="Slimsy:BGColor" value="fff"/>`

You can specify the default quality for all images, unless specified via helper.

  `<add key="Slimsy:DefaultQuality" value="90"/>`

You can specify the max width for the generated srcset sizes.

  `<add key="Slimsy:MaxWidth" value="2048"/>`

You can specify the width step for the generated srcset sizes.

  `<add key="Slimsy:WidthStep" value="160"/>`

# Lazysizes.js

Lazysizes.js is awesome and it's what makes Slimsy v2 so easy to implement. If you need to find out more information about it or how to hook into it's Javascript events be sure to check out it's [GitHub](https://github.com/aFarkas/lazysizes#combine-data-srcset-with-data-src)

# Razor Helper

It may be useful to use a Razor Helper to render `img` or `picture` elements, there is an reusable example included in the test site which can be adapted to your own requirement. You can find it [here](https://github.com/Jeavon/Slimsy/blob/develop/TestSite/App_Code/SlimsyHelper.cshtml) and see it in use [here](https://github.com/Jeavon/Slimsy/blob/develop/TestSite/Views/Partials/umbFeatures.cshtml#L76)

# Test Site & Source Code

A test site is included in the solution, the username and password for Umbraco are admin/password.
By default the test site is configured to use full IIS (due to IIS Express SQL CE persistence issue) on the domain slimsy.local, you can change it to use IIS Express if you prefer.

Visual Studio 2015 is required for compiling the source code

# Credits and references

This project includes [LazySizes](https://github.com/aFarkas/lazysizes) and [Picturefill](https://github.com/scottjehl/picturefill) Both projects are MIT licensed.

Without the amazing [ImageProcessor](http://imageprocessor.org) this package wouldn't exist, so many thanks go to [James](https://github.com/JimBobSquarePants) for creating ImageProcessor!

Many thanks to Douglas Robar for naming Slimsy.

# Change log

[Here](Changelog.md)