using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VBN_Editor
{
    public class LM : FileBase
    {
        enum ChunkType : int
        {
            Invalid = 0x0000,

            Unk000A = 0x000A,
            DynamicText = 0x0025,
            Symbols = 0xF001,
            Colors = 0xF002,
            Transforms = 0xF003,
            Extents = 0xF004,
            ActionScript = 0xF005,
            TextureAtlases = 0xF007,
            UnkF008 = 0xF008,
            UnkF009 = 0xF009,
            UnkF00A = 0xF00A,
            UnkF00B = 0xF00B,
            Properties = 0xF00C,
            UnkF00D = 0xF00D,
            Shape = 0xF022,
            Graphic = 0xF024,
            Positions = 0xF103,

            MovieClip = 0x0027,
            Frame = 0x0001,
            Keyframe = 0xF105,
            ObjectPlacement = 0x0004,
            ObjectDeletion = 0x0005,
            Action = 0x000C,

            End = 0xFF00
        }

        public struct Color
        {
            public short r;
            public short g;
            public short b;
            public short a;
        }

        public struct Properties
        {
            public int unk1;
            public int unk2;
            public int unk3;
            public int unk4;
            public int unk5;
            public int unk6;
            public int unk7;
            public int framerate;
            public int width;
            public int height;
            public int unk8;
            public int unk9;
        }

        public struct TextureAtlas
        {
            public int id;
            public int unk;

            public float width;
            public float height;
        }

        public class Vertex
        {
            public Vector2 pos;
            public Vector2 uv;

            public Vertex()
            {

            }

            public Vertex(float x, float y, float u, float v)
            {
                pos = new Vector2(x, y);
                uv = new Vector2(u, v);
            }
        }

        public struct Graphic
        {
            public int nameId;
            public int atlasId;
            public short unk1;

            public List<Vertex> verts { get; set; }
            public List<int> indices { get; set; }
        }

        public class Shape
        {
            public int id;
            public int unk1;
            public int texlistEntry;
            public int unk2;

            public List<Graphic> graphics { get; set; }

            public Shape(FileData f)
            {
                id = f.readInt();
                unk1 = f.readInt();
                texlistEntry = f.readInt();
                unk2 = f.readInt();
                graphics = new List<Graphic>();

                int numGraphics = f.readInt();

                for (int i = 0; i < numGraphics; i++)
                {
                    f.skip(0x08); // graphic chunk header
                    Graphic graphic = new Graphic();
                    graphic.atlasId = f.readInt();
                    graphic.unk1 = f.readShort();
                    graphic.verts = new List<Vertex>();
                    graphic.indices = new List<int>();

                    ushort numVerts = f.readUShort();
                    int numIndices = f.readInt();

                    for (int j = 0; j < numVerts; j++)
                    {
                        graphic.verts.Add(new Vertex(
                            f.readFloat(), f.readFloat(),
                            f.readFloat(), f.readFloat()
                        ));
                    }

                    for (int j = 0; j < numIndices; j++)
                    {
                        graphic.indices.Add(f.readUShort());
                    }

                    // indices are padded to word boundaries
                    if ((numIndices % 2) != 0)
                    {
                        f.skip(0x02);
                    }

                    graphics.Add(graphic);
                }
            }
        }

        public struct DynamicText
        {
            public int id;
            public int unk1;
            public string placeholderText;
            public int unk2;
            public int colorId;
            public int unk3;
            public int unk4;
            public int unk5;
            public short alignment;
            public short unk6;
            public int unk7;
            public int unk8;
            public float size;
            public int unk9;
            public int unk10;
            public int unk11;
            public int unk12;
        }

        public class MovieClip
        {
            public struct Label
            {
                public int nameId;
                public int startFrame;
                public int unk1;
            }

            public struct Placement
            {
                public int objectId;
                public int placementId;
                public int unk1;
                public int nameId;
                public short unk2;
                public short unk3;
                public short mcObjectId;
                public short unk4;

                public short transformFlags;
                public short transformId;
                public short positionFlags;
                public short positionId;
                public int colorId1;
                public int colorId2;

                public int numF037s;
                public int numF014s;
            }

            public struct Deletion
            {
                public int unk1;
                public short mcObjectId; // or was it placement id?
                public short unk2;
            }

            public struct Action
            {
                public int actionId;
                public int unk1;
            }

            public struct Frame
            {
                public int id;

                public List<Placement> placements { get; set; }
                public List<Deletion> deletions { get; set; }
                public List<Action> actions { get; set; }
            }

            public int id;
            public int unk1;
            public int unk2;
            public int unk3;

            public List<Label> labels { get; set; }
            public List<Frame> frames { get; set; }
            public List<Frame> keyframes { get; set; }

            public MovieClip()
            {
                labels = new List<Label>();
                frames = new List<Frame>();
                keyframes = new List<Frame>();
            }

            public MovieClip(FileData f) : this()
            {
                id = f.readInt();
                unk1 = f.readInt();
                unk2 = f.readInt();

                int numLabels = f.readInt();
                int numFrames = f.readInt();
                int numKeyframes = f.readInt();

                unk3 = f.readInt();

                for (int i = 0; i < numLabels; i++)
                {
                    f.skip(0x08);

                    Label label = new Label();
                    label.nameId = f.readInt();
                    label.startFrame = f.readInt();
                    label.unk1 = f.readInt();
                    labels.Add(label);
                }

                int totalFrames = numFrames + numKeyframes;
                for (int frameId = 0; frameId < totalFrames; frameId++)
                {
                    ChunkType frameType = (ChunkType)f.readInt();
                    f.skip(0x04);

                    Frame frame = new Frame();
                    frame.id = f.readInt();
                    int numChildren = f.readInt();

                    for (int childId = 0; childId < numChildren; childId++)
                    {
                        ChunkType childType = (ChunkType)f.readInt();
                        int childSize = f.readInt();

                        if (childType == ChunkType.ObjectPlacement)
                        {
                            Placement placement = new Placement();
                            placement.objectId = f.readInt();
                            placement.placementId = f.readInt();
                            placement.unk1 = f.readInt();
                            placement.nameId = f.readInt();
                            placement.unk2 = f.readShort();
                            placement.unk3 = f.readShort();
                            placement.mcObjectId = f.readShort();
                            placement.unk4 = f.readShort();
                            placement.transformFlags = f.readShort();
                            placement.transformId = f.readShort();
                            placement.positionFlags = f.readShort();
                            placement.positionId = f.readShort();
                            placement.colorId1 = f.readInt();
                            placement.colorId2 = f.readInt();
                            placement.numF037s = f.readInt();
                            placement.numF014s = f.readInt();

                            int numUnhandled = placement.numF037s + placement.numF014s;
                            for (int i = 0; i < numUnhandled; i++)
                            {
                                f.skip(0x04);
                                int size = f.readInt();
                                f.skip(size * 4);
                            }

                            frame.placements.Add(placement);
                        }
                        else if (childType == ChunkType.ObjectDeletion)
                        {
                            Deletion deletion = new Deletion();
                            deletion.unk1 = f.readInt();
                            deletion.mcObjectId = f.readShort();
                            deletion.unk2 = f.readShort();
                            frame.deletions.Add(deletion);
                        }
                        else if (childType == ChunkType.Action)
                        {
                            Action action = new Action();
                            action.actionId = f.readInt();
                            action.unk1 = f.readInt();
                            frame.actions.Add(action);
                        }
                    }

                    if (frameType == ChunkType.Keyframe)
                    {
                        keyframes.Add(frame);
                    }
                    else
                    {
                        frames.Add(frame);
                    }
                }
            }
        }

        public override System.IO.Endianness Endian { get; set; }

        public List<string> symbols { get; set; }
        public List<Color> colors { get; set; }
        public List<Matrix3x2> transforms { get; set; }
        public List<Vector2> positions { get; set; }
        public List<TextureAtlas> textureAtlases { get; set; }
        public Properties properties;
        public List<Shape> shapes { get; set; }
        public List<DynamicText> texts { get; set; }
        public List<MovieClip> movieclips { get; set; }

        public LM()
        {
            symbols = new List<string>();
            colors = new List<Color>();
            transforms = new List<Matrix3x2>();
            positions = new List<Vector2>();
            textureAtlases = new List<TextureAtlas>();
            shapes = new List<Shape>();
            texts = new List<DynamicText>();
            movieclips = new List<MovieClip>();
        }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            int magic = f.readInt();

            // I don't know what this is for, but it's always the same value
            // so I use it as an endianness sentinel.
            int endianness = f.readInt();
            if (endianness == 0x10)
            {
                f.Endian = System.IO.Endianness.Big;
                Endian = System.IO.Endianness.Big;
            }
            else
            {
                f.Endian = System.IO.Endianness.Little;
                Endian = System.IO.Endianness.Little;
            }

            f.skip(0x14);
            int fileSize = f.readInt();
            f.skip(0x20);

            bool done = false;
            while (!done)
            {
                ChunkType chunkType = (ChunkType)f.readInt();
                int chunkSize = f.readInt(); // in dwords!

                switch (chunkType)
                {
                    // Invalid chunk
                    case ChunkType.Invalid:
                        // uhhh. i think there's a specific exception for this
                        throw new Exception("Malformed file");

                    case ChunkType.Symbols:
                        int numSymbols = f.readInt();

                        while (symbols.Count() < numSymbols)
                        {
                            int len = f.readInt();

                            symbols.Add(f.readString());
                            f.skip(4 - (f.pos() % 4));
                        }

                        break;

                    case ChunkType.Colors:
                        int numColors = f.readInt();

                        for (int i = 0; i < numColors; i++)
                        {
                            Color color;
                            color.r = f.readShort();
                            color.g = f.readShort();
                            color.b = f.readShort();
                            color.a = f.readShort();

                            colors.Add(color);
                        }
                        break;

                    case ChunkType.End:
                        done = true;
                        break;

                    case ChunkType.Transforms:
                        int numTransforms = f.readInt();

                        for (int i = 0; i < numTransforms; i++)
                        {
                            // idk if this is the right order for opentk. should be?
                            transforms.Add(new Matrix3x2(
                                f.readFloat(), f.readFloat(),
                                f.readFloat(), f.readFloat(),
                                f.readFloat(), f.readFloat()
                            ));
                        }

                        break;

                    case ChunkType.Positions:
                        int numPositions = f.readInt();

                        for (int i = 0; i < numPositions; i++)
                        {
                            positions.Add(new Vector2(f.readFloat(), f.readFloat()));
                        }

                        break;

                    case ChunkType.Properties:
                        properties.unk1 = f.readInt();
                        properties.unk2 = f.readInt();
                        properties.unk3 = f.readInt();
                        properties.unk4 = f.readInt();
                        properties.unk5 = f.readInt();
                        properties.unk6 = f.readInt();
                        properties.unk7 = f.readInt();
                        properties.framerate = f.readInt();
                        properties.width = f.readInt();
                        properties.height = f.readInt();
                        properties.unk8 = f.readInt();
                        properties.unk9 = f.readInt();
                        break;

                    case ChunkType.TextureAtlases:
                        int numAtlases = f.readInt();

                        for (int i = 0; i < numAtlases; i++)
                        {
                            TextureAtlas atlas = new TextureAtlas();
                            atlas.id = f.readInt();
                            atlas.unk = f.readInt();
                            atlas.width = f.readFloat();
                            atlas.height = f.readFloat();

                            textureAtlases.Add(atlas);
                        }

                        break;

                    case ChunkType.Shape:
                        shapes.Add(new Shape(f));
                        break;

                    case ChunkType.DynamicText:
                        DynamicText text = new DynamicText();
                        text.id = f.readInt();
                        text.unk1 = f.readInt();
                        text.placeholderText = symbols[f.readInt()];
                        text.unk2 = f.readInt();
                        text.colorId = f.readInt();
                        text.unk3 = f.readInt();
                        text.unk4 = f.readInt();
                        text.unk5 = f.readInt();
                        text.alignment = f.readShort();
                        text.unk6 = f.readShort();
                        text.unk7 = f.readInt();
                        text.unk8 = f.readInt();
                        text.size = f.readFloat();
                        text.unk9 = f.readInt();
                        text.unk10 = f.readInt();
                        text.unk11 = f.readInt();
                        text.unk12 = f.readInt();
                        texts.Add(text);
                        break;

                    case ChunkType.MovieClip:
                        movieclips.Add(new MovieClip(f));
                        break;

                    default:
                        f.skip(chunkSize * 4);
                        break;
                }
            }
        }

        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }
    }
}
