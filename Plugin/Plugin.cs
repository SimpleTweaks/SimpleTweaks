using System.Diagnostics;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Game;

namespace SimpleTweaks.Plugin;

public class Plugin : IDalamudPlugin {
    public string Name => "Simple Tweaks [Mk2]";

    private readonly Framework framework;
    private readonly DalamudPluginInterface pluginInterface;

    private bool loadingPlugin;
    private DateTime writeTime;
    private DateTime updatingTime;
    private readonly Stopwatch lastUpdateCheck = Stopwatch.StartNew();

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

    private readonly string assemblyDirectory;
    private readonly string assemblyPath;

    public Plugin(DalamudPluginInterface pluginInterface, Framework framework) {
        assemblyDirectory = Path.Join(pluginInterface.AssemblyLocation.DirectoryName, "SimpleTweaks");
        assemblyPath = Path.Join(assemblyDirectory, "SimpleTweaks.dll");

        this.framework = framework;
        this.pluginInterface = pluginInterface;
        LoadPlugin();
        this.framework.Update += CheckForUpdate;
    }

    public void LoadPlugin() {
        loadingPlugin = true;
        TweakManager?.Dispose();
        TweakManager = null;
        PluginLog.Verbose("Loading Simple Tweaks");
        var clientStructsAssemblyPath = GetClientStructs(pluginInterface);
        var loadContext = new SimpleLoadContext("SimpleTweaks", new DirectoryInfo(assemblyDirectory));
        if (clientStructsAssemblyPath != null) {
            var clientStructsAssembly = loadContext.LoadFromFile(clientStructsAssemblyPath);
            loadContext.AddAssembly("FFXIVClientStructs", clientStructsAssembly);
        }
        else {
            loadContext.AddAssembly("FFXIVClientStructs", typeof(FFXIVClientStructs.Interop.Resolver).Assembly);
        }

        var fileInfo = new FileInfo(assemblyPath);
        if (!fileInfo.Exists)
            throw new Exception("Assembly 'SimpleTweaks.dll' is missing.");
        writeTime = fileInfo.LastWriteTime;
        var simpleTweaks = loadContext.LoadFromFile(assemblyPath);
        var tweakManagerT = simpleTweaks.GetTypes().FirstOrDefault(t => t.GetInterface(nameof(ITweakManager)) != null);
        if (tweakManagerT != null) {
            PluginLog.Verbose("Creating SimpleTweaks");
            var createMethod = pluginInterface.GetType().GetMethod("Create")?.MakeGenericMethod(tweakManagerT);
            if (createMethod == null) {
                PluginLog.Fatal("Failed to create the Create Method");
                loadingPlugin = false;
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
                        loadingPlugin = false;
                    });
                });
            }
            catch (Exception ex) {
                while (ex.InnerException != null) ex = ex.InnerException;
                PluginLog.Fatal(ex, "Failed to load Simple Tweaks");
                loadingPlugin = false;
            }
        }
        else {
            PluginLog.Fatal("SimpleTweaks contains no TweakManager");
            loadingPlugin = false;
        }
    }

    private void CheckForUpdate(Framework framework) {
        if (loadingPlugin || framework.IsFrameworkUnloading || TweakManager == null) {
            lastUpdateCheck.Restart();
            return;
        }

        if (lastUpdateCheck.ElapsedMilliseconds < 1000) return;
        lastUpdateCheck.Restart();
        var fileInfo = new FileInfo(assemblyPath);
        if (fileInfo.Exists && fileInfo.LastWriteTime != writeTime) {
            if (fileInfo.LastWriteTime == updatingTime) {
                LoadPlugin();
            }
            else {
                updatingTime = fileInfo.LastWriteTime;
                PluginLog.Log("Reloading Plugin...");
            }
        }
    }

    public void Dispose() {
        framework.Update -= CheckForUpdate;
        TweakManager?.Dispose();
    }
}
