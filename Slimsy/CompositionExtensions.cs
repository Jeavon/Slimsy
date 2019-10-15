using System;
using Umbraco.Core.Composing;

namespace Slimsy
{
    public static class CompositionExtensions
    {
        public static void SetSlimsyOptions(this Composition composition, Func<IFactory, SlimsyOptions> factory)
        {
            composition.RegisterUnique(factory);
        }
    }
}
