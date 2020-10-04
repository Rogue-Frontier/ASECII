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
                    }, 'V') {
                    Position = new Point(0, index)
                }) ;

                if (index < layers.Count - 1) {
                    this.Children.Add(new ColorCellButton(() => Color.White,
                        () => {
                            layers.RemoveAt(index);
                            layers.Insert(index + 1, l);
                            UpdateListing();
                        }, '-') {
                        Position = new Point(10, index)
                    });
                }

                if (index > 0) {
                    this.Children.Add(new ColorCellButton(() => Color.White,
                        () => {
                            layers.RemoveAt(index);
                            layers.Insert(index - 1, l);
                            UpdateListing();
                        }, '+') {
                        Position = new Point(11, index)
                    });
                }

                this.Children.Add(new ColorButton($"Layer {index}",
                    () => model.currentLayer == index ? Color.Yellow : Color.White,
                    () => model.currentLayer = index) {
                Position = new Point(2, index)
                });
                i++;
            }
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            return base.ProcessMouse(state);
        }
    }
}
