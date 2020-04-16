namespace Slimsy.IOC
{
    using Configurations;
    using Interfaces;
    using Services;
    using Umbraco.Core;
    using Umbraco.Core.Composing;

    public class SlimsyIocRegistration
    {
        public void RegisterDependencies(Composition composition)
        {
            //register slimsy options
            composition.Register<ISlimsyOptions, SlimsyWebConfigOptions>();

            //register slimsy services
            composition.Register<ISlimsyService, SlimsyService>();
        }
    }
}
