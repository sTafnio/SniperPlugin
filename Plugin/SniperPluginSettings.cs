using System.Numerics;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using Newtonsoft.Json;

namespace SniperPlugin;

public class SniperPluginSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new(true);

    [JsonIgnore]
    [Menu(null, "Start / Stop server")]
    public CustomNode StartStopServer { get; set; } = new();

    [Menu("Global Enable", "Quick way to enable/disable the plugin")]
    public ToggleNode GlobalEnable { get; set; } = new(false);

    [Menu("Focus Window", "Focus the game window when an item is found")]
    public ToggleNode FocusWindow { get; set; } = new(false);

    [Menu("Draw Item Location", "Draw the location of the item on screen")]
    public ToggleNode DrawItemLocation { get; set; } = new(false);

    [Menu("Gold Check", "Ignore Items with higher Gold fee than available Gold")]
    public ToggleNode GoldCheck { get; set; } = new(false);

    [Menu("Move Mouse", "Move mouse to item location")]
    public ToggleNode MoveMouse { get; set; } = new(false);

    [Menu("Lock Mouse", "Lock mouse after moving to item location")]
    public ToggleNode LockMouse { get; set; } = new(false);

    [Menu("Auto Purchase", "Attempt to auto purchase items after moving mouse to item location")]
    public ToggleNode AutoPurchase { get; set; } = new(false);

    [Menu("Purchase Window Timeout", "Timeout for purchase window to appear (from start of teleport)")]
    public RangeNode<int> PurchaseWindowTimeout { get; set; } = new(6, 1, 20);

    [Menu("Skip Key", "Key to skip the current item")]
    public HotkeyNodeV2 SkipKey { get; set; } = new(Keys.None);

    [Menu("Cache", "Cache game window, purchase window location and size")]
    public ButtonNode Cache { get; set; } = new();

    public Vector2 GameWindowLocation { get; set; } = Vector2.Zero;
    public Vector2 PurchaseWindowLocation { get; set; } = Vector2.Zero;
    public Vector2 PurchaseWindowSize { get; set; } = Vector2.Zero;
}