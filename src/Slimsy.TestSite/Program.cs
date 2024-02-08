using Slimsy.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddSlimsy()
    //.AddSlimsy(options =>
    //{
    //    options.DefaultQuality = 60;
    //    options.WidthStep = 60;
    //    options.UseCropAsSrc = true;
    //    options.TagHelper.RenderPictureSources = new string[] { "jxl" };
    //    options.TagHelper.SingleSourceExtensions = new string[] { "gif" };
    //})
    .Build();
WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();