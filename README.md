Slimsy v3 (this docs are written for beta5+)
============
**Effortless Responsive & Lazy Images with LazySizes and Umbraco**

# Slimsy v3 is made for Umbraco v8!

![](Slimsy.png)

__Release Downloads__

NuGet Package: [![NuGet release](https://img.shields.io/nuget/vpre/Our.Umbraco.Slimsy.svg)](https://www.nuget.org/packages/Our.Umbraco.Slimsy/)

Umbraco Package: [![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/website-utilities/slimsy)

__Prerelease Downloads__

NuGet Package: [![MyGet build](https://img.shields.io/myget/umbraco-packages/vpre/Our.Umbraco.Slimsy.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/Our.Umbraco.Slimsy)

Umbraco Package (zip file): [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/CrumpledDog/slimsy/build/artifacts)

[![Build status](https://ci.appveyor.com/api/projects/status/a7rxrfkxc5dx8cuo?svg=true)](https://ci.appveyor.com/project/CrumpledDog/slimsy)

**Note** Slimsy v3.0.0-beta1-beta3 requires Umbraco v8.1.0+
**Note** Slimsy v3.0.0-beta4+ requires Umbraco v8.6.0+

LazySizes.js used in conjunction with ImageProcessor.Web and the built-in Umbraco Image Cropper will make your responsive websites images both adaptive and "retina" quality (if supported by the client browser), the images are also be lazy loaded.

Slimsy includes lazysizes.min.js and picturefill.min.js and some helper methods.

## Implementing post package installation

### 1. Add lazysizes.min.js & picturefill.min.js to your pages

In your master template add the  Javascript files

```
<script src="/scripts/picturefill.min.js"></script>
<script src="/scripts/lazysizes.min.js" async=""></script>
```

You can of course bundle these together.

### 2. Ensure all img elements are set to `display: block` or `display: inline-block;`

e.g.

```
img {
    display:block;
}
```

### 3. Adjust your image elements, adding `data-srcset`, `data-src`, `sizes="auto"` & `class="lazyload"` attributes


Use the `GetSrcSetUrls` UrlHelper extension methods to generate your `data-srcset` attributes. For these methods to function correctly your image property types should use the built-in **Image Cropper**.

#### `Url.GetSrcSetUrls(IPublishedContent publishedContent, int width, int height)`
Use this method for setting the crop dimensions in your Razor code, assumes your image cropper property alias is "umbracoFile"

e.g. An initial image size of 323 x 300. This example is looping people, each page has a media picker property that Models Builder is generating named "Photo"

```C#
<div class="employee-grid">
    @foreach (ContentModels.Person person in Model.Children)
    {
        <div class="employee-grid__item">
            <div class="employee-grid__item__image">
                <img src="@Url.GetCropUrl(person.Photo, 323, 300, quality:30, furtherOptions:"&format=auto")"
                     data-srcset="@Url.GetSrcSetUrls(person.Photo, 323, 300)"
                     data-src="@Url.GetCropUrl(person.Photo, 323, 300)"
                     sizes="auto" class="lazyload" />
            </div>
        </div>
    }
</div>
```

This example uses the LQIP (low quality image place holder) technique.

#### `Url.GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, [optional] string propertyAlias, [optional] int quality, [optional] string outputFormat,[optional] string furtherOptions)`

There are additional optional parameters that can be utilised for different requirements 

- propertyAlias - set if your image cropper property alias is not "umbracoFile"
- quality - change the quality setting, default is 90
- outputFormat - if you require a specific output format, e.g. "webp"
- furtherOptions - if you require additional Imageprocessor processors, e.g. "&tint=purple"

#### `Url.GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, ImageCropMode imageCropMode, [optional] string propertyAlias, [optional] ImageCropAnchor imageCropAnchor , [optional] int quality, [optional] string outputFormat, [optional] string furtherOptions)`

Use this method overload if you need to change the ImageCropMode

#### `Url.GetSrcSetUrls(publishedContent, AspectRatio aspectRatio, [optional] string propertyAlias, [optional] int quality, [optional] string outputFormat, [optional] string furtherOptions)`

Slimsy allows you to define a predefined ratio for your image so you don't need to work out the math associated with it, first you instantiate a new built in class of AspectRatio and pass in two integer values, this will crop the image(s) to the desired ratio.

```C#
<div class="employee-grid">
    @foreach (ContentModels.Person person in Model.Children)
    {
        var ratio = new AspectRatio(16, 9);

        <div class="employee-grid__item">
            <div class="employee-grid__item__image">
                <img data-srcset="@Url.GetSrcSetUrls(person.Photo, ratio)" data-sizes="auto" class="lazyload" />
            </div>
        </div>
    }
</div>
```

#### `Url.GetSrcSetUrls(publishedContent, string cropAlias)`
Use this method when you want to use a predefined crop, assumes your image cropper property alias is "umbracoFile".

In this example the crop name is "feature"
```C#
<div class="employee-grid">
    @foreach (ContentModels.Person person in Model.Children)
    {
        <div class="employee-grid__item">
            <div class="employee-grid__item__image">
                <img data-srcset="@Url.GetSrcSetUrls(person.Photo, "feature")" data-src="@Url.GetCropUrl(person.Photo, "feature")" sizes="auto" class="lazyload" />

            </div>
        </div>
    }
</div>
```

#### `Url.GetSrcSetUrls(publishedContent, string cropAlias, [optional] string propertyAlias, [optional] int quality, [optional] string outputFormat,[optional] string furtherOptions)` 

There are additional optional parameters that can be utilised for different requirements 

- propertyAlias - set if your image cropper property alias is not "umbracoFile"
- quality - change the quality setting, default is 90
- outputFormat - if you require a specific output format, e.g. "webp"
- furtherOptions - if you require additional Imageprocessor processors, e.g. "&tint=purple"

### 4 (optional). Adjust the rendering of your TinyMce Richtext editors

#### `Html.ConvertImgToSrcSet(IPublishedContent publishedContent, string propertyAlias, bool generateLqip, bool removeStyleAttribute)`

Use this method to convert images entered into TinyMce Rich Text editors to use img source set using generated paths

```C#
@Html.ConvertImgToSrcSet(Model, "richTextBody")

```

#### `Html.ConvertImgToSrcSet(this HtmlHelper htmlHelper, string sourceValueHtml, bool generateLqip, bool removeStyleAttribute)`
Use this method to convert images entered in a TinyMce Rich Text editor within the Grid to use img source set using generated paths. This method will also take care of parsing Umbraco links and Macros.

e.g. within `Rte.chtml` found within the `Partials/Grid/Editors` folder

```C#
@SlimsyExtensions.ConvertImgToSrcSet(Html, Model.value.ToString(), true)

```

# Using `<picture>` element

Below is an example of how to use the `<picture>` element to provide automated WebP versions of your images using the [ImageProcessor WebP plugin](http://imageprocessor.org/imageprocessor/plugins/#webp) (needs to be installed from NuGet), this example also implements a optional LQIP image.

```C#
<div class="employee-grid">
    @foreach (Person person in Model.Children)
    {
        <div class="employee-grid__item">
            <div class="employee-grid__item__image">
                <picture>
                    <!--[if IE 9]><video style="display: none"><![endif]-->
                    <source data-srcset="@Url.GetSrcSetUrls(person.Photo, 323, 300, "umbracoFile", 80, "webp")" type="image/webp" data-sizes="auto" />
                    <source data-srcset="@Url.GetSrcSetUrls(person.Photo, 323, 300)" type="image/jpeg" data-sizes="auto" />
                    <!--[if IE 9]></video><![endif]-->
                    <img src="data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw=="
                            data-src="@Url.GetCropUrl(person.Photo, 323, 300)"
                            class="lazyload"
                            alt="image"
                            data-sizes="auto" />
                </picture>
            </div>
        </div>
    }
</div>
```

# SlimsyService

You can inject SlimsyService when you want to use Slimsy methods in C#.

For example in a RenderMvcController

```C#
    public class ProductController : RenderMvcController
    {
        private readonly SlimsyService _slimsyService;

        public ProductController(SlimsyService slimsyService)
        {
            this._slimsyService = slimsyService;
        }

        public override ActionResult Index(ContentModel model)
        {
            var product = model.Content as Product;

            var photo = product.Photos;
            var vm = new ProductViewModel(product)
            {
                PhotoSrc = this._slimsyService.GetCropUrl(photo, "feature"),
                PhotoSrcSetUrls = this._slimsyService.GetSrcSetUrls(photo, "feature")
            };

            return this.CurrentTemplate(vm);
        }
    }
```

# SlimsyOptions

You can change Slimsy's configuration in a composer

e.g. 

```C#
public void Compose(Composition composition)
{
    composition.SetSlimsyOptions(factory =>
    {
        var options = SlimsyComposer.GetDefaultOptions(factory);
        options.DomainPrefix = "https://setviacomposer.com";
        options.WidthStep = 200;
        return options;
    });
}
```
 Or you could replace with your own custom options

```C#
public void Compose(Composition composition)
{
    composition.RegisterUnique<ISlimsyOptions, SlimsyCustomConfigOptions>();
}
 ```

```C#
public class SlimsyCustomConfigOptions : ISlimsyOptions
{
    public SlimsyCustomConfigOptions()
    {
        Format = "png";
        BackgroundColor = "";
        MaxWidth = 4000;
        WidthStep = 50;
        DefaultQuality = 95;
        DomainPrefix = "https://setviacustomconfigoptions.com";
    }
    public string Format { get; set; }
    public string BackgroundColor { get; set; }
    public int DefaultQuality { get; set; }
    public int MaxWidth { get; set; }
    public int WidthStep { get; set; }
    public string DomainPrefix { get; set; }
}
 ```
## SlimsyWebConfigOptions

By default Slimsy uses AppSettings in web.config

You can specify the default output format for all images.

  `<add key="Slimsy:Format" value="jpg"/>`

You can specify the default background color by adding another appsetting to web.config. As an example this setting is used if ImageProcessor is converting a png to a jpg and it has some transparency.

  `<add key="Slimsy:BGColor" value="fff"/>`

You can specify the default quality for all images, unless specified via helper.

  `<add key="Slimsy:DefaultQuality" value="90"/>`

You can specify the max width for the generated srcset sizes.

  `<add key="Slimsy:MaxWidth" value="2048"/>`

You can specify the width step for the generated srcset sizes.

  `<add key="Slimsy:WidthStep" value="160"/>`

You can add a domain prefix which will be prepended to all urls, this can be used with proxy CDNs (pull)

  `<add key="Slimsy:DomainPrefix" value="https://mydomain.azureedge.net" />`

# Lazysizes.js

Lazysizes.js is awesome and it's what makes so easy to implement. If you need to find out more information about it or how to hook into it's Javascript events be sure to check out it's [GitHub](https://github.com/aFarkas/lazysizes#combine-data-srcset-with-data-src)

# Razor Helper

It may be useful to use a Razor Helper to render `img` or `picture` elements, there is an reusable example included in the test site which can be adapted to your own requirement. You can find it [here](https://github.com/Jeavon/Slimsy/blob/dev-v3/TestSite/App_Code/SlimsyHelper.cshtml) and see it in use [here](https://github.com/Jeavon/Slimsy/blob/dev-v3/TestSite/Views/people.cshtml#L97)

# Using with Azure CDN (beta5+)

Follow these [steps](Docs/Azure-CDN/index.md)

# Test Site & Source Code

A test site is included in the solution, the username and password for Umbraco are admin@admin.com/password1234567890.

Visual Studio 2019 is required for compiling the source code

# Credits and references

This project includes [LazySizes](https://github.com/aFarkas/lazysizes) and [Picturefill](https://github.com/scottjehl/picturefill) Both projects are MIT licensed.

Without the amazing [ImageProcessor](http://imageprocessor.org) this package wouldn't exist, so many thanks go to [James](https://github.com/JimBobSquarePants) for creating ImageProcessor!

Many thanks to Douglas Robar for naming Slimsy.

# Change log

[Here](Changelog.md)
