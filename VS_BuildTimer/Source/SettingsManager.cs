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
        public struct UserSettings
        {
            static public UserSettings CreateDefault()
            {
                UserSettings settings = new UserSettings();
                settings.Version = 1;
                settings.ZoomLevel = 10;
                settings.SortingColumn = "StartTime";
                settings.AscendingOrder = true;
                return settings;
            }
            public int Version { set;  get; }

            public double ZoomLevel { set; get; }

            public string SortingColumn { set; get; }

            public bool AscendingOrder { get; set; }
        }
    }


    public class SettingsManager : IDisposable
    {
        public SettingsManager()
        {
            m_timer = new System.Timers.Timer(500);
            m_timer.Elapsed += this.OnTimerTick;
            m_dirty = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_timer.Dispose();
            }
        }

        public SettingsV1.UserSettings GetSettings()
        {
            if (m_settings.HasValue)
                return m_settings.Value;

            try
            {
                string filename = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VSBuildTimer.json");
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    m_settings = (SettingsV1.UserSettings)serializer.Deserialize(file, 
                                    typeof(SettingsV1.UserSettings));
                }

            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(String.Format("Error while reading user settings: {0}", e.Message));
                m_settings = SettingsV1.UserSettings.CreateDefault();
            }

            return m_settings.Value;
        }

        public void SetSettings(SettingsV1.UserSettings settings)
        {
            m_settings = settings;
            m_dirty = true;
        }
        public void SaveSettings()
        {
            try
            {
                string filename = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VSBuildTimer.json");

                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.Write(JsonConvert.SerializeObject(m_settings, Formatting.Indented));
                }
                m_dirty = false;
                m_lastSaveTime = System.DateTime.Now;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(String.Format("Error while saving user settings {0}", e.Message));
            }
        }

        private void OnTimerTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (m_dirty && System.DateTime.Now - m_lastSaveTime > System.TimeSpan.FromSeconds(2))
                SaveSettings();
        }

        private SettingsV1.UserSettings? m_settings;
        private bool m_dirty;
        private DateTime m_lastSaveTime;
        private readonly System.Timers.Timer m_timer;
    }
}
