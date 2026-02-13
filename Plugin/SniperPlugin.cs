using ExileCore;
using SniperPlugin.Communication;
using SniperPlugin.Logic;

namespace SniperPlugin;

// {"success":false} - response when item in demand
public class SniperPlugin : BaseSettingsPlugin<SniperPluginSettings>
{
    private SniperServer? _server;
    public SniperLogic? Logic;

    public override bool Initialise()
    {
        Logic = new SniperLogic(this);
        _server = new SniperServer(this);
        _server.Start(Settings.ServerPort.Value);

        return true;
    }

    public override void Render()
    {
    }

    public override void OnUnload()
    {
        _server?.Stop();
    }

    public override void OnClose()
    {
        base.OnClose();
        _server?.Stop();
    }

}