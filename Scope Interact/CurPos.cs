using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static int isTop = 0;
        public static int isLeft = 0;
        public static double linearVeloX = 0;
        public static double linearVeloY = 0;
        public static double linearVeloZ = 0;
        static double _isMovingX = 0;
        public static double isMovingX
        {
            get
            {
                return _isMovingX;
            }
            set
            {
                //if (_isMovingX != 0 && value == 0) linearVeloX = 0;
                _isMovingX = value;
                Debug.WriteLine(value + " x ");
            }
        }
        public static double isMovingY = 0;
        public static double isMovingZ
        {
            get
            {
                return _isMovingZ;
            }
            set
            {
                //if (_isMovingZ != 0 && value == 0) linearVeloZ = 0;
                _isMovingZ = value;
                Debug.WriteLine(value + " z ");
            }
        }
        static double _isMovingZ = 0;
    }
}
