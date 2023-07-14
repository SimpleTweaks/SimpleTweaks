using System.Reflection;
using System.Runtime.Loader;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace SimpleTweaks.Plugin; 

public class SimpleLoadContext : AssemblyLoadContext {

    private readonly DirectoryInfo directory;
    private readonly string name;
    
    
    
    
    public SimpleLoadContext(string name, DirectoryInfo directoryInfo) : base(true) {
        this.name = name;
        directory = directoryInfo;
        
        handledAssemblies = new Dictionary<string, Assembly>() {
            ["Dalamud"] = typeof(DalamudPluginInterface).Assembly,
            ["SimpleTweaksPlugin2"] = typeof(Plugin).Assembly,
        };
    }

    public void AddAssembly(string name, Assembly assembly) {
        handledAssemblies.TryAdd(name, assembly);
    }


    private readonly Dictionary<string, Assembly> handledAssemblies;
    
    protected override Assembly? Load(AssemblyName assemblyName) {
        PluginLog.Verbose($"[{name}] Attempting to load {assemblyName.FullName}");

        if (assemblyName.Name != null && handledAssemblies.ContainsKey(assemblyName.Name)) {
            PluginLog.Verbose($"[{name}] Forwarded reference to {assemblyName.Name}");
            return handledAssemblies[assemblyName.Name];
        }
        
        var file = Path.Join(directory.FullName, $"{assemblyName.Name}.dll");
        if (File.Exists(file)) {
            try {
                PluginLog.Verbose($"[{name}] Attempting to load {assemblyName.Name} from {file}");
                return LoadFromFile(file);
            } catch {
                //
            }
        }
        
        return base.Load(assemblyName);
    }

    public Assembly LoadFromFile(string filePath) {
        using var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var pdbPath = Path.ChangeExtension(filePath, ".pdb");
        if (!File.Exists(pdbPath)) return LoadFromStream(file);
        using var pdbFile = File.Open(pdbPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return LoadFromStream(file, pdbFile);
    }
}