using System.Numerics;
using ExileCore;
using ExileCore.Shared.Helpers;

namespace SniperPlugin.Utils;

public class WindowCache(SniperPlugin plugin)
{
    private readonly SniperPlugin _plugin = plugin;
    public Vector2 PurchaseWindowLocation => _plugin.Settings.PurchaseWindowLocation;
    public Vector2 PurchaseWindowSize => _plugin.Settings.PurchaseWindowSize;
    public bool IsFullyCached => PurchaseWindowLocation != Vector2.Zero && PurchaseWindowSize != Vector2.Zero;

    public void Cache()
    {
        var stashInventoryPanel = _plugin.GameController?.IngameState?.IngameUi?.PurchaseWindow?.TabContainer?.StashInventoryPanel;

        if (stashInventoryPanel == null)
        {
            DebugWindow.LogError("[Sniper] Cache Failed: Purchase Window or its TabContainer / StashInventoryPanel not found.");
            return;
        }

        var stashTopLeft = stashInventoryPanel.GetClientRectCache.TopLeft;
        var stashSize = stashInventoryPanel.GetClientRectCache.Size;

        _plugin.Settings.PurchaseWindowLocation = new Vector2(stashTopLeft.X, stashTopLeft.Y);
        _plugin.Settings.PurchaseWindowSize = new Vector2(stashSize.Width, stashSize.Height);

        var gameWindowTopLeft = _plugin.GameController?.Window.GetWindowRectangleTimeCache.TopLeft.ToVector2Num() ?? Vector2.Zero;
        _plugin.Settings.GameWindowLocation = gameWindowTopLeft;

        DebugWindow.LogMsg($"[Sniper] Cached! Stash Pos: {_plugin.Settings.PurchaseWindowLocation}, Size: {_plugin.Settings.PurchaseWindowSize}, Game Window: {_plugin.Settings.GameWindowLocation}");
    }
}