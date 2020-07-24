using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.SerializedTypes;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static SadConsole.Input.Keys;

namespace ASECII {
    class EditorMain : SadConsole.Console {
        SpriteModel model;
        public EditorMain(int width, int height, SpriteModel model) :base(width, height) {
            UseKeyboard = true;
            UseMouse = true;
            DefaultBackground = Color.Black;

            this.model = model;
            var tileModel = model.tiles;
            var paletteModel = model.palette;
            var pickerModel = new PickerModel(16, 16, model);

            pickerModel.UpdateColors();
            pickerModel.UpdateBrushPoints(paletteModel);
            pickerModel.UpdatePalettePoints(paletteModel);

            CellButton tileButton = null;
            tileButton = new CellButton(() => {
                var c = model.brush.cell;
                return tileModel.brushIndex == null;
            }, () => {
                tileModel.AddTile(model.brush.cell);
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
            }) {
                Position = new Point(15, 3),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            CellButton foregroundButton = null, backgroundButton = null;
            foregroundButton = new CellButton(() => {
                return paletteModel.foregroundIndex == null;
            }, () => {
                paletteModel.AddColor(model.brush.foreground);
                paletteModel.UpdateIndexes(model);

                PaletteChanged();
            }) {
                Position = new Point(15, 27),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            backgroundButton = new CellButton(() => {
                return paletteModel.backgroundIndex == null;
            }, () => {
                paletteModel.AddColor(model.brush.background);
                paletteModel.UpdateIndexes(model);
                PaletteChanged();
            }) {
                Position = new Point(14, 27),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            void PaletteChanged() {
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();

                pickerModel.UpdateColors();
                pickerModel.UpdateBrushPoints(paletteModel);
                pickerModel.UpdatePalettePoints(paletteModel);
            }

            var spriteMenu = new SpriteMenu(Width - 16, Height, model) {
                Position = new Point(16, 0),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var tileMenu = new TileMenu(16, 8, model, tileModel, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
                
                paletteModel.UpdateIndexes(model);
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();

                pickerModel.UpdateColors();
                pickerModel.UpdateBrushPoints(paletteModel);
                pickerModel.UpdatePalettePoints(paletteModel);
            }) {
                Position = new Point(0, 0),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var glyphMenu = new GlyphMenu(16, 16, model, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
            }) {
                Position = new Point(0, 8),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var paletteMenu = new PaletteMenu(16, 4, model, paletteModel, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();

                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();

                pickerModel.UpdateBrushPoints(paletteModel);

                pickerModel.UpdateColors();
                pickerModel.UpdateBrushPoints(paletteModel);
                pickerModel.UpdatePalettePoints(paletteModel);
            }) {
                Position = new Point(0, 24),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            var pickerMenu = new PickerMenu(16, 16, model, pickerModel, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();

                paletteModel.UpdateIndexes(model);
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();
            }) {
                Position = new Point(0, 28),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var colorBar = new ColorBar(16, 1, paletteModel, pickerModel) {
                Position = new Point(0, 44),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            tileModel.UpdateIndexes(model);
            tileButton.UpdateActive();

            paletteModel.UpdateIndexes(model);
            foregroundButton.UpdateActive();
            backgroundButton.UpdateActive();

            this.Children.Add(tileMenu);
            this.Children.Add(tileButton);
            this.Children.Add(spriteMenu);
            this.Children.Add(glyphMenu);
            this.Children.Add(paletteMenu);
            this.Children.Add(foregroundButton);
            this.Children.Add(backgroundButton);
            this.Children.Add(pickerMenu);
            this.Children.Add(colorBar);
        }
        public override bool ProcessKeyboard(Keyboard info) {
            return base.ProcessKeyboard(info);
        }
        public override void Render(TimeSpan timeElapsed) {
            this.Clear();
            base.Render(timeElapsed);
        }
    }

    class SpriteMenu : SadConsole.Console {
        SpriteModel model;
        public SpriteMenu(int width, int height, SpriteModel model) : base(width, height) {
            UseMouse = true;
            UseKeyboard = true;
            this.model = model;
        }
        public override void Update(TimeSpan timeElapsed) {
            base.Update(timeElapsed);
        }
        public override void Render(TimeSpan timeElapsed) {
            this.Clear();
            model.ticks++;
            model.ticksSelect++;

            int hx = Width / 2;
            int hy = Height / 2;

            var camera = model.camera - model.pan.offsetPan;

            var c1 = new Color(25, 25, 25);
            var c2 = new Color(51, 51, 51);

            model.sprite.UpdatePreview();
            var center = new Point(hx, hy);

            if(model.infinite) {
                for (int x = -hx; x < hx + 1; x++) {
                    for (int y = -hy; y < hy + 1; y++) {
                        var pos = camera + new Point(x, y) + center;

                        int ax = center.X + x;
                        int ay = center.Y + y;
                        if (model.sprite.preview.TryGetValue(pos, out var tile)) {
                            this.SetCellAppearance(ax, ay, tile.cg);
                        } else {
                            var c = ((ax + ay) % 2 == 0) ? c1 : c2;
                            this.SetCellAppearance(ax, ay, new ColoredGlyph(Color.Transparent, c));
                        }
                    }
                }
            } else {
                for (int x = -hx; x < hx + 1; x++) {
                    for (int y = -hy; y < hy + 1; y++) {
                        var pos = camera + new Point(x, y) + center;

                        int ax = center.X + x;
                        int ay = center.Y + y;
                        if (model.InBounds(pos)) {
                            if (model.sprite.preview.TryGetValue(pos, out var tile)) {
                                this.SetCellAppearance(ax, ay, tile.cg);
                            } else {
                                var c = ((ax + ay) % 2 == 0) ? c1 : c2;
                                this.SetCellAppearance(ax, ay, new ColoredGlyph(Color.Transparent, c, ' '));
                            }
                        } else {
                            this.SetCellAppearance(ax, ay, new ColoredGlyph(Color.Transparent, Color.Black, ' '));
                        }
                    }
                }
                var origin = new Point(0, 0);
                var p = origin - camera;

                this.SetCellAppearance(p.X, p.Y, new ColoredGlyph(Color.White, Color.Black, '+'));
            }
            
            if(model.pan.allowPan) {

            } else {
                switch (model.mode) {
                    case Mode.Edit:
                        if (model.ticksSelect % 30 < 15) {
                            /*
                            if (model.selectRect.GetAdjustedRect(out Rectangle r)) {
                                DrawRect(r);
                            }
                            */
                            DrawSelection();
                        }
                        if ((model.ticks % 30) < 15) {
                            if (IsMouseOver) {
                                this.SetCellAppearance(model.cursorScreen.X, model.cursorScreen.Y, model.brush.cell);
                            }
                        }
                        

                        break;
                    case Mode.Keyboard:
                        var c = model.brush.cell;
                        if(model.ticksSelect%30 < 15) {
                            /*
                            if (model.selectRect.GetAdjustedRect(out Rectangle r)) {
                                DrawRect(r);
                            }
                            */
                            DrawSelection();
                        }
                        if (model.ticks % 30 < 15) {
                            var p = (model.keyboard.keyCursor ?? model.cursor) - camera;
                            this.SetCellAppearance(p.X, p.Y, new ColoredGlyph(c.Foreground, c.Background, '_'));
                            //SetCellAppearance(hx - model.keyboard.margin.X + model.camera.X - 1, hy - model.keyboard.keyCursor.Y + model.camera.Y, new Cell(c.Foreground, c.Background, '>'));
                        }
                        break;
                    case Mode.SelectRect:
                        if (model.ticksSelect % 20 < 10) {
                            DrawSelection();

                            if (model.selectRect.GetAdjustedRect(out Rectangle r)) {
                                DrawRect(r);
                            } else {
                                var p = model.cursorScreen;
                                DrawBox(p.X, p.Y, new BoxGlyph { n = Line.Single, e = Line.Single, s = Line.Single, w = Line.Single });
                            }
                        }
                        
                        break;
                    case Mode.Move:
                        DrawSelection();
                        //Draw offset


                        //TO DO: Fix
                        if (model.ticksSelect % 20 < 10 && model.move.current.HasValue) {
                            var offset = model.move.end - model.move.start.Value;

                            int x = model.move.start.Value.X;
                            int y = model.move.start.Value.Y;
                            bool first = true;
                            if(Math.Abs(offset.X) > Math.Abs(offset.Y)) {
                                DrawHorizontal();
                                DrawVertical();
                            } else if (Math.Abs(offset.X) < Math.Abs(offset.Y)) {
                                DrawVertical();
                                DrawHorizontal();
                            } else {
                                char glyph = Math.Sign(offset.X) == Math.Sign(offset.Y) ? '\\' : '/';
                                for (int i = 0; i < Math.Abs(offset.X) + 1; i++) {
                                    /*
                                    DrawBox(x, y, new BoxGlyph {
                                        n = Line.Single,
                                        e = Line.Single,
                                        s = Line.Single,
                                        w = Line.Single,
                                    });
                                    */
                                    this.SetCellAppearance(x, y, new ColoredGlyph(model.brush.foreground, model.brush.background, glyph));
                                    x += Math.Sign(offset.X);
                                    y += Math.Sign(offset.Y);
                                }
                            }

                            void DrawHorizontal() {
                                if(offset.X == 0) {
                                    return;
                                }
                                //Start
                                if(first) {
                                    DrawBox(x, y, new BoxGlyph {
                                        e = offset.X > 0 ? Line.Double : Line.None,
                                        w = offset.X < 0 ? Line.Double : Line.None,
                                        n = Line.Single,
                                        s = Line.Single
                                    });
                                }
                                //Body
                                for (int i = 1; i < Math.Abs(offset.X) + 1; i++) {
                                    x += Math.Sign(offset.X);
                                    DrawBox(x, y, new BoxGlyph {
                                        e = Line.Double,
                                        w = Line.Double
                                    });
                                }
                                //End
                                if(first) {
                                    DrawBox(x, y, new BoxGlyph {
                                        e = offset.X < 0 ? Line.Double : offset.X == 0 ? Line.Single : Line.None,
                                        w = offset.X > 0 ? Line.Double : offset.X == 0 ? Line.Single : Line.None,
                                        n = offset.Y < 0 ? Line.Double : offset.Y == 0 ? Line.Single : Line.None,
                                        s = offset.Y > 0 ? Line.Double : offset.Y == 0 ? Line.Single : Line.None
                                    });
                                } else {
                                    DrawBox(x, y, new BoxGlyph {
                                        e = offset.X < 0 ? Line.Double : Line.None,
                                        w = offset.X > 0 ? Line.Double : Line.None,
                                        n = Line.Single,
                                        s = Line.Single
                                    });
                                }
                                
                                first = false;
                            }
                            void DrawVertical() {
                                if (offset.Y == 0) {
                                    return;
                                }
                                //Start
                                if (first) {
                                    DrawBox(x, y, new BoxGlyph {
                                        n = offset.Y < 0 ? Line.Double : Line.None,
                                        s = offset.Y > 0 ? Line.Double : Line.None,
                                        e = Line.Single,
                                        w = Line.Single
                                    });
                                }
                                //Body
                                for (int i = 1; i < Math.Abs(offset.Y) + 1; i++) {
                                    y += Math.Sign(offset.Y);
                                    DrawBox(x, y, new BoxGlyph {
                                        n = Line.Double,
                                        s = Line.Double
                                    });
                                }
                                //End
                                if(first) {
                                    DrawBox(x, y, new BoxGlyph {
                                        e = offset.X > 0 ? Line.Double : offset.X == 0 ? Line.Single : Line.None,
                                        w = offset.X < 0 ? Line.Double : offset.X == 0 ? Line.Single : Line.None,
                                        n = offset.Y > 0 ? Line.Double : offset.Y == 0 ? Line.Single : Line.None,
                                        s = offset.Y < 0 ? Line.Double : offset.Y == 0 ? Line.Single : Line.None
                                    });
                                } else {
                                    DrawBox(x, y, new BoxGlyph {
                                        n = offset.Y > 0 ? Line.Double : Line.None,
                                        s = offset.Y < 0 ? Line.Double : Line.None,
                                        e = Line.Single,
                                        w = Line.Single
                                    });
                                }
                                
                                first = false;
                            }
                        }
                        break;
                }
            }
            
            

            base.Render(timeElapsed);
            void DrawSelection() {
                var all = model.selection.GetAll();
                foreach(var point in all) {
                    
                    bool n = Contains(new Point(0, -1)), e = Contains(new Point(1, 0)), s = Contains(new Point(0, +1)), w = Contains(new Point(-1, 0)), ne = Contains(new Point(1, -1)), se = Contains(new Point(1, 1)), sw = Contains(new Point(-1, 1)), nw = Contains(new Point(-1, -1));
                    bool Contains(Point offset) => all.Contains(point + offset);
                    var p = point - camera;
                    if (!n && !e && !s && !w) {
                        //Isolated cell
                        this.AddDecorator(p.X, p.Y, 1, new CellDecorator(model.brush.foreground, '+', Mirror.None));
                        continue;
                    } else if (n && e && s && w && ne && nw && se && sw) {
                        //Surrounded cell
                        DrawBox(p.X, p.Y, new BoxGlyph { n = Line.Single, e = Line.Single, s = Line.Single, w = Line.Single });
                    } else {
                        BoxGlyph g = new BoxGlyph {
                            n = n && (!e && !w) ? Line.Double :                     //Thin Line Body
                                n && (!ne && !nw) ? Line.Double :                   //Intersection
                                n && (!e || !w || !ne || !nw) ? Line.Single :       //Edge Line Body
                                !n && !s && (w != e) ? Line.Single :                //Thin line End
                                Line.None,
                            e = e && (!n && !s) ? Line.Double :                     //Thin Line Body
                                e && (!ne && !se) ? Line.Double :                   //Intersection
                                e && (!n || !s || !se || !ne) ? Line.Single :       //Edge Line Body
                                !e && !w && (n != s) ? Line.Single :                //Thin line End
                                Line.None,
                            s = s && (!e && !w) ? Line.Double :                     //Thin Line Body
                                s && (!se && !sw) ? Line.Double :                   //Intersection
                                s && (!e || !w || !se || !sw) ? Line.Single :       //Edge Line Body
                                !s && !n && (w != e) ? Line.Single :                //Thin line End
                                Line.None,
                            w = w && (!n && !s) ? Line.Double :                     //Thin Line Body
                                w && (!nw && !sw) ? Line.Double :                   //Intersection
                                w && (!n || !s || !nw || !sw) ? Line.Single :       //Edge Line Body
                                !w && !e && (n != s) ? Line.Single :                //Thin line End
                                Line.None,
                        };
                        if(!BoxInfo.IBMCGA.glyphFromInfo.ContainsKey(g)) {
                            if(g.n == Line.Single && g.e == Line.Single) {
                                g = new BoxGlyph {
                                n = g.n,
                                e = g.e
                                };
                            } else if (g.e == Line.Single && g.s == Line.Single) {
                                g = new BoxGlyph {
                                    e = g.e,
                                    s = g.s
                                };
                            } else if (g.s == Line.Single && g.w == Line.Single) {
                                g = new BoxGlyph {
                                    s = g.s,
                                    w = g.w
                                };
                            } else if (g.w == Line.Single && g.n == Line.Single) {
                                g = new BoxGlyph {
                                    w = g.w,
                                    n = g.n
                                };
                            }
                        }
                        DrawBox(p.X, p.Y, g);
                    }
                }
            }
            void DrawRect(Rectangle r) {
                var (left, top) = (r.X, r.Y);


                if (r.Width > 1 && r.Height > 1) {
                    DrawSidesX(Line.Single);
                    DrawSidesY(Line.Single);
                    DrawCornerNW();
                    DrawCornerNE();
                    DrawCornerSW();
                    DrawCornerSE();
                } else if (r.Width > 1) {
                    DrawSidesX(Line.Double);
                    DrawBox(left, top, new BoxGlyph { n = Line.Single, e = Line.Double, s = Line.Single });
                    DrawBox(left + r.Width - 1, top, new BoxGlyph { n = Line.Single, w = Line.Double, s = Line.Single });
                } else if (r.Height > 1) {
                    DrawSidesY(Line.Double);
                    DrawBox(left, top, new BoxGlyph { s = Line.Double, e = Line.Single, w = Line.Single });
                    DrawBox(left, top + r.Height - 1, new BoxGlyph { n = Line.Double, w = Line.Single, e = Line.Single });
                } else {
                    DrawBox(left, top, new BoxGlyph { n = Line.Single, e = Line.Single, s = Line.Single, w = Line.Single });
                }

                void DrawCornerNW() {
                    //Left top
                    DrawBox(left, top, new BoxGlyph { e = Line.Single, s = Line.Single });
                }
                void DrawCornerNE() {
                    //Right top
                    DrawBox(left + r.Width - 1, top, new BoxGlyph { w = Line.Single, s = Line.Single });
                }
                void DrawCornerSW() {
                    //Left bottom
                    DrawBox(left, top + r.Height - 1, new BoxGlyph { e = Line.Single, n = Line.Single });
                }
                void DrawCornerSE() {
                    //Right bottom
                    DrawBox(left + r.Width - 1, top + r.Height - 1, new BoxGlyph { n = Line.Single, w = Line.Single });
                }
                void DrawSidesX(Line style = Line.Single) {
                    foreach (var x in Enumerable.Range(left, r.Width)) {
                        DrawBox(x, top, new BoxGlyph { e = style, w = style });
                        DrawBox(x, top + r.Height - 1, new BoxGlyph { e = style, w = style });
                    }
                }
                void DrawSidesY(Line style = Line.Single) {
                    foreach (var y in Enumerable.Range(top, r.Height)) {
                        DrawBox(left, y, new BoxGlyph { n = style, s = style });
                        DrawBox(left + r.Width - 1, y, new BoxGlyph { n = style, s = style });
                    }
                }
            }
            void DrawBox(int x, int y, BoxGlyph g) {
                this.AddDecorator(x, y, 1, new CellDecorator(model.brush.foreground, BoxInfo.IBMCGA.glyphFromInfo[g], Mirror.None));
            }
        }
        public override bool ProcessKeyboard(Keyboard info) {
            if (info.IsKeyPressed(Escape)) {
                if(model.mode == Mode.Keyboard) {
                    model.ProcessKeyboard(info);
                } else {
                    Parent.Parent.Children.Remove(Parent);
                }
            } else if (info.IsKeyPressed(S) && info.IsKeyDown(LeftControl)) {
                //File.WriteAllText(Path.Combine(Environment.CurrentDirectory, Path.GetFileName(Path.GetTempFileName())), JsonConvert.SerializeObject(model));
                var s = SadConsole.Game.Instance.Screen;
                s.Children.Add(new FileMenu(s.Width, s.Height, new SaveMode(model)));
            } else {
                model.ProcessKeyboard(info);
            }
            return base.ProcessKeyboard(info);
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            IsFocused = true;
            model.ProcessMouse(state, IsMouseOver);
            return base.ProcessMouse(state);
        }
    }
    public enum Mode {
        Edit, SelectRect, SelectCircle, SelectLasso, SelectPoly, SelectWand, Move, Keyboard
    }
    [JsonObject(MemberSerialization.Fields)]
    public class SpriteModel {
        public bool infinite = true;
        public int width, height;

        public LinkedList<SingleEdit> Undo;
        public LinkedList<SingleEdit> Redo;

        public string filepath;
        public Sprite sprite;

        public TileModel tiles;
        public PaletteModel palette;
        public BrushMode brush;
        public KeyboardMode keyboard;
        public MoveMode move;
        public Selection selection;
        public SelectRectMode selectRect;
        public PanMode pan;

        public int ticks = 0;
        public int ticksSelect = 0;

        public Point camera = new Point();
        public Point cursor = new Point();       //Position on the image; stored as offset from camera
        public Point cursorScreen = new Point();
        public bool keyboardMode;

        public Point prevCell;
        public bool prevLeft;

        public Mode mode;

        public int currentLayer = 0;

        public SpriteModel(int width, int height) {
            this.width = width;
            this.height = height;
            sprite = new Sprite();
            tiles = new TileModel();
            palette = new PaletteModel();
            brush = new BrushMode(this);
            keyboard = new KeyboardMode(this);
            selection = new Selection();
            selectRect = new SelectRectMode(this, selection);
            pan = new PanMode(this);
            Undo = new LinkedList<SingleEdit>();
            Redo = new LinkedList<SingleEdit>();
            mode = Mode.Edit;
        }
        public bool InBounds(Point p) => (p.X > -1 && p.X < width && p.Y > -1 && p.Y < height);
        public bool IsEditable(Point p) {
            bool result = infinite || InBounds(p);
            
            /*
            if(selectRect.rect.HasValue) {
                var r = selectRect.rect.Value;
                result = result && p.X >= r.X && p.X <= r.X + r.Width && p.Y >= r.Y && p.Y <= r.Y + r.Height;
            }
            */
            var selected = selection.GetAll();
            if (selected.Any()) {
                result = result && selected.Contains(p);
            }
            return result;
        }
        public void ProcessKeyboard(Keyboard info) {
            if (info.IsKeyDown(LeftControl) && info.IsKeyUp(LeftShift) && info.IsKeyPressed(Z)) {
                if (Undo.Any()) {
                    var u = Undo.Last();
                    Undo.RemoveLast();
                    u.Undo();
                    Redo.AddLast(u);
                }
                return;
            }
            if (info.IsKeyDown(LeftControl) && info.IsKeyDown(LeftShift) && info.IsKeyPressed(Z)) {
                if (Redo.Any()) {
                    var u = Redo.Last();
                    Redo.RemoveLast();
                    u.Do();
                    Undo.AddLast(u);
                }
                return;
            }
            if (mode == Mode.Keyboard) {
                if(info.IsKeyPressed(Escape)) {
                    mode = Mode.Edit;
                } else {
                    keyboard.ProcessKeyboard(info);
                }
            } else {
                if (info.IsKeyPressed(Space)) {
                    if(!pan.allowPan) {
                        pan.allowPan = true;
                        pan.startPan = cursor;
                    }
                } else if (info.IsKeyReleased(Space)) {
                    pan.allowPan = false;
                    camera -= pan.offsetPan;
                    pan.startPan = new Point(0, 0);
                    pan.offsetPan = new Point(0, 0);
                } else if(info.IsKeyPressed(T)) {
                    ExitMode();
                    mode = Mode.Keyboard;
                    keyboard.keyCursor = null;
                    //keyboard.keyCursor = cursor;
                    //keyboard.margin = cursor;
                } else if(info.IsKeyPressed(S) && !info.IsKeyDown(LeftControl)) {
                    ExitMode();
                    mode = Mode.SelectRect;
                } else if(info.IsKeyPressed(B)) {
                    ExitMode();
                    mode = Mode.Edit;
                } else if(info.IsKeyPressed(M)) {
                    ExitMode();
                    var moveLayer = Cut(selection.GetAll());
                    if(currentLayer == sprite.layers.Count - 1) {
                        sprite.layers.Add(moveLayer);
                        currentLayer++;
                    } else {
                        currentLayer++;
                        sprite.layers.Insert(currentLayer, moveLayer);
                    }
                    move = new MoveMode(this, selection, currentLayer, moveLayer);
                    mode = Mode.Move;
                }

                if (pan.allowPan) {
                    pan.ProcessKeyboard(info);
                }
            }
            void ExitMode() {
                if(mode == Mode.Move) {
                    if(currentLayer == 0) {
                        //If for some reason we're the first layer, then we have nothing to flatten onto
                        return;
                    } else {
                        //Flatten the layer and decrement currentLayer
                        var prevLayer = sprite.layers[currentLayer - 1];

                        foreach (var (point, cell) in move.layer.cells) {
                            prevLayer[point + move.layer.pos] = cell;
                        }
                        sprite.layers.RemoveAt(currentLayer);
                        currentLayer--;
                    }
                }
            }
        }
        public Layer Cut(HashSet<Point> points) {
            Layer result = new Layer();
            foreach(var point in points) {
                result[point] = sprite.layers[currentLayer][point];
                sprite.layers[currentLayer][point] = null;
            }
            return result;
        }
        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver) {
            cursorScreen = state.SurfaceCellPosition;
            cursor = cursorScreen + camera;
            if (pan.allowPan) {
                pan.ProcessMouse(state);
            } else {
                switch (mode) {
                    case Mode.Edit:
                        brush.ProcessMouse(state, IsMouseOver);
                        break;
                    case Mode.SelectRect:
                        selectRect.ProcessMouse(state);
                        break;
                    case Mode.Move:
                        move.ProcessMouse(state);
                        break;
                    case Mode.Keyboard:
                        keyboard.ProcessMouse(state);
                        break;
                }
            }


            prevCell = cursor;
            prevLeft = state.Mouse.LeftButtonDown;
        }
        public void AddAction(SingleEdit edit) {
            if(edit.IsRedundant()) {
                return;
            }
            Undo.AddLast(edit);
            Redo.Clear();
            edit.Do();
        }
    }
    public interface Edit {
        void Undo();
        void Do();
    }
    public class SingleEdit {
        public Point cursor;
        public Layer layer;
        public TileRef prev;
        public TileRef next;
        public SingleEdit(Point cursor, Layer layer, TileRef next) {
            this.cursor = cursor;
            this.layer = layer;
            this.prev = layer[cursor];
            if(next != null && next.Foreground.A == 0 && next.Background.A == 0) {
                next = null;
            }
            this.next = next;
        }
        public void Undo() {
            layer[cursor] = prev;
        }
        public void Do() {
            layer[cursor] = next;

        }
        public bool IsRedundant() => prev == null ? next == null : next != null && (prev.Background == next.Background && prev.Foreground == next.Foreground && prev.Glyph == next.Glyph);
    }
    public class PanMode {
        public SpriteModel model;
        public bool allowPan;
        public Point startPan;     //Position on screen where user held down space and left clicked
        public Point offsetPan;

        public PanMode(SpriteModel model) {
            this.model = model;
        }
        public void ProcessKeyboard(Keyboard info) {
            if (info.IsKeyPressed(Right)) {
                startPan += new Point(-1, 0);
            }
            if (info.IsKeyPressed(Left)) {
                startPan += new Point(1, 0);
            }
            if (info.IsKeyPressed(Up)) {
                startPan += new Point(0, 1);
            }
            if (info.IsKeyPressed(Down)) {
                startPan += new Point(0, -1);
            }
            UpdateOffset();
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            UpdateOffset();
        }
        public void UpdateOffset() {
            offsetPan = model.cursor - (Point)startPan;
        }
    }
    [JsonObject(MemberSerialization.Fields)]
    public class BrushMode {
        public SpriteModel model;
        public MouseWatch mouse;
        public int glyph = 'A';
        public Color foreground = Color.Red;
        public Color background = Color.Black;
        public TileValue cell {
            get => new TileValue(foreground, background, glyph); set {
                foreground = value.Foreground;
                background = value.Background;
                glyph = value.Glyph;
            }
        }
        public BrushMode(SpriteModel model) {
            this.model = model;
            mouse = new MouseWatch();
        }

        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver) {
            mouse.Update(state, IsMouseOver);
            glyph = (char)((glyph + state.Mouse.ScrollWheelValueChange / 120 + 255) % 255);
            if(state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown && mouse.leftPressedOnScreen) {
                    var prev = model.prevCell;
                    var offset = (model.cursor - prev);
                    var length = offset.Length();
                    for (int i = 0; i < length; i++) {

                        var p = prev + new Point((int)(i * offset.X / length), (int)(i * offset.Y / length));
                        if (model.IsEditable(p)) {
                            Place(p);
                        }
                    }

                    if (model.IsEditable(model.cursor)) {
                        Place(model.cursor);
                    }
                }
            }
        }
        void Place(Point p) {
            var layer = model.sprite.layers[model.currentLayer];
            SingleEdit action;
            if(model.tiles.brushIndex.HasValue) {
                action = new SingleEdit(p, layer, model.tiles.brushTile);
            } else {
                action = new SingleEdit(p, layer, model.brush.cell);
            }
            
            model.AddAction(action);
        }
    }
    public class KeyboardMode {
        public SpriteModel model;
        public Point? keyCursor;
        public Point margin;

        public KeyboardMode(SpriteModel model) {
            this.model = model;
            keyCursor = new Point();
        }
        public void ProcessKeyboard(Keyboard info) {
            ref int ticks = ref model.ticks;
            if (info.IsKeyPressed(Right)) {
                keyCursor = (keyCursor ?? model.cursor) + new Point(1, 0);
                ticks = 0;
            }
            if (info.IsKeyPressed(Left)) {
                keyCursor = (keyCursor ?? model.cursor) + new Point(-1, 0);
                ticks = 0;
            }
            if (info.IsKeyPressed(Up)) {
                keyCursor = (keyCursor ?? model.cursor) + new Point(0, -1);
                ticks = 0;
            }
            if (info.IsKeyPressed(Down)) {
                keyCursor = (keyCursor ?? model.cursor) + new Point(0, 1);
                ticks = 0;
            }
            if(info.IsKeyPressed(Enter) && keyCursor.HasValue) {
                keyCursor = new Point(margin.X, keyCursor.Value.Y + 1);
                ticks = 0;
            }

            ref var sprite = ref model.sprite;
            ref var brush = ref model.brush;
            var pressed = info.KeysPressed.Where(k => k.Character != 0);
            if (pressed.Any()) {
                char c = pressed.First().Character;
                var p = keyCursor ?? model.cursor;
                if(model.IsEditable(p)) {
                    if (c != 0) {
                        var layer = sprite.layers[0];
                        var tile = new TileValue(brush.foreground, brush.background, c);
                        var action = new SingleEdit(p, layer, tile);
                        model.AddAction(action);
                        ticks = 15;
                        //keyCursor += new Point(1, 0);
                    }
                }

            } else if (info.IsKeyPressed(Back)) {
                var layer = sprite.layers[0];
                var p = keyCursor ?? model.cursor;
                var action = new SingleEdit(p, layer, null);
                model.AddAction(action);
                ticks = 15;

            }
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            if(state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown) {
                    keyCursor = margin = model.cursor;
                }
            }
        }
    }
    public class MoveMode {
        public SpriteModel model;
        public Selection selection;
        public int layerIndex;
        public Layer layer;

        public Point? start;
        public Point? current;
        public Point end;
        //public Point moved => start.HasValue ? end - start.Value : new Point(0, 0);
        public MouseWatch mouse;
        public MoveMode(SpriteModel model, Selection selection, int layerIndex, Layer layer) {
            this.model = model;
            this.selection = selection;
            this.layerIndex = layerIndex;
            this.layer = layer;
            mouse = new MouseWatch();
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            mouse.Update(state, state.IsOnScreenObject);
            if (mouse.left == ClickState.Pressed) {
                if(mouse.leftPressedOnScreen) {
                    //Start moving with the mouse
                    current = model.cursor;
                    start = current;
                }
            } else if(mouse.prevLeft) {
                //Update the layer's apparent position
                end = model.cursor;
                if(end == current) {
                    return;
                }
                var offset = end - current.Value;
                layer.pos += offset;
                selection.Offset(offset);
                current = model.cursor;
            }
        }
    }
    public class Selection {
        public HashSet<Rectangle> rects;
        public HashSet<Point> points;
        public Selection() {
            rects = new HashSet<Rectangle>();
            points = new HashSet<Point>();
        }
        public HashSet<Point> GetAll() {
            HashSet<Point> result = new HashSet<Point>();
            foreach(var rect in rects) {
                result.UnionWith(rect.Positions());
            }
            result.UnionWith(points);
            return result;
        }
        public void Offset(Point offset) {
            rects = new HashSet<Rectangle>(rects.Select(r => new Rectangle(r.X + offset.X, r.Y + offset.Y, r.Width, r.Height)));
            points = new HashSet<Point>(points.Select(p => p + offset));
        }

    }
    public class SelectRectMode {
        public SpriteModel model;
        public Selection selection;
        public Point? start;
        public Point end;
        public Rectangle? rect;
        bool prevLeft;

        public SelectRectMode(SpriteModel model, Selection selection) {
            this.model = model;
            this.selection = selection;
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            if(state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown) {

                    if (prevLeft) {
                        if (end != model.cursor) {
                            end = model.cursor;

                            int leftX = Math.Min(start.Value.X, end.X);
                            int width = Math.Max(start.Value.X, end.X) - leftX + 1;
                            int topY = Math.Min(start.Value.Y, end.Y);
                            int height = Math.Max(start.Value.Y, end.Y) - topY + 1;
                            rect = new Rectangle(leftX, topY, width, height);

                            //var t = model.ticks % 30;
                            //model.ticksSelect = t < 15 ? t - 15 : t - 30;
                            model.ticksSelect = -15;
                        }
                    } else {
                        start = model.cursor;
                        rect = new Rectangle(start.Value, new Point(0, 0));
                    }
                } else if (prevLeft) {
                    if (start != end) {
                        //selection.rects.Clear();
                        selection.rects.Add(rect.Value);
                    }
                    rect = null;
                }
                prevLeft = state.Mouse.LeftButtonDown;
            }
            
        }
        public bool GetAdjustedRect(out Rectangle r) {
            if(rect.HasValue) {
                r = rect.Value.Translate(new Point() - model.camera);
            } else {
                r = Rectangle.Empty;
            }
            return rect.HasValue;
        }
    }
}
