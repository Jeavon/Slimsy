using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Slimsy
{
    public sealed class SlimsyComposer : IUserComposer
    {
        public static SlimsyOptions GetDefaultOptions(IFactory factory)
        {
            var logger = factory.GetInstance<ILogger>();
            return new SlimsyOptions();
        }

        public void Compose(Composition composition)
        {
            composition.SetSlimsyOptions(GetDefaultOptions);
        }
    }

}
