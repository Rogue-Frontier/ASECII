using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASECII {
    class MouseWatch {
        enum ButtonStates {
            PrevUp = 1,
            PrevDown = 2,
            NowUp = 4,
            NowDown = 8
        }
        enum MouseState {
            Up = ButtonStates.PrevUp & ButtonStates.NowUp,
            Pressed = ButtonStates.PrevUp & ButtonStates.NowDown,
            Held = ButtonStates.PrevDown & ButtonStates.NowDown,
            Released = ButtonStates.PrevDown & ButtonStates.NowUp;
        }
        MouseState left;
        MouseState right;
        public bool leftPressedOnScreen;
        public bool rightPressedOnScreen;
        public Point leftPressedPos;
        public Point rightPressedPos;
        public bool prevLeft;
        public bool prevRight;
        public bool nowLeft;
        public bool nowRight;
        public void Update(MouseScreenObjectState state) {
            prevLeft = nowLeft;
            prevRight = nowRight;
            nowLeft = state.Mouse.LeftButtonDown;
            nowRight = state.Mouse.RightButtonDown;
            left = !prevLeft ? (!nowLeft ? MouseState.Up : MouseState.Pressed) : (nowLeft ? MouseState.Held : MouseState.Released);
            right = !prevRight ? (!nowRight ? MouseState.Up : MouseState.Pressed) : (nowRight ? MouseState.Held : MouseState.Released);
            if(left == MouseState.Pressed) {
                leftPressedOnScreen = state.IsOnScreenObject;
                leftPressedPos = state.SurfaceCellPosition;
            }
            if(right == MouseState.Pressed) {
                rightPressedOnScreen = state.IsOnScreenObject;
                rightPressedPos = state.SurfaceCellPosition;
            }
        }
    }
}
