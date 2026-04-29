using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace XIVControllerToggle.Windows;

public sealed class ChangelogWindow : Window, IDisposable {
    private string CurrentVersion = string.Empty;

    private bool showOlderVersions = true;

    public ChangelogWindow(string currentVersion) : base(
        "The Great Controller HUD Switcher Changelog###XIVC_CL",
        ImGuiWindowFlags.NoCollapse
    ) {
        this.CurrentVersion = currentVersion;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(520, 420),
            MaximumSize = new Vector2(760, 720),
        };

        this.Size = new Vector2(620, 560);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() {
        //
    }

    public override void Draw() {
        DrawHeader();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawContributorThanks();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Checkbox("Show older versions", ref this.showOlderVersions);

        DrawShowChangelogOnVersionChangeCheckbox();

        ImGui.Spacing();

        var footerHeight =
            ImGui.GetFrameHeightWithSpacing()
            + ImGui.GetStyle().ItemSpacing.Y
            + ImGui.GetTextLineHeightWithSpacing();

        var childSize = new Vector2(0, -footerHeight);

        if (ImGui.BeginChild("##ChangelogScrollRegion", childSize, true)) {
            foreach (var entry in Entries) {
                if (!this.showOlderVersions && entry.Version != CurrentVersion)
                    continue;

                DrawVersionEntry(entry);
            }
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DrawFooter();
    }

    private void DrawShowChangelogOnVersionChangeCheckbox() {
        var dontShowChangelogEveryUpdate = !Plugin.PluginConfig.ShowChangelogOnVersionChange;

        if (ImGui.Checkbox("Don't show changelog every update", ref dontShowChangelogEveryUpdate)) {
            Plugin.PluginConfig.ShowChangelogOnVersionChange = !dontShowChangelogEveryUpdate;

            if (dontShowChangelogEveryUpdate)
                Plugin.PluginConfig.PluginVersion = CurrentVersion;

            Plugin.PluginConfig.Save();
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("When enabled, this popup will not automatically appear after plugin updates.");
        }
    }

    private void DrawHeader() {
        ImGui.TextUnformatted("The Great Controller HUD Switcher");
        ImGui.SameLine();

        ImGui.TextDisabled($"v{CurrentVersion}");

        ImGui.PushTextWrapPos();
        ImGui.TextWrapped("Thanks for updating! Here are the latest changes and recent plugin history.");
        ImGui.PopTextWrapPos();
    }

    private static void DrawContributorThanks() {
        ImGui.TextUnformatted("Special thanks");

        ImGui.PushTextWrapPos();
        ImGui.BulletText("@Aida-Enna: Updating the plugin to be Dawntrail compatible.");
        ImGui.BulletText("@grecaun, @imvaskel: Updating the plugin Dalamud API and to FFXIV patch.");
        ImGui.BulletText("@TheThirdDoor: Quality of life improvements and WASD binding.");
        ImGui.PopTextWrapPos();
    }

    private void DrawVersionEntry(ChangelogEntry entry) {
        ImGui.PushID(entry.Version);

        var isCurrentVersion = entry.Version == CurrentVersion;

        if (isCurrentVersion) {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.35f, 0.85f, 1.0f, 1.0f));
        }

        ImGui.TextUnformatted($"The Great Controller HUD Switcher {entry.Version}");

        if (isCurrentVersion) {
            ImGui.SameLine();
            ImGui.TextDisabled("Latest");
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();

        ImGui.PushTextWrapPos();

        foreach (var item in entry.Items) {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextWrapped(item);
        }

        ImGui.PopTextWrapPos();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PopID();
    }

    private void DrawFooter() {
        if (ImGui.Button("Close")) {
            this.IsOpen = false;
        }

        ImGui.SameLine();

        ImGui.TextDisabled("You can show this again from the settings menu.");
    }

    private sealed record ChangelogEntry(string Version, IReadOnlyList<string> Items);

    private static readonly IReadOnlyList<ChangelogEntry> Entries = new List<ChangelogEntry> {
        new("1.2.5.0", new[] {
            "Update to Dalamud API 15",
            "Implement changelog popup screen.",
        }),

        new("1.2.4.0", new[] {
            "Change WASD keys to use FFXIV keys bound for character movement, adding support for other keyboard layouts like AZERTY or custom movement keybinds. Thanks @TheThirdDoor.",
            "Ignore keyboard inputs when a chat/text field is active. Thanks @TheThirdDoor.",
        }),

        new("1.2.3.0", new[] {
            "Updated to latest Dalamud API and FFXIV patch version. Thanks @imvaskel.",
        }),

        new("1.2.2.0", new[] {
            "Updated to latest API version and FFXIV patch. Thanks @grecaun.",
        }),

        new("1.2.1.0", new[] {
            "Fix issue with several configuration variables not saving.",
        }),

        new("1.2.0.0", new[] {
            "Added UI Scaling option to change scaling when swapping between KBM / PAD.",
        }),

        new("1.1.0.0", new[] {
            "Updated to DT 7.2",
            "Added collection enabling / disabling upon switching.",
        }),

        new("1.0.1.3", new[] {
            "Updated to DT 7.1",
            "Added hide chat on switch when changing hud layout.",
        }),

        new("1.0.1.2", new[] {
            "Fixed issue with configuration UI not changing HUD selection.",
        }),

        new("1.0.1.1", new[] {
            "Updated to Dawntrail 7.X by Aida-Enna.",
        }),

        new("1.0.1.0", new[] {
            "Attempted to fix display scaling on larger DPI monitors.",
            "Fixed issue where main plugin command was registered twice.",
        }),

        new("1.0.0.0", new[] {
            "Made it work(tm).",
        }),
    };
}