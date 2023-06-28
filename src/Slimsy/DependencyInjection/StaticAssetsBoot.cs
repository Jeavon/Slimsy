using System.Collections.Generic;
using System.Diagnostics;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace Slimsy.DependencyInjection
{
    public class StaticAssetsBoot : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
#if NET6_0
            builder.AddSlimsyStaticAssets();
#endif
        }
    }

    public static class uSyncStaticAssetsExtensions
    {
        internal static IUmbracoBuilder AddSlimsyStaticAssets(this IUmbracoBuilder builder)
        {
            if (builder.ManifestFilters().Has<SlimsyAssetManifestFilter>())
                return builder;

            builder.ManifestFilters().Append<SlimsyAssetManifestFilter>();

            return builder;
        }
    }

    internal class SlimsyAssetManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(SlimsyAssetManifestFilter).Assembly;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            manifests.Add(new PackageManifest
            {
                PackageName = "Slimsy",
                Version = version,
                AllowPackageTelemetry = true,
            });
        }
    }
}