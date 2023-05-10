using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScopeInteract.SmootherCursor
{
    public class Vector2
    {
        public double x { get; set; }
        public double y { get; set; }

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 lerp(Vector2 v2, double time)
        {
            return this * (1 - time) + v2 * time;
        }

        public static Vector2 operator +(Vector2 self, Vector2 o)
        {
            return new Vector2(self.x + o.x, self.y + o.y);
        }

        public static Vector2 operator *(Vector2 self, double scale)
        {
            return new Vector2(self.x * scale, self.y * scale);
        }
    }
}
