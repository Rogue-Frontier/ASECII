using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.Xna.Framework.Input.Keys;

namespace ASECII {
    class EditorMain : SadConsole.Console {
        SpriteModel model;
        public EditorMain(int width, int height, Font font) :base(width, height, font) {
            UseKeyboard = true;
            UseMouse = true;
            model = new SpriteModel(32, 32);
            this.Children.Add(new SpriteMenu(32, 32, font, model) {
                Position = new Point(16, 0),
                FocusOnMouseClick = true
            });
            
            this.Children.Add(new GlyphMenu(16, 16, font, model) {
                Position = new Point(0),
                FocusOnMouseClick = true
            });
            
            this.Children.Add(new PaletteMenu(16, 4, font.Master.GetFont(Font.FontSizes.Two), model) {
                Position = new Point(0, 16),
                FocusOnMouseClick = true
            });
        }
        public override bool ProcessKeyboard(Keyboard info) {
            return base.ProcessKeyboard(info);
        }
        public override void Draw(TimeSpan timeElapsed) {
            Clear();
            base.Draw(timeElapsed);
        }
    }

    class SpriteMenu : SadConsole.Console {
        SpriteModel model;
        public SpriteMenu(int width, int height, Font font, SpriteModel model) : base(width, height, font) {
            UseMouse = true;
            UseKeyboard = true;
            this.model = model;
        }
        public override void Update(TimeSpan timeElapsed) {
            base.Update(timeElapsed);
        }
        public override void Draw(TimeSpan timeElapsed) {
            Clear();
            model.ticks++;

            int hx = Width / 2;
            int hy = Height / 2;

            var camera = model.camera - model.pan.offsetPan;

            for (int x = -hx; x < hx+1; x++) {
                for(int y = -hy; y < hy+1; y++) {
                    var pos = camera + new Point(x, y);
                    if(model.sprite.InBounds(pos)) {
                        var cg = model.sprite[pos];
                        Print(hx - x, hy - y, cg);
                    } else {
                        Print(hx - x, hy - y, " ", Color.Transparent, Color.Blue);
                    }
                }
            }
            switch(model.mode) {
                case Mode.Edit:
                    if (IsMouseOver && model.ticks % 30 < 15) {
                        SetCellAppearance(hx - model.cursor.X + model.camera.X, hy - model.cursor.Y + model.camera.Y, model.brush.cell);
                    }
                    break;
            }
            

            base.Draw(timeElapsed);
        }
        public override bool ProcessKeyboard(Keyboard info) {
            model.ProcessKeyboard(info);
            return base.ProcessKeyboard(info);
        }
        public override bool ProcessMouse(MouseConsoleState state) {
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
        public PanMode pan;

        public int ticks = 0;

        public Point camera = new Point();
        public Point cursor = new Point();       //Position on the image; stored as offset from camera
        public Point prevPixel = new Point();
        public bool keyboardMode;

        public Point prevCell;
        public bool prevLeft;

        public Mode mode;

        public SpriteModel(int width, int height) {
            this.width = width;
            this.height = height;
            brush = new BrushMode(this);
            pan = new PanMode(this);
            Undo = new Stack<SingleEdit>();
            Redo = new Stack<SingleEdit>();
            mode = Mode.Edit;
        }
        public void ProcessKeyboard(Keyboard info) {
            if (info.IsKeyPressed(Space) && pan.startPan == null) {
                pan.startPan = cursor;
            } else if(info.IsKeyReleased(Space)) {
                camera -= pan.offsetPan;
                pan.startPan = null;
                pan.offsetPan = Point.Zero;
            }

            if(info.IsKeyDown(LeftControl) && info.IsKeyUp(LeftShift) && info.IsKeyPressed(Z)) {
                if(Undo.Any()) {
                    var u = Undo.Pop();
                    u.Undo();
                    Redo.Push(u);
                }
            }
            if(info.IsKeyDown(LeftControl) && info.IsKeyDown(LeftShift) && info.IsKeyPressed(Z)) {
                if(Redo.Any()) {
                    var u = Redo.Pop();
                    u.Do();
                    Undo.Push(u);
                }
            }

            if (info.IsKeyPressed(Right)) {
                cursor.X--;
                keyboardMode = true;
            }
            if (info.IsKeyPressed(Left)) {
                cursor.X++;
                keyboardMode = true;
            }
            if (info.IsKeyPressed(Up)) {
                cursor.Y++;
                keyboardMode = true;
            }
            if (info.IsKeyPressed(Down)) {
                cursor.Y--;
                keyboardMode = true;
            }
            if (info.KeysPressed.Count == 1 && info.KeysDown.Count == 1) {
                char c = info.KeysPressed[0].Character;
                if (char.IsLetterOrDigit(c) && sprite.InBounds(cursor)) {
                    var layer = sprite.layers[0];
                    var cg = new ColoredGlyph(c, brush.foreground, brush.background);
                    var action = new SingleEdit(cursor, layer, cg);
                    AddAction(action);
                }
            }
        }
        public void ProcessMouse(MouseConsoleState state) {
            var pressedLeft = !prevLeft && state.Mouse.LeftButtonDown;

            if (state.ConsolePixelPosition != prevPixel) {
                keyboardMode = false;
            }
            prevPixel = state.ConsolePixelPosition;

            if (!keyboardMode) {
                cursor = new Point(width / 2, height / 2) - state.ConsoleCellPosition + camera;
            }


            brush.glyph = (char)((brush.glyph + state.Mouse.ScrollWheelValueChange / 120 + 255) % 255);

            if (pan.startPan != null) {
                pan.ProcessMouse(state);
            } else {

                switch(mode) {
                    case Mode.Edit:
                        brush.ProcessMouse(state);
                        break;
                    case Mode.Select:

                        break;
                    case Mode.Keyboard:
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
        public Point? startPan;     //Position on screen where user held down space and left clicked
        public Point offsetPan;

        public PanMode(SpriteModel model) {
            this.model = model;
        }
        public bool ProcessMouse(MouseConsoleState state) {
            if (startPan != null) {
                offsetPan = model.cursor - (Point)startPan;
                return true;
            }
            return false;
        }
    }
    class BrushMode {
        SpriteModel model;
        public char glyph = 'A';
        public Color foreground = Color.Red;
        public Color background = Color.Black;
        public ColoredGlyph cell => new ColoredGlyph(glyph, foreground, background);
        public BrushMode(SpriteModel model) {
            this.model = model;
        }

        public void ProcessMouse(MouseConsoleState state) {
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
    class SelectMode {
        SpriteModel model;
        Point? start;
        Point end;
        Rectangle rect;
        public SelectMode(SpriteModel model) {
            this.model = model;
        }
        public void ProcessMouse(MouseConsoleState state) {
            if(state.Mouse.LeftButtonDown) {
                if (start == null) {
                    start = model.cursor;
                } else {
                    end = model.cursor;

                    int leftX = Math.Min(start.Value.X, end.X);
                    int width = Math.Max(start.Value.X, end.X) - leftX;
                    int topY = Math.Min(start.Value.Y, end.Y);
                    int height = Math.Max(start.Value.Y, end.Y);
                    rect = new Rectangle(leftX, topY, width, height);
                }
            }
        }

    }
    class PaletteMenu : SadConsole.Console {
        SpriteModel spriteModel;
        PaletteModel paletteModel;
        bool prevLeft;
        bool prevRight;
        public PaletteMenu(int width, int height, Font font, SpriteModel spriteModel) : base(width, height, font) {
            this.spriteModel = spriteModel;
            this.paletteModel = new PaletteModel();
            paletteModel.palette.Add(Color.BlueViolet);
        }

        public override bool ProcessMouse(MouseConsoleState state) {
            if(state.ConsoleCellPosition.X < Width && state.ConsoleCellPosition.Y < Height) {
                int index = (state.ConsoleCellPosition.X) + (state.ConsoleCellPosition.Y * Width);
                if (index > -1 && index < paletteModel.palette.Count) {
                    if (!prevLeft && state.Mouse.LeftButtonDown) {
                        if(paletteModel.foregroundIndex != index) {
                            paletteModel.foregroundIndex = index;
                            spriteModel.brush.foreground = paletteModel.palette[index];
                        } else {
                            paletteModel.foregroundIndex = null;
                            spriteModel.brush.foreground = Color.Transparent;
                        }

                    }
                    if (!prevRight && state.Mouse.RightButtonDown) {
                        if(paletteModel.backgroundIndex != index) {
                            paletteModel.backgroundIndex = index;
                            spriteModel.brush.background = paletteModel.palette[index];
                        } else {
                            paletteModel.backgroundIndex = null;
                            spriteModel.brush.background = Color.Transparent;
                        }
                    }
                }
            }
            prevLeft = state.Mouse.LeftButtonDown;
            prevRight = state.Mouse.RightButtonDown;

            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for(int i = 0; i < paletteModel.palette.Count; i++) {
                SetCellAppearance(i % Width, i / Width, new Cell(Color.Transparent, paletteModel.palette[i]));
            }
            if(paletteModel.foregroundIndex != null || paletteModel.backgroundIndex != null) {
                if(paletteModel.foregroundIndex == paletteModel.backgroundIndex) {
                    var i = paletteModel.foregroundIndex.Value;
                    int x = i % Width;
                    int y = i / Width;

                    SetCellAppearance(x, y, new Cell(paletteModel.palette[i].GetLuma() > 0.5 ? Color.Black : Color.White, paletteModel.palette[i], 'X'));
                } else {
                    if(paletteModel.foregroundIndex != null) {
                        var i = paletteModel.foregroundIndex.Value;
                        int x = i % Width;
                        int y = i / Width;

                        SetCellAppearance(x, y, new Cell(paletteModel.palette[i].GetLuma() > 0.5 ? Color.Black : Color.White, paletteModel.palette[i], 'F'));
                    }
                    if(paletteModel.backgroundIndex != null) {
                        var i = paletteModel.backgroundIndex.Value;
                        int x = i % Width;
                        int y = i / Width;

                        SetCellAppearance(x, y, new Cell(paletteModel.palette[i].GetLuma() > 0.5 ? Color.Black : Color.White, paletteModel.palette[i], 'B'));
                    }
                }
            }

            base.Draw(timeElapsed);
        }
    }

    class PaletteModel {
        public List<Color> palette = new List<Color>();
        public int? foregroundIndex;
        public int? backgroundIndex;
    }

    class GlyphMenu : SadConsole.Console {
        SpriteModel spriteModel;
        bool prevLeft;
        bool prevRight;
        public GlyphMenu(int width, int height, Font font, SpriteModel spriteModel) : base(width, height, font) {
            this.spriteModel = spriteModel;
        }

        public override bool ProcessMouse(MouseConsoleState state) {
            if (state.ConsoleCellPosition.X < Width && state.ConsoleCellPosition.Y < Height) {
                int index = (state.ConsoleCellPosition.X) + (state.ConsoleCellPosition.Y * Width);
                if (index < 256) {
                    if (!prevLeft && state.Mouse.LeftButtonDown) {
                        if (spriteModel.brush.glyph != index) {
                            spriteModel.brush.glyph = (char)index;
                        } else {
                            //spriteModel.glyph = null;
                        }

                    }
                }
            }
            prevLeft = state.Mouse.LeftButtonDown;
            prevRight = state.Mouse.RightButtonDown;

            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for (int i = 0; i < 256; i++) {
                string s = ((char)i).ToString();
                Print(i % Width, i / Width, s, spriteModel.brush.foreground, spriteModel.brush.background);
            }
            var x = spriteModel.brush.glyph % Width;
            var y = spriteModel.brush.glyph / Width;
            var c = GetCellAppearance(x, y);
            SetCellAppearance(x, y, new Cell(c.Background, c.Foreground, c.Glyph));


            base.Draw(timeElapsed);
        }
    }
    class ColorPicker : SadConsole.Console {
        public ColorPicker(int width, int height, Font font) : base(width, height, font) { }
    }
}
