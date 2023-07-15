using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using SimpleTweaks.Attributes;
using SimpleTweaks.UI;

namespace SimpleTweaks.Services;

[TweakService(LoadOnStartup = true)]
public class UserInterface : IDisposable {
    private readonly PluginConfig pluginConfig;
    private readonly DalamudPluginInterface pluginInterface;
    private readonly WindowSystem windowSystem = new WindowSystem("SimpleTweaks");
    private readonly ConfigWindow? configWindow;
    

    public UserInterface(ServiceManager serviceManager, PluginConfig pluginConfig, DalamudPluginInterface pluginInterface) {
        this.pluginConfig = pluginConfig;
        this.configWindow = serviceManager.Create<ConfigWindow>();
        this.pluginInterface = pluginInterface;
        if (this.configWindow != null) {
            this.configWindow.IsOpen = true;
            this.windowSystem.AddWindow(this.configWindow);
        }
        pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;

    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
    }
}
