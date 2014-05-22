Slimsy
============

Slimmage for Umbraco

# Implementing post package installation #

1. In your master template add the js files to the top of your head section (Slimmage should be the first js library)

* without bundling scripts (plain HTTP requests) *

	<script type="text/javascript">
        	window.slimmage = { verbose: false };
    	</script>
    	<script src="/scripts/slimmage.min.js"></script>

* with bundling of your scripts you can fetch the configuration from a seperate file *

    	<script src="/scripts/slimmage.settings.js"></script>
    	<script src="/scripts/slimmage.min.js"></script>

2. Use the GetResponsiveImageUrl or GetResponsiveCropUrl methods on your dynamic or typed content/media items

		<img src="@featureImage.GetResponsiveImageUrl(270, 161)" alt="" />
		<img src="@featureImage.GetResponsiveImageUrl(270, 161, "Image")" alt="" />
		<img src="@featureImage.GetResponsiveCropUrl("home")" alt="" />
		<img src="@featureImage.GetResponsiveCropUrl("umbracoFile", "home")" alt="" />

# Test Site #

A test site is included in the solution, the username and password for Umbraco are admin/password
