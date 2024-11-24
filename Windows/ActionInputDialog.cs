using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;

using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using ImGExt = XIVControllerToggle.ImGuiExtensions;

namespace XIVControllerToggle.Windows;

internal class ActionInputDialog : Window, IDisposable {
    private readonly Plugin plugin;
    private int selectedActionIdx = 0;

    private bool capturingKeys = false;

    public ActionInputDialog(Plugin plugin) : base(
            "Action Input Configuration", ImGuiWindowFlags.AlwaysAutoResize) {

        this.SizeCondition = ImGuiCond.Appearing;
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(640, 480),
            MaximumSize = new Vector2(1200, 4000)
        };

        this.plugin = plugin;
    }

    public void Dispose() { 
        
    }

    public override void OnClose() {
        base.OnClose();
        plugin.WindowSystem.RemoveWindow(this);
    }

    private (List<(VirtualKey key, bool notPressed)> keyboardKeys, List<(GamepadButtons key, bool notPressed)> padKeys) capturedKeys = (new(), new());

    private (Dictionary<VirtualKey, bool> keyboardKeys, Dictionary<GamepadButtons, bool> padKeys) captureKeyState() {
        Dictionary<VirtualKey, bool> capKeyboard = new Dictionary<VirtualKey, bool>();
        Dictionary<GamepadButtons, bool> capPad = new Dictionary<GamepadButtons, bool>();

        // Get keyboard keys
        var validVks = Plugin.KeyState.GetValidVirtualKeys();
        foreach (var vk in validVks) {
            var state = Plugin.KeyState[vk];
            capKeyboard.Add(vk, state);
        }

        // Get controller keys
        foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons))) {
            capPad.Add(button, Plugin.GamepadState.Pressed(button) == 1);
        }

        return (capKeyboard, capPad);
    }

    private void updateCapturedState() {
        var isEscapePressed = Plugin.KeyState[VirtualKey.ESCAPE];
        if (isEscapePressed) {
            capturingKeys = false;
            return;
        }

        var capture = captureKeyState();

        foreach (var vk in capture.keyboardKeys) {
            if (vk.Value) 
                capturedKeys.keyboardKeys.Add((vk.Key, false));
        }

        foreach (var pk in capture.padKeys) {
            if (pk.Value)
                capturedKeys.padKeys.Add((pk.Key, false));
        }
    }

    public override void Draw() {
        //renderUserInterface();
        renderUserInterface();
    }

    private bool m_EnablePlugin = true;
    private int m_ConfigurationType = 0;
    private int m_Simple_SwitchOnJoystick = 0;
    private int m_Simple_StickDeadzone = 25;
    private string m_DetectedInput = "<nothing>";

    private bool _devBool = true;
    private int _comboIdx1 = 1;
    private int _comboIdx2 = 1;
    private int _comboIdx3 = 1;
    private int _comboIdx4 = 0;
    private int _comboIdx5 = 2;
    private int _comboIdx6 = 2;
    private string _inputLine1 = "[ CTRL, ALT, 5 ]";
    private string _inputLine2 = "w, d, [ !alt, s ], [ !ALT, A ]";
    private string _inputLine3 = "p:RS, p:Y";
    private string _inputLine4 = "F9, [ p:LS, p:Y ]";
    private string _inputLine5 = "p:LSV, p:RSV";
    private string _inputLine6 = "p:LSV>10, p:RSV>10";

    private void renderUserInterface() {
        ImGui.Checkbox("Enable plugin (Enables switching)", ref m_EnablePlugin);

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Stick switching threshold:");
        ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
        ImGui.SetNextItemWidth(200);
        ImGui.SliderInt("##m_Simple_StickDeadzone", ref m_Simple_StickDeadzone, 1, 100, "%i");

        ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
        ImGui.TextUnformatted("   (note: this is applied after the deadzone set in-game)");
        ImGui.PopStyleColor();

        ImGuiExtensions.Spacing(4);
        ImGui.TextUnformatted("Please select a configuration type:");
        ImGui.SameLine();
        ImGui.RadioButton("Simple", ref m_ConfigurationType, 0);

        ImGui.SameLine(0, 4 * ImGui.GetStyle().ItemSpacing.X);
        ImGui.RadioButton("Advanced", ref m_ConfigurationType, 1);

        if (ImGui.BeginTabBar("tbConfigTypes", ImGuiTabBarFlags.None)) {
            if (ImGui.BeginTabItem("Simple Settings")) {
                ImGui.AlignTextToFramePadding();
                if (m_ConfigurationType == 1) ImGui.BeginDisabled();

                ImGui.TextUnformatted("Switch to Pad Crossbars on: ");

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.RadioButton("LS", ref m_Simple_SwitchOnJoystick, 0);

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.RadioButton("RS", ref m_Simple_SwitchOnJoystick, 1);

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.RadioButton("Either", ref m_Simple_SwitchOnJoystick, 2);

                // ImGExt.Spacing(5);
                // ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 2 * ImGui.GetStyle().IndentSpacing / 2);
                // ImGui.Button("Add current configuration to advanced");
                // ImGui.PopStyleVar();

                if (m_ConfigurationType == 1) ImGui.EndDisabled();

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Advanced Settings")) {
                ImGui.AlignTextToFramePadding();

                if (m_ConfigurationType == 0) ImGui.BeginDisabled();
                ImGui.TextUnformatted("This is an advanced input editor that allows you specify what keys to change on.");

#pragma warning disable SeStringRenderer
                var id = (ImGuiId)ImGui.GetID("OpenLink##LinkOpen");
                var seString = ImGuiHelpers.CompileSeStringWrapped(
                    "Please <link(0x0E,0,0,0,\\[\"\"\\, \"\"\\])><colortype(502)><edgecolortype(503)>view examples<colortype(0)>" 
                    + "<edgecolortype(0)><link(0xCE)> to find out how to add or modify keybinds.<br>",
                    imGuiId: id);
                if (seString.InteractedPayload != null && seString.Clicked) {
                    ImGui.TextUnformatted("clicked");
                }
#pragma warning restore SeStringRenderer

                ImGExt.Spacing(1);
                ImGui.Button("Detect input");

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.SetNextItemWidth(200);
                ImGui.InputText("##value6", ref m_DetectedInput, 400);

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.Button("Reset");

                ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
                ImGui.Button("Add key presses");

                int maxVisibleRows = 10;
                float rowHeight = ImGui.GetTextLineHeightWithSpacing();
                float maxTableHeight = maxVisibleRows * rowHeight;

                using (ImRaii.Child("##TableContainer", new Vector2(0, maxTableHeight))) {
                    using (ImRaii.Table("advInputConfig", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV)) {
                        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 50);
                        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed, 120);
                        ImGui.TableSetupColumn("Keybinding", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("Options", ImGuiTableColumnFlags.WidthFixed, 70);
                        ImGui.TableHeadersRow();

                        renderTableInputRow(0, ref _comboIdx1, ref _inputLine1);
                        renderTableInputRow(1, ref _comboIdx2, ref _inputLine2);
                        renderTableInputRow(2, ref _comboIdx3, ref _inputLine3);
                        renderTableInputRow(3, ref _comboIdx4, ref _inputLine4);
                        renderTableInputRow(4, ref _comboIdx5, ref _inputLine5);
                        renderTableInputRow(5, ref _comboIdx6, ref _inputLine6);
                    }
                }

                if (m_ConfigurationType == 0) ImGui.EndDisabled();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.Dummy(new Vector2(0, 3));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 3));

        ImGExt.ImguiCheckbox("Enable HUD Layout Changing", () => Plugin.PluginConfig.SwitchHudLayouts, (v) => Plugin.PluginConfig.SwitchHudLayouts = v);
        if (Plugin.PluginConfig.SwitchHudLayouts == false) ImGui.BeginDisabled();
       
        ImGui.Dummy(new Vector2(0, 3));

        using (ImGExt.GroupPanel("Keyboard Layout")) {
            ImGExt.ImguiRadioButton("Layout 1", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 1, () => Plugin.PluginConfig.HudSwitchMKB = 1); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 2", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 2, () => Plugin.PluginConfig.HudSwitchMKB = 2); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 3", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 3, () => Plugin.PluginConfig.HudSwitchMKB = 3); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 4", "KBL", () => Plugin.PluginConfig.HudSwitchMKB == 4, () => Plugin.PluginConfig.HudSwitchMKB = 4); ImGui.SameLine();
            ImGui.Dummy(new Vector2(0, 30));
        }

        using (ImGExt.GroupPanel("Pad Layout")) {
            ImGExt.ImguiRadioButton("Layout 1", "PL", () => Plugin.PluginConfig.HudSwitchController == 1, () => Plugin.PluginConfig.HudSwitchController = 1); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 2", "PL", () => Plugin.PluginConfig.HudSwitchController == 2, () => Plugin.PluginConfig.HudSwitchController = 2); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 3", "PL", () => Plugin.PluginConfig.HudSwitchController == 3, () => Plugin.PluginConfig.HudSwitchController = 3); ImGui.SameLine();
            ImGExt.ImguiRadioButton("Layout 4", "PL", () => Plugin.PluginConfig.HudSwitchController == 4, () => Plugin.PluginConfig.HudSwitchController = 4); ImGui.SameLine();
            ImGui.Dummy(new Vector2(0, 30));
        }

        ImGui.Dummy(new Vector2(0, 5));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        if (Plugin.PluginConfig.SwitchHudLayouts == false) ImGui.EndDisabled();
    }

    private void renderTableInputRow(int rowIdx, ref int comboIndx, ref string input) {
        ImGui.TableNextRow();
        
        ImGui.TableSetColumnIndex(0);

        var colWid = ImGui.GetColumnWidth();
        var height = ImGui.GetFrameHeight();
        var width  = height * 1.55f;
        var padding = (colWid / 2) - (width / 2);
        float currentPosX = ImGui.GetCursorPosX();
        ImGui.SetCursorPosX(currentPosX + padding);
        ImGuiComponents.ToggleButton("##advTblRw_" + rowIdx + "_Cbx", ref _devBool);

        ImGui.TableSetColumnIndex(1);
        ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
        ImGui.Combo("##advTblRw_" + rowIdx + "_Combo", ref comboIndx, "Toggle\0Set to KBM\0Set to Pad");

        ImGui.TableSetColumnIndex(2);
        ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
        ImGui.PushFont(UiBuilder.MonoFont);
        ImGui.InputText("##advTblRw_" + rowIdx + "_Input", ref input, 2000);
        ImGui.PopFont();

        ImGui.TableSetColumnIndex(3);
        if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Times)) {
            // Delete
        }

        ImGui.SameLine();
        if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Keyboard)) {
            // Popout into popup large input window.
        }
    }
}
