using Dalamud.IoC;
using Dalamud.Plugin;
using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;

namespace GearsetReorder;

public class Plugin : IDalamudPlugin {
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    private GearsetWindow gearsetWindow { get; init; }
    private readonly WindowSystem windowSystem = new("GearsetReorder");

    [PluginService]
    internal static IToastGui toastGui { get; private set; } = null!;

    public Plugin() {
        gearsetWindow = new GearsetWindow(toastGui);
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
    private int? draggedItemId = null;

    private IToastGui toastGui;

    public GearsetWindow(IToastGui toastGui): base("Gearsets") {
        this.toastGui = toastGui;
        Size = new Vector2(232, 700);
    }

    public override unsafe void Draw() {
        var gearsetModule = RaptureGearsetModule.Instance();
        var gearsets = new List<int>();
        for (var i = 0; i < gearsetModule->NumGearsets; i++) {
            var gearset = gearsetModule->GetGearset(i);
            if (gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists)) {
                gearsets.Add(i);
            }
        }
        for (var i = 0; i < gearsets.Count; i++) {
             var gearset = gearsetModule->GetGearset(gearsets[i]);
            ImGui.Button(gearset->NameString);
            if (ImGui.BeginDragDropSource()) {
                ImGui.Text(gearset->NameString);
                ImGui.SetDragDropPayload(typeof(string).FullName, IntPtr.Zero, 0);
                draggedItemId = i;
                ImGui.EndDragDropSource();
            }
            if (ImGui.BeginDragDropTarget()) {
                ImGui.Separator();
                var payload = ImGui.AcceptDragDropPayload(typeof(string).FullName);
                if (payload.NativePtr != null && draggedItemId != null) {
                    if (draggedItemId < i) {
                        for (var j = (int) draggedItemId; j < i; j++) {
                            gearsetModule->ReassignGearsetId(gearsets[j], gearsets[j + 1]);
                        }
                    } else {
                        for (var j = (int) draggedItemId; j > i + 1; j--) {
                            gearsetModule->ReassignGearsetId(gearsets[j], gearsets[j - 1]);
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
        }
    }

    public override void OnOpen() {
        toastGui.Toast += ToastHandler;
    }

    public override void OnClose() {
        toastGui.Toast -= ToastHandler;
    }

    private void ToastHandler(ref SeString message, ref ToastOptions options, ref bool isHandled) {
        if (message.ToString() == "Gear set number changed.") {
            isHandled = true;
        }
    }

    public void Dispose() {
        OnClose();
    }
}
