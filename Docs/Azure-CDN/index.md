# Slimsy v3 with Azure CDN
============

## Step 1 - Create a Azure CDN + Endpoint

Either use a custom orgin and set the domain of your website (the example is a Umbraco Cloud site) or select a WebApp

![Create CDN](create-cdn.png)

## Step 2 - Set caching rules

Set Caching Rules to "Cache every unique URL"

![Caching Rules](caching-rules.png)

## Step 3 - Add Mime types

Add the following types

- image/jpeg
- image/pjpeg
- image/png
- image/gif
- image/webp

![Mime Types](mime-types.png)

## Step 4 - Set DomainPrefix in web.config

```xml
<add key="Slimsy:DomainPrefix" value="https://extrarooms.azureedge.net" />
```

## Step 5 - Update GetCropurl 

If you would also like any GetCropUrl methods to return via the CDN then you need to update your code

e.g.

```csharp
<img src="@Url.GetCropUrl(Umbraco.Media(1084), width: 150)"/>
```

change to:

```csharp
<img src="@SlimsyExtensions.GetCropUrl(Umbraco.Media(1084), width: 150)"/>
 ```