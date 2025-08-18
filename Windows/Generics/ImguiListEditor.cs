using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

using System;
using System.Collections.Generic;
using System.Numerics;

namespace XIVControllerToggle.Windows.Generics {
    public enum ChangedType {
        Added,
        Removed,
        UpdatedText
    }

    public class StringListChangedEventArgs {
        public ChangedType TypeOfChange { get; set; }
        public List<string> Items { get; set; } = new List<string>();
        public int UpdatedIndex { get; internal set; }
    }
    public delegate void DImguiStringListChanged(ImguiStringListEditor sender, StringListChangedEventArgs eventArgs);

    public class ImguiStringListEditor : Window, IDisposable {
        private List<string> items = new List<string>();
        private string newItem = string.Empty;

        private Plugin plugin;

        List<DImguiStringListChanged> delegatesListChanged = new List<DImguiStringListChanged>();
        private event DImguiStringListChanged? eventListChanged;
        public event DImguiStringListChanged ListChanged {
            add {
                eventListChanged += value;
                delegatesListChanged.Add(value);
            }

            remove {
                eventListChanged -= value;
                delegatesListChanged.Remove(value);
            }
        }

        public void RemoveAllChangedEvents() {
            foreach (DImguiStringListChanged eh in delegatesListChanged) 
                eventListChanged -= eh;
            delegatesListChanged.Clear();
        }


        public ImguiStringListEditor(Plugin plugin) 
                : base("List editor", ImGuiWindowFlags.NoScrollbar, forceMainWindow: false) {
            this.ShowCloseButton = true;
            this.SizeConstraints = new WindowSizeConstraints {
                MinimumSize = new System.Numerics.Vector2(400, 200),
                MaximumSize = new Vector2(float.MaxValue)
            };
            
            this.plugin = plugin;
        }

        public void SetItemsAndDisplay(string title, List<string> listItems) {
            this.items = listItems;
            this.WindowName = title + "##ImguiStringListEditor";

            RemoveAllChangedEvents();

            this.IsOpen = true;
            this.BringToFront();
        }

        private void invokeListChanged(ChangedType changedType, int updatedIndex = -1) {
            var eventArgs = new StringListChangedEventArgs() {
                 TypeOfChange = changedType,
                 UpdatedIndex = updatedIndex,
                 Items = items
            };

            eventListChanged?.Invoke(this, eventArgs);
        }

        public override void Draw() {
            // Define the fixed height for the second row (bottom row)
            float bottomRowHeight = 40.0f; // Adjust as needed

            // Get the available size of the window
            Vector2 windowSize = ImGui.GetContentRegionAvail();

            // Calculate the height for the top row (remaining height)
            float topRowHeight = windowSize.Y - bottomRowHeight;

            // Top row (dynamically sized, with a scrollbar if content exceeds)
            using (ImRaii.Child("TopRow", new Vector2(0, topRowHeight), true, ImGuiWindowFlags.HorizontalScrollbar)) {
                // Top Row

                // Render the table if there are items
                if (items.Count > 0) {
                    using (var table = ImRaii.Table("tblStringEditor", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders)) {
                        ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 50);
                        ImGui.TableHeadersRow();

                        for (int i = 0; i < items.Count; i++) {
                            ImGui.TableNextRow();

                            // Editable text box
                            ImGui.TableSetColumnIndex(0);
                            string editedItem = items[i];
                            float columnWidth = ImGui.GetColumnWidth();
                            ImGui.PushItemWidth(columnWidth);
                            if (ImGui.InputText($"##item_{i}", ref editedItem, 100)) {
                                items[i] = editedItem;
                                invokeListChanged(ChangedType.UpdatedText, i);
                            }
                            ImGui.PopItemWidth();

                            // Remove button
                            ImGui.TableSetColumnIndex(1);

                            if (ImGuiExtensions.ImGuiCenteredTableColumnIconButton($"rmIdx{i}", Dalamud.Interface.FontAwesomeIcon.Times)) {
                                items.RemoveAt(i);
                                invokeListChanged(ChangedType.Removed);
                                break;
                            }

                            ImGui.SameLine();
                        }
                    }
                }
            }

            // Bottom row (fixed height)
            using (ImRaii.Child("BottomRow", new Vector2(0, bottomRowHeight - 2), true)) {
                // Bottom row

                // Text box to add a new item
                ImGui.InputText("New Item", ref newItem, 100);
                ImGui.SameLine();
                if (ImGui.Button("Add")) {
                    if (!string.IsNullOrWhiteSpace(newItem)) {
                        items.Add(newItem);
                        invokeListChanged(ChangedType.Added);
                        newItem = string.Empty; // Clear the input box
                    }
                }
            }
        }

        public void Dispose() {
            // 
            this.RemoveAllChangedEvents();
        }
    }
}
