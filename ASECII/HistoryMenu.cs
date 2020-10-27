﻿
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
    public class HistoryMenu : Console {
        SpriteModel model;
        List<LabelButton> buttons;
        ScrollVertical scroll;
        MouseWatch mouse;
        int historyLength => model.Undo.Count + model.Redo.Count;
        public HistoryMenu(int width, int height, SpriteModel model) : base(width, height) {
            this.model = model;
            buttons = new List<LabelButton>();
            scroll = new ScrollVertical(height, historyLength, UpdateListing) { Position = new Point(width - 1, 0) };
            this.Children.Add(scroll);
            mouse = new MouseWatch();
        }


        public void SnapIndex() {
            scroll.index = Math.Max(0, model.Undo.Count - Math.Max(0, Height - model.Redo.Count));

            //Math.Max(0, model.Undo.Count);
            //Math.Max(0, model.Undo.Count - Height + model.Redo.Count);
        }
        public void HistoryChanged() {
            scroll.range = historyLength;
            UpdateListing();
        }
        public void UpdateListing() {
            buttons.ForEach(this.Children.Remove);
            buttons.Clear();

            int y = 0;
            foreach(var e in model.Undo.Skip(scroll.index)) {
                var b = new LabelButton($"<{e.Name}", () => UndoTo(e)) { Position = new Point(0, y) };
                this.Children.Add(b);
                buttons.Add(b);
                y++;
            }

            foreach (var e in model.Redo.Reverse().Skip(scroll.index - model.Undo.Count())) {
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
            scroll.index += state.Mouse.ScrollWheelValueChange / 60;
            UpdateListing();

            mouse.Update(state, IsMouseOver);
            if (mouse.left == ClickState.Held && mouse.leftPressedOnScreen) {
                var deltaY = mouse.prevPos.Y - mouse.nowPos.Y;
                if (deltaY != 0) {
                    scroll.index += deltaY;
                }
            }

            return base.ProcessMouse(state);
        }
        public override void Render(TimeSpan delta) {
            this.Clear();
            var mid = model.Undo.Skip(scroll.index).Count();
            if(mid > 0 && mid < Height) {
                this.Print(0, mid, new string('-', 16));
            } else if(mid <= 0) {
                this.Print(0, 0, new string('^', 16));
            } else if(mid >= Height) {
                this.Print(0, Height-1, new string('v', 16));
            }
            base.Render(delta);
        }
    }
}
