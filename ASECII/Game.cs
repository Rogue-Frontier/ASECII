using Point = Microsoft.Xna.Framework.Point;
using Color = Microsoft.Xna.Framework.Color;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Themes;
using System;
using Microsoft.Xna.Framework;

namespace ASECII {
	public class Game : SadConsole.Game {
		/*
		public static readonly int screenwidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
		public static readonly int screenheight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

		public static readonly int width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 16;
		public static readonly int height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 16;
		*/
		public static readonly int width = 64;
		public static readonly int height = 64;
		public Game() : base("Content/Square.font", width, height, null) {
			Content.RootDirectory = "Content";
		}

		protected override void Initialize() {

			// Generally you don't want to hide the mouse from the user
			IsMouseVisible = true;

			// Finish the initialization of SadConsole before you start your game init
			base.Initialize();

			// Create your console
			var firstConsole = new EditorMain(width, height, SadConsole.Global.FontDefault.Master.GetFont(Font.FontSizes.One));


			SadConsole.Global.CurrentScreen = firstConsole;
			firstConsole.FocusOnMouseClick = true;
		}
	}
}