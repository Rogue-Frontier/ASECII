using SadRogue.Primitives;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace ASECII {
    public class Sprite {
        public List<Layer> layers;
        public Point origin;
        public Point end;
        public Dictionary<(int, int), TileValue> preview;
        public static TileValue empty => new TileValue(Color.Transparent, Color.Transparent, 0);
        public Sprite() {
            layers = new List<Layer>();
            preview = new Dictionary<(int, int), TileValue>();
        }
        public void UpdatePreview() {
            preview = new Dictionary<(int, int), TileValue>();

            int left = 0, top = 0, right = 0, bottom = 0;
            foreach(var layer in layers) {
                if(!layer.visible) {
                    continue;
                }
                foreach(var (point, tile) in layer.cells) {
                    var (x, y) = point + layer.pos;

                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    top = Math.Min(top, x);
                    bottom = Math.Max(bottom, x);

                    TileValue t = new TileValue(tile.Foreground, tile.Background, tile.Glyph);
                    if (preview.TryGetValue((x, y), out var current)) {
                        if (t.Background.A != 0) {
                            current.Background = t.Background;
                        }
                        if (t.Foreground.A != 0) {
                            current.Foreground = t.Foreground;
                            current.Glyph = t.Glyph;
                        }
                        preview[(x, y)] = current;
                    } else {
                        preview[(x, y)] = t;
                    }
                }
            }
            origin = new Point(left, top);
            end = new Point(right, bottom);
        }
    }
    public class Layer {
        public Point pos;
        public bool visible;
        public Dictionary<(int, int), TileRef> cells;

        public string name = "Layer 0";
        public Layer() {
            pos = new Point(0, 0);
            visible = true;
            cells = new Dictionary<(int, int), TileRef>();
        }
        public HashSet<Point> GetGlobalFill((Color, Color, int)? source, Point origin, Point end) {
            HashSet<Point> affected = new HashSet<Point>();
            foreach (var p in new Rectangle(origin, end).Positions()) {
                var pt = this[p];
                var pg = pt != null ? (pt.Foreground, pt.Background, pt.Glyph) : (Color.Transparent, Color.Transparent, 0);
                if (pg == source) {
                    affected.Add(p);
                }
            }
            return affected;
        }
        public HashSet<Point> GetFloodFill((int, int) start, (Color, Color, int)? source, Point origin, Point end) {
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            HashSet<Point> affected = new HashSet<Point>();
            Queue<(int, int)> next = new Queue<(int, int)>();

            next.Enqueue(start);
            while (next.Any()) {
                (int x, int y) = next.Dequeue();

                foreach ((int x, int y) p in new List<(int, int)>() {
                                (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) }
                    ) {

                    if (visited.Contains(p)) {
                        continue;
                    }

                    visited.Add(p);

                    if (p.x < origin.X || p.y < origin.Y || p.x > end.X || p.y > end.Y) {
                        continue;
                    }

                    var c = this[p];

                    if ((c != null ? (c.Foreground, c.Background, c.Glyph) : (Color.Transparent, Color.Transparent, 0)) != source) {
                        continue;
                    }

                    affected.Add(p);
                    next.Enqueue(p);
                }
            }
            return affected;
        }

        public TileRef this[Point p] {
            get => cells.TryGetValue(p - pos, out TileRef cg) ? cg : null;
            set {
                if (value == null) cells.Remove(p - pos);
                else cells[p - pos] = value;
            }
        }
        public void Zero() {
            Dictionary<(int, int), TileRef> updated = new Dictionary<(int, int), TileRef>();
            foreach (var (point, tile) in cells) {
                updated[point + pos] = tile;
            }
            cells = updated;
            pos = new Point(0, 0);
        }
    }

    public interface TileRef {
        Color Foreground { get; }
        Color Background { get; }
        int Glyph { get; }
        ColoredGlyph cg { get; }
    }
    public class TileIndex : TileRef {
        [IgnoreDataMember]
        public Color Foreground => cg.Foreground;
        [IgnoreDataMember]
        public Color Background => cg.Background;
        [IgnoreDataMember]
        public int Glyph => cg.Glyph;
        [IgnoreDataMember]
        public ColoredGlyph cg => tiles.tiles[index];

        public TileModel tiles;
        public int index;
        public TileIndex(TileModel tiles, int index) {
            this.tiles = tiles;
            this.index = index;
        }
    }
    public class TilePalette : TileRef {
        [IgnoreDataMember]
        public Color Foreground => palette.palette[foregroundIndex];
        [IgnoreDataMember]
        public Color Background => palette.palette[backgroundIndex];
        [IgnoreDataMember]
        public ColoredGlyph cg => new ColoredGlyph(Foreground, Background, Glyph);

        public PaletteModel palette;
        public int foregroundIndex;
        public int backgroundIndex;
        public int Glyph { get; set; }
        public TilePalette(PaletteModel palette, int foregroundIndex, int backgroundIndex, int Glyph) {
            this.palette = palette;
            this.foregroundIndex = foregroundIndex;
            this.backgroundIndex = backgroundIndex;
            this.Glyph = Glyph;
        }
    }
    [DataContract]
    public class TileValue : TileRef {
        [DataMember]
        public Color Foreground { get; set; }
        [DataMember]
        public Color Background { get; set; }
        [DataMember]
        public int Glyph { get; set; }
        [IgnoreDataMember]
        public ColoredGlyph cg => new ColoredGlyph(Foreground, Background, Glyph);
        public TileValue(Color Foreground, Color Background, int Glyph) {
            this.Foreground = Foreground;
            this.Background = Background;
            this.Glyph = Glyph;
        }
        public static implicit operator ColoredGlyph(TileValue tv) => new ColoredGlyph(tv.Foreground, tv.Background, tv.Glyph);
    }
    public class NotepadTile : TileRef {
        public Color Foreground => Color.Black;
        public Color Background => Color.White;
        public int Glyph { get; set; }
        public ColoredGlyph cg => new ColoredGlyph(Foreground, Background, Glyph);

    }
}
