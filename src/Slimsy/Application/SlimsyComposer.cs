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
            return new SlimsyWebConfigOptions();
        }
        
        public void Compose(IUmbracoBuilder builder)
        {
            //builder.Services.AddUnique<ISlimsyOptions, SlimsyWebConfigOptions>();
            builder.SetSlimsyOptions(GetDefaultOptions);
            builder.Services.AddSingleton<SlimsyService>();
        }
    }
}
