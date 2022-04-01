using EstoqueWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EstoqueWeb.Services;

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
            options.UseSqlite(Configuration.GetConnectionString("Conexao")));

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {

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
                options.LoginPath = "/Usuario/Login";
            });

            services.Configure<EmailModel>(Configuration.GetSection("EmailModel"));
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
        }
    }
}
