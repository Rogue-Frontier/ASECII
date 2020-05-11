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
        public List<ILayer> layers;
        public Layer preview;
        public static ColoredGlyph empty => new ColoredGlyph(Color.Transparent, Color.Transparent, 0);
        public Sprite(int width, int height) {
            this.width = width;
            this.height = height;
            layers = new List<ILayer>();
            layers.Add(new Layer(width, height));
            preview = new Layer(width, height);
        }
        public void UpdatePreview() {
            foreach(var x in Enumerable.Range(0, width)) {
                foreach(var y in Enumerable.Range(0, height)) {
                    var p = new Point(x, y);
                    ColoredGlyph c = empty;
                    foreach(var layer in Enumerable.Range(0, layers.Count)) {
                        var cg = layers[layer][p];
                        if (cg.Background.A != 0) {
                            c.Background = cg.Background;
                        }
                        if(cg.Foreground.A != 0) {
                            c.Foreground = cg.Foreground;
                            c.Glyph = cg.Glyph;
                        }
                    }
                    preview[p] = c;
                }
            }
        }
        public bool InBounds(Point p) => p.X > -1 && p.X < width && p.Y > -1 && p.Y < height;
    }
    interface ILayer {
        ColoredGlyph this[Point p] { get; set; }
    }
    class Layer : ILayer {
        public ColoredGlyph[,] cells;
        public Layer(int width, int height) {
            cells = new ColoredGlyph[width, height];
            var cg = new ColoredGlyph(Color.Transparent, Color.Transparent, ' ');
            foreach (var x in Enumerable.Range(0, width)) {
                foreach(var y in Enumerable.Range(0, height)) {
                    cells[x, y] = cg;
                }
            }
        }
        public ColoredGlyph this[Point p] { get => cells[p.X, p.Y]; set => cells[p.X, p.Y] = value; }
    }
    class ObjectLayer : ILayer {
        public Point pos;
        public Dictionary<Point, ColoredGlyph> cells;
        public ColoredGlyph this[Point p] {
            get => cells.TryGetValue(pos + p, out ColoredGlyph cg) ? cg : new ColoredGlyph(Color.Transparent, Color.Transparent, ' ');
            set {
                if (value == null || value.Foreground.A == 0) cells.Remove(pos + p);
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
