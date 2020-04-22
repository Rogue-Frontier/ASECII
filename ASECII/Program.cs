using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASECII {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			Settings.ResizeMode = Settings.WindowResizeOptions.Scale;
			using (var game = new Game())
				game.Run();
		}
	}
}
