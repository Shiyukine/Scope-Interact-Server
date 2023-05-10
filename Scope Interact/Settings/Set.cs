using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeInteract.Settings
{
    public class Set
    {
        public static void syncSettings(MainWindow main)
        {
            if (!Infos.sf.settingExists("Logs")) Infos.sf.setSetting("Logs", false, null);
            main.logs_e.IsChecked = Infos.sf.getBoolSetting("Logs");
            if (!Infos.sf.settingExists("Monitor")) Infos.sf.setSetting("Monitor", 0, null);
            main.moni.SelectedIndex = Infos.sf.getIntSetting("Monitor");
            if (!Infos.sf.settingExists("Debug")) Infos.sf.setSetting("Debug", false, null);
            main.debug_c.IsChecked = Infos.sf.getBoolSetting("Debug");
            if (!Infos.sf.settingExists("UseUSB")) Infos.sf.setSetting("UseUSB", false, null);
            main.isusb.IsChecked = Infos.sf.getBoolSetting("UseUSB");
            //keys 
            if (!Infos.sf.settingExists("key_1")) Infos.sf.setSetting("key_1", "CONTROL;VK_Z", null);
            if (!Infos.sf.settingExists("key_2")) Infos.sf.setSetting("key_2", "VK_E", null);
            if (!Infos.sf.settingExists("key_3")) Infos.sf.setSetting("key_3", "VK_B", null);
            if (!Infos.sf.settingExists("key_4")) Infos.sf.setSetting("key_4", "CONTROL;OEM_PLUS", null);
            if (!Infos.sf.settingExists("key_5")) Infos.sf.setSetting("key_5", "CONTROL;VK_6", null);
            if (!Infos.sf.settingExists("key_6")) Infos.sf.setSetting("key_6", "OEM_4", null);
            if (!Infos.sf.settingExists("key_7")) Infos.sf.setSetting("key_7", "OEM_6", null);
            if (!Infos.sf.settingExists("key_8")) Infos.sf.setSetting("key_8", "SPACE", null);
            //upd
            if (!Infos.sf.settingExists("Url_Server")) Infos.sf.setSetting("Url_Server", "aketsuky.com", new string[] { "Server" });
        }
    }
}
