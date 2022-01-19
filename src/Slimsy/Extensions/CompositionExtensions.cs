namespace Slimsy.Extensions
{
    using System;
    using Slimsy.Interfaces;
    using Umbraco.Cms.Core.DependencyInjection;
    using Umbraco.Extensions;

    public static class CompositionExtensions
    {
        public static void SetSlimsyOptions(this IUmbracoBuilder builder, Func<IServiceProvider, ISlimsyOptions> serviceProvider)
        {
            builder.Services.AddUnique(serviceProvider);
        }
    }
}
