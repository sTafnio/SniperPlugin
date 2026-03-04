using ExileCore;
using SniperPlugin.Communication;
using SniperPlugin.Logic;
using ImGuiNET;
using SniperPlugin.Utils;
using ExileCore.Shared;
using System;

namespace SniperPlugin;

// {"success":false} - response when item in demand
public class SniperPlugin : BaseSettingsPlugin<SniperPluginSettings>
{
    public SniperServer Server;
    public SniperLogic Logic;
    public WindowCache Cache;
    public ItemDeduplicator Deduplicator;
    public ExecutionQueue Queue;

    public Action? StopStashie;
    public Action? StopMyLittleCrafter;

    public SniperPlugin() : base()
    {
        Server = new(this);
        Logic = new(this);
        Cache = new(this);
        Deduplicator = new();
        Queue = new(this);
    }

    public override bool Initialise()
    {
        Force = true;

        Settings.StartStopServer.DrawDelegate = DrawServerStatus;
        Settings.Cache.OnPressed = Cache.Cache;

        StopStashie = GameController.PluginBridge.GetMethod<Action>("Stashie.Stop");
        if (StopStashie == null)
            LogError("Stahie.Stop is null");
        else
            DebugWindow.LogMsg("[Sniper] Stashie.Stop is available");

        StopMyLittleCrafter = GameController.PluginBridge.GetMethod<Action>("MyLittleCrafter.Stop");
        if (StopMyLittleCrafter == null)
            LogError("MyLittleCrafter.Stop is null");
        else
            DebugWindow.LogMsg("[Sniper] MyLittleCrafter.Stop is available");

        return true;
    }

    private void DrawServerStatus()
    {
        ImGui.Text("Server: ");
        ImGui.SameLine();

        var isRunning = Server.IsRunning;
        if (ImGui.RadioButton("On", isRunning))
        {
            if (!isRunning) Server.Start();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Off", !isRunning))
        {
            if (isRunning) Server.Stop();
        }
    }

    public override void Render()
    {
        if (Logic.PurchaseSequenceTask != null)
            TaskUtils.RunOrRestart(ref Logic.PurchaseSequenceTask, () => null);

        if (Settings.DrawItemLocation.Value)
        {
            Logic.CurrentItem?.DrawItemLocation(this);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        Server.Stop();
    }


}