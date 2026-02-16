using SharpDX;

namespace SniperPlugin.Communication;

public static class ItemDataExtensions
{
    public static RectangleF GetScreenRectangle(this ItemDataRecord item, SniperPlugin plugin, bool forDrawing = false)
    {
        var gameWindowPos = plugin.Settings.GameWindowLocation;
        var purchaseWindowPos = plugin.Settings.PurchaseWindowLocation;
        var purchaseWindowSize = plugin.Settings.PurchaseWindowSize;

        var cellW = purchaseWindowSize.X / 12f;
        var cellH = purchaseWindowSize.Y / 12f;

        var itemX = (forDrawing ? 0 : gameWindowPos.X) + purchaseWindowPos.X + (item.Position.X * cellW);
        var itemY = (forDrawing ? 0 : gameWindowPos.Y) + purchaseWindowPos.Y + (item.Position.Y * cellH);

        var width = item.Size.W * cellW;
        var height = item.Size.H * cellH;

        return new RectangleF(itemX, itemY, width, height);
    }

    public static Vector2 GetScreenCenter(this ItemDataRecord item, SniperPlugin plugin)
    {
        var rect = item.GetScreenRectangle(plugin);
        return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    }

    public static void DrawItemLocation(this ItemDataRecord item, SniperPlugin plugin)
    {
        var rect = item.GetScreenRectangle(plugin, forDrawing: true);
        plugin.Graphics.DrawFrame(rect, Color.Red, 3);
    }
}
