using ShiyukiUtils.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScopeInteract;

namespace ShiyukiUtils.Settings
{
    public class SettingsReaderString
    {
        string file;
        string text;
        SettingsManager sm;
        Dictionary<string, string> settings = new Dictionary<string, string>();
        string type = "text";

        public SettingsReaderString(string path, bool isFile)
        {
            try
            {
                file = path;
                type = "file";
                reloadSetting();
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
            }
        }

        public SettingsReaderString(SettingsManager sm)
        {
            try
            {
                settings = sm.getAll();
                this.sm = sm;
                type = "sm";
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
            }
        }

        public SettingsReaderString(string text)
        {
            try
            {
                this.text = text;
                reloadSetting();
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
            }
        }

        public void reloadSetting()
        {
            try
            {
                settings.Clear();
                switch (type)
                {
                    case "file":
                        foreach (string str in File.ReadLines(file.ToString()))
                        {
                            string[] lines = str.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                            if (!str.StartsWith("#") && !settings.ContainsKey(lines[0]) && lines.Length > 1)
                            {
                                settings.Add(lines[0], lines[1]);
                            }
                        }
                        break;

                    case "text":
                        foreach (string str in text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string[] lines = str.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                            if (!str.StartsWith("#") && !settings.ContainsKey(lines[0]) && lines.Length > 1)
                            {
                                settings.Add(lines[0], lines[1]);
                            }
                        }
                        break;

                    case "sm":
                        settings = sm.getAll();
                        break;
                }
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
                throw new SettingsFileIsCorruptedException("The settings file " + file + " is corrupted. Please check this file.", e.InnerException);
            }
        }

        public string[] getSettings()
        {
            return settings.Keys.ToArray();
        }

        public Dictionary<string, string> getAll()
        {
            return settings;
        }

        public string getAllInString()
        {
            string a = "";
            foreach(KeyValuePair<string, string> l in settings)
            {
                a = a + l.Key + " = " + l.Value;
            }
            return a;
        }

        public bool settingExists(string setting)
        {
            return settings.ContainsKey(setting);
        }

        public string getStringSetting(string setting)
        {
            string value = null;
            bool hasValue = settings.TryGetValue(setting, out value);
            if (hasValue)
            {
                return value;
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found.");
            }
        }

        public int getIntSetting(string setting)
        {
            string value;
            bool hasValue = settings.TryGetValue(setting, out value);
            if (hasValue)
            {
                return int.Parse(value);
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found.");
            }
        }

        public double getDoubleSetting(string setting)
        {
            string value;
            bool hasValue = settings.TryGetValue(setting, out value);
            if (hasValue)
            {
                return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found.");
            }
        }

        public string[] getStringListSetting(string setting)
        {
            string value;
            bool hasValue = settings.TryGetValue(setting, out value);
            if (hasValue)
            {
                string[] ss = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                return ss;
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found.");
            }
        }

        public double getDoubleByBool(string setting)
        {
            if (settings[setting] == "True")
            {
                return 0.1;
            }
            else
            {
                return 0;
            }
        }

        public bool getBoolSetting(string setting)
        {
            bool value = false;
            bool hasValue = bool.TryParse(settings[setting], out value);
            if (hasValue)
            {
                return value;
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found.");
            }
        }

        public void setSetting(string setting, bool value, string[] com)
        {
            saveSetting(setting, value.ToString(), com);
        }

        public void setSetting(string setting, string value, string[] com)
        {
            saveSetting(setting, value, com);
        }

        public void setSetting(string setting, string[] value, string[] com)
        {
            string str = "";
            foreach (string r in value)
            {
                str = str + r + ";";
            }
            saveSetting(setting, str, com);
        }

        public void setSetting(string setting, int value, string[] com)
        {
            saveSetting(setting, value.ToString(), com);
        }

        public void setSetting(string setting, double value, string[] com)
        {
            saveSetting(setting, value.ToString(), com);
        }

        private void saveSetting(string setting, string value, string[] com)
        {
            try
            {
                if (!settings.ContainsKey(setting))
                {
                    settings.Add(setting, value);
                }
                else
                {
                    string set = settings[setting];
                    settings[setting] = value;
                }
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
            }
        }

        public void removeSetting(string setting)
        {
            settings.Remove(setting);
            string str = "";
            foreach (string l in text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (l != "" && !l.Contains(setting))
                {
                    str = str + l + "\n";
                }
            }
        }

        public string getSettingByValue(string value)
        {
            return settings.FirstOrDefault(x => x.Value == value).Key;
        }
    }
}
//By Shiyuki~Neko