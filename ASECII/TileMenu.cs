using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASECII {
    class TileMenu : SadConsole.Console {
        SpriteModel spriteModel;
        bool prevLeft;
        bool prevRight;
        public TileMenu(int width, int height, Font font, SpriteModel spriteModel) : base(width, height, font) {
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
}
