using System;
using System.Collections.Generic;
using System.Numerics;

using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using Lumina.Excel.GeneratedSheets;

namespace XIVControllerToggle.Windows;

internal class ActionInputDialog : Window, IDisposable {
    private readonly Plugin plugin;
    private int selectedActionIdx = 0;

    private bool capturingKeys = false;

    public ActionInputDialog(Plugin plugin) : base(
            "Action Input Configuration", ImGuiWindowFlags.AlwaysAutoResize) {

        this.SizeCondition = ImGuiCond.Always;
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(420, 340),
            MaximumSize = new Vector2(800, 800)
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
        if (ImGui.Combo("Action Type", ref selectedActionIdx, "Toggle\0Switch to KBM\0Switch to Pad")) {
            // Action changed
        }

        ImGui.Text("Press escape to stop capturing keys.");

        var buttonText = capturingKeys ? "Stop capture...##btnCapture" : "Capture keys.##btnCapture";
        if (ImGui.Button(buttonText)) {
            capturingKeys = !capturingKeys;
        }; ImGui.SameLine();

        if (ImGui.Button("Reset capture combo")) {
            capturedKeys = (new(), new());
        }; ImGui.SameLine();

        if (ImGui.Button("Add detected keys")) { }
        if (capturingKeys) updateCapturedState();

        using (ImRaii.Table("detectedKeys", 3)) {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Actions");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Not Pressed");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Key");

            // Sample data test
            ImGui.TableNextColumn();
            ImGui.Text("_actions1"); 
            ImGui.TableNextColumn();
            ImGui.Text("_np1"); 
            ImGui.TableNextColumn();
            ImGui.Text("_key1");

            // Sample data test
            ImGui.TableNextColumn();
            ImGui.Text("_actions2");
            ImGui.TableNextColumn();
            ImGui.Text("_np2");
            ImGui.TableNextColumn();
            ImGui.Text("_key2");
        }



        //for (int x = 0; x < capturedKeys.keyboardKeys.Count; x++) {
        //    var kv = capturedKeys.keyboardKeys[x];

        //    ImGui.TableNextRow();
        //    ImGui.TableNextColumn();

        //    ImGui.Button("Edit"); ImGui.SameLine();
        //    ImGui.Button("Remove");

        //    ImGui.TableNextColumn();
        //    ImGui.Checkbox("!##NotPressed", ref kv.notPressed);

        //    ImGui.TableNextColumn();
        //    ImGui.Text("(KBM) " + kv.key.ToString());
        //}

        // 
        ImGui.BeginTable("Grouped Key Actions", 2);


        ImGui.EndTable();
    }
}
