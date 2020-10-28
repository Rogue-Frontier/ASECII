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
                if (!layer.visible) {
                    continue;
                }
                Action<TileValue, TileValue> modifier = null;
                if(layer.applyBackground) {
                    modifier += (t, current) => {
                        if (t.Background.A != 0) {
                            current.Background = t.Background.Blend(current.Background);
                        }
                    };
                }
                if (layer.applyForeground) {
                    modifier += (t, current) => {
                        if (t.Foreground.A != 0) {
                            current.Foreground = t.Foreground;
                        }
                    };
                }
                if(layer.applyGlyph) {
                    modifier += (t, current) => {
                        if (t.Glyph != 0) {
                            current.Glyph = t.Glyph;
                        }
                    };
                }
                if(modifier == null) {
                    continue;
                }

                foreach(var (point, tile) in layer.cells) {
                    var (x, y) = point + layer.pos;

                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    top = Math.Min(top, x);
                    bottom = Math.Max(bottom, x);

                    TileValue t = new TileValue(tile.Foreground, tile.Background, tile.Glyph);

                    if (!preview.TryGetValue((x, y), out var current)) {
                        current = preview[(x, y)] = new TileValue(Color.Transparent, Color.Transparent, 0);
                    }

                    modifier?.Invoke(t, current);
                }
            }
            origin = new Point(left, top);
            end = new Point(right, bottom);
        }
    }
    public class Layer {
        public Point pos;
        public bool visible, applyGlyph, applyForeground, applyBackground;
        public Dictionary<(int, int), TileRef> cells;

        public string name = "Layer 0";
        public Layer() {
            pos = new Point(0, 0);
            visible = true;
            
            applyGlyph = true;
            applyForeground = true;
            applyBackground = true;

            cells = new Dictionary<(int, int), TileRef>();
        }
        public static (Color, Color, int)? Triple(TileRef t) => t != null ? (t.Foreground, t.Background, t.Glyph) : (Color.Transparent, Color.Transparent, 0);
        public HashSet<Point> GetGlobalFill((Color, Color, int)? source, Point origin, Point end) {
            HashSet<Point> affected = new HashSet<Point>();
            foreach (var p in new Rectangle(origin, end).Positions()) {
                var pt = Triple(this[p]);
                if (pt == source) {
                    affected.Add(p);
                }
            }
            return affected;
        }

        public HashSet<Point> GetGrowFill((int, int) start, (Color, Color, int)? brush, Point origin, Point end) {
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            HashSet<Point> affected = new HashSet<Point>();
            Queue<(int, int)> next = new Queue<(int, int)>();

            next.Enqueue(start);
            while (next.Any()) {
                (int x, int y) = next.Dequeue();

                {
                    var p = (x, y);
                    var c = Triple(this[p]);

                    if (new List<(int, int)>() {
                                (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)
                    }.All(p => Triple(this[p]) != brush)) {
                        continue;
                    }
                    affected.Add(p);
                }

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
                    next.Enqueue(p);
                }
            }
            return affected;
        }
        public HashSet<Point> GetOutlineFill((int, int) start, (Color, Color, int)? source, Point origin, Point end) {
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            HashSet<Point> affected = new HashSet<Point>();
            Queue<(int, int)> next = new Queue<(int, int)>();

            next.Enqueue(start);
            while (next.Any()) {
                (int x, int y) = next.Dequeue();

                {
                    var p = (x, y);
                    var c = Triple(this[p]);

                    if (c != source) {
                        continue;
                    }
                    if (new List<(int, int)>() {
                                (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)
                    }.Any(p => Triple(this[p]) != source)) {
                        affected.Add(p);
                    }
                }

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
                    next.Enqueue(p);
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

                {
                    var p = (x, y);
                    var c = Triple(this[p]);

                    if (c != source) {
                        continue;
                    }

                    affected.Add(p);
                }

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
                    next.Enqueue(p);
                }
            }
            return affected;
        }

        public HashSet<Point> GetBoundedFill((int, int) start, HashSet<(int, int)> bounds, Point origin, Point end) {
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            HashSet<Point> affected = new HashSet<Point>();
            Queue<(int, int)> next = new Queue<(int, int)>();

            next.Enqueue(start);
            while (next.Any()) {
                (int x, int y) = next.Dequeue();

                {
                    var p = (x, y);

                    if (bounds.Contains(p)) {
                        continue;
                    }

                    affected.Add(p);
                }

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

        //To do: Test with both layers moved
        public void Flatten(Layer from) {
            foreach ((var p, var t) in from.cells) {
                this[p + from.pos] = t;
            }
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
        public NotepadTile(int Glyph) => this.Glyph = Glyph;
    }
}
