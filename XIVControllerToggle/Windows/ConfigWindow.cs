using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace XIVControllerToggle.Windows;

public class ConfigWindow : Window, IDisposable {
    private readonly Configuration cfg;
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base(
        "The Great Controller Hud Switcher", ImGuiWindowFlags.NoResize) {
        this.Size = new Vector2(400, 310); // 280
        this.SizeCondition = ImGuiCond.Always;

        this.plugin = plugin;
        this.cfg = plugin.Configuration;
    }

    public void Dispose() { }


    public override void Draw() {
        ImguiCheckbox("Enable KB / Controller Switching", () => cfg.Enabled, (v) => cfg.Enabled = v);

        ImGui.Text("Switch to Controller on: "); ImGui.SameLine();
        ImguiRadioButton("LS",   () => cfg.ControllerSticks == ControllerSticks.LS,   () => cfg.ControllerSticks = ControllerSticks.LS,   sameLine: true);
        ImguiRadioButton("RS",   () => cfg.ControllerSticks == ControllerSticks.RS,   () => cfg.ControllerSticks = ControllerSticks.RS,   sameLine: true);
        ImguiRadioButton("Both", () => cfg.ControllerSticks == ControllerSticks.Both, () => cfg.ControllerSticks = ControllerSticks.Both, sameLine: false);

        int stickDeadzone = cfg.StickDeadzone;
        ImGui.Text("Stick deadzone"); ImGui.SameLine();
        if (ImGui.DragInt("##StickDeadzone", ref stickDeadzone, 1, 1, 100)) 
            cfg.StickDeadzone = stickDeadzone;

        ImguiCheckbox("Enable Hud Layout Changing", () => cfg.SwitchHudLayouts, (v) => cfg.SwitchHudLayouts = v);

        if (cfg.SwitchHudLayouts == false) ImGui.BeginDisabled();

        ImGui.BeginChild("layHud", new Vector2(380, 140), true);
        {
            ImGui.Text("Keyboard Layout");
            ImGui.BeginChild("layHud_Keyboard", new Vector2(360, 38), true); {
                ImguiRadioButton("Layout 1", () => cfg.HudSwitchMKB == 1, () => cfg.HudSwitchMKB = 1); ImGui.SameLine();
                ImguiRadioButton("Layout 2", () => cfg.HudSwitchMKB == 2, () => cfg.HudSwitchMKB = 2); ImGui.SameLine();
                ImguiRadioButton("Layout 3", () => cfg.HudSwitchMKB == 3, () => cfg.HudSwitchMKB = 3); ImGui.SameLine();
                ImguiRadioButton("Layout 4", () => cfg.HudSwitchMKB == 4, () => cfg.HudSwitchMKB = 4); ImGui.SameLine();
            } ImGui.EndChild();

            ImGui.Text("Controller Layout");
            ImGui.BeginChild("layHud_Controller", new Vector2(360, 38), true); {
                ImguiRadioButton("Layout 1", () => cfg.HudSwitchController == 1, () => cfg.HudSwitchController = 1); ImGui.SameLine();
                ImguiRadioButton("Layout 2", () => cfg.HudSwitchController == 2, () => cfg.HudSwitchController = 2); ImGui.SameLine();
                ImguiRadioButton("Layout 3", () => cfg.HudSwitchController == 3, () => cfg.HudSwitchController = 3); ImGui.SameLine();
                ImguiRadioButton("Layout 4", () => cfg.HudSwitchController == 4, () => cfg.HudSwitchController = 4); ImGui.SameLine();
            } ImGui.EndChild();
        }
        ImGui.EndChild();

        if (cfg.SwitchHudLayouts == false) ImGui.EndDisabled();

        if (ImGui.Button("Show Input Information"))
            plugin.DrawDebugUI();
    }

    private void ImguiCheckbox(string text, Func<bool> Get, Action<bool> Set) {
        bool @bool = Get();
        if (ImGui.Checkbox(text, ref @bool)) {
            Set(@bool);
            this.cfg.Save();
        }
    }

    private void ImguiRadioButton(string text, Func<bool> Get, Action Set, bool sameLine = false) {
        bool @bool = Get();
        if (ImGui.RadioButton(text, @bool)) {
            Set();
            this.cfg.Save();
        }

        if (sameLine) ImGui.SameLine();
    }
}
