using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Diagnostics;



namespace Smash_Forge
{
    public abstract class LVDEntry
    {
        public abstract string magic { get; }
        public string name;
        public string subname;
        public float[] startPos = new float[3];
        public bool useStartPos = false;
        public int unk1 = 0;
        public float[] unk2 = new float[3];
        public int unk3 = 0;
        public char[] boneName = new char[0x40];
        
        public void read(FileData f)
        {
            f.skip(0xC);
            
            f.skip(1);
            name = f.readString(f.pos(), 0x38);
            f.skip(0x38);
            
            f.skip(1);
            subname = f.readString(f.pos(), 0x40);
            f.skip(0x40);
            
            f.skip(1);
            for (int i = 0; i < 3; i++)
                startPos[i] = f.readFloat();
            useStartPos = Convert.ToBoolean(f.readByte());
            
            f.skip(1);
            unk1 = f.readInt(); //Some kind of count? Only seen it as 0 so I don't know what it's for
            
            //Not sure what this is for, but it seems like an x,y,z followed by a hash
            f.skip(1);
            for (int i = 0; i < 3; i++)
                unk2[i] = f.readFloat();
            unk3 = f.readInt();
            
            f.skip(1);
            boneName = new char[0x40];
            for (int i = 0; i < 0x40; i++)
                boneName[i] = (char)f.readByte();
        }
        public void save(FileOutput f)
        {
            f.writeHex(magic);
            
            f.writeByte(1);
            f.writeString(name.PadRight(0x38, (char)0));
            
            f.writeByte(1);
            f.writeString(subname.PadRight(0x40, (char)0));
            
            f.writeByte(1);
            foreach (float i in startPos)
                f.writeFloat(i);
            f.writeFlag(useStartPos);
            
            f.writeByte(1);
            f.writeInt(unk1);
            
            f.writeByte(1);
            foreach (float i in unk2)
                f.writeFloat(i);
            f.writeInt(unk3);
            
            f.writeByte(1);
            f.writeChars(boneName);
        }
    }

    public class Vector2D
    {
        public float x;
        public float y;
    }
    
    public class Point : LVDEntry
    {
        public override string magic { get { return ""; } }
        public float x;
        public float y;
    }
    
    public class CollisionMat
    {
        //public bool leftLedge;
        //public bool rightLedge;
        //public bool noWallJump;
        //public byte physicsType;
        public byte[] material = new byte[0xC];

        public bool getFlag(int n)
        {
            return ((material[10] & (1 << n)) != 0);
        }

        public byte getPhysics()
        {
            return material[3];
        }

        public void setFlag(int flag, bool value)
        {
            //Console.WriteLine("B - " + getFlag(flag));
            byte mask = (byte)(1 << flag);
            bool isSet = (material[10] & mask) != 0;
            if(value)
                material[10] |= mask;
            else
                material[10] &= (byte)~mask;
            //Console.WriteLine("A - " + getFlag(flag));
        }

        public void setPhysics(byte b)
        {
            material[3] = b;
        }
    }

    public class CollisionCliff : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }
        
        public Vector2D pos;
        public Vector2D angle; //Someone figure out what these angles are
        
        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(1);
            pos = new Vector2D();
            pos.x = f.readFloat();
            pos.y = f.readFloat();
            angle = new Vector2D();
            angle.x = f.readFloat();
            angle.y = f.readFloat();
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeByte(1);
            f.writeFloat(pos.x);
            f.writeFloat(pos.y);
            f.writeFloat(angle.x);
            f.writeFloat(angle.y);
        }
    }
    
    public class Collision : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }
        
        public List<Vector2D> verts = new List<Vector2D>();
        public List<Vector2D> normals = new List<Vector2D>();
        public List<CollisionCliff> cliffs = new List<CollisionCliff>();
        public List<CollisionMat> materials = new List<CollisionMat>();
        public bool flag1 = false, flag2 = false, flag3 = false, flag4 = false;

        public Collision()
        {

        }

        public void read(FileData f)
        {
            base.read(f);
            
            flag1 = Convert.ToBoolean(f.readByte());
            flag2 = Convert.ToBoolean(f.readByte());
            flag3 = Convert.ToBoolean(f.readByte());
            flag4 = Convert.ToBoolean(f.readByte());
            
            f.skip(1);
            int vertCount = f.readInt();
            for(int i = 0; i < vertCount; i++)
            {
                f.skip(1);
                Vector2D temp = new Vector2D();
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                verts.Add(temp);
            }
            
            f.skip(1);
            int normalCount = f.readInt();
            for(int i = 0; i < normalCount; i++)
            {
                f.skip(1);
                Vector2D temp = new Vector2D();
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                normals.Add(temp);
            }
            
            f.skip(1);
            int cliffCount = f.readInt();
            for(int i = 0; i < cliffCount; i++)
            {
                CollisionCliff temp = new CollisionCliff();
                temp.read(f);
                cliffs.Add(temp);
            }
            
            f.skip(1);
            int materialCount = f.readInt();
            for(int i = 0; i < materialCount; i++)
            {
                f.skip(1);
                CollisionMat temp = new CollisionMat();
                temp.material = f.read(0xC);//Temporary, will work on fleshing out material more later
                
                materials.Add(temp);
            }

        }
        public void save(FileOutput f)
        {
            base.save(f);

            f.writeFlag(flag1);
            f.writeFlag(flag2);
            f.writeFlag(flag3);
            f.writeFlag(flag4);
            
            f.writeByte(1);
            f.writeInt(verts.Count);
            foreach(Vector2D v in verts)
            {
                f.writeByte(1);
                f.writeFloat(v.x);
                f.writeFloat(v.y);
            }
            
            f.writeByte(1);
            f.writeInt(normals.Count);
            foreach (Vector2D n in normals)
            {
                f.writeByte(1);
                f.writeFloat(n.x);
                f.writeFloat(n.y);
            }
            
            f.writeByte(1);
            f.writeInt(cliffs.Count);
            foreach (CollisionCliff c in cliffs)
            {
                c.save(f);
            }
            
            f.writeByte(1);
            f.writeInt(materials.Count);
            foreach (CollisionMat m in materials)
            {
                f.writeByte(1);
                f.writeBytes(m.material);
            }
        }
    }

    public class Spawn : LVDEntry
    {
        public override string magic { get { return "020401017735BB7500000002"; } }
        
        public float x;
        public float y;
        
        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(1);
            x = f.readFloat();
            y = f.readFloat();
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeByte(1);
            f.writeFloat(x);
            f.writeFloat(y);
        }
    }
    
    public class Bounds : LVDEntry //For Camera Bounds and Blast Zones
    {
        public override string magic { get { return "020401017735BB7500000002"; } }
        
        public float top;
        public float bottom;
        public float left;
        public float right;
        
        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(1);
            left = f.readFloat();
            right = f.readFloat();
            top = f.readFloat();
            bottom = f.readFloat();
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeByte(1);
            f.writeFloat(left);
            f.writeFloat(right);
            f.writeFloat(top);
            f.writeFloat(bottom);
        }
    }

    public class Section
    {
        public List<Vector2D> points = new List<Vector2D>();
        
        public void read(FileData f)
        {
            f.skip(1);
            f.skip(0x16);// unknown data
            
            f.skip(1);
            points = new List<Vector2D>();
            int vertCount = f.readInt();
            for(int j = 0; j < vertCount; j++)
            {
                f.skip(1);
                Vector2D point = new Vector2D();
                point.x = f.readFloat();
                point.y = f.readFloat();
                points.Add(point);
            }
        }
        public void save(FileOutput f)
        {
            f.writeByte(1);
            f.writeHex("03000000040000000000000000000000000000000001");
            
            f.writeByte(1);
            f.writeInt(points.Count);
            foreach(Vector2D v in points)
            {
                f.writeByte(1);
                f.writeFloat(v.x);
                f.writeFloat(v.y);
            }
        }
    }


    public class ItemSpawner : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }
        
        public List<Section> sections = new List<Section>();

        public ItemSpawner()
        {

        }

        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(1);
            f.skip(0x5);// unknown
            
            f.skip(1);
            int sectionCount = f.readInt();
            for(int i = 0; i < sectionCount; i++)
            {
                Section temp = new Section();
                temp.read(f);
                sections.Add(temp);
            }
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeByte(1);
            f.writeHex("0984000101");
            
            f.writeByte(1);
            f.writeInt(sections.Count);
            foreach(Section s in sections)
            {
                s.save(f);
            }
        }
    }

    public class EnemyGenerator : ItemSpawner
    {
        
    }

    public enum shape //I don't think these are accurate
    {
        point = 1,
        rectangle = 3,
        path = 4
    }

    public class LVDGeneralShape : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }
        
        public int type;

        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(1);
            f.readInt(); //unknown
            
            f.skip(1);
            type = f.readInt();
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeByte(1);
            f.writeInt(0);
            
            f.writeByte(1);
            f.writeInt(type);
        }
    }

    public class GeneralPoint : LVDGeneralShape
    {
        public float x, y;

        public GeneralPoint()
        {
            type = 1;
        }

        public void read(FileData f)
        {
            base.read(f);
            
            x = f.readFloat();
            y = f.readFloat();
            f.skip(0x14);
        }
        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeFloat(x);
            f.writeFloat(y);
            f.writeHex("0000000000000000000000000000000000000000");
        }
    }

    public class GeneralRect : LVDGeneralShape
    {
        public float x1, y1, x2, y2;

        public GeneralRect()
        {
            type = 3;
        }

        public void read(FileData f)
        {
            base.read(f);
        
            x1 = f.readFloat();
            y1 = f.readFloat();
            x2 = f.readFloat();
            y2 = f.readFloat();
            f.skip(0x6);
        }

        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeFloat(x1);
            f.writeFloat(y1);
            f.writeFloat(x2);
            f.writeFloat(y2);
            f.writeHex("010100000000");
        }
    }

    public class GeneralPath : LVDGeneralShape
    {
        public List<Vector2D> points = new List<Vector2D>();

        public GeneralPath()
        {
            type = 4;
        }

        public void read(FileData f)
        {
            base.read(f);
            
            f.skip(0x12);
            int pointCount = f.readInt();
            for(int i = 0; i < pointCount; i++)
            {
                f.skip(1);
                points.Add(new Vector2D() { x = f.readFloat(), y = f.readFloat() });
            }
        }

        public void save(FileOutput f)
        {
            base.save(f);
            
            f.writeHex("000000000000000000000000000000000101");
            f.writeInt(points.Count);
            foreach (Vector2D point in points)
            {
                f.writeByte(1);
                f.writeFloat(point.x);
                f.writeFloat(point.y);
            }
        }
    }
    
    public class Sphere : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }
        public float x;
        public float y;
        public float z;
        public float radius;
    }

    public class Capsule : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }
        public float x;
        public float y;
        public float z;
        public float r;
        public float dx;
        public float dy;
        public float dz;
        public float unk;
    }

    public class LVD : FileBase
    { 
        public LVD()
        {
            collisions = new List<Collision>();
            spawns = new List<Spawn>();
            respawns = new List<Spawn>();
            cameraBounds = new List<Bounds>();
            blastzones = new List<Bounds>();
            generalShapes = new List<LVDGeneralShape>();
            generalPoints = new List<GeneralPoint>();
            damageSpheres = new List<Sphere>();
            items = new List<ItemSpawner>();
            damageCapsules = new List<Capsule>();
            enemySpawns = new List<EnemyGenerator>();
        }
        public LVD(string filename) : this()
        {
            Read(filename);
        }
        public List<Collision> collisions { get; set; }
        public List<Spawn> spawns { get; set; }
        public List<Spawn> respawns { get; set; }
        public List<Bounds> cameraBounds { get; set; }
        public List<Bounds> blastzones { get; set; }
        public List<LVDGeneralShape> generalShapes { get; set; }
        public List<GeneralPoint> generalPoints { get; set; }
        public List<Sphere> damageSpheres { get; set; }
        public List<ItemSpawner> items { get; set; }
        public List<Capsule> damageCapsules { get; set; }
        public List<EnemyGenerator> enemySpawns { get; set; }

        public override Endianness Endian { get; set; }

        /*type 1  - collisions
          type 2  - spawns
          type 3  - respawns
          type 4  - camera bounds
          type 5  - death boundaries
          type 6  - ???
          type 7  - ITEMPT_transform
          type 8  - enemyGenerator
          type 9  - ITEMPT
          type 10 - fsAreaCam (and other fsArea's ? )
          type 11 - fsCamLimit
          type 12 - damageShapes (damage sphere and damage capsule are the only ones I've seen, type 2 and 3 respectively)
          type 13 - item spawners
          type 14 - general shapes (general rect, general path, etc.)
          type 15 - general points
          type 16 - ???
          type 17 - FsStartPoint
          type 18 - ???
          type 19 - ???*/

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.skip(0xA);//It's magic
            
            f.skip(1);
            int collisionCount = f.readInt();
            for (int i = 0; i < collisionCount; i++)
            {
                Collision temp = new Collision();
                temp.read(f);
                collisions.Add(temp);
            }
            
            f.skip(1);
            int spawnCount = f.readInt();
            for (int i = 0; i < spawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                spawns.Add(temp);
            }
            
            f.skip(1);
            int respawnCount = f.readInt();
            for (int i = 0; i < respawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                respawns.Add(temp);
            }
            
            f.skip(1);
            int cameraCount = f.readInt();
            for (int i = 0; i < cameraCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                cameraBounds.Add(temp);
            }
            
            f.skip(1);
            int blastzoneCount = f.readInt();
            for (int i = 0; i < blastzoneCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                blastzones.Add(temp);
            }
            
            f.skip(1);
            if (f.readInt() != 0) //6
                return;
                
            f.skip(1);
            if (f.readInt() != 0) //7
                return;
            
            f.skip(1);
            int enemyGeneratorCount = f.readInt();
            if (enemyGeneratorCount != 0) //Are these just item spawners?
                return;
            
            f.skip(1);
            if (f.readInt() != 0) //9
                return;
            
            f.skip(1);
            int fsAreaCamCount = f.readInt();
            if (fsAreaCamCount != 0)
                return;
            
            f.skip(1);
            int fsCamLimitCount = f.readInt();
            if (fsCamLimitCount != 0)
                return;
            
            f.skip(1);
            int damageShapeCount = f.readInt();
            for(int i=0; i < damageShapeCount; i++)
            {
                f.skip(0xD);

                string tempName = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);
                string tempSubname = f.readString(f.pos(), 0x40);
                f.skip(0xA6);
                int shapeType = f.readInt();
                if (shapeType == 2) {
                    Sphere temp = new Sphere();
                    temp.name = tempName;
                    temp.subname = tempSubname;
                    temp.x = f.readFloat();
                    temp.y = f.readFloat();
                    temp.z = f.readFloat();
                    temp.radius = f.readFloat();
                    f.skip(0x11);
                    damageSpheres.Add(temp);
                }
                else if(shapeType == 3)
                {
                    Capsule temp = new Capsule();
                    temp.name = tempName;
                    temp.subname = tempSubname;
                    temp.x = f.readFloat();
                    temp.y = f.readFloat();
                    temp.z = f.readFloat();
                    temp.dx = f.readFloat();
                    temp.dy = f.readFloat();
                    temp.dz = f.readFloat();
                    temp.r = f.readFloat();
                    temp.unk = f.readFloat();
                    f.skip(1);
                    damageCapsules.Add(temp);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            
            f.skip(1);
            int itemCount = f.readInt();
            for(int i = 0; i < itemCount; i++)
            {
                ItemSpawner temp = new ItemSpawner();
                temp.read(f);
                items.Add(temp);
            }
            
            f.skip(1);
            int generalShapeCount = f.readInt();
            for (int i = 0; i < generalShapeCount; i++)
            {
                int tempOff = f.pos();
                LVDGeneralShape p = new LVDGeneralShape();
                f.seek(tempOff);
                if (p.type == 1)
                    p = new GeneralPoint();
                else if (p.type == 3)
                    p = new GeneralRect();
                else if (p.type == 4)
                    p = new GeneralPath();
                else
                    throw new Exception($"Unknown shape type {p.type} at offset {tempOff}");
                p.read(f);
                generalShapes.Add(p);
            }
            
            f.skip(1);
            int generalPointCount = f.readInt();
            for(int i = 0; i < generalPointCount; i++)
            {
                GeneralPoint temp = new GeneralPoint();
                temp.read(f);
                generalPoints.Add(temp);
            }

            f.skip(1);
            if (f.readInt() != 0) //16
                return; //no clue how to be consistent in reading these so...
                
            f.skip(1);
            if (f.readInt() != 0) //17
                return; //no clue how to be consistent in reading these so...
                
            f.skip(1);
            if (f.readInt() != 0) //18
                return; //no clue how to be consistent in reading these so...
            
            f.skip(1);
            if (f.readInt() != 0) //19
                return; //no clue how to be consistent in reading these so...

            //LVD doesn't end here and neither does my confusion, will update this part later
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeHex("000000010A014C564431");
            
            f.writeByte(1);
            f.writeInt(collisions.Count);
            foreach (Collision c in collisions)
                c.save(f);
            
            f.writeByte(1);
            f.writeInt(spawns.Count);
            foreach (Spawn s in spawns)
                s.save(f);
            
            f.writeByte(1);
            f.writeInt(spawns.Count);
            foreach (Spawn s in respawns)
                s.save(f);
            
            f.writeByte(1);
            f.writeInt(cameraBounds.Count);
            foreach (Bounds b in cameraBounds)
                b.save(f);
            
            f.writeByte(1);
            f.writeInt(blastzones.Count);
            foreach (Bounds b in blastzones)
                b.save(f);

            for (int i = 0; i < 7; i++)
            {
                f.writeByte(1);
                f.writeInt(0);
            }

            f.writeByte(1);
            f.writeInt(items.Count);
            foreach (ItemSpawner item in items)
                item.save(f);

            f.writeByte(1);
            f.writeInt(generalShapes.Count);
            foreach (LVDGeneralShape shape in generalShapes)
                shape.save(f);
            

            f.writeByte(1);
            f.writeInt(generalPoints.Count);
            foreach (GeneralPoint p in generalPoints)
                p.save(f);

            for (int i = 0; i < 4; i++)
            {
                f.writeByte(1);
                f.writeInt(0);
            }

            return f.getBytes();
        }

        #region rendering

        public static void DrawSpawn(Spawn s, bool isRespawn)
        {
            DrawRespawnQuad(s, Color.Blue);

            //Draw respawn platform
            if (isRespawn)
            {
                DrawRespawnArrow(s, Color.Gray, Color.Black);
            }

        }

        private static void DrawRespawnQuad(Spawn s, Color color)
        {
            float width = 3.0f;
            float height = 10.0f;
            GL.Color4(Color.FromArgb(100, color));
            GL.Begin(PrimitiveType.QuadStrip);
            GL.Vertex3(s.x - width, s.y, 0f);
            GL.Vertex3(s.x + width, s.y, 0f);
            GL.Vertex3(s.x - width, s.y + height, 0f);
            GL.Vertex3(s.x + width, s.y + height, 0f);
            GL.End();
        }

        private static void DrawRespawnArrow(Spawn s, Color arrowColor, Color wireframeColor)
        {
            float scale = 5.0f;
            GL.Color4(Color.FromArgb(200, arrowColor));
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(s.x - scale, s.y, 0);
            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y, scale);

            GL.Vertex3(s.x - scale, s.y, 0);
            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y, -scale);

            GL.Vertex3(s.x - scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x, s.y, scale);

            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x, s.y, -scale);

            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x, s.y, scale);

            GL.Vertex3(s.x - scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x, s.y, -scale);
            GL.End();

            // wireframe
            GL.Color4(Color.FromArgb(200, wireframeColor));
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(s.x - scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y - scale, 0);

            GL.Vertex3(s.x, s.y, -scale);
            GL.Vertex3(s.x, s.y - scale, 0);
            GL.Vertex3(s.x, s.y, scale);
            GL.Vertex3(s.x, s.y - scale, 0);

            GL.Vertex3(s.x, s.y, -scale);
            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y, -scale);
            GL.Vertex3(s.x - scale, s.y, 0);

            GL.Vertex3(s.x, s.y, scale);
            GL.Vertex3(s.x + scale, s.y, 0);
            GL.Vertex3(s.x, s.y, scale);
            GL.Vertex3(s.x - scale, s.y, 0);

            GL.End();
        }

        public static void DrawCameraBounds(Bounds b, Color color)
        {
            GL.Color4(Color.FromArgb(128, color));
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(b.left, b.top, 0);
            GL.Vertex3(b.right, b.top, 0);
            GL.Vertex3(b.right, b.bottom, 0);
            GL.Vertex3(b.left, b.bottom, 0);
            GL.End();
        }

        public static void RenderItemSpawners()
        {
            foreach (ItemSpawner c in Runtime.TargetLVD.items)
            {
                foreach (Section s in c.sections)
                {
                    // draw the item spawn quads
                    GL.LineWidth(2);

                    // draw outside borders
                    GL.Color3(Color.Black);
                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (Vector2D vi in s.points)
                    {
                        GL.Vertex3(vi.x, vi.y, 5);
                    }
                    GL.End();
                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (Vector2D vi in s.points)
                    {
                        GL.Vertex3(vi.x, vi.y, -5);
                    }
                    GL.End();


                    // draw vertices
                    GL.Color3(Color.White);
                    GL.Begin(PrimitiveType.Lines);
                    foreach (Vector2D vi in s.points)
                    {
                        GL.Vertex3(vi.x, vi.y, 5);
                        GL.Vertex3(vi.x, vi.y, -5);
                    }
                    GL.End();
                }
            }
        }

        public static void DrawCollisions(Stopwatch timeSinceSelected)
        {
            Color color;
            GL.LineWidth(4);
            Matrix4 transform = Matrix4.Identity;
            foreach (Collision c in Runtime.TargetLVD.collisions)
            {
                bool colSelected = (Runtime.LVDSelection == c);
                float addX = 0, addY = 0, addZ = 0;
                if (c.useStartPos)
                {
                    addX = c.startPos[0];
                    addY = c.startPos[1];
                    addZ = c.startPos[2];
                }
                if (c.flag2)
                {
                    //Flag2 == rigged collision
                    ModelContainer riggedModel = null;
                    Bone riggedBone = null;
                    foreach (ModelContainer m in Runtime.ModelContainers)
                    {
                        if (m.name.Equals(c.subname))
                        {
                            riggedModel = m;
                            if (m.vbn != null)
                            {
                                foreach (Bone b in m.vbn.bones)
                                {
                                    if (b.Equals(c.boneName))
                                    {
                                        riggedBone = b;
                                    }
                                }
                            }
                        }
                    }
                    if (riggedModel != null)
                    {
                        if (riggedBone == null && riggedModel.vbn != null && riggedModel.vbn.bones.Count > 0)
                        {
                            riggedBone = riggedModel.vbn.bones[0];
                        }
                        if (riggedBone != null)
                            transform = riggedBone.invert * riggedBone.transform;
                    }
                }

                for (int i = 0; i < c.verts.Count - 1; i++)
                {
                    Vector3 v1Pos = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ + 5), transform);
                    Vector3 v1Neg = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ - 5), transform);
                    Vector3 v1Zero = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ), transform);
                    Vector3 v2Pos = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ + 5), transform);
                    Vector3 v2Neg = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ - 5), transform);
                    Vector3 v2Zero = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ), transform);

                    GL.Begin(PrimitiveType.Quads);
                    if (c.normals.Count > i)
                    {
                        if (Runtime.renderCollisionNormals)
                        {
                            Vector3 v = Vector3.Add(Vector3.Divide(Vector3.Subtract(v1Zero, v2Zero), 2), v2Zero);
                            GL.End();
                            GL.Begin(PrimitiveType.Lines);
                            GL.Color3(Color.Blue);
                            GL.Vertex3(v);
                            GL.Vertex3(v.X + (c.normals[i].x * 5), v.Y + (c.normals[i].y * 5), v.Z);
                            GL.End();
                            GL.Begin(PrimitiveType.Quads);
                        }

                        if (c.flag4)
                            color = Color.FromArgb(128, Color.Yellow);
                        else if (c.materials[i].getFlag(4) && Math.Abs(c.normals[i].x) > Math.Abs(c.normals[i].y))
                            color = Color.FromArgb(128, Color.Purple);
                        else if (Math.Abs(c.normals[i].x) > Math.Abs(c.normals[i].y))
                            color = Color.FromArgb(128, Color.Lime);
                        else if (c.normals[i].y < 0)
                            color = Color.FromArgb(128, Color.Red);
                        else
                            color = Color.FromArgb(128, Color.Cyan);

                        if ((colSelected || Runtime.LVDSelection == c.normals[i]) && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                            color = RenderTools.invertColor(color);

                        GL.Color4(color);
                    }
                    else
                    {
                        GL.Color4(Color.FromArgb(128, Color.Gray));
                    }
                    GL.Vertex3(v1Pos);
                    GL.Vertex3(v1Neg);
                    GL.Vertex3(v2Neg);
                    GL.Vertex3(v2Pos);
                    GL.End();

                    GL.Begin(PrimitiveType.Lines);
                    if (c.materials.Count > i)
                    {
                        if (c.materials[i].getFlag(6) || (i > 0 && c.materials[i - 1].getFlag(7)))
                            color = Color.Purple;
                        else
                            color = Color.Orange;

                        if ((colSelected || Runtime.LVDSelection == c.verts[i]) && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                            color = RenderTools.invertColor(color);
                        GL.Color4(color);
                    }
                    else
                    {
                        GL.Color4(Color.Gray);
                    }
                    GL.Vertex3(v1Pos);
                    GL.Vertex3(v1Neg);

                    if (i == c.verts.Count - 2)
                    {
                        if (c.materials.Count > i)
                        {
                            if (c.materials[i].getFlag(7))
                                color = Color.Purple;
                            else
                                color = Color.Orange;

                            if (Runtime.LVDSelection == c.verts[i + 1] && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                                color = RenderTools.invertColor(color);
                            GL.Color4(color);
                        }
                        else
                        {
                            GL.Color4(Color.Gray);
                        }
                        GL.Vertex3(v2Pos);
                        GL.Vertex3(v2Neg);
                    }
                    GL.End();
                }
            }
        }

        private static Color getLinkColor(DAT.COLL_DATA.Link link)
        {
            if ((link.flags & 1) != 0)
                return Color.FromArgb(128, Color.Yellow);
            if ((link.collisionAngle & 4) + (link.collisionAngle & 8) != 0)
                return Color.FromArgb(128, Color.Lime);
            if ((link.collisionAngle & 2) != 0)
                return Color.FromArgb(128, Color.Red);

            return Color.FromArgb(128, Color.DarkCyan);
        }

        public static void DrawDATCollisions(ModelContainer m)
        {
            float scale = m.dat_melee.stageScale;
            List<int> ledges = new List<int>();
            foreach (DAT.COLL_DATA.Link link in m.dat_melee.collisions.links)
            {

                GL.Begin(PrimitiveType.Quads);
                GL.Color4(getLinkColor(link));
                Vector2D vi = m.dat_melee.collisions.vertices[link.vertexIndices[0]];
                GL.Vertex3(vi.x * scale, vi.y * scale, 5);
                GL.Vertex3(vi.x * scale, vi.y * scale, -5);
                vi = m.dat_melee.collisions.vertices[link.vertexIndices[1]];
                GL.Vertex3(vi.x * scale, vi.y * scale, -5);
                GL.Vertex3(vi.x * scale, vi.y * scale, 5);
                GL.End();

                if ((link.flags & 2) != 0)
                {
                    ledges.Add(link.vertexIndices[0]);
                    ledges.Add(link.vertexIndices[1]);
                }
            }

            GL.LineWidth(4);
            for (int i = 0; i < m.dat_melee.collisions.vertices.Count; i++)
            {
                Vector2D vi = m.dat_melee.collisions.vertices[i];
                if (ledges.Contains(i))
                    GL.Color3(Color.Purple);
                else
                    GL.Color3(Color.Tomato);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(vi.x * scale, vi.y * scale, 5);
                GL.Vertex3(vi.x * scale, vi.y * scale, -5);
                GL.End();
            }
        }

        public static void DrawBlastZones(Bounds b, Color color)
        {
            GL.Color4(Color.FromArgb(128, color));
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(b.left, b.top, 0);
            GL.Vertex3(b.right, b.top, 0);
            GL.Vertex3(b.right, b.bottom, 0);
            GL.Vertex3(b.left, b.bottom, 0);
            GL.End();
        }

    }

    #endregion

}



