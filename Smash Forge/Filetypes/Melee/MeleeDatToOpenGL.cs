using MeleeLib.GCX;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Filetypes.Melee
{
    public static class MeleeDatToOpenGL
    {
        public static AlphaFunction GetAlphaFunctionFromCompareType(GXCompareType compareType)
        {
            switch (compareType)
            {
                case GXCompareType.Never:
                    return AlphaFunction.Never;
                case GXCompareType.Less:
                    return AlphaFunction.Less;
                case GXCompareType.Equal:
                    return AlphaFunction.Equal;
                case GXCompareType.LEqual:
                    return AlphaFunction.Lequal;
                case GXCompareType.Greater:
                    return AlphaFunction.Greater;
                case GXCompareType.NEqual:
                    return AlphaFunction.Notequal;
                case GXCompareType.GEqual:
                    return AlphaFunction.Gequal;
                case GXCompareType.Always:
                    return AlphaFunction.Always;
                default:
                    return AlphaFunction.Always;
            }
        }

        public static PrimitiveType GetGLPrimitiveType(GXPrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case GXPrimitiveType.Points:
                    return PrimitiveType.Points;
                case GXPrimitiveType.Lines:
                    return PrimitiveType.Lines;
                case GXPrimitiveType.LineStrip:
                    return PrimitiveType.LineStrip;
                case GXPrimitiveType.TriangleFan:
                    return PrimitiveType.TriangleFan;
                case GXPrimitiveType.TriangleStrip:
                    return PrimitiveType.TriangleStrip;
                case GXPrimitiveType.Triangles:
                    return PrimitiveType.Triangles;
                case GXPrimitiveType.Quads:
                    return PrimitiveType.Quads;
                default:
                    return PrimitiveType.Triangles;
            }
        }
    }
}
