using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Input;
using System;

namespace ASECII {
    class CellButton : SadConsole.Console {
        public delegate bool Active();
        public delegate void Click();
        Active active;
        bool isActive;
        Click click;
        bool prevLeft;
        bool pressed;
        public CellButton(Font font, Active active, Click click) : base(1, 1, font) {
            this.active = active;
            this.click = click;
        }
        public void UpdateActive() {
            isActive = active();
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            if(isActive && IsMouseOver) {
                if(state.Mouse.LeftButtonDown) {
                    if(!prevLeft) {
                        pressed = true;
                    }
                } else if (pressed && !state.Mouse.LeftButtonDown) {
                    pressed = false;
                    click();
                }

            }
            prevLeft = state.Mouse.LeftButtonDown;
            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            if(pressed) {
                Print(0, 0, "+", Color.White, Color.Black);
            } else if(isActive) {
                Print(0, 0, "+", Color.Black, IsMouseOver ? Color.Yellow : Color.White);
            } else {
                Print(0, 0, " ", Color.Transparent, Color.White);
            }
            

            base.Draw(timeElapsed);
        }
    }
}
