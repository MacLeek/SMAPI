using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Inheritance;
using StardewModdingAPI.Inheritance.Menus;
using StardewValley;
using StardewValley.Menus;

namespace StardewModdingAPI
{
    public class Program
    {
        private static List<string> _modPaths;

        public static SGame gamePtr;
        public static bool ready;

        public static Assembly StardewAssembly;
        public static Type StardewProgramType;
        public static FieldInfo StardewGameInfo;

        public static Thread gameThread;
        public static Thread consoleInputThread;
        private static List<string> _modContentPaths;

        public static Texture2D DebugPixel { get; private set; }

        // ReSharper disable once PossibleNullReferenceException
        public static int BuildType => (int) StardewProgramType.GetField("buildType", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Main method holding the API execution
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            try
            {
                Log.AsyncY("SDV Version: " + Game1.version);
                Log.AsyncY("SMAPI Version: " + Constants.Version.VersionString);
                ConfigureUI();
                ConfigurePaths();
				GameRunInvoker();
                ConfigureSDV();

                
            }
            catch (Exception e)
            {
                // Catch and display all exceptions. 
                Console.WriteLine(e);
                Console.ReadKey();
                Log.AsyncR("Critical error: " + e);
            }

            Log.AsyncY("The API will now terminate. Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        ///     Set up the console properties
        /// </summary>
        private static void ConfigureUI()
        {
            Console.Title = Constants.ConsoleTitle;

#if DEBUG
            Console.Title += " - DEBUG IS NOT FALSE, AUTHOUR NEEDS TO REUPLOAD THIS VERSION";
#endif
        }

        /// <summary>
        ///     Setup the required paths and logging
        /// </summary>
        private static void ConfigurePaths()
        {
            Log.AsyncY("Validating api paths...");

            _modPaths = new List<string> {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Mods"), Path.Combine(Constants.ExecutionPath, "Mods")};
            //_modContentPaths = new List<string>();

            //TODO: Have an app.config and put the paths inside it so users can define locations to load mods from

            //Mods need to make their own content paths, since we're doing a different, manifest-driven, approach.
            //_modContentPaths.Add(Path.Combine(Constants.ExecutionPath, "Mods", "Content"));
            //_modContentPaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Mods", "Content"));

            //Checks that all defined modpaths exist as directories
            _modPaths.ForEach(VerifyPath);
            //_modContentPaths.ForEach(path => VerifyPath(path));
            VerifyPath(Constants.LogDir);

            if (!File.Exists(Constants.ExecutionPath + "//StardewValley.exe"))
            {
                throw new FileNotFoundException($"Could not found: {Constants.ExecutionPath}\\StardewValley.exe");
            }
        }

        /// <summary>
        ///     Load Stardev Valley and control features
        /// </summary>
        private static void ConfigureSDV()
        {
            Log.AsyncY("Initializing SDV Assembly...");

            // Load in the assembly - ignores security
            StardewAssembly = Assembly.UnsafeLoadFrom(Constants.ExecutionPath + "//StardewValley.exe");
            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            // Change the game's version
            Log.AsyncY("Injecting New SDV Version...");
            Game1.version += $"-Z_MODDED | SMAPI {Constants.Version.VersionString}";

			// Create the thread for the game to run in.
			//gameThread = new Thread(RunGame);
			//Log.AsyncY("Starting SDV...");
			//gameThread.Start();
			RunGame();
            // Wait for the game to load up
            //while (!ready)
            //{
            //}

        }

        /// <summary>
        ///     Wrap the 'RunGame' method for console output
        /// </summary>
        private static void GameRunInvoker()
        {
            
            //Begin listening to input
			consoleInputThread = new Thread(ConsoleInputThread);
            consoleInputThread.Start();


   //         while (ready)
			//{
   //             //Check if the game is still running 10 times a second
   //             Thread.Sleep(1000 / 10);
   //         }

   //         //abort the thread, we're closing
   //         if (consoleInputThread != null && consoleInputThread.ThreadState == ThreadState.Running)
   //             consoleInputThread.Abort();

   //         Log.AsyncY("Game Execution Finished");
   //         Log.AsyncY("Shutting Down...");
   //         Thread.Sleep(100);
   //         Environment.Exit(0);
        }

        /// <summary>
        ///     Create the given directory path if it does not exist
        /// </summary>
        /// <param name="path">Desired directory path</param>
        private static void VerifyPath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Log.AsyncR("Could not create a path: " + path + "\n\n" + ex);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void RunGame()
        {
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomain_UnhandledException;

            try
            {
                gamePtr = new SGame();
                Log.AsyncY("Patching SDV Graphics Profile...");
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;
                LoadMods();

                ready = true;

                StardewGameInfo.SetValue(StardewProgramType, gamePtr);
                gamePtr.Run();
            }
            catch (Exception ex)
            {
                Log.AsyncR("Game failed to start: " + ex);
            }
        }

        public static void LoadMods()
        {
            Log.AsyncY("LOADING MODS");
            foreach (var ModPath in _modPaths)
            {
                foreach (var d in Directory.GetDirectories(ModPath))
                {
                    foreach (var s in Directory.GetFiles(d, "manifest.json"))
                    {
                        if (s.Contains("StardewInjector"))
                            continue;
                        Log.AsyncG("Found Manifest: " + s);
                        var manifest = new Manifest();
                        try
                        {
                            var t = File.ReadAllText(s);
                            if (string.IsNullOrEmpty(t))
                            {
                                Log.AsyncR($"Failed to read mod manifest '{s}'. Manifest is empty!");
                                continue;
                            }

                            manifest = manifest.InitializeConfig(s);

                            if (string.IsNullOrEmpty(manifest.EntryDll))
                            {
                                Log.AsyncR($"Failed to read mod manifest '{s}'. EntryDll is empty!");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.AsyncR($"Failed to read mod manifest '{s}'. Exception details:\n" + ex);
                            continue;
                        }
                        var targDir = Path.GetDirectoryName(s);
                        var psDir = Path.Combine(targDir, "psconfigs");
                        Log.AsyncY($"Created psconfigs directory @{psDir}");
                        try
                        {
                            if (manifest.PerSaveConfigs)
                            {
                                if (!Directory.Exists(psDir))
                                {
                                    Directory.CreateDirectory(psDir);
                                    Log.AsyncY($"Created psconfigs directory @{psDir}");
                                }

                                if (!Directory.Exists(psDir))
                                {
                                    Log.AsyncR($"Failed to create psconfigs directory '{psDir}'. No exception occured.");
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.AsyncR($"Failed to create psconfigs directory '{targDir}'. Exception details:\n" + ex);
                            continue;
                        }
                        var targDll = string.Empty;
                        try
                        {
                            targDll = Path.Combine(targDir, manifest.EntryDll);
                            if (!File.Exists(targDll))
                            {
                                Log.AsyncR($"Failed to load mod '{manifest.EntryDll}'. File {targDll} does not exist!");
                                continue;
                            }

                            var mod = Assembly.UnsafeLoadFrom(targDll);

                            if (mod.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                            {
                                Log.AsyncY("Loading Mod DLL...");
                                var tar = mod.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                                var m = (Mod) mod.CreateInstance(tar.ToString());
                                if (m != null)
                                {
                                    m.PathOnDisk = targDir;
                                    m.Manifest = manifest;
                                    Log.AsyncG($"LOADED MOD: {m.Manifest.Name} by {m.Manifest.Authour} - Version {m.Manifest.Version} | Description: {m.Manifest.Description} (@ {targDll})");
                                    Constants.ModsLoaded += 1;
                                    m.Entry();
                                }
                            }
                            else
                            {
                                Log.AsyncR("Invalid Mod DLL");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.AsyncR($"Failed to load mod '{targDll}'. Exception details:\n" + ex);
                        }
                    }
                }
            }
            Log.AsyncG($"LOADED {Constants.ModsLoaded} MODS");
            Console.Title = Constants.ConsoleTitle;
        }

        public static void ConsoleInputThread()
        {
            var input = string.Empty;
			while (!ready);
			//SDV is running
			Log.AsyncY("SDV Loaded Into Memory");

			//Create definition to listen for input
			Log.AsyncY("Initializing Console Input Thread...");

			// The only command in the API (at least it should be, for now)
			Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;
			//Command.RegisterCommand("crash", "crashes sdv").CommandFired += delegate { Game1.player.draw(null); };

			//Subscribe to events
			ControlEvents.KeyPressed += Events_KeyPressed;
			GameEvents.LoadContent += Events_LoadContent;
			//Events.MenuChanged += Events_MenuChanged; //Idk right now

			Log.AsyncY("Applying Final SDV Tweaks...");
			//Game's in memory now, send the event
			Log.AsyncY("Game Loaded");
			GameEvents.InvokeGameLoaded();

			Log.AsyncY("Type 'help' for help, or 'help <cmd>' for a command's usage");
			while (true)
            {
				var cmds = Console.ReadLine();
				if(cmds!=null)
                Command.CallCommand(cmds);
            }
        }

        private static void Events_LoadContent(object o, EventArgs e)
        {
            Log.AsyncY("Initializing Debug Assets...");
            DebugPixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            DebugPixel.SetData(new[] {Color.White});

#if DEBUG
            StardewModdingAPI.Log.Async("REGISTERING BASE CUSTOM ITEM");
            SObject so = new SObject();
            so.Name = "Mario Block";
            so.CategoryName = "SMAPI Test Mod";
            so.Description = "It's a block from Mario!\nLoaded in realtime by SMAPI.";
            so.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(_modContentPaths[0] + "\\Test.png", FileMode.Open));
            so.IsPassable = true;
            so.IsPlaceable = true;
            StardewModdingAPI.Log.Async("REGISTERED WITH ID OF: " + SGame.RegisterModItem(so));

            //StardewModdingAPI.Log.Async("REGISTERING SECOND CUSTOM ITEM");
            //SObject so2 = new SObject();
            //so2.Name = "Mario Painting";
            //so2.CategoryName = "SMAPI Test Mod";
            //so2.Description = "It's a painting of a creature from Mario!\nLoaded in realtime by SMAPI.";
            //so2.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(_modContentPaths[0] + "\\PaintingTest.png", FileMode.Open));
            //so2.IsPassable = true;
            //so2.IsPlaceable = true;
            //StardewModdingAPI.Log.Async("REGISTERED WITH ID OF: " + SGame.RegisterModItem(so2));

            Command.CallCommand("load");
#endif
        }

        private static void Events_KeyPressed(object o, EventArgsKeyPressed e)
        {
        }

        private static void Events_MenuChanged(IClickableMenu newMenu)
        {
            Log.AsyncY("NEW MENU: " + newMenu.GetType());
            if (newMenu is GameMenu)
            {
                Game1.activeClickableMenu = SGameMenu.ConstructFromBaseClass(Game1.activeClickableMenu as GameMenu);
            }
        }

        private static void Events_LocationsChanged(List<GameLocation> newLocations)
        {
#if DEBUG
            SGame.ModLocations = SGameLocation.ConstructFromBaseClasses(Game1.locations);
#endif
        }

        private static void Events_CurrentLocationChanged(GameLocation newLocation)
        {
            //SGame.CurrentLocation = null;
            //System.Threading.Thread.Sleep(10);
#if DEBUG
            Console.WriteLine(newLocation.name);
            SGame.CurrentLocation = SGame.LoadOrCreateSGameLocationFromName(newLocation.name);
#endif
            //Game1.currentLocation = SGame.CurrentLocation;
            //Log.LogComment(((SGameLocation) newLocation).name);
            //Log.LogComment("LOC CHANGED: " + SGame.currentLocation.name);
        }

        private static void help_CommandFired(object o, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                var fnd = Command.FindCommand(e.Command.CalledArgs[0]);
                if (fnd == null)
                    Log.AsyncR("The command specified could not be found");
                else
                {
                    Log.AsyncY(fnd.CommandArgs.Length > 0 ? $"{fnd.CommandName}: {fnd.CommandDesc} - {fnd.CommandArgs.ToSingular()}" : $"{fnd.CommandName}: {fnd.CommandDesc}");
                }
            }
            else
                Log.AsyncY("Commands: " + Command.RegisteredCommands.Select(x => x.CommandName).ToSingular());
        }
    }
}