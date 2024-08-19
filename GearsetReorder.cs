using Dalamud.IoC;
using Dalamud.Plugin;
using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using ImGuiNET;

namespace GearsetReorder;

public class Plugin : IDalamudPlugin {
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    private GearsetWindow gearsetWindow { get; init; }
    private readonly WindowSystem windowSystem = new("GearsetReorder");

    public Plugin() {
        gearsetWindow = new GearsetWindow();
        windowSystem.AddWindow(gearsetWindow);
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
    }

    private void DrawUI() => windowSystem.Draw();

    private void OpenMainUi() {
        gearsetWindow.Toggle();
    }

    public void Dispose() {
        windowSystem.RemoveAllWindows();
        gearsetWindow.Dispose();
    }
}

public class GearsetWindow : Window, IDisposable {
    private byte? draggedItemId = null;

    public GearsetWindow(): base("Gearsets") {
        Size = new Vector2(232, 700);
    }

    public override unsafe void Draw() {
        var gearsets = RaptureGearsetModule.Instance();
        for (var i = 0; i < gearsets->NumGearsets; i++) {
            var gearset = gearsets->GetGearset(i);
            ImGui.Button(gearset->NameString);
            if (ImGui.BeginDragDropSource()) {
                ImGui.Text(gearset->NameString);
                ImGui.SetDragDropPayload(typeof(string).FullName, IntPtr.Zero, 0);
                draggedItemId = gearset->Id;
                ImGui.EndDragDropSource();
            }
            if (ImGui.BeginDragDropTarget()) {
                ImGui.Separator();
                var payload = ImGui.AcceptDragDropPayload(typeof(string).FullName);
                if (payload.NativePtr != null && draggedItemId != null) {
                    if (draggedItemId < i) {
                        for (var j = (int) draggedItemId; j < i; j++) {
                            gearsets->ReassignGearsetId(j, j + 1);
                        }
                    } else {
                        for (var j = (int) draggedItemId; j > i + 1; j--) {
                            gearsets->ReassignGearsetId(j, j - 1);
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
        }
    }

    public void Dispose() {}
}
