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
            foreach(var l in model.sprite.layers) {
                int index = i;

                this.Children.Add(new ColorCellButton(() => l.visible ? Color.White : Color.Black,
                    () => {
                        l.visible = !l.visible;
                    }, 'V') {
                    Position = new Point(0, i)
                }) ;

                this.Children.Add(new ColorButton($"Layer {i}",
                    () => model.currentLayer == index ? Color.Yellow : Color.White,
                    () => model.currentLayer = index) {
                Position = new Point(2, i)
                });
                i++;
            }
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            return base.ProcessMouse(state);
        }
    }
}
