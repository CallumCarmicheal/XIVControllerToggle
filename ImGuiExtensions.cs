using Dalamud.Interface.Utility;

using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XIVControllerToggle {
    public static class ImGuiExtensions {
        public static void Spacing(int n) {
            float sp = ImGui.GetStyle().ItemSpacing.Y;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + n * sp);
        }

        public static void ImguiCheckbox(string text, Func<bool> Get, Action<bool> Set) {
            bool @bool = Get();
            if (ImGui.Checkbox(text, ref @bool)) {
                Set(@bool);
                Plugin.PluginConfig.Save();
            }
        }

        public static void ImguiRadioButton(string text, string sectionId, Func<bool> Get, Action Set, bool sameLine = false) {
            bool @bool = Get();

            // Fix issue with Imgui not triggering button due to duplicate id's (same label text)
            var trimmed = text.Trim().Replace(" ", "");
            if (ImGui.RadioButton(text + "##" + sectionId + trimmed, @bool)) {
                Set();
                Plugin.PluginConfig.Save();
            }

            if (sameLine)
                ImGui.SameLine();
        }

        public static void BeginGroupPanel(string name = "", Vector2? size_arg = null) {
            Vector2 size;
            if (size_arg != null) size = size_arg.Value;
            else size = new Vector2(-1.0f, -1.0f);

            ImGui.BeginGroup();

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 itemSpacing = ImGui.GetStyle().ItemSpacing;
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, 0.0f));

            float frameHeight = ImGui.GetFrameHeight();
            ImGui.BeginGroup();

            Vector2 effectiveSize = size;
            if (size.X < 0.0f)
                effectiveSize.X = ImGui.GetContentRegionAvail().X; // GetContentRegionAvailWidth
            else
                effectiveSize.X = size.X;
            ImGui.Dummy(new Vector2(effectiveSize.X, 0.0f));

            ImGui.Dummy(new Vector2(frameHeight * 0.5f, 0.0f));
            ImGui.SameLine(0.0f, 0.0f);
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(frameHeight * 0.5f, 0.0f));
            ImGui.SameLine(0.0f, 0.0f);
            ImGui.TextUnformatted(name);
            ImGui.SameLine(0.0f, 0.0f);
            ImGui.Dummy(new Vector2(0.0f, frameHeight + itemSpacing.Y));
            ImGui.BeginGroup();

            ImGui.PopStyleVar(2);

            Vector2 windowSize = ImGui.GetWindowSize();
            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() - frameHeight * 0.5f, ImGui.GetCursorPosY()));
            windowSize.X -= frameHeight;
            ImGui.SetWindowSize(windowSize);

            //ImGui.GetCurrentWindow().ContentsRegionRect.Max.X -= frameHeight * 0.5f;
            //ImGui.GetCurrentWindow().Size.X -= frameHeight;

            ImGui.PushItemWidth(effectiveSize.X - frameHeight);

            ImGui.Dummy(new Vector2(0, 1));
        }

        public static void EndGroupPanel() {
            ImGui.PopItemWidth();

            Vector2 itemSpacing = ImGui.GetStyle().ItemSpacing;

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0.0f, 0.0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, 0.0f));

            float frameHeight = ImGui.GetFrameHeight();

            ImGui.EndGroup();

            ImGui.EndGroup();

            ImGui.SameLine(0.0f, 0.0f);
            ImGui.Dummy(new Vector2(frameHeight * 0.5f, 0.0f));
            ImGui.Dummy(new Vector2(0.0f, frameHeight - frameHeight * 0.5f - itemSpacing.Y));

            ImGui.EndGroup();

            Vector2 itemMin = ImGui.GetItemRectMin();
            Vector2 itemMax = ImGui.GetItemRectMax();

            Vector2 halfFrame = new Vector2(frameHeight * 0.25f, frameHeight) * 0.5f;

            unsafe {
                bool defaultColour = false;

                try {
                    var pColourVec = ImGui.GetStyleColorVec4(ImGuiCol.Border);

                    if (pColourVec != null) {
                        uint borderColorUint = ImGui.ColorConvertFloat4ToU32(*pColourVec);
                        ImGui.GetWindowDrawList().AddRect(
                            itemMin + halfFrame, itemMax - new Vector2(halfFrame.X, 0.0f),
                            borderColorUint,
                            halfFrame.X);
                    } else {
                        defaultColour = true;
                    }
                } catch {
                    defaultColour = true;
                }

                if (defaultColour) {
                    Vector4 gray400Color = new Vector4(0.635f, 0.635f, 0.635f, 1.0f);
                    uint gray400ColorUint = ImGui.ColorConvertFloat4ToU32(gray400Color);

                    ImGui.GetWindowDrawList().AddRect(
                        itemMin + halfFrame, itemMax - new Vector2(halfFrame.X, 0.0f),
                        gray400ColorUint,
                        halfFrame.X);
                }
            }

            ImGui.PopStyleVar(2);

            //ImGui.GetCurrentWindow().ContentsRegionRect.Max.X += frameHeight * 0.5f;
            //ImGui.GetCurrentWindow().Size.X += frameHeight;

            Vector2 windowSize = ImGui.GetWindowSize();
            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + frameHeight * 0.5f, ImGui.GetCursorPosY()));
            windowSize.X += frameHeight;
            ImGui.SetWindowSize(windowSize);

            ImGui.Dummy(new Vector2(0.0f, 0.0f));
            ImGui.EndGroup();
        }

        public static IEndObject GroupPanel(string name = "", Vector2? size_arg = null) {
            BeginGroupPanel(name, size_arg);
            return new EndUnconditionally(EndGroupPanel, true);
        }

        #region ImRaii functions
        public interface IEndObject : IDisposable {
            static readonly IEndObject Empty = new EndConditionally(Nop, success: false);

            bool Success { get; }

            static bool operator true(IEndObject i) {
                return i.Success;
            }

            static bool operator false(IEndObject i) {
                return !i.Success;
            }

            static bool operator !(IEndObject i) {
                return !i.Success;
            }

            static bool operator &(IEndObject i, bool value) {
                return i.Success && value;
            }

            static bool operator |(IEndObject i, bool value) {
                return i.Success || value;
            }

            private static void Nop() {
            }
        }

        private struct EndUnconditionally : IEndObject, IDisposable {
            private Action EndAction { get; }

            public bool Success { get; }

            public bool Disposed { get; private set; }

            public EndUnconditionally(Action endAction, bool success) {
                EndAction = endAction;
                Success = success;
                Disposed = false;
            }

            public void Dispose() {
                if (!Disposed) {
                    EndAction();
                    Disposed = true;
                }
            }
        }

        private struct EndConditionally : IEndObject, IDisposable {
            public bool Success { get; }

            public bool Disposed { get; private set; }

            private Action EndAction { get; }

            public EndConditionally(Action endAction, bool success) {
                EndAction = endAction;
                Success = success;
                Disposed = false;
            }

            public void Dispose() {
                if (!Disposed) {
                    if (Success) {
                        EndAction();
                    }

                    Disposed = true;
                }
            }
        }
        #endregion
    }
}
