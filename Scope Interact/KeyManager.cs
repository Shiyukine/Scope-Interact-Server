using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace ScopeInteract
{
    public static class KeyManager
    {
        public static VirtualKeyCode getKeyByString(string key)
        {
            try
            {
                return (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), key);
            }
            catch
            {
                Infos.addLog("Invalide key ! '" + key + "'");
                return VirtualKeyCode.ACCEPT;
            }
        }
        
        public static string getStringByKey(VirtualKeyCode key)
        {
            return key.ToString();
        }

        public static bool isKeyModifier(string key)
        {
            if (key.StartsWith("VK_")) return false;
            else return true;
        }
    }
}
