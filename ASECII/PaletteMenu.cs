using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASECII {
    class PaletteMenu : SadConsole.Console {
        SpriteModel spriteModel;
        PaletteModel paletteModel;
        PickerModel colorPicker;
        bool prevLeft;
        bool prevRight;
        public PaletteMenu(int width, int height, Font font, SpriteModel spriteModel, PaletteModel paletteModel, PickerModel colorPicker) : base(width, height, font) {
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
}
