using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Http;
using HydraWebClient.Models;

namespace HydraWebClient.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        [AllowAnonymous]
        public async Task<bool> Post(LoginViewModel model)
        {
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            switch (result)
            {
                case SignInStatus.Success:
                    return true;
                default:
                    return false;
            }
        }

        [AllowAnonymous]
        public async Task<object> Put(RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return new
                    {
                        Messages = result.Errors
                    };

            await SignInManager.SignInAsync(user, false, false);
            return true;
        }
    }
}