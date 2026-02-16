using System;
using System.Threading;
using System.Threading.Tasks;
using ExileCore;

namespace SniperPlugin.Logic;

public class ExecutionQueue(SniperPlugin plugin)
{
    private readonly SniperPlugin _plugin = plugin;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private const int MaxWaitSeconds = 30;
    private DateTime _lastLogTime = DateTime.MinValue;

    public async Task<bool> WaitForReadyAsync(DateTime reportedAt)
    {
        var timeSinceReport = (DateTime.Now - reportedAt).TotalSeconds;
        var remainingSeconds = MaxWaitSeconds - timeSinceReport;

        if (remainingSeconds <= 0) return false;

        if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(remainingSeconds)))
        {
            return false;
        }

        try
        {
            while (true)
            {
                if ((DateTime.Now - reportedAt).TotalSeconds > MaxWaitSeconds)
                {
                    _semaphore.Release();
                    return false;
                }

                if (IsGameReady(out var reason)) return true;

                if ((DateTime.Now - _lastLogTime).TotalSeconds > 3)
                {
                    DebugWindow.LogMsg($"[Sniper] Queue Waiting: {reason}");
                    _lastLogTime = DateTime.Now;
                }

                await Task.Delay(1);
            }
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    public void Release()
    {
        if (_semaphore.CurrentCount == 0)
        {
            _semaphore.Release();
        }
    }

    private bool IsGameReady(out string reason)
    {
        if (!_plugin.GameController.IngameState.InGame)
        {
            reason = "Not in Game";
            return false;
        }

        if (_plugin.GameController.IsLoading)
        {
            reason = "Game is Loading";
            return false;
        }

        if (!_plugin.GameController.Area.CurrentArea.IsHideout &&
            !_plugin.GameController.Area.CurrentArea.IsTown)
        {
            reason = $"Not in Hideout or Town (Current: {_plugin.GameController.Area.CurrentArea.Name})";
            return false;
        }

        if (_plugin.GameController.IngameState.IngameUi.PurchaseWindow.IsVisible)
        {
            reason = "Purchase Window is Visible";
            return false;
        }

        reason = "Ready";
        return true;
    }
}
