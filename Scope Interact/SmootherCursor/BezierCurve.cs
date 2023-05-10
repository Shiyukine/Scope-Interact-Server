using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScopeInteract.SmootherCursor
{
    class BezierCurve
    {
        Point p1;
        Point p2;
        Point p3;

        public BezierCurve(Point p1, Point p2, Point p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        // Parametric functions for drawing a degree 3 Bezier curve.
        public float X(float t)
        {
            return (float)(Math.Pow(1 - t, 2) * p1.X + 2 * (1 - t) * t * p2.X + Math.Pow(t, 2) * p3.X);
        }

        public float Y(float t)
        {
            return (float)(Math.Pow(1 - t, 2) * p1.Y + 2 * (1 - t) * t * p2.Y + Math.Pow(t, 2) * p3.Y);
        }

        public Point getPoint(float t)
        {
            return new Point(X(t), Y(t));
        }

        // Draw the Bezier curve.
        /*public static void DrawBezier(float dt, Point pt0, Point pt1, Point pt2, Point pt3)
        {
            // Draw the curve.
            List<PointF> points = new List<PointF>();
            for (float t = 0.0f; t < 1.0; t += dt)
            {
                points.Add(new PointF(
                    X(t, pt0.X, pt1.X, pt2.X, pt3.X),
                    Y(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y)));
            }

            // Connect to the final point.
            points.Add(new PointF(
                X(1.0f, pt0.X, pt1.X, pt2.X, pt3.X),
                Y(1.0f, pt0.Y, pt1.Y, pt2.Y, pt3.Y)));

            // Draw the curve.
            gr.DrawLines(the_pen, points.ToArray());

            // Draw lines connecting the control points.
            gr.DrawLine(Pens.Red, pt0, pt1);
            gr.DrawLine(Pens.Green, pt1, pt2);
            gr.DrawLine(Pens.Blue, pt2, pt3);
        }*/
    }
}
