using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookSpace.Data;
using BookSpace.Data.Contracts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookSpace.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                IDatabaseMigrateService migrateService = services.GetRequiredService<IDatabaseMigrateService>();
                migrateService.Migrate();

                IDatabaseSeedService seedService = services.GetRequiredService<IDatabaseSeedService>();
                seedService.SeedDataAsync().GetAwaiter().GetResult();
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
