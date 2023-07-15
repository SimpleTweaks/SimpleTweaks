using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json.Linq;
using SimpleTweaks.Attributes;

namespace SimpleTweaks.Services; 

[TweakService]
public class ConfigService<T> {

    private FieldInfo[] fields;

    private string configFilePath;
    

    public ConfigService(DalamudPluginInterface pluginInterface) {
        var configDir = pluginInterface.GetPluginConfigDirectory();
        this.configFilePath = Path.Join(configDir, $"{typeof(T).FullName}.json");
        this.fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.GetCustomAttribute<ConfigOptionAttribute>() != null).ToArray();
    }
    
    public void Load(T instance) {
        var json = File.ReadAllText(this.configFilePath).Trim();
        if (!json.StartsWith('{') && json.EndsWith('}')) return;
        try {
            var jObject = JObject.Parse(json);
            foreach (var f in this.fields) {
                try {
                    if (jObject.TryGetValue(f.Name, out var token)) {
                        var value = token.ToObject(f.FieldType);
                        PluginLog.Log($" [load] {f.Name} <= {value}");
                        f.SetValue(instance, value);
                    }
                    else {
                        PluginLog.Log($" [load] {f.Name} is default.");
                    }
                }
                catch (Exception ex) {
                    
                }
               
                
            }
        }
        catch (Exception ex) {
            PluginLog.Error(ex, "Error loading config for: {0}", nameof(T));
        }


    }


    public void Save(T instance) {
        PluginLog.Log($"Saving Config for {instance}");
        var output = new JObject();
        foreach (var f in this.fields) {
            var value = f.GetValue(instance);
            PluginLog.Log($" [save] {f.Name} => {value}");
            output.Add(f.Name, value == null ? null : JToken.FromObject(value));
        }
        
        File.WriteAllText(this.configFilePath, output.ToString());
    }
    
    
}
