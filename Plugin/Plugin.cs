using System.Diagnostics;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Game;

namespace SimpleTweaks.Plugin;

public class Plugin : IDalamudPlugin {
    public string Name => "Simple Tweaks [Mk2]";

    private int GetClientStructsVersion(string? directory, out string clientStructs) {
        clientStructs = string.Empty;
        if (directory == null) return 0;
        var clientStructsFile = new FileInfo(Path.Join(directory, "FFXIVClientStructs.dll"));

        if (!clientStructsFile.Exists) {
            PluginLog.Verbose($"{directory} does not have a ClientStructs file.");
            return 0;
        }

        var csVersion = FileVersionInfo.GetVersionInfo(clientStructsFile.FullName);
        PluginLog.Verbose($"{directory} provides ClientStructs v{csVersion.ProductPrivatePart}");
        clientStructs = clientStructsFile.FullName;
        return csVersion.ProductPrivatePart;
    }
    
    private string? GetClientStructs(DalamudPluginInterface pluginInterface) {
        PluginLog.Verbose("Checking ClientStructs Version");
        var dalamudClientStructsVersion = GetClientStructsVersion(Path.GetDirectoryName(typeof(IDalamudPlugin).Assembly.Location), out _);
        var localClientStructsVersion = GetClientStructsVersion(Path.Join(pluginInterface.AssemblyLocation.DirectoryName, "ClientStructs"), out var ownClientStructs);
        if (localClientStructsVersion <= dalamudClientStructsVersion) {
            PluginLog.Verbose("Using Dalamud ClientStructs");
            return null;
        }

        PluginLog.Verbose("Using Own ClientStructs");
        return ownClientStructs;
    }

    public ITweakManager? TweakManager { get; private set; }
    
    public Plugin(DalamudPluginInterface pluginInterface, Framework framework) {
        PluginLog.Verbose("Loading Simple Tweaks");
        var clientStructsAssemblyPath = GetClientStructs(pluginInterface);
        var simpleTweaksDir = Path.Join(pluginInterface.AssemblyLocation.DirectoryName, "SimpleTweaks");
        var loadContext = new SimpleLoadContext("SimpleTweaks", new DirectoryInfo(simpleTweaksDir));
        if (clientStructsAssemblyPath != null) {
            var clientStructsAssembly = loadContext.LoadFromFile(clientStructsAssemblyPath);
            loadContext.AddAssembly("FFXIVClientStructs",clientStructsAssembly);
        } else {
            loadContext.AddAssembly("FFXIVClientStructs", typeof(FFXIVClientStructs.Interop.Resolver).Assembly);
        }
        
        var simpleTweaks = loadContext.LoadFromFile(Path.Join(simpleTweaksDir, "SimpleTweaks.dll"));
        var tweakManagerT = simpleTweaks.GetTypes().FirstOrDefault(t => t.GetInterface(nameof(ITweakManager)) != null);
        if (tweakManagerT != null) {
            PluginLog.Verbose("Creating SimpleTweaks");
            var createMethod = pluginInterface.GetType().GetMethod("Create")?.MakeGenericMethod(tweakManagerT);
            if (createMethod == null) {
                PluginLog.Fatal("Failed to create the Create Method");
                return;
            }

            try {
                TweakManager = (ITweakManager?)createMethod.Invoke(pluginInterface, new object?[] { Array.Empty<object>() });

                Task.Run(() => {
                    if (clientStructsAssemblyPath != null) {
                        TweakManager?.InitializeClientStructs();
                    }

                    framework.RunOnFrameworkThread(() => {
                        TweakManager?.Initialize();
                    });
                    
                    
                });
                
            } catch (Exception ex) {
                while (ex.InnerException != null) ex = ex.InnerException;
                PluginLog.Fatal(ex, "Failed to load Simple Tweaks");
            }

        } else {
            PluginLog.Fatal("SimpleTweaks contains no TweakManager");
        }
    }
    
    public void Dispose() {
        TweakManager?.Dispose();
    }

}