using System.Numerics;
using Newtonsoft.Json;

namespace SimpleTweaks.Options; 

public class Color {
    [JsonIgnore]
    public Vector3 Backing;

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
    
    public Color(float r, float g, float b) {
        this.Backing = new Vector3(r, g, b);
    }
}
