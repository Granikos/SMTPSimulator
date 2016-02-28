using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Http;
using Granikos.NikosTwo.WebClient.Models;
using Microsoft.Owin.Security;

namespace Granikos.NikosTwo.WebClient.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private IAuthenticationManager _authManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IAuthenticationManager authManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            AuthenticationManager = authManager;
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

        public IAuthenticationManager AuthenticationManager
        {
            get { return _authManager ?? HttpContext.Current.GetOwinContext().Authentication; }
            private set { _authManager = value; }
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public string GetInfo()
        {
            return User.Identity.IsAuthenticated? AuthenticationManager.User.Identity.Name : null;
        }

        [HttpDelete]
        [Route("")]
        public bool Logout()
        {
            AuthenticationManager.SignOut();

            return true;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        public async Task<bool> Login(LoginViewModel model)
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
        [HttpPut]
        [Route("")]
        public async Task<object> Register(RegisterViewModel model)
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