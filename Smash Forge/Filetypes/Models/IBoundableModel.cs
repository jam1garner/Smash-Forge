using OpenTK;

namespace Smash_Forge
{
    public interface IBoundableModel
    {
        Vector4 BoundingSphere { get; }
    }
}
