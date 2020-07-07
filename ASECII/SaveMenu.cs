using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.MonoGame;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.UI.Themes;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

            var preview = model.sprite.preview;
            StringBuilder str = new StringBuilder();
            for (int y = model.sprite.origin.Y; y <= model.sprite.end.Y; y++) {
                for (int x = model.sprite.origin.X; x <= model.sprite.end.X; x++) {
                    if(preview.TryGetValue((x, y), out var tile)) {
                        str.Append((char)tile.Glyph);
                    } else {
                        str.Append(' ');
                    }
                }
                str.AppendLine();
            }
            File.WriteAllText($"{text}.txt", str.ToString());

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
                model.sprite.layers.Add(new Layer());
                File.WriteAllText(text, JsonConvert.SerializeObject(model, SFileMode.settings));
                console.Children.Add(new EditorMain(Width, Height, model));
            }
        }
    }
    class FileMenu : ControlsConsole {
        SpriteModel hoveredFile;
        Dictionary<string, SpriteModel> preloaded;

        HashSet<string> recentFiles;
        List<Button> recentListing;
        
        List<Button> folderListing;
        TextBox textbox;
        FileMode mode;

        int folderListingX;

        public FileMenu(int width, int height, FileMode mode, HashSet<string> recentFiles = null) : base(width, height) {
            
            this.recentFiles = recentFiles;
            this.preloaded = new Dictionary<string, SpriteModel>();
            this.recentListing = new List<Button>();
            int n = 0;
            if(recentFiles != null) {
                folderListingX = 16;
                foreach (var f in recentFiles) {
                    var b = new Button(16) {
                        Text = Path.GetFileName(f),
                        Position = new Point(0, n),
                    };
                    recentListing.Add(b);
                    n++;
                }
            }
            

            this.mode = mode;
            UseMouse = true;
            UseKeyboard = true;
            IsFocused = true;
            FocusOnMouseClick = true;
            folderListing = new List<Button>();


            textbox = new TextBox(width - folderListingX) {
                Position = new Point(folderListingX, 1),
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
            folderListing.ForEach(b => this.ControlHostComponent.Remove(b));
            folderListing.Clear();
            int i = 2;
            if (string.IsNullOrWhiteSpace(filepath)) {
                filepath = Environment.CurrentDirectory;
            }
            if (Directory.Exists(filepath)) {

                i++;
                var b = new Button(32, 1) {
                    Position = new Point(folderListingX, i),
                    Text = "..",
                    TextAlignment = HorizontalAlignment.Left,
                };
                b.Click += (e, args) => {
                    textbox.Text = Directory.GetParent(filepath).FullName;
                };
                folderListing.Add(b);

                ShowDirectories(Directory.GetDirectories(filepath).Where(p => p.StartsWith(filepath)));
                ShowFiles(Directory.GetFiles(filepath).Where(p => p.StartsWith(filepath)));
            } else if (File.Exists(filepath)) {
                i++;
                var b = new Button(32, 1) {
                    Position = new Point(folderListingX, i),
                    Text = "..",
                    TextAlignment = HorizontalAlignment.Left,
                };
                b.Click += (e, args) => {
                    textbox.Text = Directory.GetParent(filepath).FullName;
                };
                folderListing.Add(b);

                //File button
                i++;
                b = new Button(32, 1) {
                    Position = new Point(folderListingX, i),
                    Text = Path.GetFileName(filepath),
                    TextAlignment = HorizontalAlignment.Left,
                };
                b.MouseEnter += (e, args) => {
                    ShowPreview(filepath);
                };
                b.Click += (e, args) => {
                    textbox.Text = filepath;
                };
                folderListing.Add(b);
            } else {
                var parent = Directory.GetParent(filepath).FullName;
                if (Directory.Exists(parent)) {
                    i++;
                    var b = new Button(32, 1) {
                        Position = new Point(folderListingX, i),
                        Text = "..",
                        TextAlignment = HorizontalAlignment.Left,
                    };
                    b.Click += (e, args) => {
                        //textbox.Text = Directory.GetParent(parent).FullName;
                        textbox.Text = parent;
                    };
                    folderListing.Add(b);

                    ShowDirectories(Directory.GetDirectories(parent).Where(p => p.StartsWith(filepath)));
                    ShowFiles(Directory.GetFiles(parent).Where(p => p.StartsWith(filepath)));
                }
            }
            foreach (var button in folderListing) {
                this.ControlHostComponent.Add(button);
            }

            void ShowDirectories(IEnumerable<string> directories) {
                foreach (var directory in directories) {
                    i++;
                    var b = new Button(32, 1) {
                        Position = new Point(folderListingX, i),
                        Text = Path.GetFileName(directory),
                        TextAlignment = HorizontalAlignment.Left,
                    };
                    b.Click += (e, args) => {
                        textbox.Text = directory;
                    };
                    folderListing.Add(b);
                }
            }
            void ShowFiles(IEnumerable<string> files) {
                foreach (var file in files) {
                    i++;
                    var b = new Button(32, 1) {
                        Position = new Point(folderListingX, i),
                        Text = Path.GetFileName(file),
                        TextAlignment = HorizontalAlignment.Left,
                    };
                    b.MouseEnter += (e, args) => {
                        ShowPreview(file);
                    };
                    b.Click += (e, args) => {
                        textbox.Text = file;
                    };
                    folderListing.Add(b);
                }
            }
            void ShowPreview(string file) {
                if(preloaded.TryGetValue(file, out hoveredFile)) {
                    return;
                } else {
                    try {
                        TypeDescriptor.AddAttributes(typeof((int, int)), new TypeConverterAttribute(typeof(Int2Converter)));
                        var model = JsonConvert.DeserializeObject<SpriteModel>(File.ReadAllText(file), SFileMode.settings);
                        if(model.filepath == null) {
                            preloaded[file] = null;
                            hoveredFile = null;
                            return;
                        }
                        preloaded[file] = model;
                        hoveredFile = model;
                    } catch(Exception e) {
                        preloaded[file] = null;
                        hoveredFile = null;
                    }
                }
            }
        }
        public override void Draw(TimeSpan delta) {
            base.Draw(delta);

            if (hoveredFile != null) {

                var previewStart = new Point(32, 0);
                this.Print(previewStart.X, previewStart.Y, "Preview");
                var origin = hoveredFile.sprite.origin;

                for (int x = 0; x < Width; x++) {
                    for (int y = 0; y < Height; y++) {
                        var screenPos = new Point(x, y);
                        var spritePos = screenPos - previewStart + origin;
                        this.SetCellAppearance(x, y, hoveredFile.sprite.preview.TryGetValue(spritePos, out var tile) ? tile : new TileValue(Color.Black, Color.Black, 0));
                    }
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
