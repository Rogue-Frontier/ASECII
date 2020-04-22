using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using static Microsoft.Xna.Framework.Input.Keys;

namespace ASECII {
    class EditorMain : SadConsole.Console {
        SpriteModel model;
        public EditorMain(int width, int height, Font font) :base(width, height, font) {
            UseKeyboard = true;
            UseMouse = true;
            model = new SpriteModel();
            this.Children.Add(new SpriteMenu(32, 32, font, model) {
                Position = new Point(8, 0),
                FocusOnMouseClick = true
            });
            this.Children.Add(new PaletteMenu(8, 16, font.Master.GetFont(Font.FontSizes.Two), model) {
                Position = new Point(0, 0),
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

            var camera = model.camera - model.offsetPan;

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

            if(IsMouseOver && model.ticks%30 < 15) {
                SetCellAppearance(hx - model.cursor.X + model.camera.X, hy - model.cursor.Y + model.camera.Y, model.cg);
            }

            base.Draw(timeElapsed);
        }
        public override bool ProcessKeyboard(Keyboard info) {
            if (info.IsKeyDown(Space)) {
                model.allowPan = true;
            } else {
                model.allowPan = false;
            }

            if (info.IsKeyPressed(Right)) {
                model.cursor.X--;
                model.keyboardMode = true;
            }
            if (info.IsKeyPressed(Left)) {
                model.cursor.X++;
                model.keyboardMode = true;
            }
            if (info.IsKeyPressed(Up)) {
                model.cursor.Y++;
                model.keyboardMode = true;
            }
            if (info.IsKeyPressed(Down)) {
                model.cursor.Y--;
                model.keyboardMode = true;
            }
            if (info.KeysPressed.Count == 1) {
                char c = info.KeysPressed[0].Character;
                if (char.IsLetterOrDigit(c)) {
                    model.sprite.layers[0][model.cursor] = new ColoredGlyph(c, model.foreground, model.background);
                }
            }
            return base.ProcessKeyboard(info);
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            var pressedLeft = !model.prevLeft && state.Mouse.LeftButtonDown;

            if (state.ConsolePixelPosition != model.prevPixel) {
                model.keyboardMode = false;
            }
            model.prevPixel = state.ConsolePixelPosition;

            if(!model.keyboardMode) {
                model.cursor = new Point(Width / 2, Height / 2) - state.ConsoleCellPosition + model.camera;
            }
            

            model.glyph = (char)((model.glyph + state.Mouse.ScrollWheelValueChange/120 + 255) % 255);


            IsFocused = true;
            if (model.startPan != null) {
                if (!state.Mouse.LeftButtonDown) {
                    model.camera -= model.offsetPan;
                    model.startPan = null;
                    model.offsetPan = Point.Zero;
                } else {
                    model.offsetPan = (Point)model.startPan - state.ConsoleCellPosition;
                }
            } else if (state.Mouse.LeftButtonDown) {
                if (model.startPan == null && model.allowPan) {
                    model.startPan = state.ConsoleCellPosition;
                } else {
                    var prev = model.prevCell;
                    var offset = (model.cursor - prev);
                    var length = offset.Length();
                    for(int i = 0; i < length; i++) {
                        var p = prev + new Point((int)(i * offset.X / length), (int)(i * offset.Y / length));
                        if (model.sprite.InBounds(p)) {
                            model.sprite.layers[0][p] = model.cg;
                        }
                    }

                    if (model.sprite.InBounds(model.cursor)) {
                        model.sprite.layers[0][model.cursor] = model.cg;
                    }
                }
            }
            model.prevCell = model.cursor;
            model.prevLeft = state.Mouse.LeftButtonDown;

            Done:
            return base.ProcessMouse(state);
        }
    }
    class SpriteModel {
        public char glyph = 'A';
        public Color foreground = Color.Red;
        public Color background = Color.Black;
        public ColoredGlyph cg => new ColoredGlyph(glyph, foreground, background);

        public Sprite sprite = new Sprite(16, 16);

        public int ticks = 0;

        public Point camera = new Point();
        public Point cursor = new Point();       //Position on the image; stored as offset from camera
        public Point prevPixel = new Point();
        public bool keyboardMode;

        public bool allowPan;
        public Point? startPan;     //Position on screen where user held down space and left clicked
        public Point offsetPan;

        public Point prevCell;
        public bool prevLeft;
    }
    class PaletteMenu : SadConsole.Console {
        SpriteModel spriteModel;
        PaletteModel paletteModel;
        public PaletteMenu(int width, int height, Font font, SpriteModel spriteModel) : base(width, height, font) {
            this.spriteModel = spriteModel;
            paletteModel = new PaletteModel();
            paletteModel.palette.Add(Color.Red);
            paletteModel.palette.Add(Color.Green);
        }

        public override bool ProcessMouse(MouseConsoleState state) {
            if(state.ConsoleCellPosition.X < Width && state.ConsoleCellPosition.Y < Height) {
                int index = (state.ConsoleCellPosition.X) + (state.ConsoleCellPosition.Y);
                if (index < paletteModel.palette.Count) {
                    if (state.Mouse.LeftButtonDown) {
                        paletteModel.foregroundIndex = index;
                        spriteModel.foreground = paletteModel.palette[index];
                    } else if (state.Mouse.RightButtonDown) {
                        paletteModel.backgroundIndex = index;
                        spriteModel.background = paletteModel.palette[index];
                    }
                }
            }


            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for(int i = 0; i < paletteModel.palette.Count; i++) {
                string s = " ";
                if(i == paletteModel.foregroundIndex && i == paletteModel.backgroundIndex) {
                    s = "X";
                } else if (i == paletteModel.foregroundIndex) {
                    s = "F";
                } else if(i == paletteModel.backgroundIndex) {
                    s = "B";
                }
                Print(i % 8, i / 8, s, paletteModel.palette[i].GetLuma() > 0.5 ? Color.Black : Color.White, paletteModel.palette[i]);
            }
            base.Draw(timeElapsed);
        }


    }
    class PaletteModel {
        public List<Color> palette = new List<Color>();
        public int? foregroundIndex;
        public int? backgroundIndex;
    }
    class ColorPicker : SadConsole.Console {
        public ColorPicker(int width, int height, Font font) : base(width, height, font) { }
    }
}
