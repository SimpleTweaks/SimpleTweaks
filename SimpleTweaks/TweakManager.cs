using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin;
using SimpleTweaks.Plugin;
using SimpleTweaks.Services;

namespace SimpleTweaks;

public class TweakManager : ITweakManager {
    private readonly ISigScanner sigScanner;
    private readonly DalamudPluginInterface pluginInterface;
    private ServiceManager? serviceManager;

    public TweakManager(DalamudPluginInterface pluginInterface, ISigScanner sigScanner) {
        PluginLog.Debug("Simple Tweaks is Starting...");
        this.pluginInterface = pluginInterface;
        this.sigScanner = sigScanner;
    }

    public void Dispose() {
        this.serviceManager?.Dispose();
    }

    public void InitializeClientStructs() {
        PluginLog.Debug($"Resolving ClientStructs");
        FFXIVClientStructs.Interop.Resolver.GetInstance.SetupSearchSpace(this.sigScanner.SearchBase);
        FFXIVClientStructs.Interop.Resolver.GetInstance.Resolve();
    }

    public void Initialize() {
        this.serviceManager = this.pluginInterface.Create<ServiceManager>();
        PluginLog.Debug($"SimpleTweaks is Ready");
    }
}
