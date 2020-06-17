using SadRogue.Primitives;
using Newtonsoft.Json;

namespace ASECII {
    internal class Program {
        static int width = 108;
        static int height = 90;
        private static void Main(string[] args) {
            var s = JsonConvert.SerializeObject(new TileValue(new Color(1, 2, 3, 4), new Color(5, 6, 7, 8), 9), SFileMode.settings);
            var s2 = JsonConvert.DeserializeObject<TileValue>(s);

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