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
using ArchConsole;
using Newtonsoft.Json.Serialization;

namespace ASECII {
    public interface FileMode {
        string InitialPath { get; }
        void Enter(Console console, string text);
    }
    public static class SFileMode {
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
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
        public void Enter(Console console, string filepath) {
            model.filepath = filepath;
            model.Save(renderer);
            Game.Instance.Screen = console;
        }
    }
    class LoadMode : FileMode {
        public string InitialPath => Environment.CurrentDirectory;
        public LoadMode() {

        }
        public void Enter(Console console, string filepath) {
            var Width = console.Width;
            var Height = console.Height;

            if (File.Exists(filepath)) {
                try {
                    var sprite = ASECIILoader.DeserializeObject<SpriteModel>(File.ReadAllText(filepath));

                    if(sprite.filepath != filepath) {
                        sprite.filepath = filepath;
                        File.WriteAllText($"{filepath}", ASECIILoader.SerializeObject(sprite));
                    }

                    sprite.OnLoad();
                    Game.Instance.Screen = new EditorMain(Width, Height, sprite);

                    Program.SaveState(new EditorState(filepath));
                } catch {
                    throw;
                }
            } else {
                var model = new SpriteModel(Width, Height) { filepath = filepath };
                model.sprite.layers.Add(new Layer());

                File.WriteAllText(filepath, ASECIILoader.SerializeObject(model));
                console.Children.Add(new EditorMain(Width, Height, model));
            }
        }
    }
    class FileMenu : ControlsConsole {
        public static string RECENTFILES = "RecentFiles.json";

        SpriteModel hoveredFile;
        Dictionary<string, SpriteModel> preloaded;

        HashSet<string> recentFiles;
        List<LabelButton> recentListing;
        
        List<LabelButton> folderListing;
        TextField textbox;
        FileMode mode;

        int folderListingX;

        public FileMenu(int width, int height, FileMode mode) : base(width, height) {

            DefaultBackground = Color.Black;


            this.recentFiles = File.Exists(RECENTFILES) ? ASECIILoader.DeserializeObject<HashSet<string>>(File.ReadAllText(RECENTFILES)) : new HashSet<string>();
            this.preloaded = new Dictionary<string, SpriteModel>();
            this.recentListing = new List<LabelButton>();
            int n = 3;

            if(recentFiles.Any()) {
                folderListingX = 32;
                foreach (var f in recentFiles) {
                    var p = Path.GetFileName(f);
                    var b = new LabelButton(p, Load) {
                        Position = new Point(4, n),
                    };

                    b.MouseEnter += (e, args) => {
                        ShowPreview(f);
                    };

                    this.Children.Add(b);
                    recentListing.Add(b);
                    n++;

                    void Load() {
                        mode.Enter(this, f);
                        AddRecentFile(f);
                    }
                }
            } else {
                folderListingX = 8;
            }
            

            this.mode = mode;
            UseMouse = true;
            UseKeyboard = true;
            IsFocused = true;
            FocusOnMouseClick = true;
            folderListing = new List<LabelButton>();


            textbox = new TextField(width - folderListingX) {
                Position = new Point(folderListingX, 1),
                
                UseKeyboard = true,
                UseMouse = true,
                IsFocused = true,
                text = mode.InitialPath,
            };
            textbox.TextChanged += (tf) => {
                UpdateListing(textbox.text);
            };
            this.Children.Add(textbox);
            UpdateListing(textbox.text);
        }

        public void AddRecentFile(string s) {
            recentFiles.Add(s);
            File.WriteAllText(RECENTFILES, ASECIILoader.SerializeObject(recentFiles));
        }
        public void UpdateListing(string filepath) {
            folderListing.ForEach(b => this.Children.Remove(b));
            folderListing.Clear();
            int i = 2;
            if (string.IsNullOrWhiteSpace(filepath)) {
                filepath = Environment.CurrentDirectory;
            } else {
                filepath = Path.GetFullPath(filepath);
            }
            if (Directory.Exists(filepath)) {

                i++;
                var b = new LabelButton("..", () => textbox.text = Directory.GetParent(filepath).FullName) {
                    Position = new Point(folderListingX, i),
                };
                folderListing.Add(b);

                ShowDirectories(Directory.GetDirectories(filepath).Where(p => p.StartsWith(filepath)));
                ShowFiles(Directory.GetFiles(filepath).Where(p => p.StartsWith(filepath)));
            } else {
         var parent = Directory.GetParent(filepath).FullName;
                if (Directory.Exists(parent)) {
                    i++;
                    var b = new LabelButton("..", () => textbox.text = parent) {
                        Position = new Point(folderListingX, i),
                    };
                    folderListing.Add(b);

                    ShowDirectories(Directory.GetDirectories(parent).Where(p => p.StartsWith(filepath)));
                    ShowFiles(Directory.GetFiles(parent).Where(p => p.StartsWith(filepath)));
                }
            }


            foreach (var button in folderListing.Take(64)) {
                this.Children.Add(button);
            }

            void ShowDirectories(IEnumerable<string> directories) {
                foreach (var directory in directories) {
                    i++;
                    var b = new LabelButton(Path.GetFileName(directory), () => textbox.text = directory) {
                        Position = new Point(folderListingX, i),
                    };
                    folderListing.Add(b);
                }
            }
            void ShowFiles(IEnumerable<string> files) {
                foreach (var file in files) {
                    i++;
                    var b = new LabelButton(Path.GetFileName(file), Load) {
                        Position = new Point(folderListingX, i),
                    };
                    b.MouseEnter += (e, args) => {
                        ShowPreview(file);
                    };
                    folderListing.Add(b);

                    void Load() {
                        mode.Enter(this, file);
                        AddRecentFile(file);
                    }
                }
            }
        }

        public void ShowPreview(string file) {
            if (preloaded.TryGetValue(file, out hoveredFile)) {
                return;
            } else {
                preloaded[file] = null;

                System.Threading.Tasks.Task.Run(StartLoad);
                void StartLoad() {
                    try {
                        var model = ASECIILoader.DeserializeObject<SpriteModel>(File.ReadAllText(file));
                        if (model?.filepath == null) {
                            preloaded[file] = null;
                            hoveredFile = null;
                            return;
                        }
                        preloaded[file] = model;
                        hoveredFile = model;
                    } catch (Exception e) {
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

                var s = hoveredFile.sprite;

                var previewX = (Width - (s.end - s.origin).X) < 64 ? 0 : 64;
                var previewY = 0;
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
            }
        }
        public override bool ProcessKeyboard(Keyboard keyboard) {
            if (keyboard.IsKeyPressed(Enter)) {
                var f = textbox.text;
                mode.Enter(this, f);
                AddRecentFile(f);
            }
            return base.ProcessKeyboard(keyboard);
        }
    }

}
