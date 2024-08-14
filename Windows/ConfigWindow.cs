using System;
using System.Numerics;

using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace XIVControllerToggle.Windows;

public class ConfigWindow : Window, IDisposable {
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base (
        "The Great Controller HUD Switcher", ImGuiWindowFlags.AlwaysAutoResize) {
        
        this.SizeCondition = ImGuiCond.Always;
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(420, 340),
            MaximumSize = new Vector2(800, 800)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw() {
        ImGuiExtensions.ImguiCheckbox("Enable KB / Pad Switching on movement", () => Plugin.PluginConfig.Enabled, (v) => Plugin.PluginConfig.Enabled = v);

        ImGui.Text("Switch to Pad Crossbars on: "); ImGui.SameLine();
        ImGuiExtensions.ImguiRadioButton("LS", "SPCB", () => Plugin.PluginConfig.ControllerSticks == ControllerSticks.LS, () => Plugin.PluginConfig.ControllerSticks = ControllerSticks.LS, sameLine: true);
        ImGuiExtensions.ImguiRadioButton("RS", "SPCB", () => Plugin.PluginConfig.ControllerSticks == ControllerSticks.RS, () => Plugin.PluginConfig.ControllerSticks = ControllerSticks.RS, sameLine: true);
        ImGuiExtensions.ImguiRadioButton("Both", "SPCB", () => Plugin.PluginConfig.ControllerSticks == ControllerSticks.Both, () => Plugin.PluginConfig.ControllerSticks = ControllerSticks.Both, sameLine: false);

        int stickDeadzone = Plugin.PluginConfig.StickDeadzone;
        ImGui.Text("Stick: Switching threashold"); ImGui.SameLine();
        if (ImGui.DragInt("##StickDeadzone", ref stickDeadzone, 1, 1, 100))
            Plugin.PluginConfig.StickDeadzone = stickDeadzone;

        ImGuiExtensions.ImguiCheckbox("Enable HUD Layout Changing", () => Plugin.PluginConfig.SwitchHudLayouts, (v) => Plugin.PluginConfig.SwitchHudLayouts = v);

        if (Plugin.PluginConfig.SwitchHudLayouts == false) ImGui.BeginDisabled();

        ImGui.Dummy(new Vector2(0, 3));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 3));

        ImGuiExtensions.BeginGroupPanel("Keyboard Layout"); {
            ImGuiExtensions.ImguiRadioButton("Layout 1", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 1, () => Plugin.PluginConfig.HudSwitchMKB = 1); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 2", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 2, () => Plugin.PluginConfig.HudSwitchMKB = 2); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 3", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 3, () => Plugin.PluginConfig.HudSwitchMKB = 3); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 4", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 4, () => Plugin.PluginConfig.HudSwitchMKB = 4); ImGui.SameLine();
            ImGui.Dummy(new Vector2(0, 30));
        } ImGuiExtensions.EndGroupPanel();

        ImGuiExtensions.BeginGroupPanel("Pad Layout"); {
            ImGuiExtensions.ImguiRadioButton("Layout 1", "PL", () => Plugin.PluginConfig.HudSwitchController == 1, () => Plugin.PluginConfig.HudSwitchController = 1); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 2", "PL", () => Plugin.PluginConfig.HudSwitchController == 2, () => Plugin.PluginConfig.HudSwitchController = 2); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 3", "PL", () => Plugin.PluginConfig.HudSwitchController == 3, () => Plugin.PluginConfig.HudSwitchController = 3); ImGui.SameLine();
            ImGuiExtensions.ImguiRadioButton("Layout 4", "PL", () => Plugin.PluginConfig.HudSwitchController == 4, () => Plugin.PluginConfig.HudSwitchController = 4); ImGui.SameLine();
            ImGui.Dummy(new Vector2(0, 30));
        } ImGuiExtensions.EndGroupPanel();

        ImGui.Dummy(new Vector2(0, 5));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        if (Plugin.PluginConfig.SwitchHudLayouts == false) ImGui.EndDisabled();

        if (ImGui.Button("Show Input Information"))
            plugin.DrawDebugUI();

    }
}
