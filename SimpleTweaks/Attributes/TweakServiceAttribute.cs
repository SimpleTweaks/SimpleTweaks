namespace SimpleTweaks.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TweakServiceAttribute : Attribute {
    public bool LoadOnStartup { get; init; }
}
