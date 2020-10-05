using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Text;
using SadRogue.Primitives;
using Console = SadConsole.Console;
using ArchConsole;

namespace ASECII {
    class LayerMenu : Console {
        SpriteModel model;
        public LayerMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
        }
        public void UpdateListing() {
            this.Children.Clear();
            int i = 0;
            var layers = model.sprite.layers;
            foreach (var l in layers) {
                int index = i;

                this.Children.Add(new ColorCellButton(() => l.visible ? Color.White : Color.Black,
                    () => {
                        l.visible = !l.visible;
                    }, 'v') {
                    Position = new Point(0, index)
                });

                this.Children.Add(new CellButton(() => index < layers.Count - 1,
                    () => {
                        layers.RemoveAt(index);
                        layers.Insert(index + 1, l);

                        if(model.currentLayer == index) {
                            model.currentLayer++;
                        }

                        UpdateListing();
                    }, '-') {
                    Position = new Point(2, index)
                });

                this.Children.Add(new CellButton(() => index > 0,
                        () => {
                            if (model.currentLayer == index) {
                                model.currentLayer--;
                            }

                            layers.RemoveAt(index);
                            layers.Insert(index - 1, l);
                            UpdateListing();
                        }, '+') {
                    Position = new Point(3, index)
                });

                this.Children.Add(new ColorButton(l.name,
                    () => model.currentLayer == index ? Color.Yellow : Color.White,
                    () => model.currentLayer = index) {
                    Position = new Point(5, index)
                });

                this.Children.Add(new CellButton(() => index > 0,
                    () => {
                        var below = layers[index - 1];

                        //Flatten 
                        foreach ((var p, var t) in l.cells) {
                            below[p] = t;
                        }

                        layers.RemoveAt(index);

                        if (model.currentLayer == index) {
                            model.currentLayer--;
                        }

                        UpdateListing();
                    }, 'F') {
                    Position = new Point(13, index)
                });

                this.Children.Add(new CellButton(() => model.sprite.layers.Count > 1,
                        () => {
                            layers.RemoveAt(index);

                            if (model.currentLayer == index) {
                                model.currentLayer--;
                            }

                            UpdateListing();
                        }, 'X') {
                    Position = new Point(15, index)
                });
                i++;
            }
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            return base.ProcessMouse(state);
        }
    }
}
