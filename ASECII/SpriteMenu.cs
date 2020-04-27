using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
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
            var paletteModel = new PaletteModel();
            var colorPicker = new ColorPickerModel(16, 16, model);
            this.Children.Add(new SpriteMenu(32, 32, font, model) {
                Position = new Point(16, 0),
                FocusOnMouseClick = true
            });
            
            this.Children.Add(new GlyphMenu(16, 16, font, model) {
                Position = new Point(0),
                FocusOnMouseClick = true
            });
            
            this.Children.Add(new PaletteMenu(16, 4, font, model, paletteModel, colorPicker) {
                Position = new Point(0, 16),
                FocusOnMouseClick = true
            });
            this.Children.Add(new CellButton(font, () => {
                return !paletteModel.paletteSet.Contains(model.brush.foreground);
            }, () => {
                paletteModel.AddColor(model.brush.foreground);
                paletteModel.UpdateIndexes(model);
                colorPicker.UpdateColors();
                colorPicker.UpdateBrushPoints(paletteModel);
                colorPicker.UpdatePalettePoints(paletteModel);
            }) {
                Position = new Point(15, 19),
                FocusOnMouseClick = true
            });

            this.Children.Add(new CellButton(font, () => {
                return !paletteModel.paletteSet.Contains(model.brush.background);
            }, () => {
                paletteModel.AddColor(model.brush.background);
                paletteModel.UpdateIndexes(model);
                colorPicker.UpdateColors();
                colorPicker.UpdateBrushPoints(paletteModel);
                colorPicker.UpdatePalettePoints(paletteModel);
            }) {
                Position = new Point(14, 19),
                FocusOnMouseClick = true
            });


            this.Children.Add(new ColorPicker(16, 16, font, model, paletteModel, colorPicker) {
                Position = new Point(0, 20),
                FocusOnMouseClick = true
            });

            this.Children.Add(new ColorBar(16, 1, font, paletteModel, colorPicker) {
                Position = new Point(0, 36),
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
            if(model.pan.allowPan) {

            } else {
                switch (model.mode) {
                    case Mode.Edit:
                        if (IsMouseOver && model.ticks % 30 < 15) {
                            SetCellAppearance(hx - model.cursor.X + model.camera.X, hy - model.cursor.Y + model.camera.Y, model.brush.cell);
                        }
                        break;
                    case Mode.Keyboard:
                        var c = model.brush.cell;
                        if (model.ticks % 30 < 15) {
                            SetCellAppearance(hx - model.keyboard.keyCursor.X + model.camera.X, hy - model.keyboard.keyCursor.Y + model.camera.Y, new Cell(c.Foreground, c.Background, '_'));
                            SetCellAppearance(hx - model.keyboard.margin.X + model.camera.X - 1, hy - model.keyboard.keyCursor.Y + model.camera.Y, new Cell(c.Foreground, c.Background, '>'));
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
        public KeyboardMode keyboard;
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
                    pan.startPan = Point.Zero;
                    pan.offsetPan = Point.Zero;
                } else if(info.IsKeyPressed(T)) {
                    mode = Mode.Keyboard;
                    keyboard.keyCursor = cursor;
                    keyboard.margin = cursor;
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
        public void ProcessMouse(MouseConsoleState state) {
            cursor = new Point(width / 2, height / 2) - state.ConsoleCellPosition + camera;
            if (pan.allowPan) {
                pan.ProcessMouse(state);
            } else {
                switch (mode) {
                    case Mode.Edit:
                        brush.ProcessMouse(state);
                        break;
                    case Mode.Select:
                        
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
                startPan.X--;
            }
            if (info.IsKeyPressed(Left)) {
                startPan.X++;
            }
            if (info.IsKeyPressed(Up)) {
                startPan.Y++;
            }
            if (info.IsKeyPressed(Down)) {
                startPan.Y--;
            }
            UpdateOffset();
        }
        public void ProcessMouse(MouseConsoleState state) {
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
        public ColoredGlyph cell => new ColoredGlyph(glyph, foreground, background);
        public BrushMode(SpriteModel model) {
            this.model = model;
        }

        public void ProcessMouse(MouseConsoleState state) {
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
                keyCursor.X--;
            }
            if (info.IsKeyPressed(Left)) {
                keyCursor.X++;
            }
            if (info.IsKeyPressed(Up)) {
                keyCursor.Y++;
            }
            if (info.IsKeyPressed(Down)) {
                keyCursor.Y--;
            }
            if(info.IsKeyPressed(Enter)) {
                keyCursor.X = margin.X;
                keyCursor.Y--;
            }

            ref var sprite = ref model.sprite;
            ref var brush = ref model.brush;
            if (info.KeysPressed.Count == 1 && (info.KeysDown.Count == 1 || (info.KeysDown.Count == 2 && info.IsKeyDown(LeftShift)))) {
                char c = info.KeysPressed[0].Character;
                if (char.IsLetterOrDigit(c) && sprite.InBounds(keyCursor)) {
                    var layer = sprite.layers[0];
                    var cg = new ColoredGlyph(c, brush.foreground, brush.background);
                    var action = new SingleEdit(keyCursor, layer, cg);
                    model.AddAction(action);

                    keyCursor.X--;
                }
            }
        }
        public void ProcessMouse(MouseConsoleState state) {
            if(state.Mouse.LeftButtonDown) {
                margin = keyCursor = model.cursor;
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
        ColorPickerModel colorPicker;
        bool prevLeft;
        bool prevRight;
        public PaletteMenu(int width, int height, Font font, SpriteModel spriteModel, PaletteModel paletteModel, ColorPickerModel colorPicker) : base(width, height, font) {
            this.spriteModel = spriteModel;
            this.paletteModel = paletteModel;
            this.colorPicker = colorPicker;
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            if(state.IsOnConsole) {
                int index = (state.ConsoleCellPosition.X) + (state.ConsoleCellPosition.Y * Width);
                if (index > -1 && index < paletteModel.palette.Count) {
                    bool pressLeft = !prevLeft && state.Mouse.LeftButtonDown;
                    var pressRight = !prevRight && state.Mouse.RightButtonDown;
                    if(pressLeft || pressRight) {
                        if (pressLeft) {
                            if (paletteModel.foregroundIndex != index) {
                                paletteModel.foregroundIndex = index;
                                spriteModel.brush.foreground = paletteModel.palette[index];
                            } else {
                                paletteModel.foregroundIndex = null;
                                spriteModel.brush.foreground = Color.Transparent;
                            }

                        }

                        if (pressRight) {
                            if (paletteModel.backgroundIndex != index) {
                                paletteModel.backgroundIndex = index;
                                spriteModel.brush.background = paletteModel.palette[index];
                            } else {
                                paletteModel.backgroundIndex = null;
                                spriteModel.brush.background = Color.Transparent;
                            }

                        }
                        colorPicker.UpdateBrushPoints(paletteModel);
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

                    SetCellAppearance(x, y, new Cell(paletteModel.palette[i].GetTextColor(), paletteModel.palette[i], 'X'));
                } else {
                    if(paletteModel.foregroundIndex != null) {
                        var i = paletteModel.foregroundIndex.Value;
                        int x = i % Width;
                        int y = i / Width;

                        SetCellAppearance(x, y, new Cell(paletteModel.palette[i].GetTextColor(), paletteModel.palette[i], 'F'));
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
    class CellButton : SadConsole.Console {
        Func<bool> active;
        bool isActive;
        Action click;
        bool prevLeft;
        bool pressed;
        public CellButton(Font font, Func<bool> active, Action click) : base(1, 1, font) {
            this.active = active;
            this.click = click;
        }
        public override void Update(TimeSpan timeElapsed) {
            isActive = active();
            base.Update(timeElapsed);
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            if(isActive && IsMouseOver) {
                if(state.Mouse.LeftButtonDown) {
                    if(!prevLeft) {
                        pressed = true;
                    }
                } else if (pressed && !state.Mouse.LeftButtonDown) {
                    pressed = false;
                    click();
                }

            }
            prevLeft = state.Mouse.LeftButtonDown;
            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            if(pressed) {
                Print(0, 0, "+", Color.White, Color.Black);
            } else if(isActive) {
                Print(0, 0, "+", Color.Black, IsMouseOver ? Color.Yellow : Color.White);
            } else {
                Print(0, 0, " ", Color.Transparent, Color.White);
            }
            

            base.Draw(timeElapsed);
        }
    }
    class PaletteModel {
        public List<Color> palette = new List<Color>();
        public HashSet<Color> paletteSet = new HashSet<Color>();
        public int? foregroundIndex;
        public int? backgroundIndex;

        public void AddColor(Color c) {
            palette.Add(c);
            paletteSet.Add(c);
        }
        public void UpdateIndexes(SpriteModel spriteModel) {
            var p = palette;
            foregroundIndex = p.IndexOf(spriteModel.brush.foreground);
            if (foregroundIndex == -1) {
                foregroundIndex = null;
            }

            backgroundIndex = p.IndexOf(spriteModel.brush.background);
            if (backgroundIndex == -1) {
                backgroundIndex = null;
            }
        }
    }
    class GlyphMenu : SadConsole.Console {
        SpriteModel spriteModel;
        bool prevLeft;
        bool prevRight;
        public GlyphMenu(int width, int height, Font font, SpriteModel spriteModel) : base(width, height, font) {
            this.spriteModel = spriteModel;
        }

        public override bool ProcessMouse(MouseConsoleState state) {
            if (state.IsOnConsole) {
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
        SpriteModel model;
        PaletteModel paletteModel;
        ColorPickerModel colorPicker;
        public ColorPicker(int width, int height, Font font, SpriteModel model, PaletteModel paletteModel, ColorPickerModel colorPicker) : base(width, height, font) {
            this.model = model;
            this.paletteModel = paletteModel;
            this.colorPicker = colorPicker;
            colorPicker.UpdateColors();
            colorPicker.UpdateBrushPoints(paletteModel);
            colorPicker.UpdatePalettePoints(paletteModel);
        }
        public override bool ProcessKeyboard(Keyboard info) {
            return base.ProcessKeyboard(info);
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            var (x, y) = state.ConsoleCellPosition;
            if(state.IsOnConsole) {
                var colors = colorPicker.colors;
                if (state.Mouse.LeftButtonDown) {
                    model.brush.foreground = colors[x, y];
                    colorPicker.foregroundPoint = new Point(x, y);
                }
                if (state.Mouse.RightButtonDown) {
                    model.brush.background = colors[x, y];
                    colorPicker.backgroundPoint = new Point(x, y);
                }
                paletteModel.UpdateIndexes(model);
            }
            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            var colors = colorPicker.colors;
            for (int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    SetCellAppearance(x, y, new Cell(Color.Transparent, colors[x,y]));
                }
            }

            foreach(var (x, y) in colorPicker.palettePoints) {
                var c = colors[x, y];
                var f = c.GetTextColor();
                SetCellAppearance(x, y, new Cell(f, c, '.'));
            }
            var backgroundPoint = colorPicker.backgroundPoint;
            var foregroundPoint = colorPicker.foregroundPoint;
            if (foregroundPoint != null || backgroundPoint != null) {
                if(foregroundPoint == backgroundPoint) {
                    var (x, y) = foregroundPoint.Value;
                    var c = colors[x, y];
                    var f = c.GetTextColor();
                    SetCellAppearance(x, y, new Cell(f, c, 'X'));
                } else {
                    if(foregroundPoint != null) {
                        var (x, y) = foregroundPoint.Value;
                        var c = colors[x, y];
                        var f = c.GetTextColor();
                        SetCellAppearance(x, y, new Cell(f, c, 'F'));
                    }
                    if(backgroundPoint != null) {
                        var (x, y) = backgroundPoint.Value;
                        var c = colors[x, y];
                        var f = c.GetTextColor();
                        SetCellAppearance(x, y, new Cell(f, c, 'B'));
                    }
                }
            }
            base.Draw(timeElapsed);
        }
    }
    class ColorPickerModel {
        SpriteModel model;
        int Width, Height;
        public double hue;
        public Color[,] colors;
        public HashSet<Point> palettePoints;
        public Point? foregroundPoint;
        public Point? backgroundPoint;

        public ColorPickerModel(int Width, int Height, SpriteModel model) {
            this.model = model;
            this.Width = Width;
            this.Height = Height;
            colors = new Color[Width, Height];
        }
        public void UpdateColors() {
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    var c = Helper.HsvToRgb(hue, 1f * x / Width, 1f * y / Height);
                    colors[x, Height - y - 1] = c;
                }
            }
        }

        public void UpdateBrushPoints(PaletteModel paletteModel) {
            foregroundPoint = null;
            backgroundPoint = null;
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    var c = colors[x, y];
                    var p = new Point(x, y);
                    if (model.brush.foreground == c) {
                        foregroundPoint = p;
                    }
                    if (model.brush.background == c) {
                        backgroundPoint = p;
                    }
                }
            }
        }
        public void UpdatePalettePoints(PaletteModel paletteModel) {
            var palette = paletteModel.paletteSet;
            palettePoints = new HashSet<Point>();
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    var c = colors[x, y];
                    var p = new Point(x, y);
                    if (palette.Contains(c)) {
                        palettePoints.Add(p);
                    }
                }
            }
        }
    }
    class ColorBar : SadConsole.Console {
        ColorPickerModel colorPicker;
        PaletteModel paletteModel;
        int index = 0;
        Color[] bar;

        public ColorBar(int width, int height, Font font, PaletteModel paletteModel, ColorPickerModel model) : base(width, height, font) {
            this.paletteModel = paletteModel;
            this.colorPicker = model;
            bar = new Color[width];
            for (int i = 0; i < width; i++) {
                bar[i] = Helper.HsvToRgb(i * 360d / width, 1.0, 1.0);
            }
        }

        public override bool ProcessMouse(MouseConsoleState state) {
            var (x, y) = state.ConsoleCellPosition;
            if(state.IsOnConsole) {
                if (state.Mouse.LeftButtonDown) {
                    index = x;
                    colorPicker.hue = index * 360d / bar.Length;
                    colorPicker.UpdateColors();
                    colorPicker.UpdateBrushPoints(paletteModel);
                    colorPicker.UpdatePalettePoints(paletteModel);
                }
            }
            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for(int x = 0; x < Width; x++) {
                Print(x, 0, " ", Color.Transparent, bar[x]);
            }
            var c = GetCellAppearance(index, 0).Background;
            var g = c.GetTextColor();
            SetCellAppearance(index, 0, new Cell(g, c, '*'));

            base.Draw(timeElapsed);
        }
    }
}
