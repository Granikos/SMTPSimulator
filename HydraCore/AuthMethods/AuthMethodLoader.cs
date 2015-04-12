using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace HydraCore.AuthMethods
{
    [Export(typeof(IAuthMethodLoader))]
    public class AuthMethodLoader : DefaultModuleLoader<IAuthMethod>, IAuthMethodLoader
    {
        [ExcludeFromCodeCoverage]
        [ImportingConstructor]
        public AuthMethodLoader([Import] ComposablePartCatalog catalog)
            : base(catalog, "Name")
        {
        }
    }
}