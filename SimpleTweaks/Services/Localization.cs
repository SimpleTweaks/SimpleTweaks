using Dalamud.Logging;
using SimpleTweaks.Attributes;

namespace SimpleTweaks.Services; 

[TweakService]
public class Localization<T> {
    
    
    public Localization() {
        
    }

    public void Load(T instance) {
        PluginLog.Log($"Loading Localization for {instance}");
    }


}
