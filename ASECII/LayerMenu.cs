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
        public int mouseIndex;
        SpriteModel model;
        MouseWatch mouse;
        public LayerMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
        }
        public void UpdateListing() {

        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            mouse = mouse ?? new MouseWatch();
            mouse.Update(state, IsMouseOver);
            if(state.IsOnScreenObject) {
                switch(mouse.left) {
                    case ClickState.Up: {
                            if (state.SurfaceCellPosition.Y < model.sprite.layers.Count) {
                                mouseIndex = state.SurfaceCellPosition.Y;
                            }
                            break;
                        }
                    case ClickState.Released:
                        if (mouseIndex > -1 && mouseIndex < model.sprite.layers.Count) {
                            model.currentLayer = mouseIndex;
                        }
                        break;
                }
            } else {
                mouseIndex = -1;
            }
            return base.ProcessMouse(state);
        }
        public override void Render(TimeSpan timeElapsed) {
            this.Clear();
            for(int i = 0; i < model.sprite.layers.Count; i++) {
                var f = Color.Gray;

                bool mouseOver = mouseIndex == i;
                bool current = i == model.currentLayer;
                if(mouseOver && current) {
                    f = Color.LightYellow;
                } else if(current) {
                    f = Color.Yellow;
                } else if(mouseOver) {
                    f = Color.White;
                }
                var b = Color.Black;
                this.Print(0, i, $"Layer {i}", f, b);
            }
            base.Render(timeElapsed);
        }
    }
}
