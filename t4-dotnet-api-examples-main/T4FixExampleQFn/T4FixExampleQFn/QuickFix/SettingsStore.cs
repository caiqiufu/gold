using System;
using System.IO;
using System.Text.Json;

namespace T4FixExampleQFn.QuickFix
{
    public static class SettingsStore
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "T4FixExampleQFn");

        private static readonly string SettingsFile =
            Path.Combine(AppFolder, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return new AppSettings();

                string json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(AppFolder);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFile, json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("SettingStore Error: " + ex.Message);
            }
        }
    }
}
