using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SodaMachine
{
    class Program
    {
        protected static readonly IConfiguration _Configuration;
        protected static readonly IServiceProvider _ServiceProvider;
        protected static readonly Serilog.ILogger _Logger;

        static Program()
        {
            try
            {
                _Configuration = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("appSettings-logging.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("appSettings-logging-seq.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("appSettings-core-calculation.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("appSettings.secret.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("config/logging.json", optional: true, reloadOnChange: true)
                     .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                     .Build();

                _Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_Configuration)
                    .CreateLogger();

                var services = new ServiceCollection()
                    .AddSingleton(_Configuration)
                    .AddDbContext<SodaMachineDbContext>(options => options.UseInMemoryDatabase("SodaMachine"))
                    .AddTransient<ISodaMachineDbInitializer, SodaMachineDbInitializer>()
                    .AddTransient<ISodaMachineService, SodaMachineService>()
                    .AddSingleton<IUserInterfaceController, UserInterfaceController>();

                services.AddLogging(builder =>
                {
                    builder.AddSerilog(logger: _Logger, dispose: true);
                });

                _ServiceProvider = services.BuildServiceProvider();

                SeedData(_ServiceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Startup Error {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            _Logger.Debug("Main Starting...");
            try
            {
                Console.WriteLine("========================================================-=============================");
                Console.WriteLine("=======================*Welcome to Automated Soda Machine*============================");
                Console.WriteLine("========================================================-=============================");
                Console.WriteLine("Please follow the instructions as listed below");

                Console.WriteLine("Press c to buy soda with cash and s to buy soda with sms, and to quit press q");

                char itemType = Console.ReadKey().KeyChar;
                if (itemType.ToString().ToLower() != "c" && itemType.ToString().ToLower() != "s" && itemType.ToString().ToLower() != "q")
                {
                    Console.WriteLine("Please choose c, s or q");
                    return;
                }
                Console.WriteLine();
                var userChoice = itemType.ToString().ToLower();
                var userInterfaceController = _ServiceProvider.GetRequiredService<IUserInterfaceController>();
                while (itemType != 'q')
                {
                    switch (userChoice)
                    {
                        case "c":
                            {
                                var response = userInterfaceController.ProcessTransactionWithCash();
                                if (response > 0)
                                {
                                    itemType = 'q';
                                }
                                break;
                            }
                        case "s":
                            {
                                var response = userInterfaceController.ProcessOrderWithSMS();
                                if (response > 0)
                                {
                                    itemType = 'q';
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }


            }
            catch (Exception ex)
            {
                _Logger.Error(ex, "Main exception message {message}", ex.Message);
            }
            _Logger.Debug("Main end...");
            Console.ReadKey();
        }

        private static void SeedData(IServiceProvider services)
        {
            var dbInitializer = services.GetRequiredService<ISodaMachineDbInitializer>();
            dbInitializer.Initialize();
            dbInitializer.SeedData();
        }


        // end of class program
    }
}
