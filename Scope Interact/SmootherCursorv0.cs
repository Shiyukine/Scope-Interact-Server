using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScopeInteract
{
    public static class SmootherCursorv0
    {
        private static List<Point> points = new List<Point>();
        public static bool enable = false;

        public static async Task<List<Point>> addPoint(double x, double y)
        {
            Point np = new Point(x, y);
            /*if (System.Environment.TickCount - lastTick > 1000)
            {
                for(int i = 0; i < points.Count; i++)
                {
                    if (i < 2) points.RemoveAt(i);
                }
                //Infos._main.Dispatcher.Invoke(() => Infos.addLog("rm"));
            }*/
            if (!enable) return new List<Point>() { np };
            if (points.Count == 3)
            {
                var tmp1 = points[1];
                var tmp2 = points[2];
                var tmp3 = np;
                points[0] = tmp1;
                points[1] = tmp2;
                points[2] = tmp3;
                //
                Point i0 = points[0];
                Point i1 = points[1];
                Point i2 = points[2];
                Point inter = IntersectionOfTwoLines(i0, i1, i1, i2);
                if (inter.X != -1 && inter.Y != -1)
                {
                    int rayon = (int)RayonCercle(i1, inter);
                    return await GetAllPoints(inter, rayon, i0, i2);
                }
            }
            else
            {
                points.Add(np);
            }
            Infos._main.Dispatcher.Invoke(() => Infos.addLog("rm" + points.Count));
            return points;
        }

        public static void releasePoints()
        {
            points.Clear();
        }

        private static async Task<List<Point>> GetAllPoints(Point circle, double r, Point pointB, Point pointC)
        {
            return await Task.Run(() =>
            {
                Point main = new Point(r * Math.Cos(0) + circle.X, r * Math.Sin(0) + circle.Y);
                double start = getAngleWith3Points(pointB, circle, main, r);
                double end = getAngleWith3Points(pointC, circle, main, r);
                double total = end - start;
                if (start < 90 && end > 270) total = 360 - end + start;
                if (end < 90 && start > 270) total = end + start - 360;
                if (total > 180) total -= 360;
                if (total < -180) total += 360;
                List<Point> ps = new List<Point>
                {
                    pointB
                };
                if (total > 0)
                {
                    for (double i = start; i < start + total; i += 0.5)
                    {
                        double nangle = i;
                        //if (nangle > 360) nangle -= 360;
                        nangle = Math.PI * nangle / 180.0;
                        Point point = new Point(r * Math.Cos(nangle) + circle.X, r * Math.Sin(nangle) + circle.Y);
                        if(!double.IsNaN(point.X) && !double.IsNaN(point.Y)) ps.Add(point);
                    }
                }
                else
                {
                    for (double i = start + total; i > start; i -= 0.5)
                    {
                        double nangle = i;
                        //if (nangle < 0) nangle += 360;
                        nangle = Math.PI * nangle / 180.0;
                        Point point = new Point(r * Math.Cos(nangle) + circle.X, r * Math.Sin(nangle) + circle.Y);
                        if (!double.IsNaN(point.X) && !double.IsNaN(point.Y)) ps.Add(point);
                    }
                }
                //ps.Add(pointC);
                return ps;
            });
        }

        private static double getAngleWith3Points(Point query, Point circle, Point main, double r)
        {
            double atan = Math.Atan2(query.Y - circle.Y, query.X - circle.X) - Math.Atan2(main.Y - circle.Y, main.X - circle.X);
            double angle = atan * (180 / Math.PI);
            if (angle < 0) angle = 360 + angle;
            return angle;
        }

        private static Point getMiddlePointOfLine(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        [Obsolete("This method is obsolete.", true)]
        private static async Task<List<Point>> AndresCircle(double xc, double yc, int r)
        {
            return await Task.Run(() =>
            {
                List<Point> ret = new List<Point>();

                int x = 0;
                int y = r;
                int d = r - 1;

                while (y >= x)
                {
                    ret.Add(new Point(xc + x, yc + y));
                    ret.Add(new Point(xc + y, yc + x));
                    ret.Add(new Point(xc - x, yc + y));
                    ret.Add(new Point(xc - y, yc + x));
                    ret.Add(new Point(xc + x, yc - y));
                    ret.Add(new Point(xc + y, yc - x));
                    ret.Add(new Point(xc - x, yc - y));
                    ret.Add(new Point(xc - y, yc - x));

                    if (d >= 2 * x)
                    {
                        d -= 2 * x + 1;
                        x++;
                    }
                    else if (d < 2 * (r - y))
                    {
                        d += 2 * y - 1;
                        y--;
                    }
                    else
                    {
                        d += 2 * (y - x - 1);
                        y--;
                        x++;
                    }
                }
                return ret;
            });
        }

        private static Point IntersectionOfTwoLines(Point d1a, Point d1b, Point d2a, Point d2b)
        {
            if ((d1a.X == d2b.X && d1a.X == d1b.X) || (d1a.Y == d2b.Y && d1a.Y == d1b.Y)) return new Point(-1, -1);
            double coef1 = (d1b.X - d1a.X) / (d1b.Y - d1a.Y);
            double ordonnee1 = (coef1 > 0 ? d1a.Y - coef1 * d1a.X : d1a.Y + coef1 * d1a.X);
            //
            double coef2 = (d2b.X - d2a.X) / (d2b.Y - d2a.Y);
            double ordonnee2 = (coef2 > 0 ? d2a.Y - coef2 * d2a.X : d2a.Y + coef2 * d2a.X);
            //
            double x = (ordonnee1 - ordonnee2) / (coef2 - coef1);
            double y = coef1 * x + ordonnee1;
            return new Point(x, y);
        }

        private static double RayonCercle(Point pointInCircle, Point circlePos)
        {
            return Math.Sqrt(Math.Pow(pointInCircle.X - circlePos.X, 2) + Math.Pow(pointInCircle.Y - circlePos.Y, 2));
        }
    }
}
