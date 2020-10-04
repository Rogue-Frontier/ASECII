using SadRogue.Primitives;
using Newtonsoft.Json;

namespace ASECII {
    internal class Program {
        static int width = 160;
        static int height = 90;
        private static void Main(string[] args) {
            SadConsole.UI.Themes.Library.Default.Colors.ControlHostBack = Color.Black;
            SadConsole.UI.Themes.Library.Default.Colors.ControlBack = Color.Gray;

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
            var firstConsole = new FileMenu(width, height, new LoadMode());

            SadConsole.Game.Instance.Screen = firstConsole;
            firstConsole.FocusOnMouseClick = true;
        }
    }
}