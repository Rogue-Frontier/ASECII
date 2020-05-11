using SadConsole;
using SadConsole.Input;
using System;
using SadRogue.Primitives;

namespace ASECII {
    class CellButton : SadConsole.Console {
        public delegate bool Active();
        public delegate void Click();
        Active active;
        bool isActive;
        Click click;
        MouseWatch mouse;
        public CellButton(Active active, Click click) : base(1, 1) {
            this.active = active;
            this.click = click;
            this.mouse = new MouseWatch();
        }
        public void UpdateActive() {
            isActive = active();
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            mouse.Update(state, IsMouseOver);
            if(isActive && IsMouseOver) {
                if(mouse.leftPressedOnScreen && mouse.left == MouseState.Released) {
                    click();
                }

            }
            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            if(IsMouseOver && mouse.nowLeft && mouse.leftPressedOnScreen) {
                this.Print(0, 0, "+", Color.White, Color.Black);
            } else if(isActive) {
                this.Print(0, 0, "+", Color.Black, IsMouseOver ? Color.Yellow : Color.White);
            } else {
                this.Print(0, 0, " ", Color.Transparent, Color.White);
            }
            

            base.Draw(timeElapsed);
        }
    }
}
