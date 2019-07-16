using OpenTK;

namespace SmashForge
{
    public interface IBoundableModel
    {
        Vector4 BoundingSphere { get; }
    }
}
