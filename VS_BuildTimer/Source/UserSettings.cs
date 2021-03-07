using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSBuildTimer
{
    namespace SettingsV1
    {
        public class UserSettings
        {
            public UserSettings()
            {
                this.Version = 1;
                this.ZoomLevel = 10;
                this.SortingColumn = "Whatever";
                this.AscendingOrder = true;
            }
            public int Version { get; }

            public double ZoomLevel { set; get; }

            public string SortingColumn { set; get; }

            public bool AscendingOrder { get; set; }
        }

        class SettingsReader
        {
            public UserSettings ReadSettings()
            {
                try
                {
                    string filename = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "VSBuildTimer.json");
                    using (StreamReader file = File.OpenText(filename))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        UserSettings settings = (UserSettings)serializer.Deserialize(file, typeof(UserSettings));
                        return settings;
                    }
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(String.Format("Error while reading user settings {0}", e.Message));
                    return null;
                }
            }
        }

        class SettingsWriter
        {
            public void WriteSettings(UserSettings settings)
            {
                try
                {
                    string filename = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "VSBuildTimer.json");

                    using (StreamWriter writer = new StreamWriter(filename))
                    {
                        writer.Write(JsonConvert.SerializeObject(settings, Formatting.Indented));
                    }
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(String.Format("Error while saving user settings {0}", e.Message));
                }
            }
        }
    }

    public class SettingsManager
    {
        public SettingsV1.UserSettings GetSettings()
        {
            var reader = new SettingsV1.SettingsReader();
            this.settings = reader.ReadSettings();
            return this.settings;
        }

        public void StoreSettings()
        {
            var writter = new SettingsV1.SettingsWriter();
            writter.WriteSettings(this.settings);
        }

        private SettingsV1.UserSettings settings;
    }
}
