namespace SimpleTweaks.Exceptions; 

public class InvalidTweakServiceException : Exception {
    public InvalidTweakServiceException(Type loadingType, Type dependencyType) : base($"Invalid TweakService dependency in {loadingType}: {dependencyType} is not a valid TweakService.") { }
}
