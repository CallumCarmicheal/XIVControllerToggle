using Dalamud.Configuration;
using Dalamud.Plugin;

using System;
using System.Collections.Generic;

namespace XIVControllerToggle {

    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 1;

        public bool Enabled { get; set; } = true;
        public bool SwitchHudLayouts { get; set; } = true;

        public int StickDeadzone { get; set; } = 25;
        public ControllerSticks ControllerSticks { get; set; } = ControllerSticks.Both;

        public int HudSwitchMKB { get; set; } = 1;
        public int HudSwitchController { get; set; } = 1;

        public bool HudSwitchPad_HideChat { get; set; } = false;
        public bool HudSwitchMKB_HideChat { get; set; } = false;

        public bool EnableCollectionsOnChange { get; set; } = false;
        public List<string> CollectionsToEnableKBM { get; set; } = new List<string>();
        public List<string> CollectionsToDisableKBM { get; set; } = new List<string>();
        public List<string> CollectionsToEnablePAD { get; set; } = new List<string>();
        public List<string> CollectionsToDisablePAD { get; set; } = new List<string>();



        public int ConfigurationType { get; set; } = 0;

        // New Controller UI (WIP)
        public AdvancedKeybindConfiguration AdvancedKeybinds { get; set; } = new AdvancedKeybindConfiguration();

        // the below exist just to make saving less cumbersome
        [NonSerialized] private IDalamudPluginInterface? pluginInterface;
        [NonSerialized] private Plugin? plugin;

        public void Initialize(IDalamudPluginInterface pluginInterface, Plugin plugin) {
            this.plugin = plugin;
            this.pluginInterface = pluginInterface;
        }

        public void Save() {
            this.pluginInterface!.SavePluginConfig(this);
        }

        internal void ClampValues() {
            HudSwitchMKB = Math.Clamp(HudSwitchMKB, 1, 4);
            HudSwitchController = Math.Clamp(HudSwitchController, 1, 4);
        }
    }

    public enum EKeybindAction : int {
        Toggle = 1,
        SetToKBM = 2,
        SetToPad = 3
    }

    public class AdvancedKeybindConfiguration {
        public List<KeyAction> CustomKeyActions { get; set; } = new List<KeyAction>();
    }

    public class KeyAction {
        public EKeybindAction Action { get; set; }  // Action to perform, e.g., "swap-kbm", "swap-pad"
        public List<List<string>> Keys { get; set; }  // Key conditions

        public KeyAction(EKeybindAction action, List<List<string>> keys) {
            Action = action;
            Keys = keys;
        }
    }

    public enum ControllerSticks {
        LS,
        RS,
        Both
    }
}
