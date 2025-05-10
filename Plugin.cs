using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using System;
using System.Collections.Generic;

using TPie.Helpers;

using XIVControllerToggle.Windows;
using XIVControllerToggle.Windows.Generics;

using VK = Dalamud.Game.ClientState.Keys.VirtualKey;

namespace XIVControllerToggle {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "The Great Controller HUD Switcher";
        private const string CommandName = "/controllerhudcfg";

        public static Configuration PluginConfig { get; set; } = new Configuration();
        public WindowSystem WindowSystem = new("ControllerHUDSwitcher");

        private ConfigWindow ConfigWindow { get; init; }
        private DebugWindow DebugWindow { get; init; }
        private ImguiStringListEditor ImguiStringListEditor { get; init; }
        
        private static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        public static ICommandManager CommandManager { get; set; } = null!;
        public static IFramework Framework { get; private set; } = null!;
        public static IPluginLog Log { get; private set; } = null!;
        public static IGameGui GameGui { get; private set; } = null!;
        public static IGameConfig GameConfig { get; private set; } = null!;
        public static IKeyState KeyState { get; private set; } = null!;
        public static ISigScanner SigScanner { get; private set; } = null!;
        public static IGamepadState GamepadState { get; private set; } = null!;

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            IFramework framework,
            ICommandManager commandManager,
            IPluginLog pluginLog,
            IGameGui gameGui,
            IGameConfig gameConfig,
            ISigScanner sigScanner,
            IKeyState keyState,
            IGamepadState gamepadState
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

            // Log.Info("Startup!");

            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PluginConfig.Initialize(PluginInterface, this);
            PluginConfig.ClampValues();

            ConfigWindow = new ConfigWindow(this);
            DebugWindow = new DebugWindow(this);
            ImguiStringListEditor = new ImguiStringListEditor(this);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(DebugWindow);
            WindowSystem.AddWindow(ImguiStringListEditor);

            setupAndRemoveCommands(true);

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

                CommandManager.AddHandler("/pad", new CommandInfo(CmdSetController) {
                    HelpMessage = "Change hud mode to controller / pad"
                });

                CommandManager.AddHandler("/kbm", new CommandInfo(CmdSetMouse) {
                    HelpMessage = "Change hud mode to keyboard and mouse"
                });

                CommandManager.AddHandler("/togpad", new CommandInfo(CmdToggleController) {
                    HelpMessage = "Toggle hud mode between pad and kb/m"
                });
            } else {
                CommandManager.RemoveHandler(CommandName);
                CommandManager.RemoveHandler("/pad");
                CommandManager.RemoveHandler("/kbm");
                CommandManager.RemoveHandler("/togpad");
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
            if (PluginConfig.Enabled == false) return; // Dont run if disabled

            var gs = GamepadState;
            var ks = KeyState;

            GameConfig.TryGet(UiConfigOption.PadMode, out uint padMode_i);
            bool currentlyPadMode = padMode_i == 1;

            var ls = Math.Max(gs.LeftStick.X.abs(), gs.LeftStick.Y.abs());
            var rs = Math.Max(gs.RightStick.X.abs(), gs.RightStick.Y.abs());
            var max = PluginConfig.ControllerSticks switch {
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
                if (max >= PluginConfig.StickDeadzone) {
                    swap = true;
                }
            }

            if (swap) {
                var isPad = PerformControllerKeyboardSwitch();
                if (isPad.HasValue) {
                    if (isPad.Value && PluginConfig.HudSwitchPad_HideChat) {
                        XIVFunctions.HideChatLog();
                    }
                    else if (!isPad.Value && PluginConfig.HudSwitchMKB_HideChat) {
                        XIVFunctions.HideChatLog();
                    }
                }
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
            ImguiStringListEditor.Dispose();

            ChatHelper.Instance?.Dispose();

            CommandManager.RemoveHandler(CommandName);
            setupAndRemoveCommands(false);
        }

        private void CmdOpenSettings(string command, string args) {
            ConfigWindow.IsOpen = true;
        }

        public void DrawConfigUI() => ConfigWindow.IsOpen = true;
        public void DrawDebugUI() => DebugWindow.IsOpen = true;

        public void ShowListDialog(string title, List<string> listItems, DImguiStringListChanged? callback = null) {
            ImguiStringListEditor.SetItemsAndDisplay(title, listItems);

            if (callback != null) {
                ImguiStringListEditor.ListChanged += callback;
            }
        }

        public static DateTime SwapTimeout = DateTime.Now;
        public bool? PerformControllerKeyboardSwitch(bool? forceSwitchToController = null) {
            // Delay swapping to once a second (As chat commands are involved).
            if (SwapTimeout > DateTime.Now)
                return null;

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

            if (PluginConfig.SwitchHudLayouts) {
                int hudLayout = currentlyPadMode ? PluginConfig.HudSwitchController : PluginConfig.HudSwitchMKB;
                string rawCmd = $"/hudlayout {hudLayout}";
                ChatHelper.SendChatMessage(rawCmd);
            }

            if (PluginConfig.EnableCollectionsOnChange) {
                if (currentlyPadMode) {
                    foreach (var col in PluginConfig.CollectionsToDisablePAD) {
                        CommandManager.ProcessCommand($"/xldisablecollection {col}");
                    }
                    foreach (var col in PluginConfig.CollectionsToEnablePAD) {
                        CommandManager.ProcessCommand($"/xlenablecollection {col}");
                    }
                } else {
                    foreach (var col in PluginConfig.CollectionsToDisableKBM) {
                        CommandManager.ProcessCommand($"/xldisablecollection {col}");
                    }
                    foreach (var col in PluginConfig.CollectionsToEnableKBM) {
                        CommandManager.ProcessCommand($"/xlenablecollection {col}");
                    }
                }
            }

            if (PluginConfig.HudScaling_Enabled) {
                uint targetMode = currentlyPadMode
                    ? PluginConfig.HudScaling_PadMode 
                    : PluginConfig.HudScaling_KBMMode;

                gameConfig.Set(SystemConfigOption.UiHighScale, targetMode);
            }

            SwapTimeout = DateTime.Now.AddSeconds(1);

            return currentlyPadMode;
        }
    }
}
