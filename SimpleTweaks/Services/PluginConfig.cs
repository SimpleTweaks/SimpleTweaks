using System.Numerics;
using SimpleTweaks.Attributes;
using SimpleTweaks.Options;

namespace SimpleTweaks.Services; 


[TweakService]
public class PluginConfig : IDisposable {
    
    private readonly ConfigService<PluginConfig> configService;
    
    [ConfigOption] public bool ConfigWindowOpen = false;
    [ConfigOption] public string ConfigWindowSearch = string.Empty;
    [ConfigOption] public AlphaColor Color = new(1, 0, 0, 1);

    public PluginConfig(ConfigService<PluginConfig> configService) {
        this.configService = configService;
        this.configService.Load(this);
    }

    public void Save() {
        this.configService.Save(this);
    }
    
    public void Dispose() {
        this.configService.Save(this);
    }
}
