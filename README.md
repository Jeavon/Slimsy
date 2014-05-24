Slimsy
============
**Effortless Responsive Images with Slimmage and Umbraco**

# Implementing post package installation

### 1. Add slimmage.js  to page

In your master template add the Slimmage Javascript file(s) to the top of your head section (Slimmage should be the first js library)

**without bundling scripts (plain HTTP requests)**

```
	<script type="text/javascript">
		window.slimmage = { verbose: false };
	</script>
	<script src="/scripts/slimmage.min.js"></script>
```

**with bundling of your scripts you can fetch the configuration from a seperate file**

    	<script src="/scripts/slimmage.settings.js"></script>
    	<script src="/scripts/slimmage.min.js"></script>

### 2. Adjust your image src attributes

Use the GetResponsiveImageUrl or GetResponsiveCropUrl methods on your dynamic or typed content/media items

		<img src="@featureImage.GetResponsiveImageUrl(270, 161)" alt="" />
		<img src="@featureImage.GetResponsiveImageUrl(270, 161, "Image")" alt="" />
		<img src="@featureImage.GetResponsiveCropUrl("home")" alt="" />
		<img src="@featureImage.GetResponsiveCropUrl("umbracoFile", "home")" alt="" />





# Test Site

A test site is included in the solution, the username and password for Umbraco are admin/password.
By default the test site is configured to use full IIS (due to IIS Express SQL CE persistence issue) on the domain slimsy.local, you can change it to use IIS Express if you prefer.
