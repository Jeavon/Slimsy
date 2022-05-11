using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Slimsy.Configuration;
using Slimsy.Services;
using System;
using Umbraco.Cms.Core.DependencyInjection;

namespace Slimsy.DependencyInjection
{
    public static class SlimsyExtensions
    {
        public static IUmbracoBuilder AddSlimsy(this IUmbracoBuilder builder)
             => builder.AddInternal();

        public static IUmbracoBuilder AddSlimsy(this IUmbracoBuilder builder, Action<SlimsyOptions> configure)            
            => builder.AddInternal(optionsBuilder => optionsBuilder.Configure(configure));

        internal static IUmbracoBuilder AddInternal(this IUmbracoBuilder builder,  Action<OptionsBuilder<SlimsyOptions>>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddSingleton<SlimsyService>();

            var optionsBuilder = builder.Services.AddOptions<SlimsyOptions>()
                .BindConfiguration("Slimsy")
                .ValidateDataAnnotations();

            configure?.Invoke(optionsBuilder);

            return builder;
        }
    }
}
