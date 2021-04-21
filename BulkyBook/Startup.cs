using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BulkyBook.DataAccess.Data;
using BulkyBook.Repository.IRepository;
using BulkyBook.Repository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace BulkyBook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
         
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();


            //HOW THIS  services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();  METHOD WORKS
            //AddIdentity is extension method jo IdentityUser and IdentityRole ko register karta hai IServiceCollection IOC mai
            // Then IdentityBuilder ko return krta hai jis k ander method para hai AddDefaultTokenProviders(); ka jo nehcay use
            //huwa hai then ye i guess IdentityBuilder ko register krta hai IServiceCollection mai   
            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
            //services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton<IEmailSender, EmailSender>();



            //Setting up redirect pages when someone try to  access auouthorize pages
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });


            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "898819949926-rcsi9ctk7mlo57b62ho7d4k5lm925h0g.apps.googleusercontent.com";
                options.ClientSecret = "Xm0czwL3WdR0CVNOZ8crZi96";
            });

            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "2994866190751606";
                options.AppSecret = "58b7c55eed8490935dfee7319574c46a";
            });

            // CHANGING THE DEFAULT PASSWORD VALIDATION FROM IDENTITY WITH DEFFERNT TYPES OF METHDS
            //////////////////////////////////////////////////////////////////////////////////////////////
            //PASSING DELEGATE TO FUNCTION USING SIMPLE METHOD
            //static void Display(IdentityOptions options)
            //{
            //    options.Password.RequiredLength = 10;
            //}
            //Action<IdentityOptions> hello1 = Display;
            /////////////////////////////////////////////////////////////////////////////////////////////////

            ////PASSING DELEGATE TO FUNCTION USING AMYPUMOUES METHOD
            //Action<IdentityOptions> hello2 = delegate (IdentityOptions options)
            //{
            //    options.Password.RequiredLength = 10;

            //};
            /////////////////////////////////////////////////////////////////////////////////////////////////


            ////PASSING DELEGATE TO FUNCTION USING LAMDA EXPRESSION
            //Action<IdentityOptions> hello3 = (options) => options.Password.RequiredLength = 10;
            /////////////////////////////////////////////////////////////////////////////////////////////////

            ////PASSING TO METHOD
            //services.Configure<IdentityOptions>(hello3);
            /////////////////////////////////////////////////////////////////////////////////////////////////







        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();


            });
        }
    }
}
