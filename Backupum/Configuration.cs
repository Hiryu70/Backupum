using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Backupum
{
    static class Configuration
    {
        /// <summary>
        /// Получить настройки
        /// </summary>
        public static Settings GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration configuration = builder.Build();

            var options = configuration.GetSection("Settings").Get<Settings>();
            return options;
        }

        /// <summary>
        /// Получить временной штамп
        /// </summary>
        public static string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
        }

        /// <summary>
        /// Создать логгер
        /// </summary>
        /// <param name="settings">Настройки</param>
        /// <param name="timestamp">Временной штамп</param>
        public static void CreateLogger(Settings settings, string timestamp)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(settings.LogEventLevel)
                .WriteTo.File($"{timestamp}.log");

            Log.Logger = loggerConfiguration
                .CreateLogger();
        }
    }
}
