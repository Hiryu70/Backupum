namespace Backupum
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = Configuration.GetSettings();
            var timestamp = Configuration.GetTimestamp();
            Configuration.CreateLogger(settings, timestamp);

            var backuper = new Backuper(settings.TargetPath, settings.SourcePaths, timestamp);
            backuper.Backup();
        }
    }
}
