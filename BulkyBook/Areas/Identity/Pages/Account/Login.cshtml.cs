using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BulkyBook.Repository;
using BulkyBook.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using BulkyBook.Utility;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace BulkyBook.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public LoginModel(SignInManager<IdentityUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager,
            IUnitOfWork UnitOfWork,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _unitOfWork = UnitOfWork;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();


                //Checking if user email is not confirmed then block the login
                //var user = await _userManager.FindByEmailAsync(Input.Email);

                //if (user != null   &&  await _userManager.CheckPasswordAsync(user, Input.Password) )
                //{
                //    //We can also use this
                //    //if (!user.EmailConfirmed)   

                //    if (! await _userManager.IsEmailConfirmedAsync(user))
                //    {
                //        ModelState.AddModelError(string.Empty, "Email not confrimed yet");
                //        return Page();

                //    }
                //}
               

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    //var User = _userManager.FindByEmailAsync(Input.Email);
                    var User = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Email == Input.Email);
                    var count = _unitOfWork.ShoppingCart.GetAll((u) => u.ApplicationUserId == User.Id).Count();
                    HttpContext.Session.SetInt32(SD.ssShoppingCart, count);


                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

        
            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if(await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    //Generating Email Cofirmation Code Token using Protector class
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //Encoding the Token 
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    //Creating CallBack Url
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);


                    //Sending Email             
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    ModelState.AddModelError(string.Empty,"Email Sent On Given Email");
                }



            }


            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
