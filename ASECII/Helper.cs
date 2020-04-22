using Microsoft.Xna.Framework;
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
    }
}
