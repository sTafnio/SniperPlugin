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
        await _semaphore.WaitAsync();

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

                // Log reason every 2 seconds so we don't spam
                if ((DateTime.Now - _lastLogTime).TotalSeconds > 2)
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
        if (_plugin.GameController.IsLoading)
        {
            reason = "Game is Loading";
            return false;
        }

        if (!_plugin.GameController.Area.CurrentArea.IsHideout)
        {
            reason = $"Not in Hideout (Current: {_plugin.GameController.Area.CurrentArea.Name})";
            return false;
        }

        reason = "Ready";
        return true;
    }
}
