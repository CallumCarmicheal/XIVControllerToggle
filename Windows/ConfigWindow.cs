using System;
using System.Collections.Generic;
using System.Numerics;

using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using XIVControllerToggle.Windows.Generics;

namespace XIVControllerToggle.Windows;

public class ConfigWindow : Window, IDisposable {
    private readonly Plugin plugin;

    private string enableCollectionsStringKBM  = string.Empty;
    private string disableCollectionsStringKBM = string.Empty;

    private string enableCollectionsStringPAD = string.Empty;
    private string disableCollectionsStringPAD = string.Empty;

    public ConfigWindow(Plugin plugin) : base (
        "The Great Controller HUD Switcher##xivcontroller_02", ImGuiWindowFlags.AlwaysAutoResize) {
        
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

        ImGui.Dummy(new Vector2(0, 3));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 3));

        using (ImGuiExtensions.DisabledIf(!Plugin.PluginConfig.SwitchHudLayouts)) {
            using (ImGuiExtensions.GroupPanel("Keyboard Layout")) {
                ImGui.Dummy(new Vector2(1, 0)); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 1", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 1, () => Plugin.PluginConfig.HudSwitchMKB = 1); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 2", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 2, () => Plugin.PluginConfig.HudSwitchMKB = 2); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 3", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 3, () => Plugin.PluginConfig.HudSwitchMKB = 3); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 4", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 4, () => Plugin.PluginConfig.HudSwitchMKB = 4);
                ImGui.Dummy(new Vector2(1, 3)); ImGui.SameLine();
                ImGuiExtensions.ImguiCheckbox("Hide chat on switch##lblKBMHideChat", () => Plugin.PluginConfig.HudSwitchMKB_HideChat, (v) => Plugin.PluginConfig.HudSwitchMKB_HideChat = v);
                ImGui.Dummy(new Vector2(0, 3));
            }

            using (ImGuiExtensions.GroupPanel("Pad Layout")) {
                ImGui.Dummy(new Vector2(0, 0)); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 1", "PL", () => Plugin.PluginConfig.HudSwitchController == 1, () => Plugin.PluginConfig.HudSwitchController = 1); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 2", "PL", () => Plugin.PluginConfig.HudSwitchController == 2, () => Plugin.PluginConfig.HudSwitchController = 2); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 3", "PL", () => Plugin.PluginConfig.HudSwitchController == 3, () => Plugin.PluginConfig.HudSwitchController = 3); ImGui.SameLine();
                ImGuiExtensions.ImguiRadioButton("Layout 4", "PL", () => Plugin.PluginConfig.HudSwitchController == 4, () => Plugin.PluginConfig.HudSwitchController = 4);
                ImGui.Dummy(new Vector2(0, 3)); ImGui.SameLine();
                ImGuiExtensions.ImguiCheckbox("Hide chat on switch##lblPadHideChat", () => Plugin.PluginConfig.HudSwitchPad_HideChat, (v) => Plugin.PluginConfig.HudSwitchPad_HideChat = v);
                ImGui.Dummy(new Vector2(0, 3));
            }
        }

        ImGui.Dummy(new Vector2(0, 5));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        ImGuiExtensions.ImguiCheckbox("Change High Resolution UI Scaling", 
            () => Plugin.PluginConfig.HudScaling_Enabled, 
            (v) => Plugin.PluginConfig.HudScaling_Enabled = v);

        using (ImGuiExtensions.DisabledIf(!Plugin.PluginConfig.HudScaling_Enabled)) {
            string[] dropdown = new string[4]{
                "100% (HD)",
                "150% (Full HD)",
                "200% (WQHD)",
                "300% (4K)"
            };

            int HudScaling_KBMMode = (int)Plugin.PluginConfig.HudScaling_KBMMode;
            if (ImGui.Combo("Scaling on KBM:", ref HudScaling_KBMMode, dropdown, 4))
                Plugin.PluginConfig.HudScaling_KBMMode = (uint)HudScaling_KBMMode;

            int HudScaling_PadMode = (int)Plugin.PluginConfig.HudScaling_PadMode;
            if (ImGui.Combo("Scaling on Pad:", ref HudScaling_PadMode, dropdown, 4))
                Plugin.PluginConfig.HudScaling_PadMode = (uint)HudScaling_PadMode;
        }

        ImGui.Dummy(new Vector2(0, 5));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        bool enableCollections = Plugin.PluginConfig.EnableCollectionsOnChange;
        if (ImGui.Checkbox("Enable / Disable Dalamud Collections on change", ref enableCollections)) 
            Plugin.PluginConfig.EnableCollectionsOnChange = enableCollections;

        if (Plugin.PluginConfig.EnableCollectionsOnChange == false) ImGui.BeginDisabled();

        var tableFlags =
          ImGuiTableFlags.Resizable              // keep your resizing behaviour
        | ImGuiTableFlags.NoBordersInBody        // (redundant with NoBorders, but explicit)
        ;

        if (ImGui.BeginTable("collections_table", 3, tableFlags)) {
            // Column 0: fixed to maxLabelWidth
            ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthStretch);

            // Column 1: stretch
            ImGui.TableSetupColumn("##field", ImGuiTableColumnFlags.WidthStretch);

            // Column 2: fixed to icon/button width (e.g. 24px)
            const float iconSize = 24f;
            ImGui.TableSetupColumn("##button", ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthFixed, iconSize);

            // Render the rows.
            void DrawRow(string label, string buttonId, ref string field, List<string> list) {
                ImGui.TableNextRow();

                // Column 0: label
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(label);

                // Column 2: text field
                ImGui.TableSetColumnIndex(1);
                ImGui.PushItemWidth(-1); // take all remaining width
                ImGui.InputText($"##{buttonId}_field", ref field, 4000, ImGuiInputTextFlags.ReadOnly);
                ImGui.PopItemWidth();

                // Column 2: button
                ImGui.TableSetColumnIndex(2);
                if (ImGuiComponents.IconButton(buttonId, Dalamud.Interface.FontAwesomeIcon.PlusCircle))
                    plugin.ShowListDialog(label, list, cbCollectionsChanged);
            }

            DrawRow("(KBM) Enable collections",  "enablecolkbm",  ref enableCollectionsStringKBM,  Plugin.PluginConfig.CollectionsToEnableKBM);
            DrawRow("(KBM) Disable collections", "disablecolkbm", ref disableCollectionsStringKBM, Plugin.PluginConfig.CollectionsToDisableKBM);
            DrawRow("(PAD) Enable collections",  "enablecolpad",  ref enableCollectionsStringPAD,  Plugin.PluginConfig.CollectionsToEnablePAD);
            DrawRow("(PAD) Disable collections", "disablecolpad", ref disableCollectionsStringPAD, Plugin.PluginConfig.CollectionsToDisablePAD);

            ImGui.EndTable();
        }

        if (Plugin.PluginConfig.EnableCollectionsOnChange == false) ImGui.EndDisabled();

        if (ImGui.Button("Show Input Information"))
            plugin.DrawDebugUI();

    }

    private void cbCollectionsChanged(ImguiStringListEditor sender, StringListChangedEventArgs eventArgs) {
        // Update the collections text
        enableCollectionsStringKBM  = string.Join(", ", Plugin.PluginConfig.CollectionsToEnableKBM);
        disableCollectionsStringKBM = string.Join(", ", Plugin.PluginConfig.CollectionsToDisableKBM);
        enableCollectionsStringPAD = string.Join(", ", Plugin.PluginConfig.CollectionsToEnablePAD);
        disableCollectionsStringPAD = string.Join(", ", Plugin.PluginConfig.CollectionsToDisablePAD);

        Plugin.PluginConfig.Save();
    }
}
