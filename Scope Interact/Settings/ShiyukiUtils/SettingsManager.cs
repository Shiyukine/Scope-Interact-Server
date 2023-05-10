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
    public class SettingsManager
    {
        string file;
        Dictionary<string, string> settings = new Dictionary<string, string>();

        public SettingsManager(string sfile)
        {
            try
            {
                file = sfile;
                createSettingsFile();
                reloadSetting();
            }
            catch (Exception e)
            {
                Infos.addLog("Error : " + e.Message + "\nStacktrace :\n" + e.StackTrace);
            }
        }

        public SettingsManager(string sfile, string[] com)
        {
            try
            {
                file = sfile;
                createSettingsFile();
                StreamWriter sw = File.AppendText(file);
                foreach (string c in com)
                {
                    sw.WriteLine("#" + c);
                }
                sw.Close();
                sw.Dispose();
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
                foreach (string str in File.ReadLines(file))
                {
                    string[] lines = str.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                    if (!str.StartsWith("#") && !settings.ContainsKey(lines[0]) && lines.Length > 1)
                    {
                        settings.Add(lines[0], lines[1]);
                    }
                }
                Infos.addLog("Settings file " + file + " has been reloaded.");
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

        public string getSettingFile()
        {
            return file;
        }

        public bool settingExists(string setting)
        {
            return settings.ContainsKey(setting);
        }

        public bool settingFileExists()
        {
            return File.Exists(file);
        }

        public void createSettingsFile()
        {
            if (!File.Exists(file))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.Create(file).Close();
            }
        }

        public void removeSettingsFile()
        {
            try { File.Delete(file); } catch { }
        }

        public string getStringSetting(string setting)
        {
            string value = null;
            bool hasValue = settings.TryGetValue(setting, out value);
            Infos.addLog("Get value string setting in : " + file + ". Setting : " + setting + ", value : " + value + ".");
            if (hasValue)
            {
                return value;
            }
            else
            {
                throw new SettingNotFoundException("Setting \"" + setting + "\" not found in " + file);
            }
        }

        public int getIntSetting(string setting)
        {
            string value;
            bool hasValue = settings.TryGetValue(setting, out value);
            Infos.addLog("Get value int setting in : " + file + ". Setting : " + setting + ", value : " + value + ".");
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
            Infos.addLog("Get value double setting in : " + file + ". Setting : " + setting + ", value : " + value + ".");
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
                Infos.addLog("Get value string[] setting in : " + file + ". Setting : " + setting + ", value : " + ss + ".");
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
            Infos.addLog("Get value bool setting in : " + file + ". Setting : " + setting + ", value : " + value + ".");
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
                    StreamWriter sw = File.AppendText(file);
                    if (com != null)
                    {
                        foreach (string c in com)
                        {
                            sw.WriteLine("#" + c);
                        }
                    }
                    sw.WriteLine(setting + " = " + value);
                    sw.Close();
                    sw.Dispose();
                    settings.Add(setting, value);
                    Infos.addLog("Add new setting in : " + file + ". Setting : " + setting + ", value : " + value + ".");
                }
                else
                {
                    string set = settings[setting];
                    string strl = "";
                    foreach (string l in File.ReadLines(file))
                    {
                        if (l != "")
                        {
                            strl = strl + l.Replace(setting + " = " + set, setting + " = " + value) + "\n";
                        }
                    }
                    File.WriteAllText(file, strl);
                    settings[setting] = value;
                    Infos.addLog("Replace setting in : " + file + ". Setting : " + setting + ", value : " + set + ", new value : " + value + ".");
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
            foreach (string l in File.ReadLines(file))
            {
                if (l != "" && !l.Contains(setting))
                {
                    str = str + l + "\n";
                }
            }
            File.WriteAllText(file, str);
            Infos.addLog("Removed setting in : " + file + ". Setting :" + setting);
        }

        public string getSettingByValue(string value)
        {
            return settings.FirstOrDefault(x => x.Value == value).Key;
        }
    }
}
//By Shiyuki~Neko - v1.3.2