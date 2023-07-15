using System.Numerics;
using Newtonsoft.Json;

namespace SimpleTweaks.Options; 

public class AlphaColor {
    [JsonIgnore]
    public Vector4 Backing;

    public float Red {
        get => this.Backing.X;
        set => this.Backing.X = value;
    }
    public float Green {
        get => this.Backing.X;
        set => this.Backing.X = value;
    }
    public float Blue {
        get => this.Backing.X;
        set => this.Backing.X = value;
    }
    public float Alpha {
        get => this.Backing.X;
        set => this.Backing.X = value;
    }
    
    public AlphaColor(float r, float g, float b, float a) {
        this.Backing = new Vector4(r, g, b, a);
    }

    public AlphaColor(Color color, float a = 1) {
        this.Backing = new Vector4(color.Backing, a);
    }
}
