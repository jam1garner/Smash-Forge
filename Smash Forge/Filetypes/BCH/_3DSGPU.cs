using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace SmashForge
{
    class _3DSGPU
    {

        public enum VertexAttribute
        {
            pos = 0,
            nrm = 1,
            col = 2,
            tx0 = 3,
            tx1 = 4,
            bone = 5,
            weight = 6,
        }

        public static int getFormatSize(int i)
        {
            switch (i)
            {
                case (int)VertexAttribute.pos:
                    return 3;
                case (int)VertexAttribute.nrm:
                    return 4;
                case (int)VertexAttribute.col:
                    return 4;
                case (int)VertexAttribute.tx0:
                    return 2;
                case (int)VertexAttribute.tx1:
                    return 2;
                case (int)VertexAttribute.bone:
                    return 2;
                case (int)VertexAttribute.weight:
                    return 2;
            }
            return 0;
        }

        public enum AttributeFormat
        {
            signedByte = 2,
            unsignedByte = 1,
            signedShort = 3,
            single = 0
        }

        public static VertexAttribPointerType getType(int i)
        {
            switch (i)
            {
                case (int)AttributeFormat.signedByte:
                    return VertexAttribPointerType.Byte;
                case (int)AttributeFormat.unsignedByte:
                    return VertexAttribPointerType.UnsignedByte;
                case (int)AttributeFormat.signedShort:
                    return VertexAttribPointerType.Short;
                case (int)AttributeFormat.single:
                    return VertexAttribPointerType.Float;
            }
            return VertexAttribPointerType.Byte;
        }

        public static int getTypeSize(int i)
        {
            switch (i)
            {
                case (int)AttributeFormat.signedByte:
                    return 1;
                case (int)AttributeFormat.unsignedByte:
                    return 1;
                case (int)AttributeFormat.signedShort:
                    return 2;
                case (int)AttributeFormat.single:
                    return 4;
            }
            return 0;
        }


    }
}
