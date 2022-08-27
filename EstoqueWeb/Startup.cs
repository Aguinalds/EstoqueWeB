using EstoqueWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EstoqueWeb.Services;
using System;

namespace EstoqueWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDbContext<EstoqueWebContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("Conexao")));

            services.AddIdentity<UsuarioModel, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequiredLength = 6;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = true;

            })
                .AddEntityFrameworkStores<EstoqueWebContext>()
                .AddDefaultTokenProviders();

            services.Configure<PasswordHasherOptions>(options =>
            {
                options.IterationCount = 310000;
            });

            services.AddAuthorization();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "AppControleUsuarios";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/Usuario/Login";
                options.LogoutPath = "/Home/Index";
                options.AccessDeniedPath = "/Usuario/AcessoRestrito";
                options.SlidingExpiration = true;
                options.ReturnUrlParameter = "returnUrl";
            });

            services.Configure<EmailModel>(Configuration.GetSection("EmailModel"));
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            UserManager<UsuarioModel> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            Inicializador.InicializadorIdentity(userManager, roleManager);
        }
    }
}
