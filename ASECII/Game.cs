using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using System;
using Microsoft.Xna.Framework;
using SadConsole.Input;
using SadRogue.Primitives;
using Console = SadConsole.Console;

namespace ASECII {
    internal class Program {
        static int width = 64;
        static int height = 64;
        private static void Main(string[] args) {
            //SadConsole.Settings.UnlimitedFPS = true;
            SadConsole.Settings.UseDefaultExtendedFont = true;
            SadConsole.Game.Create(width, height, "Content/IBMCGA.font", g => {
            });
            SadConsole.Game.Instance.OnStart = Init;
            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
        }

        private static void Init() {
            // Create your console
            var firstConsole = new EditorMain(width, height);

            SadConsole.Global.Screen = firstConsole;
            firstConsole.FocusOnMouseClick = true;
        }
    }
}