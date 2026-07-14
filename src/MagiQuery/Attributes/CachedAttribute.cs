namespace MagiQuery.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class Cached: Attribute
{
    public int DepthLevel { get; }
    
    public Cached(int depthLevel = 10)
    {
        DepthLevel = depthLevel;
    }
}