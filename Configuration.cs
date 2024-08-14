using Dalamud.Configuration;
using Dalamud.Plugin;

using System;
using System.Collections.Generic;

namespace XIVControllerToggle {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public List<KeyAction> CustomKeyActions { get; set; } = new List<KeyAction>();

        public bool Enabled { get; set; } = true;
        public bool SwitchHudLayouts { get; set; } = true;

        public int StickDeadzone { get; set; } = 25;
        public ControllerSticks ControllerSticks { get; set; } = ControllerSticks.Both;

        public int HudSwitchMKB { get; set; } = 1;
        public int HudSwitchController { get; set; } = 1;

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

    public class KeyAction {
        public string Type { get; set; }    // "any-key" or "and-key"
        public string Action { get; set; }  // Action to perform, e.g., "swap-kbm", "swap-pad"
        public List<List<string>> Keys { get; set; }  // Key conditions

        public KeyAction(string type, string action, List<List<string>> keys) {
            Type = type;
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
