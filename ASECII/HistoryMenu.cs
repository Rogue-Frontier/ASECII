
using ArchConsole;
using Microsoft.Xna.Framework.Media;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = SadConsole.Console;

namespace ASECII {
    class HistoryMenu : Console {
        SpriteModel model;
        List<LabelButton> buttons;
        int startIndex;
        public HistoryMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
            buttons = new List<LabelButton>();
            CellButton up = null, down = null;
            up = new CellButton(() => startIndex > 0, Up, '-') { Position = new Point(15, 0) };
            down = new CellButton(() => startIndex < model.Undo.Count + model.Redo.Count - Height, Down, '+') { Position = new Point(15, Height - 1) };
            this.Children.Add(up);
            this.Children.Add(down);

            void Up() {
                startIndex -= Height / 2;
                ClampIndex();

                up.UpdateActive();
                down.UpdateActive();

                UpdateListing();
            }
            void Down() {
                startIndex += Height / 2;
                ClampIndex();

                up.UpdateActive();
                down.UpdateActive();

                UpdateListing();
            }
        }


        public void ClampIndex() {
            startIndex = Math.Clamp(startIndex, 0, model.Undo.Count + model.Redo.Count - Height);
        }
        public void SnapIndex() {
            startIndex = Math.Max(0, model.Undo.Count - Math.Max(0, Height - model.Redo.Count));

            //Math.Max(0, model.Undo.Count);
            //Math.Max(0, model.Undo.Count - Height + model.Redo.Count);
        }
        public void UpdateListing() {
            buttons.ForEach(this.Children.Remove);
            buttons.Clear();

            int y = 0;
            foreach(var e in model.Undo.Skip(startIndex)) {
                var b = new LabelButton($"<{e.Name}", () => UndoTo(e)) { Position = new Point(0, y) };
                this.Children.Add(b);
                buttons.Add(b);
                y++;
            }

            foreach (var e in model.Redo.Reverse().Skip(startIndex - model.Undo.Count())) {
                var b = new LabelButton($">{e.Name}", () => RedoTo(e)) { Position = new Point(0, y) };
                this.Children.Add(b);
                buttons.Add(b);
                y++;
            }
        }
        public void UndoTo(Edit e) {
            Edit current = null;
            do {
                current = model.Undo.Last();
                model.Undo.RemoveLast();
                current.Undo();
                model.Redo.AddLast(current);
            } while (current != e);

            UpdateListing();
        }
        public void RedoTo(Edit e) {
            Edit current = null;
            do {
                current = model.Redo.Last();
                model.Redo.RemoveLast();
                current.Do();
                model.Undo.AddLast(current);
            } while (current != e);

            UpdateListing();
        }
        public override bool ProcessMouse(MouseScreenObjectState state) {
            startIndex += state.Mouse.ScrollWheelValueChange / 60;
            ClampIndex();
            UpdateListing();
            return base.ProcessMouse(state);
        }
        public override void Render(TimeSpan delta) {
            this.Clear();
            var mid = model.Undo.Skip(startIndex).Count();
            if(mid > 0 && mid < Height) {
                this.Print(0, mid, new string('-', 16));
            } else if(mid <= 0) {
                this.Print(0, 0, new string('^', 16));
            } else if(mid >= Height) {
                this.Print(0, Height-1, new string('v', 16));
            }

            var count = (model.Undo.Count + model.Redo.Count);

            if(count > Height) {
                var barSize = Height * Height / count;
                var y = Height * startIndex / count;

                for(int i = 0; i < barSize; i++) {
                    this.Print(Width - 1, y + i, new ColoredGlyph(Color.White, Color.Black, '#'));
                }
            }
            

            base.Render(delta);
        }
    }
}
