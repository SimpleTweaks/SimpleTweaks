using Dalamud.Game;
using Dalamud.Logging;
using SimpleTweaks.Plugin;

namespace SimpleTweaks;

public class TweakManager : ITweakManager {

    private readonly ISigScanner sigScanner;


    public TweakManager(ISigScanner sigScanner) {
        this.sigScanner = sigScanner;
    }

    public void Dispose() {
        
    }

    public void InitializeClientStructs() {
        FFXIVClientStructs.Interop.Resolver.GetInstance.SetupSearchSpace(sigScanner.SearchBase);
        FFXIVClientStructs.Interop.Resolver.GetInstance.Resolve();
    }

    public void Initialize() {
        PluginLog.Log("SimpleTweaks is Ready");
    }
    
}
