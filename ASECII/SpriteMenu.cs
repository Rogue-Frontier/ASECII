using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using static SadConsole.Input.Keys;

namespace ASECII {
    class EditorMain : SadConsole.Console {
        SpriteModel model;
        public EditorMain(int width, int height) :base(width, height) {
            UseKeyboard = true;
            UseMouse = true;
            model = new SpriteModel(32, 32);
            var tileModel = new TileModel();
            var paletteModel = new PaletteModel();
            var pickerModel = new PickerModel(16, 16, model);

            pickerModel.UpdateColors();
            pickerModel.UpdateBrushPoints(paletteModel);
            pickerModel.UpdatePalettePoints(paletteModel);

            CellButton tileButton = null;
            tileButton = new CellButton(() => {
                var c = model.brush.cell;
                return !tileModel.tiles.Any(t => t.Background == c.Background && t.Foreground == c.Foreground && t.Glyph == c.Glyph);
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
                return !paletteModel.paletteSet.Contains(model.brush.foreground);
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
                return !paletteModel.paletteSet.Contains(model.brush.background);
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

            var spriteMenu = new SpriteMenu(32, 32, model) {
                Position = new Point(16, 0),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var tileMenu = new TileMenu(16, 8, model, tileModel, () => {
                tileButton.UpdateActive();
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();
                
                tileModel.UpdateIndexes(model);
                paletteModel.UpdateIndexes(model);

                pickerModel.UpdateColors();
                pickerModel.UpdateBrushPoints(paletteModel);
                pickerModel.UpdatePalettePoints(paletteModel);
            }) {
                Position = new Point(0, 0),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var glyphMenu = new GlyphMenu(16, 16, model) {
                Position = new Point(0, 8),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            var paletteMenu = new PaletteMenu(16, 4, model, paletteModel, () => {
                tileButton.UpdateActive();
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();

                tileModel.UpdateIndexes(model);
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
                tileButton.UpdateActive();
                foregroundButton.UpdateActive();
                backgroundButton.UpdateActive();

                tileModel.UpdateIndexes(model);
                paletteModel.UpdateIndexes(model);
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
        public override void Draw(TimeSpan timeElapsed) {
            this.Clear();
            base.Draw(timeElapsed);
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
        public override void Draw(TimeSpan timeElapsed) {
            this.Clear();
            model.ticks++;

            int hx = Width / 2;
            int hy = Height / 2;

            var camera = model.camera - model.pan.offsetPan;

            var black = Color.Black;
            var dark = new Color(25, 25, 25);

            for (int x = -hx; x < hx+1; x++) {
                for(int y = -hy; y < hy+1; y++) {
                    var pos = camera + new Point(x, y);

                    int ax = hx - x;
                    int ay = hy - y;
                    if(model.sprite.InBounds(pos)) {
                        var cg = model.sprite[pos];
                        this.Print(ax, ay, cg);
                    } else {
                        var c = ((ax + ay) % 2 == 0) ? black : dark;
                        /*
                        int r = (int)(Math.Sin(2 * pos.X + pos.Y) * 25);
                        int g = (int)(Math.Sin(1 * pos.X + pos.Y) * 25);
                        int b = (int)(Math.Sin(1 * pos.X + pos.Y) * 25);
                        var c = new Color(r, g, b);
                        */
                        this.Print(ax, ay, " ", Color.Transparent, c);
                    }
                }
            }
            if(model.pan.allowPan) {

            } else {
                switch (model.mode) {
                    case Mode.Edit:
                        if (IsMouseOver && model.ticks % 30 < 15) {
                            this.SetCellAppearance(hx - model.cursor.X + model.camera.X, hy - model.cursor.Y + model.camera.Y, model.brush.cell);
                        }
                        break;
                    case Mode.Keyboard:
                        var c = model.brush.cell;
                        if (model.ticks % 30 < 15) {
                            this.SetCellAppearance(hx - model.keyboard.keyCursor.X + model.camera.X, hy - model.keyboard.keyCursor.Y + model.camera.Y, new ColoredGlyph(c.Foreground, c.Background, '_'));
                            //SetCellAppearance(hx - model.keyboard.margin.X + model.camera.X - 1, hy - model.keyboard.keyCursor.Y + model.camera.Y, new Cell(c.Foreground, c.Background, '>'));
                        }
                        break;
                    case Mode.Select:
                        var r = model.select.rect ?? Rectangle.Empty;
                        if(r != Rectangle.Empty) {
                            var left = hx - model.select.rect.Value.X + model.camera.X;
                            var top = hy - model.select.rect.Value.Y + model.camera.Y;
                            this.SetCellAppearance(left, top, new ColoredGlyph(model.brush.background, model.brush.foreground, BoxInfo.IBMCGA.glyphFromInfo[new BoxGlyph { e = Line.Single, s = Line.Single }]));
                            this.SetCellAppearance(left + r.Width, top, new ColoredGlyph(model.brush.background, model.brush.foreground, BoxInfo.IBMCGA.glyphFromInfo[new BoxGlyph { e = Line.Single, s = Line.Single }]));
                            this.SetCellAppearance(left, top + r.Height, new ColoredGlyph(model.brush.background, model.brush.foreground, BoxInfo.IBMCGA.glyphFromInfo[new BoxGlyph { e = Line.Single, s = Line.Single }]));
                            this.SetCellAppearance(left + r.Width, top + r.Height, new ColoredGlyph(model.brush.background, model.brush.foreground, BoxInfo.IBMCGA.glyphFromInfo[new BoxGlyph { e = Line.Single, s = Line.Single }]));
                        }
                        
                        break;
                }
            }
            
            

            base.Draw(timeElapsed);
        }
        public override bool ProcessKeyboard(Keyboard info) {
            model.ProcessKeyboard(info);
            return base.ProcessKeyboard(info);
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            IsFocused = true;
            model.ProcessMouse(state);
            return base.ProcessMouse(state);
        }
    }
    enum Mode {
        Edit, Select, Keyboard
    }
    class SpriteModel {
        int width, height;

        Stack<SingleEdit> Undo;
        Stack<SingleEdit> Redo;

        public Sprite sprite = new Sprite(16, 16);

        public BrushMode brush;
        public KeyboardMode keyboard;
        public SelectMode select;
        public PanMode pan;

        public int ticks = 0;

        public Point camera = new Point();
        public Point cursor = new Point();       //Position on the image; stored as offset from camera
        public bool keyboardMode;

        public Point prevCell;
        public bool prevLeft;

        public Mode mode;

        public SpriteModel(int width, int height) {
            this.width = width;
            this.height = height;
            brush = new BrushMode(this);
            keyboard = new KeyboardMode(this);
            select = new SelectMode(this);
            pan = new PanMode(this);
            Undo = new Stack<SingleEdit>();
            Redo = new Stack<SingleEdit>();
            mode = Mode.Edit;
        }
        public void ProcessKeyboard(Keyboard info) {

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
                    mode = Mode.Keyboard;
                    keyboard.keyCursor = cursor;
                    keyboard.margin = cursor;
                } else if(info.IsKeyPressed(S)) {
                    mode = Mode.Select;
                }

                if (info.IsKeyDown(LeftControl) && info.IsKeyUp(LeftShift) && info.IsKeyPressed(Z)) {
                    if (Undo.Any()) {
                        var u = Undo.Pop();
                        u.Undo();
                        Redo.Push(u);
                    }
                }
                if (info.IsKeyDown(LeftControl) && info.IsKeyDown(LeftShift) && info.IsKeyPressed(Z)) {
                    if (Redo.Any()) {
                        var u = Redo.Pop();
                        u.Do();
                        Undo.Push(u);
                    }
                }

                if (pan.allowPan) {
                    pan.ProcessKeyboard(info);
                }
            }
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            cursor = new Point(width / 2, height / 2) - state.SurfaceCellPosition + camera;
            if (pan.allowPan) {
                pan.ProcessMouse(state);
            } else {
                switch (mode) {
                    case Mode.Edit:
                        brush.ProcessMouse(state);
                        break;
                    case Mode.Select:
                        select.ProcessMouse(state);
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
            Undo.Push(edit);
            Redo.Clear();
            edit.Do();
        }
    }
    interface Edit {
        void Undo();
        void Do();
    }
    class SingleEdit {
        public Point cursor;
        public Layer layer;
        public ColoredGlyph prev;
        public ColoredGlyph next;
        public SingleEdit(Point cursor, Layer layer, ColoredGlyph next) {
            this.cursor = cursor;
            this.layer = layer;
            this.prev = layer[cursor];
            this.next = next;
        }
        public void Undo() {
            layer[cursor] = prev;
        }
        public void Do() {
            layer[cursor] = next;
        }
        public bool IsRedundant() => prev.Background == next.Background && prev.Foreground == next.Foreground && prev.Glyph == next.Glyph;
    }
    class PanMode {
        SpriteModel model;
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
    class BrushMode {
        SpriteModel model;
        public char glyph = 'A';
        public Color foreground = Color.Red;
        public Color background = Color.Black;
        public ColoredGlyph cell {
            get => new ColoredGlyph(foreground, background, glyph); set {
                foreground = value.Foreground;
                background = value.Background;
                glyph = value.GlyphCharacter;
            }
        }
        public BrushMode(SpriteModel model) {
            this.model = model;
        }

        public void ProcessMouse(MouseScreenObjectState state) {
            glyph = (char)((glyph + state.Mouse.ScrollWheelValueChange / 120 + 255) % 255);
            if (state.Mouse.LeftButtonDown) {
                var prev = model.prevCell;
                var offset = (model.cursor - prev);
                var length = offset.Length();
                for (int i = 0; i < length; i++) {
                    
                    var p = prev + new Point((int)(i * offset.X / length), (int)(i * offset.Y / length));
                    var sprite = model.sprite;
                    if (sprite.InBounds(p)) {
                        var layer = sprite.layers[0];
                        var action = new SingleEdit(p, layer, model.brush.cell);
                        model.AddAction(action);
                    }
                }

                if (model.sprite.InBounds(model.cursor)) {
                    var layer = model.sprite.layers[0];
                    var action = new SingleEdit(model.cursor, layer, model.brush.cell);
                    model.AddAction(action);
                }
            }
        }
    }
    class KeyboardMode {
        SpriteModel model;
        public Point keyCursor;
        public Point margin;

        public KeyboardMode(SpriteModel model) {
            this.model = model;
            keyCursor = new Point();
        }
        public void ProcessKeyboard(Keyboard info) {
            if (info.IsKeyPressed(Right)) {
                keyCursor += new Point(-1, 0);
            }
            if (info.IsKeyPressed(Left)) {
                keyCursor += new Point(1, 0);
            }
            if (info.IsKeyPressed(Up)) {
                keyCursor += new Point(0, 1);
            }
            if (info.IsKeyPressed(Down)) {
                keyCursor += new Point(0, -1);
            }
            if(info.IsKeyPressed(Enter)) {
                keyCursor = new Point(margin.X, keyCursor.Y - 1);
            }

            ref var sprite = ref model.sprite;
            ref var brush = ref model.brush;
            if (info.KeysPressed.Count == 1 && (info.KeysDown.Count == 1 || (info.KeysDown.Count == 2 && info.IsKeyDown(LeftShift)))) {
                char c = info.KeysPressed[0].Character;
                if (char.IsLetterOrDigit(c) && sprite.InBounds(keyCursor)) {
                    var layer = sprite.layers[0];
                    var cg = new ColoredGlyph(brush.foreground, brush.background, c);
                    var action = new SingleEdit(keyCursor, layer, cg);
                    model.AddAction(action);

                    keyCursor += new Point(-1, 0);
                }
            }
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            if(state.Mouse.LeftButtonDown) {
                margin = keyCursor = model.cursor;
            }
        }
    }
    class SelectMode {
        SpriteModel model;
        public Point? start;
        Point end;
        public Rectangle? rect;
        bool prevLeft;
        public SelectMode(SpriteModel model) {
            this.model = model;
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            if(state.Mouse.LeftButtonDown) {
                if (start != null) {
                    end = model.cursor;

                    int leftX = Math.Max(start.Value.X, end.X);
                    int width = leftX - Math.Min(start.Value.X, end.X);
                    int topY = Math.Max(start.Value.Y, end.Y);
                    int height = topY - Math.Min(start.Value.Y, end.Y);
                    rect = new Rectangle(leftX, topY, width, height);
                } else if(!prevLeft) {
                    start = model.cursor;
                    rect = new Rectangle(start.Value, new Point(0, 0));
                }
            } 
            prevLeft = state.Mouse.LeftButtonDown;
        }

    }
}
