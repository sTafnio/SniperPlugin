using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;
using InputHumanizer.Input;
using SniperPlugin.Communication;
using SniperPlugin.Utils;

namespace SniperPlugin.Logic;

public class SniperLogic(SniperPlugin plugin)
{
    private readonly SniperPlugin _plugin = plugin;
    private readonly Random _random = new();
    public ItemDataRecord? CurrentItem { get; private set; }
    public SyncTask<bool>? PurchaseSequenceTask;

    public async Task<ItemDecision> EvaluateTeleportForItem(ItemDataRecord item)
    {
        if (!_plugin.Cache.IsFullyCached)
            return new ItemDecision(DecisionAction.Ignore, "Window coordinates not cached");

        if (!_plugin.Settings.GlobalEnable.Value)
            return new ItemDecision(DecisionAction.Ignore, "Global Toggle Disabled");

        if (_plugin.Deduplicator?.IsDuplicate(item.Id) == true)
            return new ItemDecision(DecisionAction.Ignore, "Duplicate");

        if (_plugin.Settings.GoldCheck.Value && item.Fee > _plugin.GameController.IngameState.ServerData.Gold)
            return new ItemDecision(DecisionAction.Ignore, "Not enough Gold");

        var reportedAt = DateTime.Now;
        if (!await _plugin.Queue.WaitForReadyAsync(reportedAt))
        {
            return new ItemDecision(DecisionAction.Ignore, "Queue Timeout (>30s)");
        }

        CurrentItem = item;

        DebugWindow.LogMsg($"[Sniper] Permission Granted: {item.Name}");
        if (_plugin.Settings.FocusWindow.Value) WindowFocus.FocusPoEWindow();
        return new ItemDecision(DecisionAction.Teleport, $"Approved: {item.Name}");
    }

    public void OnTeleportSuccess()
    {
        if (CurrentItem == null)
        {
            DebugWindow.LogMsg("[Sniper] Teleport Success received but CurrentItem is null. Releasing queue just in case.");
            _plugin.Queue.Release();
            return;
        }

        if (_plugin.Settings.StopStashie.Value)
            _plugin.StopStashie?.Invoke();

        if (_plugin.Settings.StopMyLittleCrafter.Value)
            _plugin.StopMyLittleCrafter?.Invoke();

        DebugWindow.LogMsg($"[Sniper] Teleport Success. Starting Purchase Sequence for {CurrentItem.Name}");
        PurchaseSequenceTask = RunPurchaseSequence(CurrentItem);
    }

    public void OnTeleportFailure(string reason)
    {
        DebugWindow.LogMsg($"[Sniper] Teleport Failed: {reason}. Releasing queue.");
        CurrentItem = null;
        _plugin.Queue.Release();
    }

    private async SyncTask<bool> RunPurchaseSequence(ItemDataRecord item)
    {
        var tryGetInputController = _plugin.GameController.PluginBridge.GetMethod<Func<string, IInputController>>("InputHumanizer.TryGetInputController");
        if (tryGetInputController == null)
        {
            DebugWindow.LogError("InputHumanizer method not registered.");
            _plugin.Queue.Release();
            return false;
        }

        var inputController = tryGetInputController(_plugin.Name);
        if (inputController == null)
        {
            DebugWindow.LogError("[Sniper] InputController could not be retrieved.");
            _plugin.Queue.Release();
            return false;
        }

        using (inputController)
        {
            try
            {
                var timeoutMs = _plugin.Settings.PurchaseWindowTimeout.Value * 1000;

                // 1. Wait for Loading Screen
                using var loadingWaitCts = new CancellationTokenSource(timeoutMs);
                var hasStartedLoading = await TaskUtils.CheckEveryFrame(() => _plugin.GameController.IsLoading || _plugin.GameController.IngameState.IngameUi.PurchaseWindow.IsVisible, loadingWaitCts.Token);

                if (!hasStartedLoading)
                {
                    DebugWindow.LogMsg($"[Sniper] Loading screen didn't appear within {timeoutMs}ms for {item.Name}");
                    return false;
                }

                DebugWindow.LogMsg($"[Sniper] Loading detected. Preparing Actions for {item.Name}");

                // 2. Move Mouse and Press Ctrl
                if (_plugin.Settings.MoveMouse.Value)
                {
                    var itemCenter = item.GetScreenCenter(_plugin);
                    await inputController.MoveMouse(itemCenter.ToVector2Num());
                    if (_plugin.Settings.LockMouse.Value) MouseLock.LockAt(itemCenter.ToVector2Num());
                }

                if (_plugin.Settings.AutoPurchase.Value)
                {
                    await inputController.KeyDown(Keys.LControlKey);
                }

                // 3. Wait for Purchase Window
                using var windowWaitCts = new CancellationTokenSource(timeoutMs);
                var isWindowVisible = await TaskUtils.CheckEveryFrame(
                    () => _plugin.GameController.IngameState.IngameUi.PurchaseWindow is { IsVisible: true },
                    windowWaitCts.Token);

                if (!isWindowVisible)
                {
                    DebugWindow.LogMsg($"[Sniper] Purchase Window timeout ({timeoutMs}ms) for {item.Name}");
                    return false;
                }

                DebugWindow.LogMsg($"[Sniper] Purchase Window detected for {item.Name}.");

                // 5. Purchase Item
                if (_plugin.Settings.AutoPurchase.Value)
                {
                    var maxAttempts = _random.Next(20, 30);
                    var startingGold = _plugin.GameController.IngameState.ServerData.Gold;
                    var successfulPurchase = false;

                    inputController.SetDelayOverrides(35, 52, 40, 15);

                    for (int i = 0; i < maxAttempts; i++)
                    {
                        if (!_plugin.GameController.IngameState.IngameUi.PurchaseWindow.IsVisible)
                        {
                            DebugWindow.LogMsg($"[Sniper] Purchase Window lost for {item.Name}. Aborting.");
                            break;
                        }

                        if (_plugin.GameController.IngameState.ServerData.Gold < startingGold)
                        {
                            DebugWindow.LogMsg($"[Sniper] Purchase Successful for {item.Name}");
                            successfulPurchase = true;
                            break;
                        }

                        var uiHover = _plugin.GameController.IngameState.UIHover;
                        if (uiHover == null)
                        {
                            DebugWindow.LogMsg($"[Sniper] Hover Item lost for {item.Name}. Aborting.");
                            break;
                        }

                        DebugWindow.LogMsg($"[Sniper] Attempt {i + 1} of {maxAttempts}");
                        await inputController.Click(MouseButtons.Left);
                    }

                    inputController.SetDelayOverrides();

                    await inputController.KeyUp(Keys.LControlKey);

                    if (_plugin.GameController.IngameState.IngameUi.PurchaseWindow is { IsVisible: true })
                    {
                        await inputController.KeyDown(Keys.Escape);
                        await inputController.KeyUp(Keys.Escape);
                    }

                    DebugWindow.LogMsg(successfulPurchase ? $"[Sniper] Successfully purchased {item.Name}" : $"[Sniper] Failed to purchase {item.Name}");
                }

                return true;
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"[Sniper] Automation Sequence Error: {e.Message}");
                return false;
            }
            finally
            {
                DebugWindow.LogMsg($"[Sniper] Sequence Complete for {item.Name}. Releasing Queue.");

                if (_plugin.Settings.LockMouse.Value) MouseLock.Unlock();
                if (Input.IsKeyDown(Keys.LControlKey)) await inputController.KeyUp(Keys.LControlKey);

                CurrentItem = null;
                PurchaseSequenceTask = null;
                _plugin.Queue.Release();
            }
        }
    }
}