using Dalamud.Game.Command;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using SimpleTweaks.Attributes;

namespace SimpleTweaks.Services; 

[TweakService(LoadOnStartup = true)]
public class Commands : IDisposable {
    private const string BaseCommand = "tweaks2";
    
    [Localizable] public string CommandHelpMessage { get; set; } = "Command handler for Simple Tweaks";
    
    private readonly ICommandManager commandManager;
    
    public Commands(ICommandManager commandManager, Localization<Commands> localization) {
        localization.Load(this);
        this.commandManager = commandManager;
        
        this.commandManager.AddHandler($"/{BaseCommand}", new CommandInfo(OnCommand) {
            HelpMessage = this.CommandHelpMessage,
            ShowInHelp = true,
        });
    }

    private void OnCommand(string command, string arguments) {
        PluginLog.Log("Command Called");
    }

    public void Dispose() {
        this.commandManager.RemoveHandler($"/{BaseCommand}");
    }
}
