﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BulkyBook.Models;
using BulkyBook.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BulkyBook.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
       
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RegisterModel(
            UserManager<IdentityUser> userManager,

            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork= unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string PhoneNumber { get; set; }
            public int? CompanyId { get; set; }
            public string Role { get; set; }

            public IEnumerable<SelectListItem> CompanyList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            Input = new InputModel()
            {
                CompanyList = _unitOfWork.Company.GetAll().Select(i=> new SelectListItem { Text =i.Name  , Value= i.Id.ToString() }),

                RoleList = _roleManager.Roles.Where(u => u.Name != SD.Role_User_Indi).Select(x=>x.Name).Select(i=> new SelectListItem {Text=i , Value=i })
            };

            if (User.IsInRole(SD.Role_Employee))
            {
                Input.RoleList = _roleManager.Roles.Where(u => u.Name == SD.Role_User_Comp).Select(x => x.Name).Select(i => new SelectListItem { Text = i, Value = i });
            };

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }




        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    CompanyId = Input.CompanyId,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    State = Input.State,
                    PostalCode = Input.PostalCode,
                    Name = Input.Name,
                    PhoneNumber = Input.PhoneNumber,
                    Role = Input.Role
                };
   

                //CREATING USER USING IDENTITIY 
                var result = await _userManager.CreateAsync(user, Input.Password);


                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                  
                    //ADDING ROLES IF NOT EXIXTS IN DB
                    //if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                    //{
                    //    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    //}
                    //if (!await _roleManager.RoleExistsAsync(SD.Role_Employee))
                    //{
                    //    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                    //}
                    //if (!await _roleManager.RoleExistsAsync(SD.Role_User_Comp))
                    //{
                    //    await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp));
                    //}
                    //if (!await _roleManager.RoleExistsAsync(SD.Role_User_Indi))
                    //{
                    //    await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi));
                    //}



                    // ASIGHNING ROLES TO USER
                    if (user.Role== null)
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_User_Indi);
                    }
                    else
                    {
                        if(user.CompanyId > 0)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_User_Comp);
                        }
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }



                    //Generating Email Cofirmation Code Token 
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //Encoding the Token 
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    //Creating CallBack Url
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //Generating path of Templates
                    var PathToFile = _hostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                        + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplates"+
                         Path.DirectorySeparatorChar.ToString() + "Confirm_Account_Registration.html";


                    var subject = "Confirm Account Registration";

                    string HtmlBody = "";

                    using (StreamReader streamReader = System.IO.File.OpenText(PathToFile) )
                    {
                        HtmlBody = streamReader.ReadToEnd();
                    }

                    //{0} : Subject  
                    //{1} : DateTime  
                    //{2} : Name  
                    //{3} : Email  
                    //{4} : callbackURL

                    string messageBody = string.Format(HtmlBody,
                        subject,
                        String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                        user.Name,
                        user.Email,
                        callbackUrl
                        );
             
                    //Sending Email             
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", messageBody);

                      



                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {   
                        //MEANS USER IS CREATED FORM WEB SITE AND NOW SIGHNING 
                        if (user.Role == null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            //ADMIN IS REGISTRING A NEW USER
                            return RedirectToAction("Index", "User", new { Area = "Admin" });
                        }
                    }
                }



                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            //Loading the Dropdowns 
            Input = new InputModel()
            {
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(u => u.Name != SD.Role_User_Indi).Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
