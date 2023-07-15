namespace SimpleTweaks.Attributes; 

public class CommandHandleAttribute : Attribute {

    public string[] Params;
    
    public CommandHandleAttribute(params string[] @params) {
        this.Params = @params;
    }
}
