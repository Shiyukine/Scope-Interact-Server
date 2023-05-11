using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ScopeInteract
{
    public static class CurPos 
    {
        public static double currentXPos = 0;
        public static double currentYPos = 0;
        public static double ny = 0;
        public static double nx = 0;
        public static double maxW = 0;
        public static double maxH = 0;
        public static bool isMouse = true;
        public static double curSensivity = 1;
        public static byte npress = 0;
        public static bool isHovering = false;
        public static bool autoMC = true;
        public static bool isClicking = false;
        public static string cur;
    }
}
