using KeelteKooli.Controllers;
using KeelteKooli.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace TARpv24_KeelteKool_XUnitTesting
{
    internal abstract class TestBase
    {
        protected IServiceProvider serviceProvider { get; set; }

        protected TestBase()
        {
            var services = new ServiceCollection();
            SetupServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
       /// <summary>
       /// Seame üles testide läbiviimiseks vajalikud kontrollerid mujalt projektis
       /// See meetod annab ka mälusoleva andmebaasi mida testideks kasutada,
       /// VIPER-tüüpi projektis, toimib kui "program.cs" analoog, ent lühidal kujul.
       /// </summary>
       /// <param name="services"></param>kollektor kuhu asetame kontrolleri instantsid</param>

        private void SetupServices(ServiceCollection services)
        {
            services.AddScoped<StudentController>();
            services.AddScoped<IHostEnvironment, MockIHostEnvironment>();

            services.AddDbContext<IdentityConfig>(x =>
            {
                x.UseInMemoryDatabase("TEST");
                x.ConfigureWarnings(b =>
                    b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });


            RegisterMacros(services);
        }

        public void Dispose()
        {

        }
        /// <summary>
        /// Leia üles kindel teenus,teenusepakkujalt.
        /// serviceProvider omab kontrollerite instantse, ning GetService halgid selle
        /// X tüüpi kontrolleri
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Svc<T>()
        {
            return serviceProvider.GetService<T>();
        }

        private void RegisterMacros(ServiceCollection services)
        {
            var macroBaseType = typeof(IMacros);

            var macros = macroBaseType.Assembly.GetTypes()
                .Where(t => macroBaseType.IsAssignableFrom(t)
                && !t.IsInterface && !t.IsAbstract);
            foreach (var macro in macros)
            {
                services.AddSingleton(macro);
            }
        }
    }
}
