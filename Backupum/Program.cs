using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace Backupum
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = GetSettings();

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");

            CreateLogger(settings, timestamp);

            Log.Information($"Создание бекапа {timestamp} началось.");

            if (!ValidatePath(settings.TargetPath))
            {
                Log.Error($"Не верно указан путь целевой папки: {settings.TargetPath}.");
                return;
            }

            Log.Information($"Исходных папок: {settings.SourcePaths.Length}");

            foreach (var sourcePath in settings.SourcePaths)
            {
                Log.Information($"---");
                Log.Information($"Исходная папка: {sourcePath}");

                if (!ValidatePath(sourcePath))
                {
                    Log.Error($"Не верно указан путь исходной папки: {sourcePath}.");
                    continue;
                }

                if (!Directory.Exists(sourcePath))
                {
                    Log.Error($"Исходная папка не существует: {sourcePath}.");
                    continue;
                }

                var targetPath = GetNewTimestampPath(settings.TargetPath, timestamp);
                Log.Information($"Целевая папка: {targetPath}");

                Directory.CreateDirectory(targetPath);
                
                CopyFilesRecursively(sourcePath, targetPath);
            }

            Log.Information($"---");
            Log.Information($"Создание бекапа {timestamp} завершено.");
        }

        private static void CreateLogger(Settings settings, string timestamp)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.File($"{timestamp}.log");

            if (settings.Debug)
            {
                loggerConfiguration.MinimumLevel.Debug();
            }

            Log.Logger = loggerConfiguration
                .CreateLogger();
        }

        public static bool ValidatePath(string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
                return false; 

            string root = null;
            string directory = null;
            try
            {
                // throw ArgumentException - The path parameter contains invalid characters, is empty, or contains only white spaces.
                root = Path.GetPathRoot(targetPath);

                // throw ArgumentException - path contains one or more of the invalid characters defined in GetInvalidPathChars.
                // -or- String.Empty was passed to path.
                directory = Path.GetDirectoryName(targetPath);
            }
            catch (ArgumentException)
            {
                return false;
            }

            // null if path is null, or an empty string if path does not contain root directory information
            if (string.IsNullOrEmpty(root)) 
                return false; 

            // null if path denotes a root directory or is null. Returns String.Empty if path does not contain directory information
            if (string.IsNullOrEmpty(directory)) 
                return false; 

            return true;
        }

        private static Settings GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration configuration = builder.Build();

            var options = configuration.GetSection("Settings").Get<Settings>();
            return options;
        }

        private static string GetNewTimestampPath(string targetPath, string timestamp)
        {
            var basePath = Path.Combine(targetPath, timestamp);
            var timestampPath = basePath;

            var n = 0;
            while (Directory.Exists(timestampPath))
            {
                timestampPath = $"{basePath}({n})";
                n++;
            }

            return timestampPath;
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                var replacedPath = dirPath.Replace(sourcePath, targetPath);
                Log.Debug($"Создание папки из {dirPath} в {replacedPath}.");
                Directory.CreateDirectory(replacedPath);
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var replacedFile = newPath.Replace(sourcePath, targetPath);
                Log.Debug($"Копирование файла из {newPath} в {replacedFile}.");
                File.Copy(newPath, replacedFile, true);
            }
        }
    }
}
