using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SadConsole.Input.Keys;

namespace ASECII {
    class SaveMenu : ControlsConsole {
        List<Button> listing;
        TextBox textbox;
        SpriteModel sprite;
        public SaveMenu(int width, int height, SpriteModel sprite) : base(width, height) {
            UseMouse = true;
            UseKeyboard = true;
            IsFocused = true;
            listing = new List<Button>();
            textbox = new TextBox(8) { Position = new Point(1, 1),
                IsCaretVisible = true,
            FocusOnClick = true,
            UseKeyboard = true,
            UseMouse = true,
            IsFocused = true,
            CanFocus = true,
            };
            /*
            textbox.TextChanged += (e, args) => {
                UpdateListing(textbox.Text);
            };
            */
            Add(textbox);
            UpdateListing("..");
            this.sprite = sprite;
        }
        public void UpdateListing(string filepath) {
            listing.ForEach(b => Remove(b));
            listing.Clear();
            int i = 0;

            if (Directory.Exists(filepath)) {

                i++;
                var b = new Button(this.Width - 2, 1) {
                    Position = new Point(0, i),
                    Text = "..",
                };
                b.Click += (e, args) => {
                    textbox.Text = Directory.GetParent(filepath).FullName;
                };
                listing.Add(b);

                ShowDirectories(Directory.GetDirectories(filepath).Where(p => p.StartsWith(filepath)));
                ShowFiles(Directory.GetFiles(filepath).Where(p => p.StartsWith(filepath)));
            } else {
                var parent = Directory.GetParent(filepath).FullName;
                if (Directory.Exists(parent)) {
                    i++;
                    var b = new Button(this.Width - 2, 1) {
                        Position = new Point(0, i),
                        Text = "..",
                    };
                    b.Click += (e, args) => {
                        textbox.Text = Directory.GetParent(parent).FullName;
                    };
                    listing.Add(b);

                    ShowDirectories(Directory.GetDirectories(parent).Where(p => p.StartsWith(filepath)));
                    ShowFiles(Directory.GetFiles(parent).Where(p => p.StartsWith(filepath)));
                }
            }

            void ShowDirectories(IEnumerable<string> directories) {
                foreach (var directory in directories) {
                    i++;
                    var b = new Button(this.Width - 2, 1) {
                        Position = new Point(0, i),
                        Text = directory,
                    };
                    b.Click += (e, args) => {
                        textbox.Text = directory;
                    };
                    listing.Add(b);
                }
            }
            void ShowFiles(IEnumerable<string> files) {
                foreach (var file in files) {
                    i++;
                    var b = new Button(this.Width - 2, 1) {
                        Position = new Point(0, i),
                        Text = file,
                    };
                    b.Click += (e, args) => {
                        UpdateListing(file);
                    };
                    listing.Add(b);
                }
            }
        }
        public override bool ProcessKeyboard(Keyboard keyboard) {
            if(keyboard.IsKeyPressed(Enter)) {
                sprite.filepath = textbox.Text;
                Parent.Children.Remove(this);
            } else if(keyboard.KeysPressed.Count == 1) {
                var pressed = keyboard.KeysPressed.First();
                if (pressed.Character != ' ' || pressed.Key == Keys.Space) {

                }
            }
            return base.ProcessKeyboard(keyboard);
        }
    }
}
