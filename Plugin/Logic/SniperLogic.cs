using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore;
using SniperPlugin.Communication;

namespace SniperPlugin.Logic;

public class SniperLogic(SniperPlugin plugin)
{
    private readonly SniperPlugin _plugin = plugin;
    private readonly ItemDeduplicator _deduplicator = new();
    private readonly ExecutionQueue _queue = new(plugin);
    private ItemDataRecord? _currentItem;

    public async Task<ItemDecision> EvaluateTeleportForItem(ItemDataRecord item)
    {
        if (!_plugin.Settings.GlobalEnable.Value)
            return new ItemDecision(DecisionAction.Ignore, "Global Toggle Disabled");

        if (_deduplicator.IsDuplicate(item.Id))
            return new ItemDecision(DecisionAction.Ignore, "Duplicate");

        if (_plugin.Settings.GoldCheck.Value && item.Fee > _plugin.GameController.IngameState.ServerData.Gold)
            return new ItemDecision(DecisionAction.Ignore, "Not enough Gold");

        var reportedAt = DateTime.Now;
        if (!await _queue.WaitForReadyAsync(reportedAt))
        {
            return new ItemDecision(DecisionAction.Ignore, "Queue Timeout (>30s)");
        }

        _currentItem = item;

        DebugWindow.LogMsg($"[Sniper] Permission Granted: {item.Name}");
        return new ItemDecision(DecisionAction.Teleport, $"Approved: {item.Name}");
    }

    public void OnTeleportSuccess()
    {
        if (_currentItem == null) return;
        _ = RunPurchaseSequence(_currentItem);
    }

    public void OnTeleportFailure(string reason)
    {
        DebugWindow.LogMsg($"[Sniper] Teleport Failed: {reason}. Releasing queue.");
        _currentItem = null;
        _queue.Release();
    }

    private async Task RunPurchaseSequence(ItemDataRecord item)
    {
        try
        {
            DebugWindow.LogMsg($"[Sniper] Whisper Success. Watching for {item.Name} Trade Window...");

            var start = DateTime.Now;
            bool windowVisible = false;
            while ((DateTime.Now - start).TotalSeconds < 10)
            {
                var window = _plugin.GameController.IngameState.IngameUi.PurchaseWindow;
                if (window is { IsVisible: true })
                {
                    windowVisible = true;
                    break;
                }

                if (_plugin.GameController.IsLoading) start = DateTime.Now;

                await Task.Delay(100);
            }

            if (windowVisible)
            {
                DebugWindow.LogMsg($"[Sniper] Purchase Window Detected. Running Automation.");
                await Task.Delay(1500);
                Input.KeyDown(Keys.Escape);
                await Task.Delay(100);
                Input.KeyUp(Keys.Escape);
            }
            else
            {
                DebugWindow.LogMsg($"[Sniper] Automation Timeout: Window never appeared for {item.Name}");
            }
        }
        catch (Exception e)
        {
            DebugWindow.LogError($"[Sniper] Automation Sequence Error: {e.Message}");
        }
        finally
        {
            DebugWindow.LogMsg($"[Sniper] Sequence Complete for {item.Name}. Releasing Queue.");
            _currentItem = null;
            _queue.Release();
        }
    }
}
