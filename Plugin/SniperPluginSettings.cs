using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace SniperPlugin;

public class SniperPluginSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Global Enable", "Quick way to enable/disable the plugin")]
    public ToggleNode GlobalEnable { get; set; } = new ToggleNode(false);

    [Menu("Gold Check", "Ignore Items with higher Gold fee than available Gold")]
    public ToggleNode GoldCheck { get; set; } = new ToggleNode(true);

    [Menu("Server Port", "Port to listen on")]
    public RangeNode<int> ServerPort { get; set; } = new RangeNode<int>(49152, 49150, 49160);
}