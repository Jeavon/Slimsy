Slimsy v4
============
**Effortless Responsive & Lazy Images with LazySizes and Umbraco**

# Slimsy v4 is made for Umbraco v10, v11 & v12!

![](https://raw.githubusercontent.com/Jeavon/Slimsy/main-v4/Slimsy.png)

__Release Downloads__

NuGet Package: [![NuGet release](https://img.shields.io/nuget/vpre/Our.Umbraco.Slimsy.svg)](https://www.nuget.org/packages/Our.Umbraco.Slimsy/)

__Prerelease Downloads__

NuGet Package: [![MyGet build](https://img.shields.io/myget/umbraco-packages/vpre/Our.Umbraco.Slimsy.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/Our.Umbraco.Slimsy)

[![Build status](https://ci.appveyor.com/api/projects/status/a7rxrfkxc5dx8cuo?svg=true)](https://ci.appveyor.com/project/CrumpledDog/slimsy)

## Installation

### 1. Install from NuGet

### 2. Add to Startup.cs in the ConfigureServices method

```c#
.AddSlimsy()
```
### 3. Add to _ViewImports.cshtml

```c#
@using Slimsy.Enums;
@addTagHelper *, Slimsy
@inject Slimsy.Services.SlimsyService SlimsyService
```

### 4. Add lazysizes.min.js & to your templates/views

In your template add the JavaScript files

```
<script src="/scripts/lazysizes.min.js" async=""></script>
```

### 5. Ensure all img elements are set to `display: block` or `display: inline-block;`

e.g.

```css
img {
    display:block;
}
```

If you are using LQIP then you will also need to ensure img elements are set to `width:100%;`

e.g.

```css
img {
    display:block;
    width:100%;
}
```

### 6. Use the `<slimsy-picture>` tag helper or manually adjust your image elements, adding `data-srcset`, `data-src`, `sizes="auto"` & `class="lazyload"` attributes

```C#
<slimsy-picture media-item="@person.Photo" width="323" height="300" css-class="myClass" render-lqip="true" render-webp-alternative="true"></slimsy-picture>
```

Use the `GetSrcSetUrls` UrlHelper extension methods to generate your `data-srcset` attributes. For these methods to function correctly your image property types should use the built-in **Image Cropper**.

```C#
<div class="employee-grid__item__image">
    <img data-srcset="@Url.GetSrcSetUrls(person.Photo, 323, 300)" srcset="@Url.GetSrcSetUrls(person.Photo, 250, 250, quality: 40)" data-sizes="auto" class="lazyload"/>
</div>
```

Or inject SlimsyService into your ViewComponents

```C#
private readonly SlimsyService _slimsyService;
public ResponsiveImageViewComponent(SlimsyService slimsyService)
{
	_slimsyService = slimsyService;
}
```

### 7 (optional). Adjust the rendering of your TinyMce Richtext editors

```C#
<div class="col-md-9">
    <article>
        @SlimsyService.ConvertImgToResponsive(Model, "richTextBody", renderPicture:true, pictureSources: new []{"webp"})
    </article>
</div>
```

### 8 (optional). Adjust the renderer of media within the Grid editor

There's quite a lot to this - so check it out in the demo site [here](https://github.com/Jeavon/Slimsy/blob/dev-v4/src/Slimsy.TestSite/Views/Partials/grid/editors/media.cshtml)

# Options

Add/Edit `appsettings.json`

```json
  "Slimsy": {
    "WidthStep": 180,
    "UseCropAsSrc": false,
    "DefaultQuality": 70,
    "Format": "",
    "BGColor": ""
  }
```

or edit `Startup.cs` to modify SlimsyOptions
```c#
.AddSlimsy(options =>
{
    options.DefaultQuality = 60;
    options.WidthStep = 60;
    options.UseCropAsSrc = true;
})
```

## Available in v4.1+

TagHelper has some options in `appsettings.json`

- SingleSources - allows specific file extensions to only render a single source
- DefaultPictureSources - allows multiple picture sources to be defined, example below is for both avif and webp formats
- ImageDimensions - defines if width and height attributes should be rendered on the `img` tag

e.g.

```json
  "Slimsy": {
    "WidthStep": 180,
    "UseCropAsSrc": true,
    "DefaultQuality": 70,
    "TagHelper": {
      "SingleSources": [ "gif" ],
      "DefaultPictureSources": [
        {
          "Extension": "avif",
          "Quality": 60
        },
        {
          "Extension": "webp",
          "Quality": 70
        }
      ],
      "ImageDimensions": true
    }
  }
```

TagHelper has new parameters

- `decorative` which renders `role="presentation"` on the `img` tag
- `fetch-priority` which renders on the `img` tag, for example `fetchpriority="high"`

# How to use AVIF format in v4.1+

There is not currently a AVIF encoder for ImageSharp, keep an eye on https://github.com/hey-red/ImageSharp.Heif which has amditions of adding a encoder in the future.

Cloudflare Image Resizing does support AVIF encoding and this can be used by Slimsy.

### 1. Install the CloudflareImageUrlGenerator package

See https://github.com/Jeavon/CloudflareImageUrlGenerator for full details.

```
dotnet add package Umbraco.Community.CloudflareImageUrlGenerator
```

### 2. Add to Startup.cs in the ConfigureServices method

```c#
.AddCloudflareImageUrlGenerator()
```

### 3. Enable Image Resizing on Cloudflare

https://developers.cloudflare.com/images/image-resizing/enable-image-resizing/

### 4. Set the avif format as a DefaultPictureSources in appsettings.json

e.g.

```json
  "Slimsy": {
    "TagHelper": {
      "DefaultPictureSources": [
        {
          "Extension": "avif",
          "Quality": 60
        },
        {
          "Extension": "webp",
          "Quality": 70
        }
      ]
    }
  }
```

### 5. Check the rendered sources

Image paths for avif and webp should begin with `/cdn-cgi/image/format=avif,quality=70`

# Lazysizes.js

Lazysizes.js is awesome and it's part of what makes Slimsy so easy to implement. If you need to find out more information about it or how to hook into it's Javascript events be sure to check out it's [GitHub](https://github.com/aFarkas/lazysizes)

# Test Site & Source Code

A test site is included in the solution, the username and password for Umbraco are `admin@admin.com/password1234567890.`

To run the Blob Test site you will need a Azure storage account, to set the connection string in a local user secret with the TestSite.TestSiteBlobs folder run

    dotnet user-secrets set "Umbraco:Storage:AzureBlob:Media:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=;AccountKey=;EndpointSuffix=core.windows.net"

# Credits and references

This project includes [LazySizes](https://github.com/aFarkas/lazysizes) 
which is MIT licensed.

Without the amazing [ImageSharp](https://github.com/SixLabors/ImageSharp) this package wouldn't exist, so many thanks go to [James](https://github.com/JimBobSquarePants) and all the [SixLabors](https://github.com/SixLabors) team for creating  ImageSharp!

Many thanks to Douglas Robar for naming Slimsy.

# Change log

[Here](https://github.com/Jeavon/Slimsy/blob/dev-v4/Changelog.md)
