
namespace Slimsy.Application
{
    using IOC;
    using Umbraco.Core.Composing;

    public sealed class SlimsyComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            //register Ioc
            var iocRegistration = new SlimsyIocRegistration();
            iocRegistration.RegisterDependencies(composition);
        }
    }
}
