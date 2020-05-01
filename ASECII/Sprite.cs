using SadRogue.Primitives;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASECII {
    class Sprite {
        public int width, height;
        public List<Layer> layers;
        public Sprite(int width, int height) {
            this.width = width;
            this.height = height;
            layers = new List<Layer>();
            layers.Add(new Layer(width, height));
        }
        public bool InBounds(Point p) => p.X > -1 && p.X < width && p.Y > -1 && p.Y < height;
        public ColoredGlyph this[Point p] => layers.First().cells.At(p);
    }
    class Layer {
        public List<List<ColoredGlyph>> cells;
        public Layer(int width, int height) {
            cells = new List<List<ColoredGlyph>>(Enumerable.Range(0, width).Select(i => new List<ColoredGlyph>(Enumerable.Range(0, height).Select(j => new ColoredGlyph(Color.Black, Color.White, ' ')))));
        }
        public ColoredGlyph this[Point p] { set => cells[p.X][p.Y] = value; get => cells[p.X][p.Y]; }
    }
}
