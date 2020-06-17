using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.UI.Themes;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static SadConsole.Input.Keys;
using Console = SadConsole.Console;

namespace ASECII {
    interface FileMode {
        string InitialPath { get; }
        void Enter(Console console, string text) {

        }
    }
    static class SFileMode {
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
    class SaveMode : FileMode {
        SpriteModel model;
        public string InitialPath => model.filepath ?? Environment.CurrentDirectory;
        public SaveMode(SpriteModel model) {
            this.model = model;
        }
        public void Enter(Console console, string text) {
            model.filepath = text;

            File.WriteAllText(text, JsonConvert.SerializeObject(model, SFileMode.settings));

            console.Parent.Children.Remove(console);
        }
    }
    class LoadMode : FileMode {
        public string InitialPath => Environment.CurrentDirectory;
        public LoadMode() {

        }
        public void Enter(Console console, string text) {
            var Width = console.Width;
            var Height = console.Height;

            if (File.Exists(text)) {

                //https://stackoverflow.com/a/57319194
                TypeDescriptor.AddAttributes(typeof((int, int)), new TypeConverterAttribute(typeof(Int2Converter)));
                
                
                
                console.Children.Add(new EditorMain(Width, Height, JsonConvert.DeserializeObject<SpriteModel>(File.ReadAllText(text), SFileMode.settings)));
            } else {
                var model = new SpriteModel(Width, Height) { filepath = text };
                File.WriteAllText(text, JsonConvert.SerializeObject(model, SFileMode.settings));
                console.Children.Add(new EditorMain(Width, Height, model));
            }
        }
    }
    class FileMenu : ControlsConsole {
        List<Button> listing;
        TextBox textbox;
        FileMode mode;
        readonly ButtonTheme button = new ButtonTheme() {
        Normal = new ColoredGlyph(Color.White, Color.Black)
        };
        public FileMenu(int width, int height, FileMode mode) : base(width, height) {
            this.mode = mode;
            UseMouse = true;
            UseKeyboard = true;
            IsFocused = true;
            FocusOnMouseClick = true;
            listing = new List<Button>();


            textbox = new TextBox(width - 2) {
                Position = new Point(1, 1),
                IsCaretVisible = true,
                FocusOnClick = true,
                UseKeyboard = true,
                UseMouse = true,
                IsFocused = true,
                CanFocus = true,
                Text = mode.InitialPath,
            };
            textbox.TextChanged += (e, args) => {
                UpdateListing(textbox.Text);
            };
            this.ControlHostComponent.Add(textbox);
            UpdateListing(textbox.Text);
        }
        public void UpdateListing(string filepath) {
            listing.ForEach(b => this.ControlHostComponent.Remove(b));
            listing.Clear();
            int i = 2;
            if (string.IsNullOrWhiteSpace(filepath)) {
                filepath = Environment.CurrentDirectory;
            }
            if (Directory.Exists(filepath)) {

                i++;
                var b = new Button(this.Width - 2, 1) {
                    Position = new Point(0, i),
                    Text = "..",
                    TextAlignment = HorizontalAlignment.Left,
                    Theme = button
                };
                b.Click += (e, args) => {
                    textbox.Text = Directory.GetParent(filepath).FullName;
                };
                listing.Add(b);

                ShowDirectories(Directory.GetDirectories(filepath).Where(p => p.StartsWith(filepath)));
                ShowFiles(Directory.GetFiles(filepath).Where(p => p.StartsWith(filepath)));
            } else if (File.Exists(filepath)) {
                i++;
                var b = new Button(this.Width - 2, 1) {
                    Position = new Point(0, i),
                    Text = "..",
                    TextAlignment = HorizontalAlignment.Left,
                    Theme = button
                };
                b.Click += (e, args) => {
                    textbox.Text = Directory.GetParent(filepath).FullName;
                };
                listing.Add(b);

                i++;
                b = new Button(this.Width - 2, 1) {
                    Position = new Point(0, i),
                    Text = Path.GetFileName(filepath),
                    TextAlignment = HorizontalAlignment.Left,
                    Theme = button
                };
                b.Click += (e, args) => {
                    textbox.Text = filepath;
                };
                listing.Add(b);
            } else {
                var parent = Directory.GetParent(filepath).FullName;
                if (Directory.Exists(parent)) {
                    i++;
                    var b = new Button(this.Width - 2, 1) {
                        Position = new Point(0, i),
                        Text = "..",
                        TextAlignment = HorizontalAlignment.Left,
                        Theme = button
                    };
                    b.Click += (e, args) => {
                        //textbox.Text = Directory.GetParent(parent).FullName;
                        textbox.Text = parent;
                    };
                    listing.Add(b);

                    ShowDirectories(Directory.GetDirectories(parent).Where(p => p.StartsWith(filepath)));
                    ShowFiles(Directory.GetFiles(parent).Where(p => p.StartsWith(filepath)));
                }
            }
            foreach (var button in listing) {
                this.ControlHostComponent.Add(button);
            }

            void ShowDirectories(IEnumerable<string> directories) {
                foreach (var directory in directories) {
                    i++;
                    var b = new Button(this.Width - 2, 1) {
                        Position = new Point(0, i),
                        Text = Path.GetFileName(directory),
                        TextAlignment = HorizontalAlignment.Left,
                        Theme = button
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
                        Text = Path.GetFileName(file),
                        TextAlignment = HorizontalAlignment.Left,
                        Theme = button
                    };
                    b.Click += (e, args) => {
                        textbox.Text = file;
                    };
                    listing.Add(b);
                }
            }
        }
        public override bool ProcessKeyboard(Keyboard keyboard) {
            if (keyboard.IsKeyPressed(Enter)) {
                var f = textbox.EditingText;
                mode.Enter(this, f);
                
            } else {
                UpdateListing(textbox.EditingText);
            }
            return base.ProcessKeyboard(keyboard);
        }
    }

}
