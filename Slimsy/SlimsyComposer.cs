using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Slimsy
{
    public sealed class SlimsyComposer : IUserComposer
    {
        public static ISlimsyOptions GetDefaultOptions(IFactory factory)
        {
            var logger = factory.GetInstance<ILogger>();
            return new SlimsyWebConfigOptions();
        }

        public void Compose(Composition composition)
        {
            composition.SetSlimsyOptions(GetDefaultOptions);
        }
    }

}
