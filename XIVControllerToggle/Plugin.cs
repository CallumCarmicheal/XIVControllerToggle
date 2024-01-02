using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using XIVControllerToggle.Windows;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Game.Config;
using Dalamud.Game;
using TPie.Helpers;
using System;

using VK = Dalamud.Game.ClientState.Keys.VirtualKey;

namespace XIVControllerToggle {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "The Great Controller Hud Switcher";
        private const string CommandName = "/controllerhudcfg";

        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("ControllerHudSwitcher");

        private ConfigWindow ConfigWindow { get; init; }
        private DebugWindow DebugWindow { get; init; }

        private static DalamudPluginInterface PluginInterface { get; set; } = null!;
        public static ICommandManager CommandManager { get; set; } = null!;
        public static IFramework Framework { get; private set; } = null!;
        public static IPluginLog Log { get; private set; } = null!;
        public static IGameGui GameGui { get; private set; } = null!;
        public static IGameConfig GameConfig { get; private set; } = null!;
        public static IKeyState KeyState { get; private set; } = null!;
        public static ISigScanner SigScanner { get; private set; } = null!;
        public static IGamepadState GamepadState { get; private set; } = null!;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] IFramework framework,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IPluginLog pluginLog,
            [RequiredVersion("1.0")] IGameGui gameGui,
            [RequiredVersion("1.0")] IGameConfig gameConfig,
            [RequiredVersion("1.0")] ISigScanner sigScanner,
            [RequiredVersion("1.0")] IKeyState keyState,
            [RequiredVersion("1.0")] IGamepadState gamepadState
        ) {
            PluginInterface = pluginInterface;
            Framework = framework;
            CommandManager = commandManager;
            Log = pluginLog;
            GameGui = gameGui;
            GameConfig = gameConfig;
            KeyState = keyState;
            SigScanner = sigScanner;
            GamepadState = gamepadState;

            Log.Info("Startup!");

            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(PluginInterface, this);
            this.Configuration.ClampValues();

            ConfigWindow = new ConfigWindow(this);
            DebugWindow = new DebugWindow(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(DebugWindow);

            setupAndRemoveCommands(true);

            CommandManager.AddHandler(CommandName, new CommandInfo(CmdOpenSettings) {
                HelpMessage = "Open settings for MK Controller Switcher"
            });

            Framework.Update += Update;
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            ChatHelper.Initialize();
        }
        
        private void setupAndRemoveCommands(bool add) {
            if (add) {
                CommandManager.AddHandler(CommandName, new CommandInfo(CmdOpenSettings) {
                    HelpMessage = "Open settings for MK Controller Switcher"
                });

                CommandManager.AddHandler("/controller", new CommandInfo(CmdSetController) {
                    HelpMessage = "Open settings for MK Controller Switcher"
                });

                CommandManager.AddHandler("/kbm", new CommandInfo(CmdSetMouse) {
                    HelpMessage = "Open settings for MK Controller Switcher"
                });

                CommandManager.AddHandler("/togglecontroller", new CommandInfo(CmdToggleController) {
                    HelpMessage = "Open settings for MK Controller Switcher"
                });
            } else {
                CommandManager.RemoveHandler(CommandName);
                CommandManager.RemoveHandler("/controller");
                CommandManager.RemoveHandler("/kbm");
                CommandManager.RemoveHandler("/togglecontroller");
            }
        }

        private void CmdToggleController(string command, string arguments) {
            PerformControllerKeyboardSwitch();
        }

        private void CmdSetMouse(string command, string arguments) {
            PerformControllerKeyboardSwitch(false);
        }

        private void CmdSetController(string command, string arguments) {
            PerformControllerKeyboardSwitch(true);
        }

        private void Update(IFramework framework) {
            Update_ProcessKeypresses();
        }

        private DateTime controllerProcessDelay = DateTime.Now;
        private void Update_ProcessKeypresses() {
            // Run every 250ms
            if (DateTime.Now < controllerProcessDelay) return;
            if (Configuration.Enabled == false) return; // Dont run if disabled

            var gs = GamepadState;
            var ks = KeyState;

            GameConfig.TryGet(UiConfigOption.PadMode, out uint padMode_i);
            bool currentlyPadMode = padMode_i == 1;

            var ls = Math.Max(gs.LeftStick.X.abs(), gs.LeftStick.Y.abs());
            var rs = Math.Max(gs.RightStick.X.abs(), gs.RightStick.Y.abs());
            var max = Configuration.ControllerSticks switch {
                ControllerSticks.Both => Math.Max(ls, rs),
                ControllerSticks.LS => ls,
                ControllerSticks.RS => rs,
                _ => Math.Max(ls, rs),
            };

            bool swap = false;
            if (currentlyPadMode) {
                // If ALT is not being pressed and (wasd) being pressed with the chat not being active
                if (ks[VK.MENU] == false && (ks[VK.W] || ks[VK.S] || ks[VK.A] || ks[VK.D]) && !ChatHelper.IsInputTextActive()) {
                    swap = true;
                }
            } else {
                // Our deadzone modifier is 25
                if (max >= 25) {
                    swap = true;
                }
            }

            if (swap) {
                PerformControllerKeyboardSwitch();
            }

            controllerProcessDelay = DateTime.Now.AddMilliseconds(250);
        }

        private void DrawUI() {
            this.WindowSystem.Draw();
        }

        public void Dispose() {
            this.WindowSystem.RemoveAllWindows();

            Framework.Update -= Update;
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

            ConfigWindow.Dispose();
            DebugWindow.Dispose();
            ChatHelper.Instance?.Dispose();

            CommandManager.RemoveHandler(CommandName);
            setupAndRemoveCommands(false);
        }

        private void CmdOpenSettings(string command, string args) {
            ConfigWindow.IsOpen = true;
        }

        public void DrawConfigUI() => ConfigWindow.IsOpen = true;
        public void DrawDebugUI() => DebugWindow.IsOpen = true;

        public static DateTime SwapTimeout = DateTime.Now;
        public void PerformControllerKeyboardSwitch(bool? forceSwitchToController = null) {
            // Delay swapping to once a second (As chat commands are involved).
            if (SwapTimeout > DateTime.Now)
                return;

            var gameConfig = Plugin.GameConfig;

            gameConfig.TryGet(UiConfigOption.PadMode, out uint padMode_i);
            bool currentlyPadMode = padMode_i == 1;

            // If we are forcefully switching to a selected layout then specify it here.
            if (forceSwitchToController != null) 
                currentlyPadMode = forceSwitchToController.Value;
            else
                // Invert pad mode if not specified
                currentlyPadMode = !currentlyPadMode; 

            gameConfig.Set(UiConfigOption.PadMode, currentlyPadMode);

            if (Configuration.SwitchHudLayouts) {
                int hudLayout = currentlyPadMode ? Configuration.HudSwitchController : Configuration.HudSwitchMKB;
                string rawCmd = $"/hudlayout {hudLayout}";
                ChatHelper.SendChatMessage(rawCmd);
            }

            SwapTimeout = DateTime.Now.AddSeconds(1);
        }
    }
}
