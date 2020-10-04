using ArchConsole;
using SadConsole;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Text;

using SadRogue.Primitives;
namespace ASECII {
    public class ColorButton : SadConsole.Console {
        public string text {
            set {
                _text = value;
                Resize(_text.Length, 1, _text.Length, 1, false);
            }
            get { return _text; }
        }
        private string _text;
        public Action click;
        MouseWatch mouse;

        public Func<Color> color;
        public Func<bool> Active;
        public bool IsActive;

        public ColorButton(string text, Action click) : base(1, 1) {
            this.text = text;
            this.click = click;
            this.mouse = new MouseWatch();
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            mouse.Update(state, IsMouseOver);
            if (IsActive) {
                if (IsMouseOver) {
                    if (mouse.leftPressedOnScreen && mouse.left == ClickState.Released) {
                        click();
                    }
                }
            }
            return base.ProcessMouse(state);
        }
        public override void Render(TimeSpan timeElapsed) {
            var b = color();
            var f = b.GetTextColor();
            if (IsMouseOver && mouse.nowLeft && mouse.leftPressedOnScreen) {
                this.Print(0, 0, text, b, f);
            } else {
                this.Print(0, 0, text, f, b);
            }


            base.Render(timeElapsed);
        }
    }
}
