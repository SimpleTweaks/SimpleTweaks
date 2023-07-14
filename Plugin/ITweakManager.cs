namespace SimpleTweaks.Plugin; 

public interface ITweakManager : IDisposable {
    public void InitializeClientStructs();
    public void Initialize();
}
