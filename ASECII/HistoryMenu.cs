
using ArchConsole;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Linq;
using Console = SadConsole.Console;

namespace ASECII {
    class HistoryMenu : Console {
        SpriteModel model;
        public HistoryMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
        }

        public void UpdateListing() {

            int startIndex = Math.Max(0, model.Undo.Count - Math.Max(0, Width - model.Redo.Count));
            int y = 0;
            foreach(var e in model.Undo.Skip(startIndex)) {
                this.Children.Add(new LabelButton($"<{e.Name}", () => {
                    Edit current = null;
                    do {
                        current = model.Undo.Last();
                        model.Undo.RemoveLast();
                        current.Undo();
                        model.Redo.AddLast(current);
                    } while (current != e);

                    UpdateListing();
                }) { Position = new Point(0, y) });
                y++;
            }
            foreach (var e in model.Redo.Reverse()) {
                this.Children.Add(new LabelButton($">{e.Name}", () => {
                    Edit current = null;
                    do {
                        current = model.Redo.Last();
                        model.Redo.RemoveLast();
                        current.Do();
                        model.Undo.AddLast(current);
                    } while (current != e);

                    UpdateListing();
                }) { Position = new Point(0, y) });
                y++;
            }
        }
    }
}
