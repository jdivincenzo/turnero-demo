using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Aplicacion.Interfaces;
using Infrastructura;
using Infrastructura.Identity;
using Infrastructure.Persistance;
using System.Reflection;

namespace Web
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

            //INICIO de Servicios utilizados por la Aplicacion provistos por Infrastructura

            services.AddScoped<IQRProvider, QRProvider>();
            services.AddScoped<IRepository, ApplicationDbContext>();

            // USAR CurrentUserService PARA PROBAR CON UN USUARIO LOGUEADO
             services.AddScoped<ICurrentUserService, CurrentUserService>();
            //services.AddScoped<ICurrentUserService, SeedCurrentUserService>();
            
            services.AddHttpContextAccessor();
            //FIN de Servicios utilizados por la Aplicacion provistos por Infrastructura

            //Registro los caso de uso
            var typesFromAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(IUseCase))));

            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(type, type, ServiceLifetime.Transient));


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(Configuration.GetConnectionString("InMemoryDB")));
            //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddDefaultIdentity<IdentityUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric =false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews();

            services.AddRazorPages();
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
                // The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "AreaClientes",
                    areaName: "Clientes",
                    pattern: "Clientes/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapAreaControllerRoute(
                    name: "AreaPropietarios",
                    areaName: "Propietarios",
                    pattern: "Propietarios/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "errorHandling",
                    pattern: "Home/Error/{id?}",
                    defaults: new {  controller = "Home", action = "Error" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                     defaults: new { area= "Clientes", controller = "Home", action = "Index" });


                endpoints.MapRazorPages();
            });
        }
    }
}
