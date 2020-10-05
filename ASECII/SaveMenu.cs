using Newtonsoft.Json;
using SadConsole;
using SadConsole.Input;
using SadConsole.Host.MonoGame;
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
    public interface FileMode {
        string InitialPath { get; }
        void Enter(Console console, string text) {

        }
    }
    public static class SFileMode {
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All
        };
    }
    class SaveMode : FileMode {
        SpriteModel model;
        Console renderer;
        public string InitialPath => model.filepath ?? Environment.CurrentDirectory;
        public SaveMode(SpriteModel model, Console renderer) {
            this.model = model;
            this.renderer = renderer;
        }
        public void Enter(Console console, string text) {
            model.filepath = text;
            model.Save(renderer);
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
                try {
                    STypeConverter.PrepareConvert();
                    var sprite = JsonConvert.DeserializeObject<SpriteModel>(File.ReadAllText(text), SFileMode.settings);
                    console.Children.Add(new EditorMain(Width, Height, sprite));
                } catch {
                    throw;
                }
            } else {
                var model = new SpriteModel(Width, Height) { filepath = text };
                model.sprite.layers.Add(new Layer());
                STypeConverter.PrepareConvert();
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

            DefaultBackground = Color.Black;


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
            this.Controls.Add(textbox);
            UpdateListing(textbox.Text);
        }
        public void UpdateListing(string filepath) {
            folderListing.ForEach(b => this.Controls.Remove(b));
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
                    mode.Enter(this, filepath);
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
                this.Controls.Add(button);
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
                        //textbox.Text = file;
                        mode.Enter(this, file);
                    };
                    folderListing.Add(b);
                }
            }
            void ShowPreview(string file) {
                if(preloaded.TryGetValue(file, out hoveredFile)) {
                    return;
                } else {
                    try {
                        STypeConverter.PrepareConvert();
                        var model = JsonConvert.DeserializeObject<SpriteModel>(File.ReadAllText(file), SFileMode.settings);
                        if(model?.filepath == null) {
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
        public override void Render(TimeSpan delta) {
            base.Render(delta);
            this.Clear();

            var c1 = new Color(25, 25, 25);
            var c2 = new Color(51, 51, 51);
            for (int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    this.SetBackground(x, y, (x + y) % 2 == 0 ? c1 : c2);
                }
            }
            if (hoveredFile != null && hoveredFile.sprite != null) {

                var previewX = 33;
                var previewY = 3;
                var origin = hoveredFile.sprite.origin;

                
                var previewStart = new Point(previewX, previewY);
                for (int x = previewX; x < Width; x++) {
                    for (int y = previewY; y < Height; y++) {
                        var screenPos = new Point(x, y);
                        var spritePos = screenPos - previewStart + origin;
                        if (hoveredFile.sprite.preview.TryGetValue(spritePos, out var tile)) {
                            this.SetCellAppearance(x, y, tile);
                        }
                    }
                }
                this.Print(previewX, previewY, "Preview", Color.White, Color.Black);
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
