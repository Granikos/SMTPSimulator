using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

namespace HydraCore.AuthMethods
{
    [Export(typeof(IAuthMethodLoader))]
    public class AuthMethodLoader : DefaultModuleLoader<IAuthMethod>, IAuthMethodLoader
    {
        [ImportingConstructor]
        public AuthMethodLoader([Import] ComposablePartCatalog catalog)
            : base(catalog, "Name")
        {
        }
    }
}