using SimpleTweaks.Attributes;
using SimpleTweaks.Services;

namespace SimpleTweaks.UI;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

public class ConfigWindow : Window {
    private PluginConfig pluginConfig;

    public ConfigWindow(PluginConfig pluginConfig) : base($"Simple Tweaks Config###SimpleTweaksConfig") {
        this.pluginConfig = pluginConfig;
        this.SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = ImGuiHelpers.ScaledVector2(600, 200),
            MaximumSize = ImGuiHelpers.ScaledVector2(800, 800),
        };

        this.IsOpen = pluginConfig.ConfigWindowOpen;
    }

    public override void OnOpen() {
        this.pluginConfig.ConfigWindowOpen = true;
        this.pluginConfig.Save();
        base.OnOpen();
    }

    public override void OnClose() {
        this.pluginConfig.ConfigWindowOpen = false;
        this.pluginConfig.ConfigWindowSearch = string.Empty;
        base.OnClose();
    }

    public override void Draw() {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("###searchInput", "Search...", ref this.pluginConfig.ConfigWindowSearch, 1024, ImGuiInputTextFlags.AutoSelectAll);
    }
}
