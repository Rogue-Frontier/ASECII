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
        public TileValue[,] preview;
        public static TileValue empty => new TileValue(Color.Transparent, Color.Transparent, 0);
        public Sprite(int width, int height) {
            this.width = width;
            this.height = height;
            layers = new List<ILayer>();
            layers.Add(new Layer(width, height));
            preview = new TileValue[width, height];
        }
        public void UpdatePreview() {
            foreach(var x in Enumerable.Range(0, width)) {
                foreach(var y in Enumerable.Range(0, height)) {
                    var p = new Point(x, y);
                    TileValue c = empty;
                    foreach(var layer in Enumerable.Range(0, layers.Count)) {
                        var cg = layers[layer][p];
                        if(cg == null) {
                            continue;
                        }
                        if (cg.Background.A != 0) {
                            c.Background = cg.Background;
                        }
                        if(cg.Foreground.A != 0) {
                            c.Foreground = cg.Foreground;
                            c.Glyph = cg.Glyph;
                        }
                    }
                    preview[x, y] = c;
                }
            }
        }
        public bool InBounds(Point p) => p.X > -1 && p.X < width && p.Y > -1 && p.Y < height;
    }
    interface ILayer {
        TileRef this[Point p] { get; set; }
    }
    class Layer : ILayer {
        public int width, height;
        public TileRef[,] cells;
        public Layer(int width, int height) {
            this.width = width;
            this.height = height;
            cells = new TileRef[width, height];
            var cg = new TileValue(Color.Transparent, Color.Transparent, ' ');
            foreach (var x in Enumerable.Range(0, width)) {
                foreach(var y in Enumerable.Range(0, height)) {
                    cells[x, y] = cg;
                }
            }
        }
        public TileRef this[Point p] {
            get => InBounds(p) ? cells[p.X, p.Y] : null;
            set {
                if (InBounds(p)) cells[p.X, p.Y] = value;
            } 
        }
        public bool InBounds(Point p) => p.X > -1 && p.Y > -1 && p.X < width && p.Y < height;
    }
    class ObjectLayer : ILayer {
        public Point pos;
        public Dictionary<Point, TileRef> cells;
        public ObjectLayer() {
            pos = new Point(0, 0);
            cells = new Dictionary<Point, TileRef>();
        }
        public TileRef this[Point p] {
            get => cells.TryGetValue(p - pos, out TileRef cg) ? cg : null;
            set {
                if (value == null) cells.Remove(p - pos);
                else cells[p - pos] = value;
            }
        }
    }

    interface TileRef {
        Color Foreground { get; }
        Color Background { get; }
        int Glyph { get; }
        ColoredGlyph cg { get; }
    }
    class TileIndex : TileRef {
        public Color Foreground => cg.Foreground;
        public Color Background => cg.Background;
        public int Glyph => cg.Glyph;
        public ColoredGlyph cg => tiles.tiles[index];

        private TileModel tiles;
        private int index;
        public TileIndex(TileModel tiles, int index) {
            this.tiles = tiles;
            this.index = index;
        }
    }
    class TileValue : TileRef {
        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public int Glyph { get; set; }
        public ColoredGlyph cg => new ColoredGlyph(Foreground, Background, Glyph);
        public TileValue(Color Foreground, Color Background, int Glyph) {
            this.Foreground = Foreground;
            this.Background = Background;
            this.Glyph = Glyph;
        }
        public static implicit operator ColoredGlyph(TileValue tv) => new ColoredGlyph(tv.Foreground, tv.Background, tv.Glyph);
    }

}
