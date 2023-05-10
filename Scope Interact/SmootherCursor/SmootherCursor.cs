using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScopeInteract.SmootherCursor
{
    public static class SmootherCursor
    {
        private static List<Point> points = new List<Point>();
        static List<Point> newPoints = new List<Point>();
        public static bool enable = false;

        public static async void addPoint(double x, double y)
        {
            await Task.Run(() =>
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
                if (enable)
                {
                    if (points.Count == 3)
                    {
                        var tmp1 = points[1];
                        var tmp2 = points[2];
                        var tmp3 = np;
                        points[0] = tmp1;
                        points[1] = tmp2;
                        points[2] = tmp3;
                        //
                        Point i2 = points[2];
                        Point i1 = points[1];
                        Point i0 = points[0];
                        /*Point i0 = new Point(400, 400);
                        Point i1 = new Point(500, 500);
                        Point i2 = new Point(400, 600);*/
                        Vector v1 = new Vector(i1.X - i0.X, i1.Y - i0.Y);
                        Vector v2 = new Vector(i2.X - i1.X, i2.Y - i1.Y);
                        Vector v = v1 + v1 / 2 + v2 / 2;
                        double nx = i1.X + v.X;
                        double ny = i1.Y + v.Y;
                        BezierCurve bc = new BezierCurve(i0, new Point(nx, ny), i2);
                        for (double i = 0.5; i < 1.0; i += 0.01)
                        {
                            newPoints.Add(bc.getPoint((float)i));
                        }
                    }
                    else
                    {
                        points.Add(np);
                    }
                    Infos._main.Dispatcher.Invoke(() => Infos.addLog("rm" + points.Count));
                }
            });
        }

        public static void releasePoints()
        {
            points.Clear();
        }

        public static List<Point> GetAllPoints()
        {
            //ps.Add(pointC);
            return newPoints;
        }

        public static void clearPoint(Point p)
        {
            newPoints.RemoveAt(0);
        }
    }
}
