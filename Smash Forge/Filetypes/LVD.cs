using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Smash_Forge
{
    public class LVDEntry
    {
        public string name;
        public string subname;
    }

    public class Vector2D
    {
        public float x;
        public float y;
    }

    public class Sphere : LVDEntry
    {
        public float x;
        public float y;
        public float z;
        public float radius;
    }

    public class Capsule : LVDEntry
    {
        public float x;
        public float y;
        public float z;
        public float r;
        public float dx;
        public float dy;
        public float dz;
        public float unk;
    }

    public class Point : LVDEntry
    {
        public float x;
        public float y;
    }

    public class Bounds : LVDEntry //Either Camera bounds or Blast zones
    {
        public float top;
        public float bottom;
        public float left;
        public float right;
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

    public class Section
    {
        public List<Vector2D> points = new List<Vector2D>();
    }


    public class ItemSpawner : LVDEntry
    {
        public List<Section> sections = new List<Section>();

        public ItemSpawner()
        {

        }

        public void read(FileData f)
        {
            f.skip(0xD);
            name = f.readString(f.pos(), 0x38);
            f.skip(0x38);
            f.skip(1);//Seperation char
            subname = f.readString(f.pos(), 0x40);
            f.skip(0xAC);
            int sectionCount = f.readInt();
            for(int i = 0; i < sectionCount; i++)
            {
                f.skip(0x18);// unknown data
                Section temp = new Section();
                temp.points = new List<Vector2D>();
                int vertCount = f.readInt();
                for(int j = 0; j < vertCount; j++)
                {
                    f.skip(1);//Seperation char
                    Vector2D point = new Vector2D();
                    point.x = f.readFloat();
                    point.y = f.readFloat();
                    temp.points.Add(point);
                }
                sections.Add(temp);
            }
        }

        public void save(FileOutput f)
        {
            f.writeHex("010401017735BB750000000201");
            f.writeChars(name.PadRight(0x38,(char)0).ToCharArray());
            f.writeByte(1);
            f.writeChars(subname.PadRight(0x40, (char)0).ToCharArray());
            f.writeHex("0100000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001098400010101");
            f.writeInt(sections.Count);
            foreach(Section s in sections)
            {
                f.writeHex("010300000004000000000000000000000000000000000101");
                f.writeInt(s.points.Count);
                foreach(Vector2D p in s.points)
                {
                    f.writeByte(1);
                    f.writeFloat(p.x);
                    f.writeFloat(p.y);
                }
            }
        }
    }

    public class EnemyGenerator : LVDEntry
    {
        public List<Section> sections = new List<Section>();

        public EnemyGenerator()
        {

        }

        public void read(FileData f)
        {
            f.skip(0xD);
            name = f.readString(f.pos(), 0x38);
            f.skip(0x38);
            f.skip(1);//Seperation char
            subname = f.readString(f.pos(), 0x40);
            f.skip(0xAC);
            int sectionCount = f.readInt();
            for (int i = 0; i < sectionCount; i++)
            {
                f.skip(0x18);// unknown data
                Section temp = new Section();
                temp.points = new List<Vector2D>();
                int vertCount = f.readInt();
                for (int j = 0; j < vertCount; j++)
                {
                    f.skip(1);//Seperation char
                    Vector2D point = new Vector2D();
                    point.x = f.readFloat();
                    point.y = f.readFloat();
                    temp.points.Add(point);
                }
                sections.Add(temp);
            }
        }
    }

    public class Collision : LVDEntry
    {
        public List<Vector2D> verts = new List<Vector2D>();
        public List<Vector2D> normals = new List<Vector2D>();
        public List<CollisionMat> materials = new List<CollisionMat>();
        public float[] startPos = new float[3];
        public bool useStartPos = false;
        public int unk2 = 0;
        public byte[] unk3 = new byte[0xC];
        public char[] unk4 = new char[0x40];
        public bool flag1 = false, flag2 = false, flag3 = false, flag4 = false;

        public Collision()
        {

        }

        public void read(FileData f)
        {
            f.skip(0xD);
            name = f.readString(f.pos(), 0x38);
            f.skip(0x38);
            f.skip(1);//Seperation char
            subname = f.readString(f.pos(), 0x40);
            f.skip(0x40);
            f.skip(1);//Seperation char
            startPos[0] = f.readFloat();
            startPos[1] = f.readFloat();
            startPos[2] = f.readFloat();
            useStartPos = (f.readByte() != 0);
            f.skip(1);//Seperation char
            unk2 = f.readInt();
            f.skip(1);
            unk3 = f.read(0xC);
            f.skip(4);//FF FF FF FF
            f.skip(1);//Seperation char
            unk4 = new char[0x40];
            for (int i = 0; i < 0x40; i++)
                unk4[i] = (char)f.readByte();
            
            flag1 = Convert.ToBoolean(f.readByte());
            flag2 = Convert.ToBoolean(f.readByte());
            flag3 = Convert.ToBoolean(f.readByte());
            flag4 = Convert.ToBoolean(f.readByte());
            f.skip(1);//Seperation char
            //f.skip(0xAA);
            //Console.WriteLine(f.pos());
            int vertCount = f.readInt();
            for(int i = 0; i < vertCount; i++)
            {
                f.skip(1);//Seperation char
                Vector2D temp = new Vector2D();
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                verts.Add(temp);
            }
            f.skip(1);//Seperation char

            int normalCount = f.readInt();
            for(int i = 0; i < normalCount; i++)
            {
                f.skip(1);//Seperation char
                Vector2D temp = new Vector2D();
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                normals.Add(temp);
            }
            f.skip(1);//Seperation char

            int cliffCount = f.readInt();//CLIFFS tend to be useless
            f.skip(0xFC * cliffCount);//Standard CLIFFS are 0xFC in length, just skip em all
            f.skip(1);//Seperation char

            int materialCount = f.readInt();
            for(int i = 0; i < materialCount; i++)
            {
                f.skip(1);//Seperation char
                CollisionMat temp = new CollisionMat();
                temp.material = f.read(0xC);//Temporary, will work on fleshing out material more later
                
                materials.Add(temp);
            }

        }

        public void save(FileOutput f)
        {
            f.writeHex("030401017735BB750000000201");
            f.writeString(name.PadRight(0x38, (char)0));
            f.writeByte(1);
            f.writeString(subname.PadRight(0x40, (char)0));
            f.writeByte(1);
            foreach (float i in startPos)
                f.writeFloat(i);
            f.writeFlag(useStartPos);
            f.writeByte(1);
            f.writeInt(unk2);
            f.writeByte(1);
            f.writeBytes(unk3);
            f.writeHex("FFFFFFFF01");
            f.writeChars(unk4);
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
            f.writeInt(0);
            f.writeByte(1);
            f.writeInt(materials.Count);
            foreach (CollisionMat m in materials)
            {
                f.writeByte(1);
                f.writeBytes(m.material);
            }
        }
    }

    public enum shape
    {
        point = 1,
        rectangle = 3,
        path = 4
    }

    public abstract class LVDGeneralShape : LVDEntry
    {
        public int type;

        public abstract void Read(FileData f);

        public abstract void save(FileOutput f);
    }

    public class GeneralPoint : LVDGeneralShape
    {
        public float x, y;

        public GeneralPoint()
        {
            type = 1;
        }

        public override void Read(FileData f)
        {
            x = f.readFloat();
            y = f.readFloat();
            f.skip(0xE);
        }

        public override void save(FileOutput f)
        {
            f.writeHex("010401017735BB750000000201");
            f.writeChars(name.PadRight(0x38).ToCharArray());
            f.writeByte(1);
            f.writeChars(subname.PadRight(0x40).ToCharArray());
            f.writeHex("0100000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001014C800203");
            f.writeInt(type);
            f.writeFloat(x);
            f.writeFloat(y);
            f.writeHex("0000000000000000010100000000");
        }
    }

    public class GeneralRect : LVDGeneralShape
    {
        public float x1, y1, x2, y2;

        public GeneralRect()
        {
            type = 3;
        }

        public override void Read(FileData f)
        {
            x1 = f.readFloat();
            y1 = f.readFloat();
            x2 = f.readFloat();
            y2 = f.readFloat();
            f.skip(0x6);
        }

        public override void save(FileOutput f)
        {
            f.writeHex("010401017735BB750000000201");
            f.writeChars(name.PadRight(0x38).ToCharArray());
            f.writeByte(1);
            f.writeChars(subname.PadRight(0x40).ToCharArray());
            f.writeHex("0100000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001014C800203");
            f.writeInt(type);
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

        public override void Read(FileData f)
        {
            f.skip(0x12);
            int pointCount = f.readInt();
            for(int i = 0; i < pointCount; i++)
            {
                f.skip(1);//seperator char
                points.Add(new Vector2D() { x = f.readFloat(), y = f.readFloat() });
            }
        }

        public override void save(FileOutput f)
        {
            f.writeHex("010401017735BB750000000201");
            f.writeChars(name.PadRight(0x38).ToCharArray());
            f.writeByte(1);
            f.writeChars(subname.PadRight(0x40).ToCharArray());
            f.writeHex("0100000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001014C800203");
            f.writeInt(type);
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

    public class LVD : FileBase
    { 
        public LVD()
        {
            collisions = new List<Collision>();
            spawns = new List<Point>();
            respawns = new List<Point>();
            cameraBounds = new List<Bounds>();
            blastzones = new List<Bounds>();
            generalPoints = new List<Point>();
            damageSpheres = new List<Sphere>();
            items = new List<ItemSpawner>();
            damageCapsules = new List<Capsule>();
            enemySpawns = new List<EnemyGenerator>();
            generalShapes = new List<LVDGeneralShape>();
        }
        public LVD(string filename) : this()
        {
            Read(filename);
        }
        public List<Collision> collisions { get; set; }
        public List<Point> spawns { get; set; }
        public List<Point> respawns { get; set; }
        public List<Bounds> cameraBounds { get; set; }
        public List<Bounds> blastzones { get; set; }
        public List<LVDGeneralShape> generalShapes { get; set; }
        public List<Point> generalPoints { get; set; }
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
            f.seek(0xB);//It's magic
            int collisionCount = f.readInt();
            for (int i = 0; i < collisionCount; i++)
            {
                Collision temp = new Collision();
                temp.read(f);
                collisions.Add(temp);
            }
            f.skip(1); //Seperation char

            int spawnCount = f.readInt();
            for (int i = 0; i < spawnCount; i++)
            {
                Point temp = new Point();
                f.skip(0xD);
                temp.name = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                temp.subname = f.readString(f.pos(), 0x40);
                f.skip(0xA6);
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                spawns.Add(temp);
            }
            f.skip(1);//Seperation char

            int respawnCount = f.readInt();
            for (int i = 0; i < respawnCount; i++)
            {
                Point temp = new Point();
                f.skip(0xD);
                temp.name = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                temp.subname = f.readString(f.pos(), 0x40);
                f.skip(0xA6);
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                respawns.Add(temp);
            }
            f.skip(1);//Seperation char

            int cameraCount = f.readInt();
            for (int i = 0; i < cameraCount; i++)
            {
                Bounds temp = new Bounds();
                f.skip(0xD);
                temp.name = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                temp.subname = f.readString(f.pos(), 0x40);
                f.skip(0xA6);
                temp.left = f.readFloat();
                temp.right = f.readFloat();
                temp.top = f.readFloat();
                temp.bottom = f.readFloat();
                cameraBounds.Add(temp);
            }
            f.skip(1);//Seperation char

            int blastzoneCount = f.readInt();
            for (int i = 0; i < blastzoneCount; i++)
            {
                Bounds temp = new Bounds();
                f.skip(0xD);
                temp.name = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                temp.subname = f.readString(f.pos(), 0x40);
                f.skip(0xA6);
                temp.left = f.readFloat();
                temp.right = f.readFloat();
                temp.top = f.readFloat();
                temp.bottom = f.readFloat();
                blastzones.Add(temp);
            }
            f.skip(1);//Seperation char

            if (f.readInt() != 0) //1
                return;
            f.skip(1);//Seperation char

            if (f.readInt() != 0)//2
                return;
            f.skip(1);//Seperation char

            int enemyGeneratorCount = f.readInt();
            if (enemyGeneratorCount != 0)
                return;
            f.skip(1);//Seperation char

            if (f.readInt() != 0)//4
                return;
            f.skip(1);//Seperation char

            int fsAreaCamCount = f.readInt();
            if (fsAreaCamCount != 0)
                return;
            f.skip(1);//Seperation char

            int fsCamLimitCount = f.readInt();
            if (fsCamLimitCount != 0)
                return;
            f.skip(1);//Seperation char

            int damageShapeCount = f.readInt();
            for(int i=0; i < damageShapeCount; i++)
            {
                f.skip(0xD);

                string tempName = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
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
            f.skip(1);//Seperation char

            int itemCount = f.readInt();
            for(int i = 0; i < itemCount; i++)
            {
                ItemSpawner temp = new ItemSpawner();
                temp.read(f);
                items.Add(temp);
            }
            f.skip(1);//Seperation char

            int generalShapeCount = f.readInt();
            for (int i = 0; i < generalShapeCount; i++)
            {
                f.skip(0xD);

                string tempName = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                string tempSubname = f.readString(f.pos(), 0x40);
                f.skip(0xAB);
                int shapeType = f.readInt();
                LVDGeneralShape p;
                if (shapeType == 1)
                    p = new GeneralPoint();
                else if (shapeType == 3)
                    p = new GeneralRect();
                else if (shapeType == 4)
                    p = new GeneralPath();
                else
                    throw new Exception($"Unknown shape type {shapeType} at offset {f.pos() - 4}");
                p.name = tempName;
                p.subname = tempSubname;
                p.Read(f);
                generalShapes.Add(p);
            }
            f.skip(1);

            int generalPointCount = f.readInt();
            for(int i = 0; i < generalPointCount; i++)
            {
                Point temp = new Point();
                f.skip(0xD);
                temp.name = f.readString(f.pos(), 0x38);
                f.skip(0x38);
                f.skip(1);//Seperation char
                temp.subname = f.readString(f.pos(), 0x40);
                f.skip(0xAF);
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                f.skip(0x14);
                generalPoints.Add(temp);
            }

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            if (f.readInt() != 0)//8
                return; //no clue how to be consistent in reading these so...
            f.skip(1);

            //LVD doesn't end here and neither does my confusion, will update this part later
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeHex("000000010A014C56443101");
            f.writeInt(collisions.Count);
            foreach (Collision c in collisions)
            {
                c.save(f);
            }
            f.writeByte(1);
            f.writeInt(spawns.Count);
            foreach (Point p in spawns)
            {
                f.writeHex("020401017735BB750000000201");
                f.writeString(p.name.PadRight(0x38, (char)0));
                f.writeByte(1);
                f.writeString(p.subname.PadRight(0x40, (char)0));
                f.writeByte(1);
                f.writeHex("00000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
                f.writeFloat(p.x);
                f.writeFloat(p.y);
            }
            f.writeByte(1);
            f.writeInt(respawns.Count);
            foreach (Point p in respawns)
            {
                f.writeHex("020401017735BB750000000201");
                f.writeString(p.name.PadRight(0x38, (char)0));
                f.writeByte(1);
                f.writeString(p.subname.PadRight(0x40, (char)0));
                f.writeByte(1);
                f.writeHex("00000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
                f.writeFloat(p.x);
                f.writeFloat(p.y);
            }
            f.writeByte(1);
            f.writeInt(cameraBounds.Count);
            foreach (Bounds b in cameraBounds)
            {
                f.writeHex("020401017735BB750000000201");
                f.writeString(b.name.PadRight(0x38, (char)0));
                f.writeByte(1);
                f.writeString(b.subname.PadRight(0x40, (char)0));
                f.writeByte(1);
                f.writeHex("00000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
                f.writeFloat(b.left);
                f.writeFloat(b.right);
                f.writeFloat(b.top);
                f.writeFloat(b.bottom);
            }
            f.writeByte(1);
            f.writeInt(blastzones.Count);
            foreach (Bounds b in blastzones)
            {
                f.writeHex("020401017735BB750000000201");
                f.writeString(b.name.PadRight(0x38, (char)0));
                f.writeByte(1);
                f.writeString(b.subname.PadRight(0x40, (char)0));
                f.writeByte(1);
                f.writeHex("00000000000000000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001");
                f.writeFloat(b.left);
                f.writeFloat(b.right);
                f.writeFloat(b.top);
                f.writeFloat(b.bottom);
            }

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
            foreach (Point p in generalPoints)
            {
                f.writeHex("010401017735BB750000000201");
                f.writeChars(p.name.PadRight(0x38,(char)0).ToCharArray());
                f.writeByte(1);
                f.writeChars(p.subname.PadRight(0x40, (char)0).ToCharArray());
                f.writeByte(1);
                f.writeHex("00000000432100000000000000010000000001000000000000000000000000FFFFFFFF010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001000000000100000004");
                f.writeFloat(p.x);
                f.writeFloat(p.y);
                f.writeBytes(new byte[0x14]);
            }

            for (int i = 0; i < 4; i++)
            {
                f.writeByte(1);
                f.writeInt(0);
            }

            return f.getBytes();
        }
    }
}
/*        type 1  - collisions
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
