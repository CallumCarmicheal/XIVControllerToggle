using System;
using System.Numerics;

using Dalamud.Game.Config;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Component.GUI;

using ImGuiNET;

using TPie.Helpers;

using XIVControllerToggle;

using VK = Dalamud.Game.ClientState.Keys.VirtualKey;

namespace XIVControllerToggle.Windows;

public class DebugWindow : Window, IDisposable {
    private Plugin plugin;

    public DebugWindow(Plugin plugin) : base(
        "KB/Controller Debug Information", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() {
        
    }

    public override void Draw() {
        var gameConfig = Plugin.GameConfig;
        var gp = Plugin.GamepadState;
        var ks = Plugin.KeyState;

        if (ImGui.Button("Show Settings"))
            this.plugin.DrawConfigUI();

        gameConfig.TryGet(UiConfigOption.PadMode, out uint padMode_i);
        bool currentlyPadMode = padMode_i == 1;

        if (ImGui.Button("Attempt Switch")) 
            plugin.PerformControllerKeyboardSwitch();
       
        ImGui.Text($"LEFT - X: {gp.LeftStick.X.abs()}, Y: {gp.LeftStick.Y.abs()}.");
        ImGui.Text($"RIGHT - X: {gp.RightStick.X.abs()}, Y: {gp.RightStick.Y.abs()}.");

        ImGui.Text($"KB W:{ks[VK.W]}, S:{ks[VK.S]}, A:{ks[VK.A]}, D:{ks[VK.D]}.");
        ImGui.Text($"Can Swap (Timeout): {(Plugin.SwapTimeout <= DateTime.Now)}");

        ImGui.Spacing();
    }
}
