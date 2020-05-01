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
        TileModel tileModel;
        Action brushChanged;
        bool prevLeft;
        bool prevRight;
        public TileMenu(int width, int height, Font font, SpriteModel spriteModel, TileModel tileModel, Action brushChanged) : base(width, height, font) {
            this.spriteModel = spriteModel;
            this.tileModel = tileModel;
            this.brushChanged = brushChanged;
        }
        public override bool ProcessMouse(MouseConsoleState state) {
            if (state.IsOnConsole) {
                int index = (state.ConsoleCellPosition.X) + (state.ConsoleCellPosition.Y * Width);
                if (index > -1 && index < tileModel.tiles.Count) {
                    bool pressLeft = !prevLeft && state.Mouse.LeftButtonDown;
                    if (pressLeft) {
                        if (spriteModel.brush.cell != tileModel.tiles[index]) {
                            spriteModel.brush.cell = tileModel.tiles[index];
                        }
                        brushChanged?.Invoke();
                    }
                }
            }
            prevLeft = state.Mouse.LeftButtonDown;

            return base.ProcessMouse(state);
        }
        public override void Draw(TimeSpan timeElapsed) {
            for (int i = 0; i < tileModel.tiles.Count; i++) {
                SetCellAppearance(i % Width, i / Width, tileModel.tiles[i]);
            }
            if (tileModel.brushIndex != null) {
                var i = tileModel.brushIndex.Value;
                int x = i % Width;
                int y = i / Width;

                var t = tileModel.tiles[i];
                SetCellAppearance(x, y, new Cell(t.Background, t.Foreground, t.Glyph));
            }

            base.Draw(timeElapsed);
        }
    }
    class TileModel {
        public List<ColoredGlyph> tiles = new List<ColoredGlyph>();
        public HashSet<ColoredGlyph> tileset = new HashSet<ColoredGlyph>();
        public int? brushIndex;

        public void AddTile(ColoredGlyph c) {
            tiles.Add(c);
            tileset.Add(c);
        }
        public void UpdateIndexes(SpriteModel spriteModel) {
            brushIndex = tiles.IndexOf(spriteModel.brush.cell);
            if (brushIndex == -1) {
                brushIndex = null;
            }
        }
    }
}
