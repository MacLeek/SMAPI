using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewModdingAPI.Inheritance
{
    /// <summary>
    /// The 'SGame' class.
    /// This summary, and many others, only exists because XML doc tags.
    /// </summary>
    public class SGame : Game1
    {
        /// <summary>
        /// Useless right now.
        /// </summary>
        public const int LowestModItemID = 1000;

        private bool FireLoadedGameEvent;

        /// <summary>
        /// Gets a jagged array of all buttons pressed on the gamepad the prior frame.
        /// </summary>
        public Buttons[][] PreviouslyPressedButtons;

        internal SGame()
        {
            Instance = this;
            FirstUpdate = true;
        }

        /// <summary>
        /// Useless at this time.
        /// </summary>
        [Obsolete]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public static Dictionary<int, SObject> ModItems { get; private set; }

        /// <summary>
        /// The current KeyboardState
        /// </summary>
        public KeyboardState KStateNow { get; private set; }

        /// <summary>
        /// The prior KeyboardState
        /// </summary>
        public KeyboardState KStatePrior { get; private set; }

        /// <summary>
        /// The current MouseState
        /// </summary>
        public MouseState MStateNow { get; private set; }

        /// <summary>
        /// The prior MouseState
        /// </summary>
        public MouseState MStatePrior { get; private set; }

        /// <summary>
        /// All keys pressed on the current frame
        /// </summary>
        public Keys[] CurrentlyPressedKeys => KStateNow.GetPressedKeys();

        /// <summary>
        /// All keys pressed on the prior frame
        /// </summary>
        public Keys[] PreviouslyPressedKeys => KStatePrior.GetPressedKeys();

        /// <summary>
        /// All keys pressed on this frame except for the ones pressed on the prior frame
        /// </summary>
        public Keys[] FramePressedKeys => CurrentlyPressedKeys.Except(PreviouslyPressedKeys).ToArray();

        /// <summary>
        /// All keys pressed on the prior frame except for the ones pressed on the current frame
        /// </summary>
        public Keys[] FrameReleasedKeys => PreviouslyPressedKeys.Except(CurrentlyPressedKeys).ToArray();

        /// <summary>
        /// Whether or not a save was tagged as 'Loaded' the prior frame.
        /// </summary>
        public bool PreviouslyLoadedGame { get; private set; }

        /// <summary>
        /// The list of GameLocations on the prior frame
        /// </summary>
        public int PreviousGameLocations { get; private set; }

        /// <summary>
        /// The list of GameObjects on the prior frame
        /// </summary>
        public int PreviousLocationObjects { get; private set; }

        /// <summary>
        /// The list of Items in the player's inventory on the prior frame
        /// </summary>
        public Dictionary<Item, int> PreviousItems { get; private set; }

        /// <summary>
        /// The player's Combat level on the prior frame
        /// </summary>
        public int PreviousCombatLevel { get; private set; }

        /// <summary>
        /// The player's Farming level on the prior frame
        /// </summary>
        public int PreviousFarmingLevel { get; private set; }

        /// <summary>
        /// The player's Fishing level on the prior frame
        /// </summary>
        public int PreviousFishingLevel { get; private set; }

        /// <summary>
        /// The player's Foraging level on the prior frame
        /// </summary>
        public int PreviousForagingLevel { get; private set; }

        /// <summary>
        /// The player's Mining level on the prior frame
        /// </summary>
        public int PreviousMiningLevel { get; private set; }

        /// <summary>
        /// The player's Luck level on the prior frame
        /// </summary>
        public int PreviousLuckLevel { get; private set; }

        //Kill me now comments are so boring

        /// <summary>
        /// The player's previous game location
        /// </summary>
        public GameLocation PreviousGameLocation { get; private set; }

        /// <summary>
        /// The previous ActiveGameMenu in Game1
        /// </summary>
        public IClickableMenu PreviousActiveMenu { get; private set; }

        /// <summary>
        /// Indicates if the MenuClosed event was fired to prevent it from re-firing.
        /// </summary>
        internal bool WasMenuClosedInvoked = false;

        /// <summary>
        /// The previous mine level
        /// </summary>
        public int PreviousMineLevel { get; private set; }

        /// <summary>
        /// The previous TimeOfDay (Int32 between 600 and 2400?)
        /// </summary>
        public int PreviousTimeOfDay { get; private set; }

        /// <summary>
        /// The previous DayOfMonth (Int32 between 1 and 28?)
        /// </summary>
        public int PreviousDayOfMonth { get; private set; }

        /// <summary>
        /// The previous Season (String as follows: "winter", "spring", "summer", "fall")
        /// </summary>
        public string PreviousSeasonOfYear { get; private set; }

        /// <summary>
        /// The previous Year
        /// </summary>
        public int PreviousYearOfGame { get; private set; }

        /// <summary>
        /// The previous result of Game1.newDay
        /// </summary>
        public bool PreviousIsNewDay { get; private set; }

        /// <summary>
        /// The previous 'Farmer' (Player)
        /// </summary>
        public Farmer PreviousFarmer { get; private set; }

        /// <summary>
        /// The current index of the update tick. Recycles every 60th tick to 0. (Int32 between 0 and 59)
        /// </summary>
        public int CurrentUpdateTick { get; private set; }

        /// <summary>
        /// Whether or not this update frame is the very first of the entire game
        /// </summary>
        public bool FirstUpdate { get; private set; }

        /// <summary>
        /// The current RenderTarget in Game1 (Private field, uses reflection)
        /// </summary>
        public RenderTarget2D Screen
        {
            get { return typeof(Game1).GetBaseFieldValue<RenderTarget2D>(Program.gamePtr, "screen"); }
            set { typeof(Game1).SetBaseFieldValue<RenderTarget2D>(this, "screen", value); }
        }

        /// <summary>
        /// The current Colour in Game1 (Private field, uses reflection)
        /// </summary>
        public Color BgColour
        {
            get { return (Color) typeof(Game1).GetBaseFieldValue<object>(Program.gamePtr, "bgColor"); }
            set { typeof(Game1).SetBaseFieldValue<object>(this, "bgColor", value); }
        }

        /// <summary>
        /// Static accessor for an Instance of the class SGame
        /// </summary>
        public static SGame Instance { get; private set; }

        /// <summary>
        /// The game's FPS. Re-determined every Draw update.
        /// </summary>
        public static float FramesPerSecond { get; private set; }

        /// <summary>
        /// Whether or not we're in a pseudo 'debug' mode. Mostly for displaying information like FPS.
        /// </summary>
        public static bool Debug { get; private set; }

        internal static Queue<String> DebugMessageQueue { get; private set; }

        /// <summary>
        /// The current player (equal to Farmer.Player)
        /// </summary>
        [Obsolete("Use Farmer.Player instead")]
        public Farmer CurrentFarmer => player;

        /// <summary>
        /// Gets ALL static fields that belong to 'Game1'
        /// </summary>
        public static FieldInfo[] GetStaticFields => typeof(Game1).GetFields();

        /// <summary>
        /// Whether or not a button was just pressed on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonState"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustPressed(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Pressed && !PreviouslyPressedButtons[(int) stateIndex].Contains(button);
        }

        /// <summary>
        /// Whether or not a button was just released on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonState"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustReleased(Buttons button, ButtonState buttonState, PlayerIndex stateIndex)
        {
            return buttonState == ButtonState.Released && PreviouslyPressedButtons[(int) stateIndex].Contains(button);
        }

        /// <summary>
        /// Whether or not an analog button was just pressed on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustPressed(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustPressed(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        /// <summary>
        /// Whether or not an analog button was just released on the controller
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private bool WasButtonJustReleased(Buttons button, float value, PlayerIndex stateIndex)
        {
            return WasButtonJustReleased(button, value > 0.2f ? ButtonState.Pressed : ButtonState.Released, stateIndex);
        }

        /// <summary>
        /// Gets an array of all Buttons pressed on a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetButtonsDown(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (state.Buttons.A == ButtonState.Pressed) buttons.Add(Buttons.A);
                if (state.Buttons.B == ButtonState.Pressed) buttons.Add(Buttons.B);
                if (state.Buttons.Back == ButtonState.Pressed) buttons.Add(Buttons.Back);
                if (state.Buttons.BigButton == ButtonState.Pressed) buttons.Add(Buttons.BigButton);
                if (state.Buttons.LeftShoulder == ButtonState.Pressed) buttons.Add(Buttons.LeftShoulder);
                if (state.Buttons.LeftStick == ButtonState.Pressed) buttons.Add(Buttons.LeftStick);
                if (state.Buttons.RightShoulder == ButtonState.Pressed) buttons.Add(Buttons.RightShoulder);
                if (state.Buttons.RightStick == ButtonState.Pressed) buttons.Add(Buttons.RightStick);
                if (state.Buttons.Start == ButtonState.Pressed) buttons.Add(Buttons.Start);
                if (state.Buttons.X == ButtonState.Pressed) buttons.Add(Buttons.X);
                if (state.Buttons.Y == ButtonState.Pressed) buttons.Add(Buttons.Y);
                if (state.DPad.Up == ButtonState.Pressed) buttons.Add(Buttons.DPadUp);
                if (state.DPad.Down == ButtonState.Pressed) buttons.Add(Buttons.DPadDown);
                if (state.DPad.Left == ButtonState.Pressed) buttons.Add(Buttons.DPadLeft);
                if (state.DPad.Right == ButtonState.Pressed) buttons.Add(Buttons.DPadRight);
                if (state.Triggers.Left > 0.2f) buttons.Add(Buttons.LeftTrigger);
                if (state.Triggers.Right > 0.2f) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// Gets all buttons that were pressed on the current frame of a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetFramePressedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (WasButtonJustPressed(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (WasButtonJustPressed(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (WasButtonJustPressed(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (WasButtonJustPressed(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (WasButtonJustPressed(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (WasButtonJustPressed(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (WasButtonJustPressed(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (WasButtonJustPressed(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (WasButtonJustPressed(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (WasButtonJustPressed(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (WasButtonJustPressed(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (WasButtonJustPressed(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (WasButtonJustPressed(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (WasButtonJustPressed(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (WasButtonJustPressed(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (WasButtonJustPressed(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (WasButtonJustPressed(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// Gets all buttons that were released on the current frame of a joystick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Buttons[] GetFrameReleasedButtons(PlayerIndex index)
        {
            var state = GamePad.GetState(index);
            var buttons = new List<Buttons>();
            if (state.IsConnected)
            {
                if (WasButtonJustReleased(Buttons.A, state.Buttons.A, index)) buttons.Add(Buttons.A);
                if (WasButtonJustReleased(Buttons.B, state.Buttons.B, index)) buttons.Add(Buttons.B);
                if (WasButtonJustReleased(Buttons.Back, state.Buttons.Back, index)) buttons.Add(Buttons.Back);
                if (WasButtonJustReleased(Buttons.BigButton, state.Buttons.BigButton, index)) buttons.Add(Buttons.BigButton);
                if (WasButtonJustReleased(Buttons.LeftShoulder, state.Buttons.LeftShoulder, index)) buttons.Add(Buttons.LeftShoulder);
                if (WasButtonJustReleased(Buttons.LeftStick, state.Buttons.LeftStick, index)) buttons.Add(Buttons.LeftStick);
                if (WasButtonJustReleased(Buttons.RightShoulder, state.Buttons.RightShoulder, index)) buttons.Add(Buttons.RightShoulder);
                if (WasButtonJustReleased(Buttons.RightStick, state.Buttons.RightStick, index)) buttons.Add(Buttons.RightStick);
                if (WasButtonJustReleased(Buttons.Start, state.Buttons.Start, index)) buttons.Add(Buttons.Start);
                if (WasButtonJustReleased(Buttons.X, state.Buttons.X, index)) buttons.Add(Buttons.X);
                if (WasButtonJustReleased(Buttons.Y, state.Buttons.Y, index)) buttons.Add(Buttons.Y);
                if (WasButtonJustReleased(Buttons.DPadUp, state.DPad.Up, index)) buttons.Add(Buttons.DPadUp);
                if (WasButtonJustReleased(Buttons.DPadDown, state.DPad.Down, index)) buttons.Add(Buttons.DPadDown);
                if (WasButtonJustReleased(Buttons.DPadLeft, state.DPad.Left, index)) buttons.Add(Buttons.DPadLeft);
                if (WasButtonJustReleased(Buttons.DPadRight, state.DPad.Right, index)) buttons.Add(Buttons.DPadRight);
                if (WasButtonJustReleased(Buttons.LeftTrigger, state.Triggers.Left, index)) buttons.Add(Buttons.LeftTrigger);
                if (WasButtonJustReleased(Buttons.RightTrigger, state.Triggers.Right, index)) buttons.Add(Buttons.RightTrigger);
            }
            return buttons.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public static MethodInfo DrawFarmBuildings = typeof(Game1).GetMethod("drawFarmBuildings", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// 
        /// </summary>
        public static MethodInfo DrawHUD = typeof(Game1).GetMethod("drawHUD", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// 
        /// </summary>
        public static MethodInfo DrawDialogueBox = typeof(Game1).GetMethod("drawDialogueBox", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo CheckForEscapeKeys = typeof(Game1).GetMethod("checkForEscapeKeys", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo UpdateControlInput = typeof(Game1).GetMethod("UpdateControlInput", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo UpdateCharacters = typeof(Game1).GetMethod("UpdateCharacters", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo UpdateLocations = typeof(Game1).GetMethod("UpdateLocations", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo getViewportCenter = typeof(Game1).GetMethod("getViewportCenter", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MethodInfo UpdateTitleScreen = typeof(Game1).GetMethod("UpdateTitleScreen", BindingFlags.NonPublic | BindingFlags.Instance);

        public delegate void BaseBaseDraw();

        /// <summary>
        /// Whether or not the game's zoom level is 1.0f
        /// </summary>
        public bool ZoomLevelIsOne => options.zoomLevel.Equals(1.0f);

        /// <summary>
        /// XNA Init Method
        /// </summary>
        protected override void Initialize()
        {
            Log.AsyncY("XNA Initialize");
            //ModItems = new Dictionary<int, SObject>();
            DebugMessageQueue = new Queue<string>();
            PreviouslyPressedButtons = new Buttons[4][];
            for (var i = 0; i < 4; ++i) PreviouslyPressedButtons[i] = new Buttons[0];

            base.Initialize();
            GameEvents.InvokeInitialize();
        }

        /// <summary>
        /// XNA LC Method
        /// </summary>
        protected override void LoadContent()
        {
            Log.AsyncY("XNA LoadContent");
            base.LoadContent();
            GameEvents.InvokeLoadContent();
        }

        /// <summary>
        /// XNA Update Method
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            QueueDebugMessage("FPS: " + FramesPerSecond);
            UpdateEventCalls();

            /*
            if (ZoomLevelIsOne)
            {
                options.zoomLevel = 0.99f;
                InvokeBasePrivateInstancedMethod("Window_ClientSizeChanged", null, null);
            }
            */

            if (FramePressedKeys.Contains(Keys.F3))
            {
                Debug = !Debug;
            }

            if (FramePressedKeys.Contains(Keys.F2))
            {
                //Built-in debug mode
                debugMode = !debugMode;
            }

            if (Constants.EnableCompletelyOverridingBaseCalls)
            {
                #region Overridden Update Call

                try
                {
                    if (Program.BuildType == 0)
                        SteamHelper.update();
                    if ((paused /*|| !this.IsActive*/) && (options == null || options.pauseWhenOutOfFocus || paused))
                        return;
                    if (quit)
                        Exit();
                    currentGameTime = gameTime;
                    if (gameMode != 11)
                    {
                        if (IsMultiplayer && gameMode == 3)
                        {
                            if (multiplayerMode == 2)
                                server.receiveMessages();
                            else
                                client.receiveMessages();
                        }
                        if (IsActive)
                            InvokeMethodInfo(CheckForEscapeKeys);
                        //InvokeBasePrivateInstancedMethod("checkForEscapeKeys");

                        //this.checkForEscapeKeys();
                        updateMusic();
                        updateRaindropPosition();
                        bloom?.tick(gameTime);
                        if (globalFade)
                        {
                            if (!dialogueUp)
                            {
                                if (fadeIn)
                                {
                                    fadeToBlackAlpha = Math.Max(0.0f, fadeToBlackAlpha - globalFadeSpeed);
                                    if (fadeToBlackAlpha <= 0.0)
                                    {
                                        globalFade = false;
                                        if (afterFade != null)
                                        {
                                            afterFadeFunction afterFadeFunction = afterFade;
                                            afterFade();
                                            if (afterFade != null && afterFade.Equals(afterFadeFunction))
                                                afterFade = null;
                                            if (nonWarpFade)
                                                fadeToBlack = false;
                                        }
                                    }
                                }
                                else
                                {
                                    fadeToBlackAlpha = Math.Min(1f, fadeToBlackAlpha + globalFadeSpeed);
                                    if (fadeToBlackAlpha >= 1.0)
                                    {
                                        globalFade = false;
                                        if (afterFade != null)
                                        {
                                            afterFadeFunction afterFadeFunction = afterFade;
                                            afterFade();
                                            if (afterFade != null && afterFade.Equals(afterFadeFunction))
                                                afterFade = null;
                                            if (nonWarpFade)
                                                fadeToBlack = false;
                                        }
                                    }
                                }
                            }
                            else
                                InvokeMethodInfo(UpdateControlInput, gameTime);
                            //InvokeBasePrivateInstancedMethod("UpdateControlInput", gameTime);
                            //this.UpdateControlInput(gameTime);
                        }
                        else if (pauseThenDoFunctionTimer > 0)
                        {
                            freezeControls = true;
                            pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
                            if (pauseThenDoFunctionTimer <= 0)
                            {
                                freezeControls = false;
                                afterPause?.Invoke();
                            }
                        }
                        if (gameMode == 3 || gameMode == 2)
                        {
                            player.millisecondsPlayed += (uint) gameTime.ElapsedGameTime.Milliseconds;
                            bool flag = true;
                            if (currentMinigame != null)
                            {
                                if (pauseTime > 0.0)
                                    updatePause(gameTime);
                                if (fadeToBlack)
                                {
                                    updateScreenFade(gameTime);
                                    if (fadeToBlackAlpha >= 1.0)
                                        fadeToBlack = false;
                                }
                                else
                                {
                                    if (thumbstickMotionMargin > 0)
                                        thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
                                    if (IsActive)
                                    {
                                        KeyboardState state1 = Keyboard.GetState();
                                        MouseState state2 = Mouse.GetState();
                                        GamePadState state3 = GamePad.GetState(PlayerIndex.One);
                                        foreach (Keys keys in state1.GetPressedKeys())
                                        {
                                            if (!oldKBState.IsKeyDown(keys))
                                                currentMinigame.receiveKeyPress(keys);
                                        }
                                        if (options.gamepadControls)
                                        {
                                            if (currentMinigame == null)
                                            {
                                                oldMouseState = state2;
                                                oldKBState = state1;
                                                oldPadState = state3;
                                                return;
                                            }
                                            foreach (Buttons b in Utility.getPressedButtons(state3, oldPadState))
                                                currentMinigame.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
                                            if (currentMinigame == null)
                                            {
                                                oldMouseState = state2;
                                                oldKBState = state1;
                                                oldPadState = state3;
                                                return;
                                            }
                                            if (state3.ThumbSticks.Right.Y < -0.200000002980232 && oldPadState.ThumbSticks.Right.Y >= -0.200000002980232)
                                                currentMinigame.receiveKeyPress(Keys.Down);
                                            if (state3.ThumbSticks.Right.Y > 0.200000002980232 && oldPadState.ThumbSticks.Right.Y <= 0.200000002980232)
                                                currentMinigame.receiveKeyPress(Keys.Up);
                                            if (state3.ThumbSticks.Right.X < -0.200000002980232 && oldPadState.ThumbSticks.Right.X >= -0.200000002980232)
                                                currentMinigame.receiveKeyPress(Keys.Left);
                                            if (state3.ThumbSticks.Right.X > 0.200000002980232 && oldPadState.ThumbSticks.Right.X <= 0.200000002980232)
                                                currentMinigame.receiveKeyPress(Keys.Right);
                                            if (oldPadState.ThumbSticks.Right.Y < -0.200000002980232 && state3.ThumbSticks.Right.Y >= -0.200000002980232)
                                                currentMinigame.receiveKeyRelease(Keys.Down);
                                            if (oldPadState.ThumbSticks.Right.Y > 0.200000002980232 && state3.ThumbSticks.Right.Y <= 0.200000002980232)
                                                currentMinigame.receiveKeyRelease(Keys.Up);
                                            if (oldPadState.ThumbSticks.Right.X < -0.200000002980232 && state3.ThumbSticks.Right.X >= -0.200000002980232)
                                                currentMinigame.receiveKeyRelease(Keys.Left);
                                            if (oldPadState.ThumbSticks.Right.X > 0.200000002980232 && state3.ThumbSticks.Right.X <= 0.200000002980232)
                                                currentMinigame.receiveKeyRelease(Keys.Right);
                                            if (isGamePadThumbstickInMotion())
                                            {
                                                setMousePosition(getMouseX() + (int) (state3.ThumbSticks.Left.X * 16.0), getMouseY() - (int) (state3.ThumbSticks.Left.Y * 16.0));
                                                lastCursorMotionWasMouse = false;
                                            }
                                            else if (getMousePosition().X != getOldMouseX() || getMousePosition().Y != getOldMouseY())
                                                lastCursorMotionWasMouse = true;
                                        }
                                        foreach (Keys keys in oldKBState.GetPressedKeys())
                                        {
                                            if (!state1.IsKeyDown(keys))
                                                currentMinigame.receiveKeyRelease(keys);
                                        }
                                        if (options.gamepadControls)
                                        {
                                            if (currentMinigame == null)
                                            {
                                                oldMouseState = state2;
                                                oldKBState = state1;
                                                oldPadState = state3;
                                                return;
                                            }
                                            if (state3.IsConnected && state3.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
                                                currentMinigame.receiveRightClick(getMouseX(), getMouseY(), true);
                                            else if (state3.IsConnected && state3.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
                                                currentMinigame.receiveLeftClick(getMouseX(), getMouseY(), true);
                                            else if (state3.IsConnected && !state3.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X))
                                                currentMinigame.releaseRightClick(getMouseX(), getMouseY());
                                            else if (state3.IsConnected && !state3.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
                                                currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
                                            foreach (Buttons b in Utility.getPressedButtons(oldPadState, state3))
                                                currentMinigame.receiveKeyRelease(Utility.mapGamePadButtonToKey(b));
                                            if (state3.IsConnected && state3.IsButtonDown(Buttons.A))
                                                currentMinigame?.leftClickHeld(0, 0);
                                        }
                                        if (currentMinigame == null)
                                        {
                                            oldMouseState = state2;
                                            oldKBState = state1;
                                            oldPadState = state3;
                                            return;
                                        }
                                        if (state2.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
                                            currentMinigame.receiveLeftClick(getMouseX(), getMouseY(), true);
                                        if (state2.RightButton == ButtonState.Pressed && oldMouseState.RightButton != ButtonState.Pressed)
                                            currentMinigame.receiveRightClick(getMouseX(), getMouseY(), true);
                                        if (state2.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                                            currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
                                        if (state2.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
                                            currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
                                        if (state2.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
                                            currentMinigame.leftClickHeld(getMouseX(), getMouseY());
                                        oldMouseState = state2;
                                        oldKBState = state1;
                                        oldPadState = state3;
                                    }
                                    if (currentMinigame != null && currentMinigame.tick(gameTime))
                                    {
                                        currentMinigame.unload();
                                        currentMinigame = null;
                                        fadeIn = true;
                                        fadeToBlackAlpha = 1f;
                                        return;
                                    }
                                }
                                flag = IsMultiplayer;
                            }
                            else if (farmEvent != null && farmEvent.tickUpdate(gameTime))
                            {
                                farmEvent.makeChangesToLocation();
                                timeOfDay = 600;
                                UpdateOther(gameTime);
                                displayHUD = true;
                                farmEvent = null;
                                currentLocation = getLocationFromName("FarmHouse");
                                player.position = Utility.PointToVector2(Utility.getHomeOfFarmer(player).getBedSpot()) * tileSize;
                                player.position.X -= tileSize;
                                changeMusicTrack("none");
                                currentLocation.resetForPlayerEntry();
                                player.forceCanMove();
                                freezeControls = false;
                                displayFarmer = true;
                                outdoorLight = Color.White;
                                viewportFreeze = false;
                                fadeToBlackAlpha = 0.0f;
                                fadeToBlack = false;
                                globalFadeToClear(null, 0.02f);
                                player.mailForTomorrow.Clear();
                                showEndOfNightStuff();
                            }
                            if (flag)
                            {
                                if (endOfNightMenus.Count() > 0 && activeClickableMenu == null)
                                    activeClickableMenu = endOfNightMenus.Pop();
                                if (activeClickableMenu != null)
                                {
                                    updateActiveMenu(gameTime);
                                }
                                else
                                {
                                    if (pauseTime > 0.0)
                                        updatePause(gameTime);
                                    if (!globalFade && !freezeControls && (activeClickableMenu == null && IsActive))
                                        InvokeMethodInfo(UpdateControlInput, gameTime);
                                    //InvokeBasePrivateInstancedMethod("UpdateControlInput", gameTime);
                                    //this.UpdateControlInput(gameTime);
                                }
                                if (showingEndOfNightStuff && endOfNightMenus.Count() == 0 && activeClickableMenu == null)
                                {
                                    showingEndOfNightStuff = false;
                                    globalFadeToClear(playMorningSong, 0.02f);
                                }
                                if (!showingEndOfNightStuff)
                                {
                                    if (IsMultiplayer || activeClickableMenu == null && currentMinigame == null)
                                        UpdateGameClock(gameTime);
                                    //this.UpdateCharacters(gameTime);
                                    //this.UpdateLocations(gameTime);
                                    //InvokeBasePrivateInstancedMethod("UpdateCharacters", gameTime);
                                    //InvokeBasePrivateInstancedMethod("UpdateLocations", gameTime);
                                    //UpdateViewPort(false, (Point)InvokeBasePrivateInstancedMethod("getViewportCenter"));

                                    InvokeMethodInfo(UpdateCharacters, gameTime);
                                    InvokeMethodInfo(UpdateLocations, gameTime);
                                    UpdateViewPort(false, (Point) InvokeMethodInfo(getViewportCenter));
                                }
                                UpdateOther(gameTime);
                                if (messagePause)
                                {
                                    KeyboardState state1 = Keyboard.GetState();
                                    MouseState state2 = Mouse.GetState();
                                    GamePadState state3 = GamePad.GetState(PlayerIndex.One);
                                    if (isOneOfTheseKeysDown(state1, options.actionButton) && !isOneOfTheseKeysDown(oldKBState, options.actionButton))
                                        pressActionButton(state1, state2, state3);
                                    oldKBState = state1;
                                    oldPadState = state3;
                                }
                            }
                        }
                        else
                        {
                            //InvokeBasePrivateInstancedMethod("UpdateTitleScreen", gameTime);
                            InvokeMethodInfo(UpdateTitleScreen, gameTime);
                            //this.UpdateTitleScreen(gameTime);
                            if (activeClickableMenu != null)
                                updateActiveMenu(gameTime);
                            if (gameMode == 10)
                                UpdateOther(gameTime);
                        }
                        audioEngine?.Update();
                        if (multiplayerMode == 2 && gameMode == 3)
                            server.sendMessages(gameTime);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred in the overridden update loop: " + ex);
                }

                //typeof (Game).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] {gameTime});
                //base.Update(gameTime);

                #endregion
            }
            else
            {
                try
                {
                    base.Update(gameTime);
                }
                catch (Exception ex)
                {
                    Log.AsyncR("An error occured in the base update loop: " + ex);
                    Console.ReadKey();
                }
            }

            GameEvents.InvokeUpdateTick();
            if (FirstUpdate)
            {
                GameEvents.InvokeFirstUpdateTick();
                FirstUpdate = false;
            }

            if (CurrentUpdateTick % 2 == 0)
                GameEvents.InvokeSecondUpdateTick();

            if (CurrentUpdateTick % 4 == 0)
                GameEvents.InvokeFourthUpdateTick();

            if (CurrentUpdateTick % 8 == 0)
                GameEvents.InvokeEighthUpdateTick();

            if (CurrentUpdateTick % 15 == 0)
                GameEvents.InvokeQuarterSecondTick();

            if (CurrentUpdateTick % 30 == 0)
                GameEvents.InvokeHalfSecondTick();

            if (CurrentUpdateTick % 60 == 0)
                GameEvents.InvokeOneSecondTick();

            CurrentUpdateTick += 1;
            if (CurrentUpdateTick >= 60)
                CurrentUpdateTick = 0;

            if (KStatePrior != KStateNow)
                KStatePrior = KStateNow;

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                PreviouslyPressedButtons[(int) i] = GetButtonsDown(i);
            }
        }

        /// <summary>
        /// XNA Draw Method
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            FramesPerSecond = 1 / (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (Constants.EnableCompletelyOverridingBaseCalls)
            {
                #region Overridden Draw

                try
                {
                    if (!ZoomLevelIsOne)
                    {
                        GraphicsDevice.SetRenderTarget(Screen);
                    }

                    GraphicsDevice.Clear(BgColour);
                    if (options.showMenuBackground && activeClickableMenu != null && activeClickableMenu.showWithoutTransparencyIfOptionIsSet())
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        activeClickableMenu.drawBackground(spriteBatch);
                        GraphicsEvents.InvokeOnPreRenderGuiEvent(null, EventArgs.Empty);
                        activeClickableMenu.draw(spriteBatch);
                        GraphicsEvents.InvokeOnPostRenderGuiEvent(null, EventArgs.Empty);
                        spriteBatch.End();
                        if (!ZoomLevelIsOne)
                        {
                            GraphicsDevice.SetRenderTarget(null);
                            GraphicsDevice.Clear(BgColour);
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                            spriteBatch.End();
                        }
                        return;
                    }
                    if (gameMode == 11)
                    {
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        spriteBatch.DrawString(smoothFont, "Stardew Valley has crashed...", new Vector2(16f, 16f), Color.HotPink);
                        spriteBatch.DrawString(smoothFont, "Please send the error report or a screenshot of this message to @ConcernedApe. (http://stardewvalley.net/contact/)", new Vector2(16f, 32f), new Color(0, 255, 0));
                        spriteBatch.DrawString(smoothFont, parseText(errorMessage, smoothFont, graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                        spriteBatch.End();
                        return;
                    }
                    if (currentMinigame != null)
                    {
                        currentMinigame.draw(spriteBatch);
                        if (globalFade && !menuUp && (!nameSelectUp || messagePause))
                        {
                            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                            spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((gameMode == 0) ? (1f - fadeToBlackAlpha) : fadeToBlackAlpha));
                            spriteBatch.End();
                        }
                        if (!ZoomLevelIsOne)
                        {
                            GraphicsDevice.SetRenderTarget(null);
                            GraphicsDevice.Clear(BgColour);
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                            spriteBatch.End();
                        }
                        return;
                    }
                    if (showingEndOfNightStuff)
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        activeClickableMenu?.draw(spriteBatch);
                        spriteBatch.End();
                        if (!ZoomLevelIsOne)
                        {
                            GraphicsDevice.SetRenderTarget(null);
                            GraphicsDevice.Clear(BgColour);
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                            spriteBatch.End();
                        }
                        return;
                    }
                    if (gameMode == 6)
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        string text = "";
                        int num = 0;
                        while (num < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0)
                        {
                            text += ".";
                            num++;
                        }
                        SpriteText.drawString(spriteBatch, "Loading" + text, 64, graphics.GraphicsDevice.Viewport.Height - 64, 999, -1, 999, 1f, 1f, false, 0, "Loading...");
                        spriteBatch.End();
                        if (!ZoomLevelIsOne)
                        {
                            GraphicsDevice.SetRenderTarget(null);
                            GraphicsDevice.Clear(BgColour);
                            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                            spriteBatch.End();
                        }
                        return;
                    }
                    if (gameMode == 0)
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    }
                    else
                    {
                        if (drawLighting)
                        {
                            GraphicsDevice.SetRenderTarget(lightmap);
                            GraphicsDevice.Clear(Color.White * 0f);
                            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                            spriteBatch.Draw(staminaRect, lightmap.Bounds, currentLocation.name.Equals("UndergroundMine") ? mine.getLightingColor(gameTime) : ((!ambientLight.Equals(Color.White) && (!isRaining || !currentLocation.isOutdoors)) ? ambientLight : outdoorLight));
                            for (int i = 0; i < currentLightSources.Count; i++)
                            {
                                if (Utility.isOnScreen(currentLightSources.ElementAt(i).position, (int) (currentLightSources.ElementAt(i).radius * tileSize * 4f)))
                                {
                                    spriteBatch.Draw(currentLightSources.ElementAt(i).lightTexture, GlobalToLocal(viewport, currentLightSources.ElementAt(i).position) / options.lightingQuality, currentLightSources.ElementAt(i).lightTexture.Bounds, currentLightSources.ElementAt(i).color, 0f, new Vector2(currentLightSources.ElementAt(i).lightTexture.Bounds.Center.X, currentLightSources.ElementAt(i).lightTexture.Bounds.Center.Y), currentLightSources.ElementAt(i).radius / options.lightingQuality, SpriteEffects.None, 0.9f);
                                }
                            }
                            spriteBatch.End();
                            GraphicsDevice.SetRenderTarget(ZoomLevelIsOne ? null : Screen);
                        }
                        if (bloomDay)
                        {
                            bloom?.BeginDraw();
                        }
                        GraphicsDevice.Clear(BgColour);
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        GraphicsEvents.InvokeOnPreRenderEvent(null, EventArgs.Empty);
                        background?.draw(spriteBatch);
                        mapDisplayDevice.BeginScene(spriteBatch);
                        currentLocation.Map.GetLayer("Back").Draw(mapDisplayDevice, viewport, Location.Origin, false, pixelZoom);
                        currentLocation.drawWater(spriteBatch);
                        if (CurrentEvent == null)
                        {
                            using (List<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    NPC current = enumerator.Current;
                                    if (current != null && !current.swimming && !current.hideShadow && !current.IsMonster && !currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current.getTileLocation()))
                                    {
                                        spriteBatch.Draw(shadowTexture, GlobalToLocal(viewport, current.position + new Vector2(current.sprite.spriteWidth * pixelZoom / 2f, current.GetBoundingBox().Height + (current.IsMonster ? 0 : (pixelZoom * 3)))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), (pixelZoom + current.yJumpOffset / 40f) * current.scale, SpriteEffects.None, Math.Max(0f, current.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                goto IL_B30;
                            }
                        }
                        foreach (NPC current2 in CurrentEvent.actors)
                        {
                            if (!current2.swimming && !current2.hideShadow && !currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current2.getTileLocation()))
                            {
                                spriteBatch.Draw(shadowTexture, GlobalToLocal(viewport, current2.position + new Vector2(current2.sprite.spriteWidth * pixelZoom / 2f, current2.GetBoundingBox().Height + (current2.IsMonster ? 0 : (pixelZoom * 3)))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), (pixelZoom + current2.yJumpOffset / 40f) * current2.scale, SpriteEffects.None, Math.Max(0f, current2.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                        IL_B30:
                        if (!player.swimming && !player.isRidingHorse() && !currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(player.getTileLocation()))
                        {
                            spriteBatch.Draw(shadowTexture, GlobalToLocal(player.position + new Vector2(32f, 24f)), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), 4f - (((player.running || player.usingTool) && player.FarmerSprite.indexInCurrentAnimation > 1) ? (Math.Abs(FarmerRenderer.featureYOffsetPerFrame[player.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, 0f);
                        }
                        currentLocation.Map.GetLayer("Buildings").Draw(mapDisplayDevice, viewport, Location.Origin, false, pixelZoom);
                        mapDisplayDevice.EndScene();
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        if (CurrentEvent == null)
                        {
                            using (List<NPC>.Enumerator enumerator3 = currentLocation.characters.GetEnumerator())
                            {
                                while (enumerator3.MoveNext())
                                {
                                    NPC current3 = enumerator3.Current;
                                    if (current3 != null && !current3.swimming && !current3.hideShadow && currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current3.getTileLocation()))
                                    {
                                        spriteBatch.Draw(shadowTexture, GlobalToLocal(viewport, current3.position + new Vector2(current3.sprite.spriteWidth * pixelZoom / 2f, current3.GetBoundingBox().Height + (current3.IsMonster ? 0 : (pixelZoom * 3)))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), (pixelZoom + current3.yJumpOffset / 40f) * current3.scale, SpriteEffects.None, Math.Max(0f, current3.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                goto IL_F5F;
                            }
                        }
                        foreach (NPC current4 in CurrentEvent.actors)
                        {
                            if (!current4.swimming && !current4.hideShadow && currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(current4.getTileLocation()))
                            {
                                spriteBatch.Draw(shadowTexture, GlobalToLocal(viewport, current4.position + new Vector2(current4.sprite.spriteWidth * pixelZoom / 2f, current4.GetBoundingBox().Height + (current4.IsMonster ? 0 : (pixelZoom * 3)))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), (pixelZoom + current4.yJumpOffset / 40f) * current4.scale, SpriteEffects.None, Math.Max(0f, current4.getStandingY() / 10000f) - 1E-06f);
                            }
                        }
                        IL_F5F:
                        if (!player.swimming && !player.isRidingHorse() && currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(player.getTileLocation()))
                        {
                            spriteBatch.Draw(shadowTexture, GlobalToLocal(player.position + new Vector2(32f, 24f)), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), 4f - (((player.running || player.usingTool) && player.FarmerSprite.indexInCurrentAnimation > 1) ? (Math.Abs(FarmerRenderer.featureYOffsetPerFrame[player.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, Math.Max(0.0001f, player.getStandingY() / 10000f + 0.00011f) - 0.0001f);
                        }
                        if (displayFarmer)
                        {
                            player.draw(spriteBatch);
                        }
                        if ((eventUp || killScreen) && !killScreen)
                        {
                            currentLocation.currentEvent?.draw(spriteBatch);
                        }
                        if (player.currentUpgrade != null && player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && currentLocation.Name.Equals("Farm"))
                        {
                            spriteBatch.Draw(player.currentUpgrade.workerTexture, GlobalToLocal(viewport, player.currentUpgrade.positionOfCarpenter), player.currentUpgrade.getSourceRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (player.currentUpgrade.positionOfCarpenter.Y + tileSize * 3 / 4) / 10000f);
                        }
                        currentLocation.draw(spriteBatch);
                        if (eventUp && currentLocation.currentEvent?.messageToScreen != null)
                        {
                            drawWithBorder(currentLocation.currentEvent.messageToScreen, Color.Black, Color.White, new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Width / 2 - borderFont.MeasureString(currentLocation.currentEvent.messageToScreen).X / 2f, graphics.GraphicsDevice.Viewport.TitleSafeArea.Height - tileSize), 0f, 1f, 0.999f);
                        }
                        if (player.ActiveObject == null && (player.UsingTool || pickingTool) && player.CurrentTool != null && (!player.CurrentTool.Name.Equals("Seeds") || pickingTool))
                        {
                            drawTool(player);
                        }
                        if (currentLocation.Name.Equals("Farm"))
                        {
                            //typeof (Game1).GetMethod("drawFarmBuildings", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Program.gamePtr, null);
                            DrawFarmBuildings.Invoke(Program.gamePtr, null);
                            //this.drawFarmBuildings();
                        }
                        if (tvStation >= 0)
                        {
                            spriteBatch.Draw(tvStationTexture, GlobalToLocal(viewport, new Vector2(6 * tileSize + tileSize / 4, 2 * tileSize + tileSize / 2)), new Rectangle(tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                        }
                        if (panMode)
                        {
                            spriteBatch.Draw(fadeToBlackRect, new Rectangle((int) Math.Floor((getOldMouseX() + viewport.X) / (double) tileSize) * tileSize - viewport.X, (int) Math.Floor((getOldMouseY() + viewport.Y) / (double) tileSize) * tileSize - viewport.Y, tileSize, tileSize), Color.Lime * 0.75f);
                            foreach (Warp current5 in currentLocation.warps)
                            {
                                spriteBatch.Draw(fadeToBlackRect, new Rectangle(current5.X * tileSize - viewport.X, current5.Y * tileSize - viewport.Y, tileSize, tileSize), Color.Red * 0.75f);
                            }
                        }
                        mapDisplayDevice.BeginScene(spriteBatch);
                        currentLocation.Map.GetLayer("Front").Draw(mapDisplayDevice, viewport, Location.Origin, false, pixelZoom);
                        mapDisplayDevice.EndScene();
                        currentLocation.drawAboveFrontLayer(spriteBatch);
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        if (currentLocation.Name.Equals("Farm") && stats.SeedsSown >= 200u)
                        {
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(3 * tileSize + tileSize / 4, tileSize + tileSize / 3)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(4 * tileSize + tileSize, 2 * tileSize + tileSize)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(5 * tileSize, 2 * tileSize)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(3 * tileSize + tileSize / 2, 3 * tileSize)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(5 * tileSize - tileSize / 4, tileSize)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(4 * tileSize, 3 * tileSize + tileSize / 6)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                            spriteBatch.Draw(debrisSpriteSheet, GlobalToLocal(viewport, new Vector2(4 * tileSize + tileSize / 5, 2 * tileSize + tileSize / 3)), getSourceRectForStandardTileSheet(debrisSpriteSheet, 16), Color.White);
                        }
                        if (displayFarmer && player.ActiveObject != null && player.ActiveObject.bigCraftable && checkBigCraftableBoundariesForFrontLayer() && currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX(), player.getStandingY()), viewport.Size) == null)
                        {
                            drawPlayerHeldObject(player);
                        }
                        else if (displayFarmer && player.ActiveObject != null && ((currentLocation.Map.GetLayer("Front").PickTile(new Location((int) player.position.X, (int) player.position.Y - tileSize * 3 / 5), viewport.Size) != null && !currentLocation.Map.GetLayer("Front").PickTile(new Location((int) player.position.X, (int) player.position.Y - tileSize * 3 / 5), viewport.Size).TileIndexProperties.ContainsKey("FrontAlways")) || (currentLocation.Map.GetLayer("Front").PickTile(new Location(player.GetBoundingBox().Right, (int) player.position.Y - tileSize * 3 / 5), viewport.Size) != null && !currentLocation.Map.GetLayer("Front").PickTile(new Location(player.GetBoundingBox().Right, (int) player.position.Y - tileSize * 3 / 5), viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))))
                        {
                            drawPlayerHeldObject(player);
                        }
                        if ((player.UsingTool || pickingTool) && player.CurrentTool != null && (!player.CurrentTool.Name.Equals("Seeds") || pickingTool) && currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX(), (int) player.position.Y - tileSize * 3 / 5), viewport.Size) != null && currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX(), player.getStandingY()), viewport.Size) == null)
                        {
                            drawTool(player);
                        }
                        if (currentLocation.Map.GetLayer("AlwaysFront") != null)
                        {
                            mapDisplayDevice.BeginScene(spriteBatch);
                            currentLocation.Map.GetLayer("AlwaysFront").Draw(mapDisplayDevice, viewport, Location.Origin, false, pixelZoom);
                            mapDisplayDevice.EndScene();
                        }
                        if (toolHold > 400f && player.CurrentTool.UpgradeLevel >= 1 && player.canReleaseTool)
                        {
                            Color color = Color.White;
                            switch ((int) (toolHold / 600f) + 2)
                            {
                                case 1:
                                    color = Tool.copperColor;
                                    break;
                                case 2:
                                    color = Tool.steelColor;
                                    break;
                                case 3:
                                    color = Tool.goldColor;
                                    break;
                                case 4:
                                    color = Tool.iridiumColor;
                                    break;
                            }
                            spriteBatch.Draw(littleEffect, new Rectangle((int) player.getLocalPosition(viewport).X - 2, (int) player.getLocalPosition(viewport).Y - (player.CurrentTool.Name.Equals("Watering Can") ? 0 : tileSize) - 2, (int) (toolHold % 600f * 0.08f) + 4, tileSize / 8 + 4), Color.Black);
                            spriteBatch.Draw(littleEffect, new Rectangle((int) player.getLocalPosition(viewport).X, (int) player.getLocalPosition(viewport).Y - (player.CurrentTool.Name.Equals("Watering Can") ? 0 : tileSize), (int) (toolHold % 600f * 0.08f), tileSize / 8), color);
                        }
                        if (isDebrisWeather && currentLocation.IsOutdoors && !currentLocation.ignoreDebrisWeather && !currentLocation.Name.Equals("Desert") && viewport.X > -10)
                        {
                            foreach (WeatherDebris current6 in debrisWeather)
                            {
                                current6.draw(spriteBatch);
                            }
                        }
                        farmEvent?.draw(spriteBatch);
                        if (currentLocation.LightLevel > 0f && timeOfDay < 2000)
                        {
                            spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * currentLocation.LightLevel);
                        }
                        if (screenGlow)
                        {
                            spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, screenGlowColor * screenGlowAlpha);
                        }
                        currentLocation.drawAboveAlwaysFrontLayer(spriteBatch);
                        if (player.CurrentTool is FishingRod && ((player.CurrentTool as FishingRod).isTimingCast || (player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (player.CurrentTool as FishingRod).fishCaught || (player.CurrentTool as FishingRod).showingTreasure))
                        {
                            player.CurrentTool.draw(spriteBatch);
                        }
                        if (isRaining && currentLocation.IsOutdoors && !currentLocation.Name.Equals("Desert") && !(currentLocation is Summit) && (!eventUp || currentLocation.isTileOnMap(new Vector2(viewport.X / tileSize, viewport.Y / tileSize))))
                        {
                            for (int j = 0; j < rainDrops.Length; j++)
                            {
                                spriteBatch.Draw(rainTexture, rainDrops[j].position, getSourceRectForStandardTileSheet(rainTexture, rainDrops[j].frame), Color.White);
                            }
                        }

                        spriteBatch.End();

                        //base.Draw(gameTime);

                        spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        if (eventUp && currentLocation.currentEvent != null)
                        {
                            foreach (NPC current7 in currentLocation.currentEvent.actors)
                            {
                                if (current7.isEmoting)
                                {
                                    Vector2 localPosition = current7.getLocalPosition(viewport);
                                    localPosition.Y -= tileSize * 2 + pixelZoom * 3;
                                    if (current7.age == 2)
                                    {
                                        localPosition.Y += tileSize / 2;
                                    }
                                    else if (current7.gender == 1)
                                    {
                                        localPosition.Y += tileSize / 6;
                                    }
                                    spriteBatch.Draw(emoteSpriteSheet, localPosition, new Rectangle(current7.CurrentEmoteIndex * (tileSize / 4) % emoteSpriteSheet.Width, current7.CurrentEmoteIndex * (tileSize / 4) / emoteSpriteSheet.Width * (tileSize / 4), tileSize / 4, tileSize / 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, current7.getStandingY() / 10000f);
                                }
                            }
                        }
                        spriteBatch.End();
                        if (drawLighting)
                        {
                            spriteBatch.Begin(SpriteSortMode.Deferred, new BlendState
                            {
                                ColorBlendFunction = BlendFunction.ReverseSubtract,
                                ColorDestinationBlend = Blend.One,
                                ColorSourceBlend = Blend.SourceColor
                            }, SamplerState.LinearClamp, null, null);
                            spriteBatch.Draw(lightmap, Vector2.Zero, lightmap.Bounds, Color.White, 0f, Vector2.Zero, options.lightingQuality, SpriteEffects.None, 1f);
                            if (isRaining && currentLocation.isOutdoors && !(currentLocation is Desert))
                            {
                                spriteBatch.Draw(staminaRect, graphics.GraphicsDevice.Viewport.Bounds, Color.OrangeRed * 0.45f);
                            }
                            spriteBatch.End();
                        }
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                        if (drawGrid)
                        {
                            int num2 = -viewport.X % tileSize;
                            float num3 = -(float) viewport.Y % tileSize;
                            for (int k = num2; k < graphics.GraphicsDevice.Viewport.Width; k += tileSize)
                            {
                                spriteBatch.Draw(staminaRect, new Rectangle(k, (int) num3, 1, graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                            }
                            for (float num4 = num3; num4 < (float) graphics.GraphicsDevice.Viewport.Height; num4 += (float) tileSize)
                            {
                                spriteBatch.Draw(staminaRect, new Rectangle(num2, (int) num4, graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                            }
                        }
                        if (currentBillboard != 0)
                        {
                            drawBillboard();
                        }

                        GraphicsEvents.InvokeOnPreRenderHudEventNoCheck(null, EventArgs.Empty);
                        if ((displayHUD || eventUp) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode)
                        {
                            GraphicsEvents.InvokeOnPreRenderHudEvent(null, EventArgs.Empty);
                            //typeof (Game1).GetMethod("drawHUD", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Program.gamePtr, null);
                            DrawHUD.Invoke(Program.gamePtr, null);
                            GraphicsEvents.InvokeOnPostRenderHudEvent(null, EventArgs.Empty);
                            //this.drawHUD();
                        }
                        else if (activeClickableMenu == null && farmEvent == null)
                        {
                            spriteBatch.Draw(mouseCursors, new Vector2(getOldMouseX(), getOldMouseY()), getSourceRectForStandardTileSheet(mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + dialogueButtonScale / 150f, SpriteEffects.None, 1f);
                        }
                        GraphicsEvents.InvokeOnPostRenderHudEventNoCheck(null, EventArgs.Empty);

                        if (hudMessages.Any() && (!eventUp || isFestival()))
                        {
                            for (int l = hudMessages.Count - 1; l >= 0; l--)
                            {
                                hudMessages[l].draw(spriteBatch, l);
                            }
                        }
                    }
                    farmEvent?.draw(spriteBatch);
                    if (dialogueUp && !nameSelectUp && !messagePause && !(activeClickableMenu is DialogueBox))
                    {
                        //typeof (Game1).GetMethod("drawDialogueBox", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Program.gamePtr, null);
                        DrawDialogueBox.Invoke(Program.gamePtr, null);
                        //this.drawDialogueBox();
                    }
                    if (progressBar)
                    {
                        spriteBatch.Draw(fadeToBlackRect, new Rectangle((graphics.GraphicsDevice.Viewport.TitleSafeArea.Width - dialogueWidth) / 2, graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - tileSize * 2, dialogueWidth, tileSize / 2), Color.LightGray);
                        spriteBatch.Draw(staminaRect, new Rectangle((graphics.GraphicsDevice.Viewport.TitleSafeArea.Width - dialogueWidth) / 2, graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - tileSize * 2, (int) (pauseAccumulator / pauseTime * dialogueWidth), tileSize / 2), Color.DimGray);
                    }
                    if (eventUp)
                    {
                        currentLocation.currentEvent?.drawAfterMap(spriteBatch);
                    }
                    if (isRaining && currentLocation.isOutdoors && !(currentLocation is Desert))
                    {
                        spriteBatch.Draw(staminaRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
                    }
                    if ((fadeToBlack || globalFade) && !menuUp && (!nameSelectUp || messagePause))
                    {
                        spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((gameMode == 0) ? (1f - fadeToBlackAlpha) : fadeToBlackAlpha));
                    }
                    else if (flashAlpha > 0f)
                    {
                        if (options.screenFlash)
                        {
                            spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, flashAlpha));
                        }
                        flashAlpha -= 0.1f;
                    }
                    if ((messagePause || globalFade) && dialogueUp)
                    {
                        //typeof (Game1).GetMethod("drawDialogueBox", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Program.gamePtr, null);
                        DrawDialogueBox.Invoke(Program.gamePtr, null);
                        //this.drawDialogueBox();
                    }
                    foreach (TemporaryAnimatedSprite current8 in screenOverlayTempSprites)
                    {
                        current8.draw(spriteBatch, true);
                    }
                    if (debugMode)
                    {
                        spriteBatch.DrawString(smallFont, string.Concat(new object[]
                        {
                            panMode ? ((getOldMouseX() + viewport.X) / tileSize + "," + (getOldMouseY() + viewport.Y) / tileSize) : string.Concat("aplayer: ", player.getStandingX() / tileSize, ", ", player.getStandingY() / tileSize),
                            Environment.NewLine,
                            "debugOutput: ",
                            debugOutput
                        }), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                    }
                    //if (inputMode)
                    //{
                    //    spriteBatch.DrawString(smallFont, "Input: " + debugInput, new Vector2(tileSize, tileSize * 3), Color.Purple);
                    //}
                    //if (showKeyHelp)
                    //{
                    //    spriteBatch.DrawString(smallFont, keyHelpString, new Vector2(tileSize, viewport.Height - tileSize - (dialogueUp ? (tileSize * 3 + (isQuestion ? (questionChoices.Count * tileSize) : 0)) : 0) - smallFont.MeasureString(keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
                    //}

                    GraphicsEvents.InvokeOnPreRenderGuiEventNoCheck(null, EventArgs.Empty);
                    if (activeClickableMenu != null)
                    {
                        GraphicsEvents.InvokeOnPreRenderGuiEvent(null, EventArgs.Empty);
                        activeClickableMenu.draw(spriteBatch);
                        GraphicsEvents.InvokeOnPostRenderGuiEvent(null, EventArgs.Empty);
                    }
                    else
                    {
                        farmEvent?.drawAboveEverything(spriteBatch);
                    }
                    GraphicsEvents.InvokeOnPostRenderGuiEventNoCheck(null, EventArgs.Empty);

                    GraphicsEvents.InvokeOnPostRenderEvent(null, EventArgs.Empty);
                    spriteBatch.End();

                    GraphicsEvents.InvokeDrawInRenderTargetTick();

                    if (!ZoomLevelIsOne)
                    {
                        GraphicsDevice.SetRenderTarget(null);
                        GraphicsDevice.Clear(BgColour);
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                        spriteBatch.End();
                    }

                    GraphicsEvents.InvokeDrawTick();
                }
                catch (Exception ex)
                {
                    Log.Error("An error occured in the overridden draw loop: " + ex);
                }

                #endregion
            }
            else
            {
                #region Base Draw Call

                try
                {
                    base.Draw(gameTime);
                }
                catch (Exception ex)
                {
                    Log.AsyncR("An error occured in the base draw loop: " + ex);
                    Console.ReadKey();
                }

                GraphicsEvents.InvokeDrawTick();

                if (Constants.EnableDrawingIntoRenderTarget)
                {
                    if (!options.zoomLevel.Equals(1.0f))
                    {
                        if (Screen.RenderTargetUsage == RenderTargetUsage.DiscardContents)
                        {
                            Screen = new RenderTarget2D(graphics.GraphicsDevice, Math.Min(4096, (int) (Window.ClientBounds.Width * (1.0 / options.zoomLevel))),
                                Math.Min(4096, (int) (Window.ClientBounds.Height * (1.0 / options.zoomLevel))),
                                false, SurfaceFormat.Color, DepthFormat.Depth16, 1, RenderTargetUsage.PreserveContents);
                        }
                        GraphicsDevice.SetRenderTarget(Screen);
                    }

                    // Not beginning the batch due to inconsistancies with the standard draw tick...
                    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                    GraphicsEvents.InvokeDrawInRenderTargetTick();

                    //spriteBatch.End();

                    //Re-draw the HUD
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    activeClickableMenu?.draw(spriteBatch);
                    /*
                if ((displayHUD || eventUp) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode)
                    typeof (Game1).GetMethod("drawHUD", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(Program.gamePtr, null);
                */
                    spriteBatch.Draw(mouseCursors, new Vector2(getOldMouseX(), getOldMouseY()), getSourceRectForStandardTileSheet(mouseCursors, options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0.0f, Vector2.Zero, pixelZoom + dialogueButtonScale / 150f, SpriteEffects.None, 1f);

                    spriteBatch.End();

                    if (!options.zoomLevel.Equals(1.0f))
                    {
                        GraphicsDevice.SetRenderTarget(null);
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                        spriteBatch.Draw(Screen, Vector2.Zero, Screen.Bounds, Color.White, 0.0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
                        spriteBatch.End();
                    }
                }

                #endregion
            }

            if (Debug)
            {
                spriteBatch.Begin();

                int i = 0;
                while (DebugMessageQueue.Any())
                {
                    string s = DebugMessageQueue.Dequeue();
                    spriteBatch.DrawString(smoothFont, s, new Vector2(0, i * 14), Color.CornflowerBlue);
                    i++;
                }
                GraphicsEvents.InvokeDrawDebug(null, EventArgs.Empty);

                spriteBatch.End();
            }
            else
            {
                DebugMessageQueue.Clear();
            }
        }

        [Obsolete("Do not use at this time.")]
        // ReSharper disable once UnusedMember.Local
        private static int RegisterModItem(SObject modItem)
        {
            if (modItem.HasBeenRegistered)
            {
                Log.AsyncR($"The item {modItem.Name} has already been registered with ID {modItem.RegisteredId}");
                return modItem.RegisteredId;
            }
            var newId = LowestModItemID;
            if (ModItems.Count > 0)
                newId = Math.Max(LowestModItemID, ModItems.OrderBy(x => x.Key).First().Key + 1);
            ModItems.Add(newId, modItem);
            modItem.HasBeenRegistered = true;
            modItem.RegisteredId = newId;
            return newId;
        }

        [Obsolete("Do not use at this time.")]
        // ReSharper disable once UnusedMember.Local
        private static SObject PullModItemFromDict(int id, bool isIndex)
        {
            if (isIndex)
            {
                if (ModItems.ElementAtOrDefault(id).Value != null)
                {
                    return ModItems.ElementAt(id).Value.Clone();
                }
                Log.AsyncR("ModItem Dictionary does not contain index: " + id);
                return null;
            }
            if (ModItems.ContainsKey(id))
            {
                return ModItems[id].Clone();
            }
            Log.AsyncR("ModItem Dictionary does not contain ID: " + id);
            return null;
        }

        private void UpdateEventCalls()
        {
            KStateNow = Keyboard.GetState();

            MStateNow = Mouse.GetState();

            foreach (var k in FramePressedKeys)
                ControlEvents.InvokeKeyPressed(k);

            foreach (var k in FrameReleasedKeys)
                ControlEvents.InvokeKeyReleased(k);

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                var buttons = GetFramePressedButtons(i);
                foreach (var b in buttons)
                {
                    if (b == Buttons.LeftTrigger || b == Buttons.RightTrigger)
                    {
                        ControlEvents.InvokeTriggerPressed(i, b, b == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                    }
                    else
                    {
                        ControlEvents.InvokeButtonPressed(i, b);
                    }
                }
            }

            for (var i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                foreach (var b in GetFrameReleasedButtons(i))
                {
                    if (b == Buttons.LeftTrigger || b == Buttons.RightTrigger)
                    {
                        ControlEvents.InvokeTriggerReleased(i, b, b == Buttons.LeftTrigger ? GamePad.GetState(i).Triggers.Left : GamePad.GetState(i).Triggers.Right);
                    }
                    else
                    {
                        ControlEvents.InvokeButtonReleased(i, b);
                    }
                }
            }


            if (KStateNow != KStatePrior)
            {
                ControlEvents.InvokeKeyboardChanged(KStatePrior, KStateNow);
            }

            if (MStateNow != MStatePrior)
            {
                ControlEvents.InvokeMouseChanged(MStatePrior, MStateNow);
                MStatePrior = MStateNow;
            }

            if (activeClickableMenu != null && activeClickableMenu != PreviousActiveMenu)
            {
                MenuEvents.InvokeMenuChanged(PreviousActiveMenu, activeClickableMenu);
                PreviousActiveMenu = activeClickableMenu;
                WasMenuClosedInvoked = false;
            }

            if (!WasMenuClosedInvoked && PreviousActiveMenu != null && activeClickableMenu == null)
            {
                MenuEvents.InvokeMenuClosed(PreviousActiveMenu);
                WasMenuClosedInvoked = true;
            }

            if (locations.GetHash() != PreviousGameLocations)
            {
                LocationEvents.InvokeLocationsChanged(locations);
                PreviousGameLocations = locations.GetHash();
            }

            if (currentLocation != PreviousGameLocation)
            {
                LocationEvents.InvokeCurrentLocationChanged(PreviousGameLocation, currentLocation);
                PreviousGameLocation = currentLocation;
            }

            if (player != null && player != PreviousFarmer)
            {
                PlayerEvents.InvokeFarmerChanged(PreviousFarmer, player);
                PreviousFarmer = player;
            }

            if (player != null && player.combatLevel != PreviousCombatLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Combat, player.combatLevel);
                PreviousCombatLevel = player.combatLevel;
            }

            if (player != null && player.farmingLevel != PreviousFarmingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Farming, player.farmingLevel);
                PreviousFarmingLevel = player.farmingLevel;
            }

            if (player != null && player.fishingLevel != PreviousFishingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Fishing, player.fishingLevel);
                PreviousFishingLevel = player.fishingLevel;
            }

            if (player != null && player.foragingLevel != PreviousForagingLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Foraging, player.foragingLevel);
                PreviousForagingLevel = player.foragingLevel;
            }

            if (player != null && player.miningLevel != PreviousMiningLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Mining, player.miningLevel);
                PreviousMiningLevel = player.miningLevel;
            }

            if (player != null && player.luckLevel != PreviousLuckLevel)
            {
                PlayerEvents.InvokeLeveledUp(EventArgsLevelUp.LevelType.Luck, player.luckLevel);
                PreviousLuckLevel = player.luckLevel;
            }

            List<ItemStackChange> changedItems;
            if (player != null && HasInventoryChanged(player.items, out changedItems))
            {
                PlayerEvents.InvokeInventoryChanged(player.items, changedItems);
                PreviousItems = player.items.Where(n => n != null).ToDictionary(n => n, n => n.Stack);
            }

            var objectHash = currentLocation?.objects?.GetHash();
            if (objectHash != null && PreviousLocationObjects != objectHash)
            {
                LocationEvents.InvokeOnNewLocationObject(currentLocation.objects);
                PreviousLocationObjects = objectHash ?? -1;
            }

            if (timeOfDay != PreviousTimeOfDay)
            {
                TimeEvents.InvokeTimeOfDayChanged(PreviousTimeOfDay, timeOfDay);
                PreviousTimeOfDay = timeOfDay;
            }

            if (dayOfMonth != PreviousDayOfMonth)
            {
                TimeEvents.InvokeDayOfMonthChanged(PreviousDayOfMonth, dayOfMonth);
                PreviousDayOfMonth = dayOfMonth;
            }

            if (currentSeason != PreviousSeasonOfYear)
            {
                TimeEvents.InvokeSeasonOfYearChanged(PreviousSeasonOfYear, currentSeason);
                PreviousSeasonOfYear = currentSeason;
            }

            if (year != PreviousYearOfGame)
            {
                TimeEvents.InvokeYearOfGameChanged(PreviousYearOfGame, year);
                PreviousYearOfGame = year;
            }

            //NOTE THAT THIS MUST CHECK BEFORE SETTING IT TO TRUE BECAUSE OF SOME SILLY ISSUES
            if (FireLoadedGameEvent)
            {
                PlayerEvents.InvokeLoadedGame(new EventArgsLoadedGameChanged(hasLoadedGame));
                FireLoadedGameEvent = false;
            }

            if (hasLoadedGame != PreviouslyLoadedGame)
            {
                FireLoadedGameEvent = true;
                PreviouslyLoadedGame = hasLoadedGame;
            }

            if (mine != null && PreviousMineLevel != mine.mineLevel)
            {
                MineEvents.InvokeMineLevelChanged(PreviousMineLevel, mine.mineLevel);
                PreviousMineLevel = mine.mineLevel;
            }

            if (PreviousIsNewDay != newDay)
            {
                TimeEvents.InvokeOnNewDay(PreviousDayOfMonth, dayOfMonth, newDay);
                PreviousIsNewDay = newDay;
            }
        }

        private bool HasInventoryChanged(List<Item> items, out List<ItemStackChange> changedItems)
        {
            changedItems = new List<ItemStackChange>();
            IEnumerable<Item> actualItems = items.Where(n => n != null).ToArray();
            foreach (var item in actualItems)
            {
                if (PreviousItems != null && PreviousItems.ContainsKey(item))
                {
                    if (PreviousItems[item] != item.Stack)
                    {
                        changedItems.Add(new ItemStackChange {Item = item, StackChange = item.Stack - PreviousItems[item], ChangeType = ChangeType.StackChange});
                    }
                }
                else
                {
                    changedItems.Add(new ItemStackChange {Item = item, StackChange = item.Stack, ChangeType = ChangeType.Added});
                }
            }

            if (PreviousItems != null)
            {
                changedItems.AddRange(PreviousItems.Where(n => actualItems.All(i => i != n.Key)).Select(n =>
                    new ItemStackChange {Item = n.Key, StackChange = -n.Key.Stack, ChangeType = ChangeType.Removed}));
            }

            return changedItems.Any();
        }

        /// <summary>
        /// Invokes a private, non-static method in Game1 via Reflection
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="parameters">Any parameters needed</param>
        /// <returns>Whatever the method normally returns. Null if void.</returns>
        [Obsolete("This is very slow. Cache the method info and then invoke it with InvokeMethodInfo().")]
        public static object InvokeBasePrivateInstancedMethod(string name, params object[] parameters)
        {
            try
            {
                return typeof(Game1).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Program.gamePtr, parameters);
            }
            catch
            {
                Log.AsyncR("Failed to call base method: " + name);
                return null;
            }
        }

        /// <summary>
        /// Invokes a given method info with the supplied parameters
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object InvokeMethodInfo(MethodInfo mi, params object[] parameters)
        {
            try
            {
                return mi.Invoke(Program.gamePtr, parameters);
            }
            catch
            {
                Log.AsyncR("Failed to call base method: " + mi.Name);
                return null;
            }
        }

        /// <summary>
        /// Queue's a message to be drawn in Debug mode (F3)
        /// </summary>
        /// <returns></returns>
        public static bool QueueDebugMessage(string message)
        {
            if (!Debug)
                return false;

            if (DebugMessageQueue.Count > 32)
                return false;

            DebugMessageQueue.Enqueue(message);
            return true;
        }
    }
}