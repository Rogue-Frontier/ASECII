using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASECII {
    public static class Helper {
        public static bool InBounds<T>(this List<List<T>> l, Point p) {
            return p.X > -1 && p.X < l.Count && p.Y > -1 && p.Y < l[p.X].Count;
        }
        public static T At<T>(this List<List<T>> l, Point p) {
            return l[p.X][p.Y];
        }
        public static Point Plus(this Point p1, Point p2) {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static double Length(this Point p1) {
            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
        }
        public static Color GetTextColor(this Color c) => c.GetLuma() > 102 ? Color.Black : Color.White;
        public static Color HsvToRgb(double h, double S, double V) {
            int r, g, b;

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0) { R = G = B = 0; } else if (S <= 0) {
                R = G = B = V;
            } else {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i) {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
            return new Color(r, g, b);
            /// <summary>
            /// Clamp a value to 0-255
            /// </summary>
            int Clamp(int i) {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return i;
            }
        }
    }
}
