using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASECII {
    class GlyphMenu : SadConsole.Console {
        SpriteModel spriteModel;
        bool prevLeft;

        public GlyphMenu(int width, int height, SpriteModel spriteModel) : base(width, height) {
            this.spriteModel = spriteModel;
        }

        public override bool ProcessMouse(MouseScreenObjectState state) {
            if (state.IsOnScreenObject) {
                int index = (state.SurfaceCellPosition.X) + (state.SurfaceCellPosition.Y * Width);
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

            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for (int i = 0; i < 256; i++) {
                string s = ((char)i).ToString();
                this.Print(i % Width, i / Width, s, spriteModel.brush.foreground, spriteModel.brush.background);
            }
            var x = spriteModel.brush.glyph % Width;
            var y = spriteModel.brush.glyph / Width;
            var c = this.GetCellAppearance(x, y);
            this.SetCellAppearance(x, y, new ColoredGlyph(c.Background, c.Foreground, c.Glyph));


            base.Draw(timeElapsed);
        }
    }
}
