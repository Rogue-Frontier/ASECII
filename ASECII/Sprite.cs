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
        public ColoredGlyph this[Point p] => layers.First()[p];
    }
    interface ILayer {
        ColoredGlyph this[Point p] { get; set; }
    }
    class Layer {
        public ColoredGlyph[,] cells;
        public Layer(int width, int height) {
            cells = new ColoredGlyph[width, height];
            var cg = new ColoredGlyph(Color.Transparent, Color.Transparent, ' ');
            foreach (var x in Enumerable.Range(0, width)) {
                foreach(var y in Enumerable.Range(0, height)) {
                    cells[width, height] = cg;
                }
            }
        }
        public ColoredGlyph this[Point p] { get => cells[p.X, p.Y]; set => cells[p.X, p.Y] = value; }
    }
    class ObjectLayer {
        public Point pos;
        public Dictionary<Point, ColoredGlyph> cells;
        public ColoredGlyph this[Point p] {
            get => cells.TryGetValue(pos + p, out ColoredGlyph cg) ? cg : new ColoredGlyph(Color.Transparent, Color.Transparent, ' ');
            set {
                if (value == null) cells.Remove(pos + p);
                else cells[pos + p] = value;
            }
        }
    }

    interface TileRef {
        Color foreground { get; }
        Color background { get; }
        int glyph { get; }
        ColoredGlyph cg { get; }
    }
    class TileIndex : TileRef {
        public Color foreground => cg.Foreground;
        public Color background => cg.Background;
        public int glyph => cg.Glyph;
        public ColoredGlyph cg => tiles.tiles[index];

        private TileModel tiles;
        private int index;
        public TileIndex(TileModel tiles, int index) {
            this.tiles = tiles;
            this.index = index;
        }
    }
    class TileValue : TileRef {
        public Color foreground { get; }
        public Color background { get; }
        public int glyph { get; }
        public ColoredGlyph cg => new ColoredGlyph(foreground, background, glyph);
    }

}
