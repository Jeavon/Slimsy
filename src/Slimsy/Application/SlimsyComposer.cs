namespace Slimsy.Application
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Slimsy.Configuration;
    using Slimsy.Extensions;
    using Slimsy.Interfaces;
    using Slimsy.Services;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.DependencyInjection;

    public class SlimsyComposer : IComposer
    {
        public static ISlimsyOptions GetDefaultOptions(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger>();
            return new SlimsyAppSettingsOptions();
        }
        
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<ISlimsyOptions, SlimsyAppSettingsOptions>();
            builder.SetSlimsyOptions(GetDefaultOptions);
            builder.Services.AddSingleton<SlimsyService>();
        }
    }
}
