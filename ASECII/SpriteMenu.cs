using ArchConsole;
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
using System.Text;
using ArchConsole;
using static SadConsole.Input.Keys;
using Microsoft.Xna.Framework.Graphics;
using Console = SadConsole.Console;
using SadConsole.Renderers;

namespace ASECII {
    class EditorMain : SadConsole.Console {
        SpriteModel model;
        SpriteMenu spriteMenu;
        Console controlsMenu;
        HistoryMenu historyMenu;
        public EditorMain(int width, int height, SpriteModel model) :base(width, height) {
            UseKeyboard = true;
            UseMouse = true;
            DefaultBackground = Color.Black;

            this.model = model;

            InitUI();

            spriteMenu.Position = new Point(controlsMenu.Width, 0);
            controlsMenu.Position = new Point(0, 0);

            this.Children.Add(spriteMenu);
            this.Children.Add(controlsMenu);
        }
        public void InitControls() {
            int x = 0;
            int y = 0;

            var tileModel = model.tiles;
            var paletteModel = model.palette;
            var pickerModel = new PickerModel(16, 16, model);

            pickerModel.UpdateColors();
            pickerModel.UpdateBrushPoints(paletteModel);
            pickerModel.UpdatePalettePoints(paletteModel);
            controlsMenu = new Console(16, Height) {
                UseKeyboard = true,
                UseMouse = true,
                DefaultBackground = Color.Black
            };

            Action UpdateChannels = () => { };
            Action UpdateColorButtons = () => { };

            ActiveLabelButton tileButton = null;

            ColorLabel foregroundLabel = null, backgroundLabel = null;
            CellButton foregroundAddButton = null, backgroundAddButton = null;
            CellButton foregroundRemoveButton = null, backgroundRemoveButton = null;

            LabelButton colorModeButton = null;

            colorModeButton = new LabelButton((model.colorMode switch
            {
                ColorMode.RGB => "Mode: RGB",
                ColorMode.Grayscale => "Mode: Grayscale",
                ColorMode.Notepad => "Mode: Notepad",
                _ => "Color Mode"
            }), ChangeColorMode) { Position = new Point(0, y) };
            y++;
            void ChangeColorMode() {
                model.colorMode = (ColorMode) ((int)(model.colorMode + 1) % Enum.GetValues(typeof(ColorMode)).Length);
                ResetControls();
            }
            void ResetControls() {
                this.Children.Remove(controlsMenu);
                InitControls();
                this.Children.Add(controlsMenu);
            }

            var tileMenu = new TileMenu(controlsMenu.Width, 8, model, tileModel, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();

                paletteModel.UpdateIndexes(model);
                PaletteChanged();

                UpdateChannels();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            y += 8;

            tileButton = new ActiveLabelButton("Add Tile", () => {
                var c = model.brush.cell;
                return tileModel.brushIndex == null;
            }, () => {
                tileModel.AddTile(model.brush.cell);
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            /*
            tileButton = new ActiveColorButton("Add Tile", () => {
                var c = model.brush.cell;
                return tileModel.brushIndex == null;
            }, () => {
                return model.brush.cell.Background;
            }, () => {
                tileModel.AddTile(model.brush.cell);
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            */

            y++;

            var glyphMenu = new GlyphMenu(16, 16, model, () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            y += 16;

            
            void AddPaletteMenu() {
                var paletteMenu = new PaletteMenu(16, 4, model, paletteModel, () => {
                    tileModel.UpdateIndexes(model);
                    tileButton.UpdateActive();
                    pickerModel.UpdateBrushPoints(paletteModel);
                    PaletteChanged();

                    UpdateChannels();
                }) {
                    Position = new Point(0, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                controlsMenu.Children.Add(paletteMenu);

                y += 4;
            }

            void AddForegroundLabel() {
                foregroundRemoveButton = new CellButton(() => {
                    return paletteModel.foregroundIndex != null;
                }, () => {
                    paletteModel.RemoveColor(model.brush.foreground);
                    paletteModel.UpdateIndexes(model);

                    PaletteChanged();
                }, '-') {
                    Position = new Point(0, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                foregroundLabel = new ColorLabel(14, () => model.brush.foreground) {
                    Position = new Point(1, y)
                };
                foregroundAddButton = new CellButton(() => {
                    return paletteModel.foregroundIndex == null;
                }, () => {
                    paletteModel.AddColor(model.brush.foreground);
                    paletteModel.UpdateIndexes(model);

                    PaletteChanged();
                }, '+') {
                    Position = new Point(15, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                UpdateColorButtons += foregroundRemoveButton.UpdateActive;
                UpdateColorButtons += foregroundAddButton.UpdateActive;

                controlsMenu.Children.Add(foregroundLabel);
                controlsMenu.Children.Add(foregroundAddButton);
                controlsMenu.Children.Add(foregroundRemoveButton);

                y += 2;
            }

            void AddBackgroundLabel() {
                backgroundRemoveButton = new CellButton(() => {
                    return paletteModel.backgroundIndex != null;
                }, () => {
                    paletteModel.RemoveColor(model.brush.background);
                    paletteModel.UpdateIndexes(model);
                    PaletteChanged();
                }, '-') {
                    Position = new Point(0, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                backgroundLabel = new ColorLabel(14, () => model.brush.background) {
                    Position = new Point(1, y)
                };
                backgroundAddButton = new CellButton(() => {
                    return paletteModel.backgroundIndex == null;
                }, () => {
                    paletteModel.AddColor(model.brush.background);
                    paletteModel.UpdateIndexes(model);
                    PaletteChanged();
                }, '+') {
                    Position = new Point(15, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                UpdateColorButtons += backgroundRemoveButton.UpdateActive;
                UpdateColorButtons += backgroundAddButton.UpdateActive;

                controlsMenu.Children.Add(backgroundLabel);
                controlsMenu.Children.Add(backgroundAddButton);
                controlsMenu.Children.Add(backgroundRemoveButton);

                y += 2;

            }
            switch (model.colorMode) {
                case ColorMode.RGB: {
                        ChannelBar foregroundR = null, foregroundG = null, foregroundB = null, foregroundA = null,
                backgroundR = null, backgroundG = null, backgroundB = null, backgroundA = null;

                        AddPaletteMenu();
                        
                        AddForegroundLabel();

                        foregroundR = new ChannelBar(16, Channel.R, () => {
                            foregroundR.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        foregroundG = new ChannelBar(16, Channel.G, () => {
                            foregroundG.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        foregroundB = new ChannelBar(16, Channel.B, () => {
                            foregroundB.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        foregroundA = new ChannelBar(16, Channel.A, () => {
                            foregroundA.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y += 2;

                        AddBackgroundLabel();
                        backgroundR = new ChannelBar(16, Channel.R, () => {
                            backgroundR.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        backgroundG = new ChannelBar(16, Channel.G, () => {
                            backgroundG.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        backgroundB = new ChannelBar(16, Channel.B, () => {
                            backgroundB.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        backgroundA = new ChannelBar(16, Channel.A, () => {
                            backgroundA.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };

                        controlsMenu.Children.Add(foregroundR);
                        controlsMenu.Children.Add(foregroundG);
                        controlsMenu.Children.Add(foregroundB);
                        controlsMenu.Children.Add(foregroundA);

                        controlsMenu.Children.Add(backgroundR);
                        controlsMenu.Children.Add(backgroundG);
                        controlsMenu.Children.Add(backgroundB);
                        controlsMenu.Children.Add(backgroundA);

                        UpdateChannels = () => {
                            var f = model.brush.foreground;
                            var b = model.brush.background;
                            foregroundR.UpdateColors(f);
                            foregroundG.UpdateColors(f);
                            foregroundB.UpdateColors(f);
                            foregroundA.UpdateColors(f);

                            backgroundR.UpdateColors(b);
                            backgroundG.UpdateColors(b);
                            backgroundB.UpdateColors(b);
                            backgroundA.UpdateColors(b);
                        };

                        y += 2;

                        AddPickerMenu();
                        break;
                    }
                case ColorMode.Grayscale: {
                        ChannelBar foreground = null, background = null;
                        ChannelBar foregroundA = null, backgroundA = null;
                        AddPaletteMenu();

                        AddForegroundLabel();
                        foreground = new ChannelBar(16, Channel.Gray, () => {
                            foreground.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        foregroundA = new ChannelBar(16, Channel.A, () => {
                            foregroundA.ModifyColor(ref model.brush.foreground);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y += 2;

                        AddBackgroundLabel();
                        background = new ChannelBar(16, Channel.Gray, () => {
                            background.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                        y++;
                        backgroundA = new ChannelBar(16, Channel.A, () => {
                            backgroundA.ModifyColor(ref model.brush.background);
                            ChannelChanged();
                        }) {
                            Position = new Point(0, y),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };


                        UpdateChannels = () => {
                            var f = model.brush.foreground;
                            var b = model.brush.background;
                            foreground.UpdateColors(f);
                            background.UpdateColors(b);

                            foregroundA.UpdateColors(f);
                            backgroundA.UpdateColors(b);
                        };
                        controlsMenu.Children.Add(foreground);
                        controlsMenu.Children.Add(background);

                        controlsMenu.Children.Add(foregroundA);
                        controlsMenu.Children.Add(backgroundA);

                        y += 2;
                        break;
                    }
                case ColorMode.Notepad:
                    y += 1;
                    break;
            }

            void ChannelChanged() {
                UpdateChannels();

                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();

                paletteModel.UpdateIndexes(model);

                UpdateColorButtons();
            }

            void PaletteChanged() {
                UpdateColorButtons();

                pickerModel.UpdateColors();
                pickerModel.UpdateBrushPoints(paletteModel);
                pickerModel.UpdatePalettePoints(paletteModel);
            }


            void AddPickerMenu() {
                var pickerMenu = new PickerMenu(16, 16, model, pickerModel, () => {
                    tileModel.UpdateIndexes(model);
                    tileButton.UpdateActive();

                    paletteModel.UpdateIndexes(model);

                    UpdateColorButtons();

                    UpdateChannels();
                }) {
                    Position = new Point(0, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                controlsMenu.Children.Add(pickerMenu);

                y += 16;

                var hueBar = new HueBar(16, 1, paletteModel, pickerModel) {
                    Position = new Point(0, y),
                    FocusOnMouseClick = true,
                    UseMouse = true
                };
                controlsMenu.Children.Add(hueBar);

                y += 2;
            }

            var layerMenu = new LayerMenu(32, 16, model) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            y += 16;

            var layerAddButton = new LabelButton("Add Layer", () => {

                //model.currentLayer = Math.Min(model.currentLayer, model.sprite.layers.Count - 1);
                model.sprite.layers.Insert(model.currentLayer + 1, new Layer() { name = $"Layer {model.sprite.layers.Count}" });
                layerMenu.UpdateListing();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };
            y++;

            var layerCutButton = new ActiveLabelButton("Cut to Layer", () => model.selection.Exists, () => {
                model.SelectionToLayer();
                layerMenu.UpdateListing();
            }) {
                Position = new Point(0, y),
                FocusOnMouseClick = true,
                UseMouse = true
            };

            model.selection.selectionChanged = layerCutButton.UpdateActive;
            
            model.pick.brushChanged = () => {
                tileModel.UpdateIndexes(model);
                tileButton.UpdateActive();
                paletteModel.UpdateIndexes(model);
                PaletteChanged();
            };

            //No per-color transparency; layer-based only

            layerMenu.UpdateListing();

            tileModel.UpdateIndexes(model);
            tileButton.UpdateActive();

            paletteModel.UpdateIndexes(model);
            UpdateColorButtons();
            layerCutButton.UpdateActive();

            UpdateChannels();

            controlsMenu.Children.Add(colorModeButton);
            controlsMenu.Children.Add(tileMenu);
            controlsMenu.Children.Add(tileButton);
            controlsMenu.Children.Add(glyphMenu);

            controlsMenu.Children.Add(layerMenu);
            controlsMenu.Children.Add(layerAddButton);
            controlsMenu.Children.Add(layerCutButton);

        }
        public void InitUI() {
            InitControls();

            historyMenu = new HistoryMenu(16, Height, model);
            historyMenu.UpdateListing();
            model.historyChanged += historyMenu.HistoryChanged;

            spriteMenu = new SpriteMenu(Width - controlsMenu.Width, Height, model) {
                FocusOnMouseClick = true,
                UseMouse = true
            };
            spriteMenu.OnKeyboard += OnKeyboard;



            void OnKeyboard(Keyboard info) {
                
                if (info.IsKeyPressed(Tab)) {
                    bool controlsVisible = this.Children.Contains(controlsMenu);

                    this.Children.Remove(spriteMenu);
                    if (controlsVisible) {
                        this.Children.Remove(controlsMenu);

                        model.camera += new Point(-controlsMenu.Width, 0);
                        spriteMenu = new SpriteMenu(Width, Height, spriteMenu.model) {
                            Position = new Point(0, 0),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                    } else {
                        this.Children.Add(controlsMenu);

                        model.camera += new Point(controlsMenu.Width, 0);

                        spriteMenu = new SpriteMenu(Width - controlsMenu.Width, Height, spriteMenu.model) {
                            Position = new Point(controlsMenu.Width, 0),
                            FocusOnMouseClick = true,
                            UseMouse = true
                        };
                    }
                    spriteMenu.OnKeyboard += OnKeyboard;
                    this.Children.Add(spriteMenu);
                }

                if (info.IsKeyPressed(Z)) {
                    //Show undo/redo history menu
                }
            }
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
        public SpriteModel model;
        public Action<Keyboard> OnKeyboard;
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
            Color GetBackColor(int ax, int ay) => ((ax + ay) % 2 == 0) ? c1 : c2;

            model.sprite.UpdatePreview();
            var center = new Point(hx, hy);

            if(model.infinite) {
                for (int x = -hx; x < hx + 1; x++) {
                    for (int y = -hy; y < hy + 1; y++) {
                        var pos = camera + new Point(x, y) + center;

                        int ax = center.X + x;
                        int ay = center.Y + y;

                        var back = GetBackColor(ax, ay);
                        if (model.sprite.preview.TryGetValue(pos, out var tile)) {
                            var cg = tile.cg;
                            if(cg.Background.A < 255) {
                                cg = cg.SetBackground(tile.cg.Background.Blend(back));
                            }

                            this.SetCellAppearance(ax, ay, cg);
                        } else {
                            this.SetCellAppearance(ax, ay, new ColoredGlyph(Color.Transparent, back));
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
                            var back = GetBackColor(ax, ay);
                            if (model.sprite.preview.TryGetValue(pos, out var tile)) {
                                var cg = tile.cg;
                                if (cg.Background.A < 255) {
                                    cg = cg.SetBackground(tile.cg.Background.Blend(back));
                                }

                                this.SetCellAppearance(ax, ay, cg);
                            } else {
                                this.SetCellAppearance(ax, ay, new ColoredGlyph(Color.Transparent, back, ' '));
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
            
            if(model.pan.quickPan) {

            } else {
                switch (model.mode) {
                    case Mode.Brush:
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
                    case Mode.Line:
                        if ((model.ticks % 15) < 7) {
                            var cell = model.brush.cell;
                            if (model.line.start.HasValue) {
                                var start = model.line.start.Value;
                                var end = model.line.end;
                                if (start == end) {
                                    this.SetCellAppearance(start.X, start.Y, cell);
                                } else {
                                    foreach(var p in model.line.GetPoints()) {
                                        (var x, var y) = p;
                                        this.SetCellAppearance(x, y - 1, cell);
                                    }
                                }
                            } else {
                                this.SetCellAppearance(model.cursorScreen.X, model.cursorScreen.Y, cell);
                            }
                        }

                        break;
                    case Mode.Erase:
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
                                this.SetCellAppearance(model.cursorScreen.X, model.cursorScreen.Y, new ColoredGlyph(Color.Transparent, Color.Transparent));
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
                        if (model.ticksSelect % 30 < 15) {
                            DrawSelection();

                            if (model.selectRect.GetAdjustedRect(out Rectangle r)) {
                                DrawRect(r);
                            } else {
                                var p = model.cursorScreen;
                                DrawBox(p.X, p.Y, new BoxGlyph { n = Line.Single, e = Line.Single, s = Line.Single, w = Line.Single });
                            }
                        }
                        
                        break;
                    case Mode.SelectWand:
                        if (model.ticksSelect % 30 < 15) {
                            DrawSelection();
                        }
                        break;
                    case Mode.Move:
                        DrawSelection();
                        //Draw offset


                        //TO DO: Fix
                        if (model.ticksSelect % 10 < 5 && model.move.current.HasValue) {
                            var offset = model.move.end - model.move.start.Value;

                            int x = model.move.start.Value.X - camera.X;
                            int y = model.move.start.Value.Y - camera.Y;
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
                                for (int i = 1; i < Math.Abs(offset.X); i++) {
                                    x += Math.Sign(offset.X);
                                    DrawBox(x, y, new BoxGlyph {
                                        e = Line.Double,
                                        w = Line.Double
                                    });
                                }
                                x += Math.Sign(offset.X);
                                //End
                                if (first) {
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
                                for (int i = 1; i < Math.Abs(offset.Y); i++) {
                                    y += Math.Sign(offset.Y);
                                    DrawBox(x, y, new BoxGlyph {
                                        n = Line.Double,
                                        s = Line.Double
                                    });
                                }
                                y += Math.Sign(offset.Y);
                                //End
                                if (first) {
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
                    case Mode.Pick:
                        if (model.ticksSelect % 30 < 15) {
                            DrawSelection();
                        }
                        if ((model.ticks % 30) < 15) {
                            if (IsMouseOver) {
                                this.SetCellAppearance(model.cursorScreen.X, model.cursorScreen.Y, new ColoredGlyph(model.brush.foreground, model.brush.background, '!'));
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
                            } else {
                                g = new BoxGlyph {
                                    n = Line.Single,
                                    e = Line.Single,
                                    s = Line.Single,
                                    w = Line.Single
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
                this.AddDecorator(x, y, 1, new CellDecorator(model.brush.foreground.SetAlpha(102), BoxInfo.IBMCGA.glyphFromInfo[g], Mirror.None));
            }
        }
        public override bool ProcessKeyboard(Keyboard info) {
            OnKeyboard?.Invoke(info);

            if (info.IsKeyPressed(Escape)) {
                if(model.mode != Mode.Read) {
                    model.ProcessKeyboard(info);
                }
            } else if (info.IsKeyPressed(S) && info.IsKeyDown(LeftControl)) {
                //File.WriteAllText(Path.Combine(Environment.CurrentDirectory, Path.GetFileName(Path.GetTempFileName())), JsonConvert.SerializeObject(model));
                
                if(model.filepath == null || info.IsKeyDown(LeftShift)) {
                    var s = SadConsole.Game.Instance.Screen;
                    s.Children.Add(new FileMenu(s.Width, s.Height, new SaveMode(model, this)));
                } else {
                    model.Save(this);
                }

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

    public enum ColorMode {
        RGB, Grayscale, Notepad
    }
    public enum Mode {
        Read, Brush, Fill, Pick, Erase, SelectRect, SelectCircle, SelectLasso, SelectPoly, SelectWand, Move, Keyboard, Pan, Line,
    }
    [JsonObject(MemberSerialization.Fields)]
    public class SpriteModel {
        public ColorMode colorMode = ColorMode.RGB;

        public bool infinite = true;
        public int width, height;

        public bool ctrl, shift, alt;

        public LinkedList<Edit> Undo;
        public LinkedList<Edit> Redo;
        public Action historyChanged;

        public string filepath;
        public Sprite sprite;

        public int brushGlyph => tiles.brushIndex.HasValue ? tiles.brushTile.Glyph : brush.cell.Glyph;
        public TileRef brushTile => colorMode switch {
            ColorMode.Notepad => new NotepadTile(brushGlyph),
            _ => (tiles.brushIndex.HasValue ? (TileRef)tiles.brushTile : (TileRef)brush.cell)
        };

        public TileModel tiles;
        public PaletteModel palette;
        public BrushMode brush;
        public LineMode line;
        public FillMode fill;
        public PickMode pick;
        public EraseMode erase;
        public KeyboardMode keyboard;
        public MoveMode move;

        public Selection selection;
        public SelectRectMode selectRect;
        public SelectWandMode selectWand;
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
            line = new LineMode(this);
            fill = new FillMode(this);
            pick = new PickMode(this);
            erase = new EraseMode(this);
            keyboard = new KeyboardMode(this);
            selection = new Selection();
            selectRect = new SelectRectMode(this, selection);
            selectWand = new SelectWandMode(this, selection);
            pan = new PanMode(this);
            Undo = new LinkedList<Edit>();
            Redo = new LinkedList<Edit>();
            mode = Mode.Brush;

        }
        public void Save(Console renderer) {
            File.WriteAllText($"{filepath}", ASECIILoader.SerializeObject(this));

            var preview = sprite.preview;

            File.WriteAllText($"{filepath}.cg", JsonConvert.SerializeObject(preview, SFileMode.settings));


            StringBuilder str = new StringBuilder();
            for (int y = sprite.origin.Y; y <= sprite.end.Y; y++) {
                for (int x = sprite.origin.X; x <= sprite.end.X; x++) {
                    if (preview.TryGetValue((x, y), out var tile)) {
                        str.Append((char)tile.Glyph);
                    } else {
                        str.Append(' ');
                    }
                }
                str.AppendLine();
            }
            File.WriteAllText($"{filepath}.txt", str.ToString());

            var t = ((ScreenSurfaceRenderer)renderer.Renderer).BackingTexture;
            t.Save($"{filepath}.png");

            AddAction(new SaveEdit());
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

        public Mode GetEffectiveMode() {
            if(pan.quickPan) {
                return Mode.Pan;
            } else if(pick.quickPick) {
                return Mode.Pick;
            } else {
                return mode;
            }
        }
        public void ProcessKeyboard(Keyboard info) {
            ctrl = info.IsKeyDown(LeftControl);
            shift = info.IsKeyDown(LeftShift);
            alt = info.IsKeyDown(LeftAlt);

            if(info.IsKeyPressed(Z)) {
                if (ctrl) {
                    if (alt) {

                    } else {
                        if (shift) {
                            RedoLast();
                            return;
                        } else {
                            UndoLast();
                            return;
                        }
                    }
                }
            }

            if (mode == Mode.Keyboard) {
                if(info.IsKeyPressed(Escape)) {
                    mode = Mode.Brush;
                } else {
                    keyboard.ProcessKeyboard(info);
                }
            } else {
                if (info.IsKeyPressed(Escape)) {
                    switch (mode) {
                        case Mode.Read:
                            //Not supposed to happen since we exit
                            break;
                        case Mode.Brush:
                            SetMode(Mode.Read);
                            break;
                        case Mode.Line:
                            SetMode(Mode.Brush);
                            break;
                        case Mode.Fill:
                            SetMode(Mode.Brush);
                            break;
                        case Mode.Pick:
                            SetMode(Mode.Brush);
                            break;
                        case Mode.SelectWand:
                            SetMode(Mode.Brush);
                            break;
                        default:
                            SetMode(Mode.Brush);
                            break;
                    }
                } else if (info.IsKeyPressed(Space)) {
                    if (!pan.quickPan) {
                        pan.quickPan = true;
                        pan.startPan = cursor;
                    }
                } else if (info.IsKeyReleased(Space)) {
                    pan.quickPan = false;
                    camera -= pan.offsetPan;
                    pan.startPan = new Point(0, 0);
                    pan.offsetPan = new Point(0, 0);
                } else if (info.IsKeyPressed(LeftAlt)) {
                    if (mode == Mode.Brush) {
                        pick.quickPick = true;
                    }
                } else if (info.IsKeyReleased(LeftAlt)) {
                    pick.quickPick = false;
                } else if (info.IsKeyPressed(Back)) {
                    AddAction(new FillEdit(sprite.layers[currentLayer], selection.GetAll(), null));
                } else if (info.IsKeyPressed(B)) {
                    SetMode(Mode.Brush);
                } else if (info.IsKeyPressed(D)) {
                    selection.Clear();
                } else if (info.IsKeyPressed(E)) {
                    SetMode(Mode.Erase);
                } else if (info.IsKeyPressed(F)) {
                    SetMode(Mode.Fill);
                } else if (info.IsKeyPressed(I)) {
                    SetMode(Mode.Pick);
                } else if(info.IsKeyPressed(L)) {
                    SetMode(Mode.Line);
                } else if (info.IsKeyPressed(M)) {
                    SetMode(mode = Mode.Move);
                    if (selection.Exists) {
                        var moveLayer = Cut(selection.GetAll());
                        move = new MoveMode(this, selection, moveLayer);
                    } else {
                        move = new MoveMode(this, selection, sprite.layers[currentLayer]);
                    }
                } else if (info.IsKeyPressed(S) && !info.IsKeyDown(LeftControl)) {
                    SetMode(Mode.SelectRect);
                } else if (info.IsKeyPressed(T)) {
                    SetMode(Mode.Keyboard);
                    keyboard.keyCursor = null;
                    //keyboard.keyCursor = cursor;
                    //keyboard.margin = cursor;
                } else if (info.IsKeyPressed(W)) {
                    SetMode(Mode.SelectWand);
                }

                if (pan.quickPan) {
                    pan.ProcessKeyboard(info);
                }
            }
            void SetMode(Mode next) {
                if(mode == Mode.Move) {
                    //If we're moving an existing layer
                    move ??= new MoveMode(this, selection, sprite.layers[currentLayer]);
                    if(sprite.layers.Contains(move.layer)) {
                    } else {
                        //We're moving a selection between layers
                        //Flatten selection onto layer
                        var layer = sprite.layers[currentLayer];
                        layer.Flatten(move.layer);
                    }
                }

                mode = next;
            }
        }
        public Layer Cut(HashSet<Point> points) {
            Layer result = new Layer() { name = $"Layer {sprite.layers.Count}" };
            foreach(var point in points) {
                result[point] = sprite.layers[currentLayer][point];
                sprite.layers[currentLayer][point] = null;
            }
            return result;
        }
        public Layer SelectionToLayer() {
            var layer = Cut(selection.GetAll());
            if (currentLayer == sprite.layers.Count - 1) {
                sprite.layers.Add(layer);
                currentLayer++;
            } else {
                currentLayer++;
                sprite.layers.Insert(currentLayer, layer);
            };
            return layer;
        }
        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver) {
            cursorScreen = state.SurfaceCellPosition;
            cursor = cursorScreen + camera;
            
            switch (GetEffectiveMode()) {
                case Mode.Pan:
                    pan.ProcessMouse(state);
                    break;
                case Mode.Brush:
                    brush.ProcessMouse(state, IsMouseOver);
                    break;
                case Mode.Line:
                    line.ProcessMouse(state);
                    break;
                case Mode.Fill:
                    fill.ProcessMouse(state, IsMouseOver, shift, ctrl, alt);
                    break;
                case Mode.Pick:
                    pick.ProcessMouse(state, IsMouseOver);
                    break;
                case Mode.Erase:
                    erase.ProcessMouse(state, IsMouseOver);
                    break;
                case Mode.SelectRect:
                    selectRect.ProcessMouse(state, ctrl, shift);
                    break;
                case Mode.SelectWand:
                    selectWand = new SelectWandMode(this, selection);
                    selectWand?.ProcessMouse(state, ctrl, shift, alt);
                    break;
                case Mode.Move:
                    move.ProcessMouse(state);
                    break;
                case Mode.Keyboard:
                    keyboard.ProcessMouse(state);
                    break;
            }


            prevCell = cursor;
            prevLeft = state.Mouse.LeftButtonDown;
        }

        public void OnLoad() {
            line ??= new LineMode(this);
            fill ??= new FillMode(this);
        }
        public void RedoLast() {
            if (Redo.Any()) {
                var u = Redo.Last();
                Redo.RemoveLast();
                u.Do();
                Undo.AddLast(u);

                historyChanged?.Invoke();
            }
        }
        public void UndoLast() {
            if (Undo.Any()) {
                var u = Undo.Last();
                Undo.RemoveLast();
                u.Undo();
                Redo.AddLast(u);

                historyChanged?.Invoke();
            }
        }
        public void AddAction(Edit edit) {
            Undo.AddLast(edit);
            //Redo.Clear();
            edit.Do();

            historyChanged?.Invoke();
        }
    }
    public interface Edit {
        string Name { get; }
        void Undo();
        void Do();
    }

    public class SaveEdit : Edit {
        public string Name => "Save";
        public void Undo() { }
        public void Do() { }
    }
    public class SingleEdit : Edit {
        public string Name => "Brush";
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
    public class MultiEdit : Edit {
        public string Name => "Multi";
        public Layer layer;
        public Dictionary<(int, int), TileRef> prev;
        public Dictionary<(int, int), TileRef> next;
        public MultiEdit() {
            this.layer = null;
            this.prev = new Dictionary<(int, int), TileRef>();
            this.next = new Dictionary<(int, int), TileRef>();
        }
        public MultiEdit(Layer layer, Dictionary<(int, int), TileRef> next) {
            this.layer = layer;
            this.next = next;

            prev = new Dictionary<(int, int), TileRef>();
            foreach((var p, var t) in next) {
                prev[p] = layer[p];
            }
        }
        public MultiEdit(Layer layer) {
            this.layer = layer;
            this.next = new Dictionary<(int, int), TileRef>();
            this.prev = new Dictionary<(int, int), TileRef>();
        }
        public void Append(SingleEdit e) {
            if(!prev.ContainsKey(e.cursor)) {
                prev[e.cursor] = e.prev;
            }
            next[e.cursor] = e.next;
        }
        public void Undo() {
            foreach ((var p, var t) in prev) {
                layer[p] = t;
            }
        }
        public void Do() {
            foreach ((var p, var t) in next) {
                layer[p] = t;
            }
        }
    }
    public class FillEdit : Edit {
        public string Name => "Fill";
        public Layer layer;
        public Dictionary<(int, int), TileRef> prev;
        public TileRef next;

        public FillEdit(Layer layer, HashSet<Point> affected, TileRef next) {
            this.layer = layer;
            this.prev = new Dictionary<(int, int), TileRef>();
            foreach(var p in affected ?? new HashSet<Point>()) {
                this.prev[p] = layer[p];
            }

            if (next != null && next.Foreground.A == 0 && next.Background.A == 0) {
                next = null;
            }
            this.next = next;
        }
        public void Undo() {
            foreach((var point, var tile) in prev) {
                layer[point] = tile;
            }
        }
        public void Do() {
            foreach (var point in prev.Keys) {
                layer[point] = next;
            }
        }
    }

    public class MoveEdit {
        public string Name => "Move";

        Layer layer;
        Point offset;
        public MoveEdit(Layer layer, Point offset) {
            this.layer = layer;
            this.offset = offset;
        }

        public void Undo() => layer.pos -= offset;
        public void Do() => layer.pos += offset;
    }
    public class ExitMoveSelection : Edit {
        public string Name => "Exit Move";

        SpriteModel model;
        Flatten flatten;
        Mode next;
        public ExitMoveSelection(SpriteModel model, Layer selectionLayer, Mode next) {
            this.model = model;
            this.flatten = new Flatten(selectionLayer, model.sprite.layers[model.currentLayer]);
            this.next = next;
        }
        public void Do() {
            this.flatten.Do();
            model.mode = next;
        }
        public void Undo() {
            model.mode = Mode.Move;
            model.move.layer = flatten.layer;
            flatten.Undo();
        }
    }
    //We don't consider this an individual edit since it's usually part of a larger action with more side effects
    //i.e. ExitMoveSelection and flatten layer button
    public class Flatten {

        public Layer layer;
        public Layer below;

        Dictionary<(int, int), TileRef> oldCells;
        public Flatten(Layer layer, Layer below) {
            this.layer = layer;
            this.below = below;

            oldCells = new Dictionary<(int, int), TileRef>();
            foreach (var p in layer.cells.Keys) {
                oldCells[p + layer.pos] = below[p + layer.pos];
            }
        }
        //To do: Test with both layers moved
        public void Undo() {
            foreach ((var p, var t) in oldCells) {
                below[p] = t;
            }
        }
        public void Do() {
            below.Flatten(layer);
        }
    }

    public class PanMode {
        public SpriteModel model;
        public bool quickPan;
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

        MultiEdit placement;
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

            if(mouse.left == ClickState.Released) {
                placement = null;
            }
        }
        void Place(Point p) {
            var layer = model.sprite.layers[model.currentLayer];
            var e = new SingleEdit(p, layer, model.brushTile);

            if (e.IsRedundant()) {
                return;
            }
            //Store all of our placements in a compound action
            if (model.Undo.Last() == placement) {
                placement.Append(e);
                e.Do();
            } else {
                placement = new MultiEdit(layer);
                placement.Append(e);
                model.AddAction(placement);
            }
        }
    }
    [JsonObject(MemberSerialization.Fields)]
    public class FillMode {
        public SpriteModel model;
        public MouseWatch mouse;
        public FillMode(SpriteModel model) {
            this.model = model;
            mouse = new MouseWatch();
        }
        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver, bool shift, bool ctrl, bool alt) {
            mouse.Update(state, IsMouseOver);
            if (state.IsOnScreenObject) {
                if (mouse.leftPressedOnScreen && mouse.left == ClickState.Released) {

                    var layer = model.sprite.layers[model.currentLayer];
                    var start = model.cursor;
                    var t = layer[start];
                    var source = Layer.Triple(t);

                    HashSet<Point> affected = new HashSet<Point>();
                    if (ctrl) {
                        affected = layer.GetGrowFill(start, Layer.Triple(model.brushTile), model.sprite.origin, model.sprite.end);
                    } else if ((model.brush.foreground, model.brush.background, model.brush.glyph) != source) {
                        
                        if (shift) {
                            affected = layer.GetOutlineFill(start, source, model.sprite.origin, model.sprite.end);
                        } else if (alt) {
                            affected = layer.GetGlobalFill(source, model.sprite.origin, model.sprite.end);
                        } else {
                            affected = layer.GetFloodFill(start, source, model.sprite.origin, model.sprite.end);
                        }
                    }

                    if (affected.Any()) {
                        model.AddAction(new FillEdit(layer, affected, model.brushTile));
                    }
                }
            }
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class LineMode {
        public SpriteModel model;
        public Layer layer;

        public Point? start;
        public Point end;

        public HashSet<Point> GetPoints() {
            var start = this.start.Value;
            var offset = end - start;
            double distance = offset.Length();
            (double dx, double dy) = (offset.X / distance, offset.Y / distance);
            (double x, double y) = (start.X, start.Y);

            HashSet<Point> points = new HashSet<Point>();
            for (int i = 0; i < distance; i++) {
                points.Add(((int)Math.Round(x), (int)Math.Round(y)));
                (x, y) = (x + dx, y + dy);
            }
            return points;
        }
        //public Point moved => start.HasValue ? end - start.Value : new Point(0, 0);
        public MouseWatch mouse;
        public LineMode(SpriteModel model) {
            this.model = model;
            mouse = new MouseWatch();
        }
        public void ProcessMouse(MouseScreenObjectState state) {
            mouse.Update(state, state.IsOnScreenObject);
            if (mouse.left == ClickState.Pressed) {
                if (mouse.leftPressedOnScreen) {
                    //Start moving with the mouse
                    start = model.cursor;
                }
            } else if(mouse.left == ClickState.Held) {
                end = model.cursor;
            } else if (mouse.prevLeft) {
                end = model.cursor;
                if(start == end) {
                    model.AddAction(new SingleEdit(model.cursor, model.sprite.layers[model.currentLayer], model.brushTile));
                } else if(start != null) {
                    Dictionary<(int, int), TileRef> affected = new Dictionary<(int, int), TileRef>();
                    
                    var cell = model.brushTile;

                    var points = GetPoints();
                    if (model.selection.Exists) {
                        points.IntersectWith(model.selection.GetAll());
                    }
                        
                    foreach (var p in points) affected[p] = cell;
                    model.AddAction(new MultiEdit(model.sprite.layers[model.currentLayer], affected));
                }
                start = null;
            }
        }
    }


    [JsonObject(MemberSerialization.Fields)]
    public class PickMode {
        public bool quickPick;
        public SpriteModel model;
        public MouseWatch mouse;

        [JsonIgnore]
        public Action brushChanged;
        public PickMode(SpriteModel model) {
            this.model = model;
            mouse = new MouseWatch();
        }

        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver) {
            mouse.Update(state, IsMouseOver);
            if (state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown && mouse.leftPressedOnScreen) {

                    TileRef t = null;
                    var layer = model.sprite.layers[model.currentLayer];
                    t = layer[model.cursor];

                    foreach(var l in model.sprite.layers) {
                        var lt = l[model.cursor];
                        if (lt != null) {
                            t = lt; 
                        }
                    }
                    if(t == null) {

                    } else if(t is TileIndex ti) {
                        model.tiles.brushIndex = ti.index;
                    } else if(t is TilePalette tp) {
                        model.palette.foregroundIndex = tp.foregroundIndex;
                        model.palette.backgroundIndex = tp.backgroundIndex;
                    }
                    model.brush.foreground = t?.Foreground ?? Color.Transparent;
                    model.brush.background = t?.Background ?? Color.Transparent;
                    model.brush.glyph = t?.Glyph ?? 0;

                    brushChanged?.Invoke();
                }
            }
        }
    }
    [JsonObject(MemberSerialization.Fields)]
    public class EraseMode {
        public SpriteModel model;
        public MouseWatch mouse;
        public EraseMode(SpriteModel model) {
            this.model = model;
            mouse = new MouseWatch();
        }

        public void ProcessMouse(MouseScreenObjectState state, bool IsMouseOver) {
            mouse.Update(state, IsMouseOver);
            if (state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown && mouse.leftPressedOnScreen) {
                    var prev = model.prevCell;
                    var offset = (model.cursor - prev);
                    var length = offset.Length();

                    var (xNorm, yNorm) = (offset.X / length, offset.Y / length);
                    for (int i = 0; i < length; i++) {

                        var p = prev + new Point((int)(i * xNorm), (int)(i * yNorm));
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
            SingleEdit action = new SingleEdit(p, layer, null);
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
            if (info.KeysPressed.Select(p => p.Key).Intersect(new Keys[] { Keys.Up, Keys.Left, Keys.Right, Keys.Down }).Any()) {
                pressed = info.KeysDown.Where(k => k.Character != 0);
            }
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

            } else if (info.IsKeyDown(Back)) {
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
        public Layer layer;

        public Point? start;
        public Point? current;
        public Point end;
        //public Point moved => start.HasValue ? end - start.Value : new Point(0, 0);
        public MouseWatch mouse;
        public MoveMode(SpriteModel model, Selection selection, Layer layer) {
            this.model = model;
            this.selection = selection;
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
        public bool Exists => rects.Any() || points.Any();

        [JsonIgnore]
        public Action selectionChanged;
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
        public void UsePointsOnly() {
            points = GetAll();
            rects.Clear();
        }
        public void Offset(Point offset) {
            rects = new HashSet<Rectangle>(rects.Select(r => new Rectangle(r.X + offset.X, r.Y + offset.Y, r.Width, r.Height)));
            points = new HashSet<Point>(points.Select(p => p + offset));
        }
        public void Clear() {
            points.Clear();
            rects.Clear();
            selectionChanged?.Invoke();
        }

    }
    //Add a menu to set and use pre-selected regions for editing
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
        public void ProcessMouse(MouseScreenObjectState state, bool ctrl, bool shift) {
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

                        if(!ctrl && !shift) {
                            selection.Clear();
                        }
                    }
                } else if (prevLeft) {
                    if (start != end) {
                        if(shift) {
                            selection.UsePointsOnly();
                            selection.points.ExceptWith(rect.Value.Positions());
                        } else {
                            selection.rects.Add(rect.Value);
                        }

                        selection.selectionChanged?.Invoke();
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
    public class SelectWandMode {
        public SpriteModel model;
        public Selection selection;
        public MouseWatch mouse;
        bool prevLeft;

        public SelectWandMode(SpriteModel model, Selection selection) {
            this.model = model;
            this.selection = selection;
        }
        public void ProcessMouse(MouseScreenObjectState state, bool ctrl, bool shift, bool alt) {
            if (state.IsOnScreenObject) {
                if (state.Mouse.LeftButtonDown) {
                    var layer = model.sprite.layers[model.currentLayer];

                    var start = model.cursor;
                    var t = layer[start];
                    var source = t != null ? (t.Foreground, t.Background, t.Glyph) : (Color.Transparent, Color.Transparent, 0);

                    HashSet<Point> affected = new HashSet<Point>();
                    if (alt) {
                        affected = layer.GetGlobalFill(source, model.sprite.origin, model.sprite.end);
                    } else {
                        affected = layer.GetFloodFill(start, source, model.sprite.origin, model.sprite.end);
                    }
                    if (affected.Any()) {
                        if(ctrl) {
                            selection.points.UnionWith(affected);
                        } else if(shift) {
                            selection.UsePointsOnly();
                            selection.points.ExceptWith(affected);
                        } else {
                            selection.Clear();
                            selection.points.UnionWith(affected);
                        }
                    }
                }
                prevLeft = state.Mouse.LeftButtonDown;
            }
        }
    }
}
