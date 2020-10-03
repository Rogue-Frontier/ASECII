using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Text;
using SadRogue.Primitives;
using Console = SadConsole.Console;

namespace ASECII {
    class LayerMenu : Console {

        SpriteModel model;
        public LayerMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
        }
        public override void Render(TimeSpan timeElapsed) {
            this.Clear();
            for(int i = 0; i < model.sprite.layers.Count; i++) {
                var f = Color.White;
                if(i == model.currentLayer) {
                    f = Color.Yellow;
                }
                this.Print(0, i, $"Layer {i}", f, Color.Black);
            }
            base.Render(timeElapsed);
        }
    }
}
