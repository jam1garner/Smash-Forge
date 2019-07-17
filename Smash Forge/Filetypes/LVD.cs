using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SmashForge
{
    public abstract class LvdEntry
    {
        public abstract string Magic { get; }
        private string name = new string(new char[0x38]);
        private string subname = new string(new char[0x40]);
        public Vector3 startPos = new Vector3();
        public bool useStartPos = false;
        public int unk1 = 0;
        public float[] unk2 = new float[3];
        public int unk3 = -1;
        private string boneName = new string(new char[0x40]);

        private static string GetString(string baseStr, int maxLength)
        {
            int length = baseStr.IndexOf((char)0);
            if (length == -1)
                length = maxLength;
            return baseStr.Substring(0, length);
        }
        private static string SetString(string baseStr, int maxLength)
        {
            return baseStr.PadRight(maxLength, (char)0).Substring(0, maxLength);
        }

        public string Name
        {
            get { return GetString(name, 0x38); }
            set { name = SetString(value, 0x38); }
        }
        public string Subname
        {
            get { return GetString(subname, 0x40); }
            set { subname = SetString(value, 0x40); }
        }
        public string BoneName
        {
            get { return GetString(boneName, 0x40); }
            set { boneName = SetString(value, 0x40); }
        }

        public void Read(FileData f)
        {
            f.Skip(0xC);

            f.Skip(1);
            name = f.ReadString(f.Pos(), 0x38);
            f.Skip(0x38);

            f.Skip(1);
            subname = f.ReadString(f.Pos(), 0x40);
            f.Skip(0x40);

            f.Skip(1);
            for (int i = 0; i < 3; i++)
                startPos[i] = f.ReadFloat();
            useStartPos = Convert.ToBoolean(f.ReadByte());

            //Some kind of count? Only seen it as 0 so I don't know what it's for
            f.Skip(1);
            unk1 = f.ReadInt();

            //Not sure what this is for, but it seems like it could be a vector followed by an index
            f.Skip(1);
            for (int i = 0; i < 3; i++)
                unk2[i] = f.ReadFloat();
            unk3 = f.ReadInt();

            f.Skip(1);
            boneName = f.ReadString(f.Pos(), 0x40);
            f.Skip(0x40);
        }
        public void Save(FileOutput f)
        {
            f.WriteHex(Magic);

            f.WriteByte(1);
            f.WriteString(name);

            f.WriteByte(1);
            f.WriteString(subname);

            f.WriteByte(1);
            for (int i = 0; i < 3; i++)
                f.WriteFloat(startPos[i]);
            f.WriteFlag(useStartPos);

            f.WriteByte(1);
            f.WriteInt(unk1);

            f.WriteByte(1);
            foreach (float i in unk2)
                f.WriteFloat(i);
            f.WriteInt(unk3);

            f.WriteByte(1);
            f.WriteString(boneName);
        }
    }

    public class Point : LvdEntry
    {
        public override string Magic => "";
        public float x;
        public float y;
    }

    public enum CollisionMatType : byte
    {
        Basic = 0x00,            //
        Rock = 0x01,             //
        Grass = 0x02,            //Increased traction (1.5)
        Soil = 0x03,             //
        Wood = 0x04,             //
        LightMetal = 0x05,       //"Iron" internally.
        HeavyMetal = 0x06,       //"NibuIron" (Iron2) internally.
        Carpet = 0x07,           //Used for Delfino Plaza roof things
        Alien = 0x08,            //"NumeNume" (squelch sound) internally. Used on Brinstar
        MasterFortress = 0x09,   //"Creature" internally.
        WaterShallow = 0x0a,     //"Asase" (shallows) internally. Used for Delfino Plaza shallow water
        Soft = 0x0b,             //Used on Woolly World
        TuruTuru = 0x0c,         //Reduced traction (0.1). Unknown meaning and use
        Snow = 0x0d,             //
        Ice = 0x0e,              //Reduced traction (0.2). Used on P. Stadium 2 ice form
        GameWatch = 0x0f,        //Used on Flat Zone
        Oil = 0x10,              //Reduced traction (0.1). Used for Flat Zone oil spill (presumably; not found in any collisions)
        Cardboard = 0x11,        //"Danbouru" (corrugated cardboard) internally. Used on Paper Mario
        SpikesTargetTest = 0x12, //Unknown. From Brawl, and appears to still be hazard-related but is probably not functional
        Hazard2SSEOnly = 0x13,   //See above
        Hazard3SSEOnly = 0x14,   //See above
        Electroplankton = 0x15,  //"ElectroP" internally. Not known to be used anywhere in this game
        Cloud = 0x16,            //Used on Skyworld, Magicant
        Subspace = 0x17,         //"Akuukan" (subspace) internally. Not known to be used anywhere in this game
        Brick = 0x18,            //Used on Skyworld, Gerudo V., Smash Run
        Unknown1 = 0x19,         //Unknown. From Brawl
        NES8Bit = 0x1a,          //"Famicom" internally. Not known to be used anywhere in this game
        Grate = 0x1b,            //Used on Delfino and P. Stadium 2
        Sand = 0x1c,             //
        Homerun = 0x1d,          //From Brawl, may not be functional
        WaterNoSplash = 0x1e,    //From Brawl, may not be functional
        Hurt = 0x1f,             //Takes hitbox data from stdat. Used for Cave and M. Fortress Danger Zones
        Unknown2 = 0x20          //Unknown. Uses bomb SFX?
    }

    public class CollisionMat
    {
        public byte[] material = new byte[0xC];

        public byte physics
        {
            get { return material[3]; }
            set { material[3] = value; }
        }
        public bool leftLedge
        {
            get { return GetFlag(6); }
            set { setFlag(6, value); }
        }
        public bool rightLedge
        {
            get { return GetFlag(7); }
            set { setFlag(7, value); }
        }
        public bool noWallJump
        {
            get { return GetFlag(4); }
            set { setFlag(4, value); }
        }

        public bool GetFlag(int n)
        {
            return ((material[10] & (1 << n)) != 0);
        }
        public void setFlag(int flag, bool value)
        {
            //Console.WriteLine("B - " + getFlag(flag));
            byte mask = (byte)(1 << flag);
            bool isSet = (material[10] & mask) != 0;
            if (value)
                material[10] |= mask;
            else
                material[10] &= (byte)~mask;
            //Console.WriteLine("A - " + getFlag(flag));
        }
    }

    public class CollisionCliff : LvdEntry
    {
        public override string Magic { get { return "030401017735BB7500000002"; } }

        public Vector2 pos;
        public float angle; //I don't know what this does exactly, but it's -1 for left and 1 for right
        public int lineIndex;

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            pos = new Vector2();
            pos.X = f.ReadFloat();
            pos.Y = f.ReadFloat();
            angle = f.ReadFloat();
            lineIndex = f.ReadInt();
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteFloat(pos.X);
            f.WriteFloat(pos.Y);
            f.WriteFloat(angle);
            f.WriteInt(lineIndex);
        }
    }

    public class Collision : LvdEntry
    {
        public override string Magic { get { return "030401017735BB7500000002"; } }

        public List<Vector2> verts = new List<Vector2>();
        public List<Vector2> normals = new List<Vector2>();
        public List<CollisionCliff> cliffs = new List<CollisionCliff>();
        public List<CollisionMat> materials = new List<CollisionMat>();
        //Flags: ???, rig collision, ???, drop-through
        public bool flag1 = false, flag2 = false, flag3 = false, flag4 = false;

        public bool IsPolygon
        {
            get
            {
                if (verts.Count < 2) return false;
                return verts[0].Equals(verts[verts.Count - 1]);
            }
        }

        public Collision() { }

        public new void read(FileData f)
        {
            base.Read(f);

            flag1 = Convert.ToBoolean(f.ReadByte());
            flag2 = Convert.ToBoolean(f.ReadByte());
            flag3 = Convert.ToBoolean(f.ReadByte());
            flag4 = Convert.ToBoolean(f.ReadByte());

            f.Skip(1);
            int vertCount = f.ReadInt();
            for (int i = 0; i < vertCount; i++)
            {
                f.Skip(1);
                Vector2 temp = new Vector2();
                temp.X = f.ReadFloat();
                temp.Y = f.ReadFloat();
                verts.Add(temp);
            }

            f.Skip(1);
            int normalCount = f.ReadInt();
            for (int i = 0; i < normalCount; i++)
            {
                f.Skip(1);
                Vector2 temp = new Vector2();
                temp.X = f.ReadFloat();
                temp.Y = f.ReadFloat();
                normals.Add(temp);
            }

            f.Skip(1);
            int cliffCount = f.ReadInt();
            for (int i = 0; i < cliffCount; i++)
            {
                CollisionCliff temp = new CollisionCliff();
                temp.read(f);
                cliffs.Add(temp);
            }

            f.Skip(1);
            int materialCount = f.ReadInt();
            for (int i = 0; i < materialCount; i++)
            {
                f.Skip(1);
                CollisionMat temp = new CollisionMat();
                temp.material = f.Read(0xC); //Temporary, will work on fleshing out material more later
                materials.Add(temp);
            }

        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteFlag(flag1);
            f.WriteFlag(flag2);
            f.WriteFlag(flag3);
            f.WriteFlag(flag4);

            f.WriteByte(1);
            f.WriteInt(verts.Count);
            foreach (Vector2 v in verts)
            {
                f.WriteByte(1);
                f.WriteFloat(v.X);
                f.WriteFloat(v.Y);
            }

            f.WriteByte(1);
            f.WriteInt(normals.Count);
            foreach (Vector2 n in normals)
            {
                f.WriteByte(1);
                f.WriteFloat(n.X);
                f.WriteFloat(n.Y);
            }

            f.WriteByte(1);
            f.WriteInt(cliffs.Count);
            foreach (CollisionCliff c in cliffs)
            {
                c.save(f);
            }

            f.WriteByte(1);
            f.WriteInt(materials.Count);
            foreach (CollisionMat m in materials)
            {
                f.WriteByte(1);
                f.WriteBytes(m.material);
            }
        }
    }

    public class Spawn : LvdEntry
    {
        public override string Magic { get { return "020401017735BB7500000002"; } }

        public float x;
        public float y;

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            x = f.ReadFloat();
            y = f.ReadFloat();
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteFloat(x);
            f.WriteFloat(y);
        }
    }

    public class Bounds : LvdEntry //For Camera Bounds and Blast Zones
    {
        public override string Magic { get { return "020401017735BB7500000002"; } }

        public float left;
        public float right;
        public float top;
        public float bottom;

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            left = f.ReadFloat();
            right = f.ReadFloat();
            top = f.ReadFloat();
            bottom = f.ReadFloat();
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteFloat(left);
            f.WriteFloat(right);
            f.WriteFloat(top);
            f.WriteFloat(bottom);
        }
    }

    public enum LVDShapeType : int
    {
        Point = 1,
        Circle = 2,
        Rectangle = 3,
        Path = 4
    }

    //Basic 2D shape structure used for a variety of purposes within LVD:
    // - ItemSpawner sections
    // - EnemyGenerator enemy spawns and trigger zones
    // - GeneralShape object type
    // - More (unimplemented) things
    public class LVDShape
    {
        public int type;
        //The object always contains four floats, but how they are used differs depending on shape type
        public float shapeValue1, shapeValue2, shapeValue3, shapeValue4;
        public List<Vector2> points = new List<Vector2>();

        //Point and Circle properties
        public float x
        {
            get { return shapeValue1; }
            set { shapeValue1 = value; }
        }
        public float y
        {
            get { return shapeValue2; }
            set { shapeValue2 = value; }
        }
        public float radius
        {
            get { return shapeValue3; }
            set { shapeValue3 = value; }
        }

        //Rectangle properties
        public float left
        {
            get { return shapeValue1; }
            set { shapeValue1 = value; }
        }
        public float right
        {
            get { return shapeValue2; }
            set { shapeValue2 = value; }
        }
        public float bottom
        {
            get { return shapeValue3; }
            set { shapeValue3 = value; }
        }
        public float top
        {
            get { return shapeValue4; }
            set { shapeValue4 = value; }
        }

        public LVDShape()
        {
            type = 1;
        }
        public LVDShape(int type)
        {
            this.type = type;
        }
        public LVDShape(FileData f)
        {
            read(f);
        }

        public void read(FileData f)
        {
            f.ReadByte();
            type = f.ReadInt();
            if (!Enum.IsDefined(typeof(LVDShapeType), type))
                throw new NotImplementedException($"Unknown shape type {type} at offset {f.Pos() - 4}");

            shapeValue1 = f.ReadFloat();
            shapeValue2 = f.ReadFloat();
            shapeValue3 = f.ReadFloat();
            shapeValue4 = f.ReadFloat();

            f.Skip(1);
            f.Skip(1);
            int pointCount = f.ReadInt();
            for (int i = 0; i < pointCount; i++)
            {
                f.Skip(1);
                points.Add(new Vector2(f.ReadFloat(), f.ReadFloat()));
            }
        }
        public void save(FileOutput f)
        {
            f.WriteByte(0x3);
            f.WriteInt(type);

            f.WriteFloat(shapeValue1);
            f.WriteFloat(shapeValue2);
            f.WriteFloat(shapeValue3);
            f.WriteFloat(shapeValue4);

            f.WriteByte(1);
            f.WriteByte(1);
            f.WriteInt(points.Count);
            foreach (Vector2 point in points)
            {
                f.WriteByte(1);
                f.WriteFloat(point.X);
                f.WriteFloat(point.Y);
            }
        }
    }

    public class ItemSpawner : LvdEntry
    {
        public override string Magic { get { return "010401017735BB7500000002"; } }

        public int id = 0x09840001;
        public List<LVDShape> sections = new List<LVDShape>();

        public ItemSpawner() { }

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            id = f.ReadInt();

            f.Skip(1);
            f.Skip(1);
            int sectionCount = f.ReadInt();
            for (int i = 0; i < sectionCount; i++)
            {
                f.Skip(1);
                sections.Add(new LVDShape(f));
            }
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteInt(id);

            f.WriteByte(1);
            f.WriteByte(1);
            f.WriteInt(sections.Count);
            foreach (LVDShape s in sections)
            {
                f.WriteByte(1);
                s.save(f);
            }
        }
    }

    public class EnemyGenerator : LvdEntry
    {
        public override string Magic { get { return "030401017735BB7500000002"; } }

        public List<LVDShape> spawns = new List<LVDShape>();
        public List<LVDShape> zones = new List<LVDShape>();
        public int egUnk1 = 0;
        public int id;
        public List<int> spawnIds = new List<int>();
        public int egUnk2 = 0;
        public List<int> zoneIds = new List<int>();

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(0x2); //x01 01
            int spawnCount = f.ReadInt();
            for (int i = 0; i < spawnCount; i++)
            {
                f.Skip(1);
                spawns.Add(new LVDShape(f));
            }

            f.Skip(0x2); //x01 01
            int zoneCount = f.ReadInt();
            for (int i = 0; i < zoneCount; i++)
            {
                f.Skip(1);
                zones.Add(new LVDShape(f));
            }

            f.Skip(0x2); //x01 01
            egUnk1 = f.ReadInt(); //Only seen as 0

            f.Skip(1); //x01
            id = f.ReadInt();

            f.Skip(1); //x01
            int spawnIdCount = f.ReadInt();
            for (int i = 0; i < spawnIdCount; i++)
            {
                f.Skip(1);
                spawnIds.Add(f.ReadInt());
            }

            f.Skip(1); //x01
            egUnk2 = f.ReadInt(); //Only seen as 0

            f.Skip(1); //x01
            int zoneIdCount = f.ReadInt();
            for (int i = 0; i < zoneIdCount; i++)
            {
                f.Skip(1);
                zoneIds.Add(f.ReadInt());
            }
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteHex("0101");
            f.WriteInt(spawns.Count);
            foreach (LVDShape temp in spawns)
            {
                f.WriteByte(1);
                temp.save(f);
            }

            f.WriteHex("0101");
            f.WriteInt(zones.Count);
            foreach (LVDShape temp in zones)
            {
                f.WriteByte(1);
                temp.save(f);
            }

            f.WriteHex("0101");
            f.WriteInt(egUnk1);

            f.WriteByte(1);
            f.WriteInt(id);

            f.WriteByte(1);
            f.WriteInt(spawnIds.Count);
            foreach (int temp in spawnIds)
            {
                f.WriteByte(1);
                f.WriteInt(temp);
            }

            f.WriteByte(1);
            f.WriteInt(egUnk2);

            f.WriteByte(1);
            f.WriteInt(zoneIds.Count);
            foreach (int temp in zoneIds)
            {
                f.WriteByte(1);
                f.WriteInt(temp);
            }
        }
    }

    //This is basically an LVDShape as a standalone object plus an id int
    public class GeneralShape : LvdEntry
    {
        public override string Magic { get { return "010401017735BB7500000002"; } }

        public int id;
        public LVDShape shape;

        public GeneralShape()
        {
            shape = new LVDShape();
        }
        public GeneralShape(int type) : this()
        {
            shape.type = type;
        }

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            id = f.ReadInt();

            shape = new LVDShape();
            shape.read(f);
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteInt(id);

            shape.save(f);
        }
    }

    public class GeneralPoint : LvdEntry
    {
        public override string Magic { get { return "010401017735BB7500000002"; } }

        public int id;
        public int type;
        public float x, y, z;

        public GeneralPoint()
        {
            //Seems to always be 4
            type = 4;
        }

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            id = f.ReadInt();

            f.Skip(1);
            type = f.ReadInt();

            x = f.ReadFloat();
            y = f.ReadFloat();
            z = f.ReadFloat();
            f.Skip(0x10);
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteInt(id);

            f.WriteByte(1);
            f.WriteInt(type);

            f.WriteFloat(x);
            f.WriteFloat(y);
            f.WriteFloat(z);
            f.WriteHex("00000000000000000000000000000000");
        }
    }

    public enum DamageShapeType
    {
        Sphere = 2,
        Capsule = 3
    }

    public class DamageShape : LvdEntry
    {
        public override string Magic { get { return "010401017735BB7500000002"; } }

        public int type;

        public float x;
        public float y;
        public float z;
        public float dx;
        public float dy;
        public float dz;
        public float radius;
        public byte dsUnk1;
        public int dsUnk2;

        public new void read(FileData f)
        {
            base.Read(f);

            f.Skip(1);
            type = f.ReadInt();
            if (!Enum.IsDefined(typeof(DamageShapeType), type))
                throw new NotImplementedException($"Unknown damage shape type {type} at offset {f.Pos() - 4}");

            x = f.ReadFloat();
            y = f.ReadFloat();
            z = f.ReadFloat();
            if (type == 2)
            {
                radius = f.ReadFloat();
                dx = f.ReadFloat();
                dy = f.ReadFloat();
                dz = f.ReadFloat();
            }
            else if (type == 3)
            {
                dx = f.ReadFloat();
                dy = f.ReadFloat();
                dz = f.ReadFloat();
                radius = f.ReadFloat();
            }
            dsUnk1 = f.ReadByte();
            dsUnk2 = f.ReadInt();
        }
        public new void save(FileOutput f)
        {
            base.Save(f);

            f.WriteByte(1);
            f.WriteInt(type);

            f.WriteFloat(x);
            f.WriteFloat(y);
            f.WriteFloat(z);
            if (type == 2)
            {
                f.WriteFloat(radius);
                f.WriteFloat(dx);
                f.WriteFloat(dy);
                f.WriteFloat(dz);
            }
            else if (type == 3)
            {
                f.WriteFloat(dx);
                f.WriteFloat(dy);
                f.WriteFloat(dz);
                f.WriteFloat(radius);
            }
            f.WriteByte(dsUnk1);
            f.WriteInt(dsUnk2);
        }
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
            enemyGenerators = new List<EnemyGenerator>();
            damageShapes = new List<DamageShape>();
            itemSpawns = new List<ItemSpawner>();
            generalShapes = new List<GeneralShape>();
            generalPoints = new List<GeneralPoint>();
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
        public List<EnemyGenerator> enemyGenerators { get; set; }
        public List<DamageShape> damageShapes { get; set; }
        public List<ItemSpawner> itemSpawns { get; set; }
        public List<GeneralShape> generalShapes { get; set; }
        public List<GeneralPoint> generalPoints { get; set; }

        public override Endianness Endian { get; set; }

        /*type 1  - collisions
          type 2  - spawns
          type 3  - respawns
          type 4  - camera boundaries
          type 5  - death boundaries
          type 6  - enemy generators
          type 7  - ITEMPT_transform
          type 8  - ???
          type 9  - ITEMPT
          type 10 - fsAreaCam (and other fsArea's ? )
          type 11 - fsCamLimit
          type 12 - damage shapes (damage sphere and damage capsule are the only ones I've seen, type 2 and 3 respectively)
          type 13 - item spawners
          type 14 - general shapes (general rect, general path, etc.)
          type 15 - general points
          type 16 - area lights?
          type 17 - FsStartPoint
          type 18 - ???
          type 19 - ???*/

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Skip(0xA); //It's magic

            f.Skip(1);
            int collisionCount = f.ReadInt();
            for (int i = 0; i < collisionCount; i++)
            {
                Collision temp = new Collision();
                temp.read(f);
                collisions.Add(temp);
            }

            f.Skip(1);
            int spawnCount = f.ReadInt();
            for (int i = 0; i < spawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                spawns.Add(temp);
            }

            f.Skip(1);
            int respawnCount = f.ReadInt();
            for (int i = 0; i < respawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                respawns.Add(temp);
            }

            f.Skip(1);
            int cameraCount = f.ReadInt();
            for (int i = 0; i < cameraCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                cameraBounds.Add(temp);
            }

            f.Skip(1);
            int blastzoneCount = f.ReadInt();
            for (int i = 0; i < blastzoneCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                blastzones.Add(temp);
            }

            f.Skip(1);
            int enemyGeneratorCount = f.ReadInt();
            for (int i = 0; i < enemyGeneratorCount; i++)
            {
                EnemyGenerator temp = new EnemyGenerator();
                temp.read(f);
                enemyGenerators.Add(temp);
            }

            f.Skip(1);
            if (f.ReadInt() != 0) //7
                return;

            f.Skip(1);
            if (f.ReadInt() != 0) //8
                return;

            f.Skip(1);
            if (f.ReadInt() != 0) //9
                return;

            f.Skip(1);
            int fsAreaCamCount = f.ReadInt();
            if (fsAreaCamCount != 0)
                return;

            f.Skip(1);
            int fsCamLimitCount = f.ReadInt();
            if (fsCamLimitCount != 0)
                return;

            f.Skip(1);
            int damageShapeCount = f.ReadInt();
            for (int i = 0; i < damageShapeCount; i++)
            {
                DamageShape temp = new DamageShape();
                temp.read(f);
                damageShapes.Add(temp);
            }

            f.Skip(1);
            int itemCount = f.ReadInt();
            for (int i = 0; i < itemCount; i++)
            {
                ItemSpawner temp = new ItemSpawner();
                temp.read(f);
                itemSpawns.Add(temp);
            }

            f.Skip(1);
            int generalShapeCount = f.ReadInt();
            for (int i = 0; i < generalShapeCount; i++)
            {
                GeneralShape temp = new GeneralShape();
                temp.read(f);
                generalShapes.Add(temp);
            }

            f.Skip(1);
            int generalPointCount = f.ReadInt();
            for (int i = 0; i < generalPointCount; i++)
            {
                GeneralPoint temp = new GeneralPoint();
                temp.read(f);
                generalPoints.Add(temp);
            }

            f.Skip(1);
            if (f.ReadInt() != 0) //16
                return; //no clue how to be consistent in reading these so...

            f.Skip(1);
            if (f.ReadInt() != 0) //17
                return; //no clue how to be consistent in reading these so...

            f.Skip(1);
            if (f.ReadInt() != 0) //18
                return; //no clue how to be consistent in reading these so...

            f.Skip(1);
            if (f.ReadInt() != 0) //19
                return; //no clue how to be consistent in reading these so...

            //LVD doesn't end here and neither does my confusion, will update this part later
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.endian = Endianness.Big;

            f.WriteHex("000000010A014C564431");

            f.WriteByte(1);
            f.WriteInt(collisions.Count);
            foreach (Collision c in collisions)
                c.save(f);

            f.WriteByte(1);
            f.WriteInt(spawns.Count);
            foreach (Spawn s in spawns)
                s.save(f);

            f.WriteByte(1);
            f.WriteInt(respawns.Count);
            foreach (Spawn s in respawns)
                s.save(f);

            f.WriteByte(1);
            f.WriteInt(cameraBounds.Count);
            foreach (Bounds b in cameraBounds)
                b.save(f);

            f.WriteByte(1);
            f.WriteInt(blastzones.Count);
            foreach (Bounds b in blastzones)
                b.save(f);

            f.WriteByte(1);
            f.WriteInt(enemyGenerators.Count);
            foreach (EnemyGenerator e in enemyGenerators)
                e.save(f);

            for (int i = 0; i < 5; i++)
            {
                f.WriteByte(1);
                f.WriteInt(0);
            }

            f.WriteByte(1);
            f.WriteInt(damageShapes.Count);
            foreach (DamageShape shape in damageShapes)
                shape.save(f);

            f.WriteByte(1);
            f.WriteInt(itemSpawns.Count);
            foreach (ItemSpawner item in itemSpawns)
                item.save(f);

            f.WriteByte(1);
            f.WriteInt(generalShapes.Count);
            foreach (GeneralShape shape in generalShapes)
                shape.save(f);

            f.WriteByte(1);
            f.WriteInt(generalPoints.Count);
            foreach (GeneralPoint p in generalPoints)
                p.save(f);

            for (int i = 0; i < 4; i++)
            {
                f.WriteByte(1);
                f.WriteInt(0);
            }

            return f.GetBytes();
        }

        public static void GeneratePassthroughs(Collision c, bool PolyCheck = false)
        {
            // Generate Normals Assuming Clockwise
            for (int i = 0; i < c.verts.Count - 1; i++)
            {
                Vector2 v1 = c.verts[i];
                Vector2 v2 = c.verts[i + 1];
                Vector2 normal = new Vector2(v2.Y - v1.Y, v2.X - v1.X).Normalized();
                normal.X *= -1;
                c.normals[i] = normal;
            }

            // If this forms a polygon we can get assume we want the angles to points outside the polygon
            // Not the fastest but lvd won't typically have a massive number of lines
            if (c.IsPolygon && PolyCheck)
            {
                for (int i = 0; i < c.verts.Count - 1; i++)
                {
                    Vector2 pos = (c.verts[i] + c.verts[i + 1]) / 2;
                    Vector2 N1 = c.normals[i];

                    // Check collision
                    // done by counting the number of intersection using the normal as a ray
                    // odd hits = inside even hits = outside
                    // https://rootllama.wordpress.com/2014/06/20/ray-line-segment-intersection-test-in-2d/
                    int count = 0;
                    for (int j = 0; j < c.verts.Count - 1; j++)
                    {
                        if (j == i) continue;

                        Vector2 v1 = c.verts[j];
                        Vector2 v2 = c.verts[j + 1];

                        Vector2 p1 = pos - v1;
                        Vector2 p2 = v2 - v1;
                        Vector2 p3 = new Vector2(-N1.Y, N1.X);

                        float dot = Vector2.Dot(p2, p3);
                        if (Math.Abs(dot) < 0.00001f)
                            continue;

                        float f1 = (p2.X * p1.Y - p2.Y * p1.X) / dot;
                        float f2 = Vector2.Dot(p1, p3) / dot;

                        //Found intersection
                        if (f1 >= 0.0f && (f2 >= 0.0f && f2 <= 1.0f))
                            count++;
                    }

                    if (count % 2 == 1)
                        //odd so flip
                        c.normals[i] = c.normals[i] * -1;
                }
            }
        }

        public static void FlipPassthroughs(Collision c)
        {
            for (int i = 0; i < c.normals.Count; i++)
            {
                c.normals[i] = c.normals[i] * -1;
            }
        }

        //Function to automatically add a cliff to every grabbable ledge in a given collision
        //Works mostly to vanilla standards, though vanilla standards are inconsistent on handling bone name/start pos
        public static void GenerateCliffs(Collision col)
        {
            int[] counts = new int[2];
            bool[,] lines = new bool[col.materials.Count, 2];
            for (int i = 0; i < col.materials.Count; i++)
            {
                lines[i, 0] = col.materials[i].leftLedge;
                lines[i, 1] = col.materials[i].rightLedge;
                if (lines[i, 0]) counts[0]++;
                if (lines[i, 1]) counts[1]++;
            }

            string nameSub;
            if (col.Name.Length > 4 && col.Name.StartsWith("COL_"))
                nameSub = col.Name.Substring(4, col.Name.Length - 4);
            else
                nameSub = "Collision";

            col.cliffs = new List<CollisionCliff>();
            counts[0] = counts[0] > 1 ? 1 : 0;
            counts[1] = counts[1] > 1 ? 1 : 0;
            for (int i = 0; i < col.materials.Count; i++)
            {
                if (lines[i, 0])
                {
                    string cliffName = "CLIFF_" + nameSub + "L" + (counts[0] > 0 ? $"{counts[0]++}" : "");
                    CollisionCliff temp = new CollisionCliff();
                    temp.Name = cliffName;
                    temp.Subname = cliffName.Substring(6, cliffName.Length - 6);
                    temp.BoneName = col.BoneName;
                    temp.useStartPos = col.useStartPos;
                    int ind = i;
                    temp.pos = new Vector2(col.verts[ind].X, col.verts[ind].Y);
                    temp.startPos = new Vector3(col.verts[ind].X, col.verts[ind].Y, 0);
                    if (col.useStartPos)
                        temp.startPos = Vector3.Add(temp.startPos, col.startPos);
                    temp.angle = -1.0f;
                    temp.lineIndex = i;
                    col.cliffs.Add(temp);
                }
                if (lines[i, 1])
                {
                    string cliffName = "CLIFF_" + nameSub + "R" + (counts[1] > 0 ? $"{counts[1]++}" : "");
                    CollisionCliff temp = new CollisionCliff();
                    temp.Name = cliffName;
                    temp.Subname = cliffName.Substring(6, cliffName.Length - 6);
                    temp.BoneName = col.BoneName;
                    temp.useStartPos = col.useStartPos;
                    int ind = i + 1;
                    temp.pos = new Vector2(col.verts[ind].X, col.verts[ind].Y);
                    temp.startPos = new Vector3(col.verts[ind].X, col.verts[ind].Y, 0);
                    if (col.useStartPos)
                        temp.startPos = Vector3.Add(temp.startPos, col.startPos);
                    temp.angle = 1.0f;
                    temp.lineIndex = i;
                    col.cliffs.Add(temp);
                }
            }
        }

        #region rendering

        public object LVDSelection;
        public MeshList MeshList;

        public void Render()
        {
            GL.Disable(EnableCap.CullFace);

            /*foreach (ModelContainer m in ModelContainers)
            {

                if (m.dat_melee != null && m.dat_melee.collisions != null)
                {
                    LVD.DrawDATCollisions(m);

                }

                if (m.dat_melee != null && m.dat_melee.blastzones != null)
                {
                    LVD.DrawBounds(m.dat_melee.blastzones, Color.Red);
                }

                if (m.dat_melee != null && m.dat_melee.cameraBounds != null)
                {
                    LVD.DrawBounds(m.dat_melee.cameraBounds, Color.Blue);
                }

                if (m.dat_melee != null && m.dat_melee.targets != null)
                {
                    foreach (Point target in m.dat_melee.targets)
                    {
                        Rendering.RenderTools.drawCircleOutline(new Vector3(target.x, target.y, 0), 2, 30);
                        Rendering.RenderTools.drawCircleOutline(new Vector3(target.x, target.y, 0), 4, 30);
                    }
                }

                if (m.dat_melee != null && m.dat_melee.respawns != null)
                    foreach (Point r in m.dat_melee.respawns)
                    {
                        Spawn temp = new Spawn() { x = r.x, y = r.y };
                        LVD.DrawSpawn(temp, true);
                    }

                if (m.dat_melee != null && m.dat_melee.spawns != null)
                    foreach (Point r in m.dat_melee.spawns)
                    {
                        Spawn temp = new Spawn() { x = r.x, y = r.y };
                        LVD.DrawSpawn(temp, false);
                    }

                GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                if (m.dat_melee != null && m.dat_melee.itemSpawns != null)
                    foreach (Point r in m.dat_melee.itemSpawns)
                        Rendering.RenderTools.drawCubeWireframe(new Vector3(r.x, r.y, 0), 3);
            }
            */
            //if (Runtime.TargetLVD != null)
            {
                if (Runtime.renderCollisions)
                {
                    DrawCollisions();
                }

                if (Runtime.renderItemSpawners)
                {
                    DrawItemSpawners();
                }

                if (Runtime.renderSpawns)
                {
                    foreach (Spawn s in spawns)
                        DrawSpawn(s, false);
                }

                if (Runtime.renderRespawns)
                {
                    foreach (Spawn s in respawns)
                        DrawSpawn(s, true);
                }

                if (Runtime.renderGeneralPoints)
                {
                    foreach (GeneralPoint p in generalPoints)
                        DrawPoint(p);

                    GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                    foreach (GeneralShape s in generalShapes)
                        DrawShape(s.shape, s.useStartPos, s.startPos);
                }

                if (Runtime.renderOtherLvdEntries)
                {
                    foreach (EnemyGenerator e in enemyGenerators)
                        DrawEnemyGenerator(e);

                    foreach (DamageShape s in damageShapes)
                        DrawDamageShape(s);

                    foreach (Bounds b in cameraBounds)
                        DrawBounds(b, Color.Blue);

                    foreach (Bounds b in blastzones)
                        DrawBounds(b, Color.Red);
                }
            }

            GL.Enable(EnableCap.CullFace);
        }

        public static void DrawPoint(GeneralPoint p)
        {
            GL.LineWidth(2);

            Vector3 pos = p.useStartPos ? p.startPos : new Vector3(p.x, p.y, p.z);

            GL.Color3(Color.Red);
            Rendering.ShapeDrawing.DrawCube(pos, 3, true);
        }

        public static void DrawShape(LVDShape s)
        {
            DrawShape(s, false, new Vector3(0, 0, 0));
        }

        public static void DrawShape(LVDShape s, bool useStartPos, Vector3 sPos)
        {
            GL.LineWidth(2);

            if (!useStartPos)
                sPos = new Vector3(0, 0, 0);

            if (s.type == (int)LVDShapeType.Point)
            {
                if (useStartPos)
                    Rendering.ShapeDrawing.DrawCube(sPos, 3, true);
                else
                    Rendering.ShapeDrawing.DrawCube(new Vector3(s.x, s.y, 0), 3, true);
            }
            else if (s.type == (int)LVDShapeType.Circle)
            {
                if (useStartPos)
                    Rendering.ShapeDrawing.DrawCircleOutline(sPos, s.radius, 24);
                else
                    Rendering.ShapeDrawing.DrawCircleOutline(new Vector3(s.x, s.y, 0), s.radius, 24);
            }
            else if (s.type == (int)LVDShapeType.Rectangle)
            {
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex3(s.left + sPos.X, s.bottom + sPos.Y, sPos.Z);
                GL.Vertex3(s.right + sPos.X, s.bottom + sPos.Y, sPos.Z);
                GL.Vertex3(s.right + sPos.X, s.top + sPos.Y, sPos.Z);
                GL.Vertex3(s.left + sPos.X, s.top + sPos.Y, sPos.Z);
            }
            else if (s.type == (int)LVDShapeType.Path)
            {
                GL.Begin(PrimitiveType.LineStrip);
                foreach (Vector2 point in s.points)
                    GL.Vertex3(point.X + sPos.X, point.Y + sPos.Y, sPos.Z);
            }

            GL.End();
        }

        public static void DrawDamageShape(DamageShape s)
        {
            GL.LineWidth(2);
            GL.Color4(Color.FromArgb(128, Color.Yellow));

            Vector3 sPos = s.useStartPos ? s.startPos : new Vector3(0, 0, 0);
            Vector3 pos = new Vector3(s.x, s.y, s.z);
            Vector3 posd = new Vector3(s.dx, s.dy, s.dz);

            if (s.type == (int)DamageShapeType.Sphere)
                Rendering.ShapeDrawing.DrawSphere(sPos + pos, s.radius, 24);
            else if (s.type == (int)DamageShapeType.Capsule)
                Rendering.ShapeDrawing.DrawCylinder(sPos + pos, sPos + pos + posd, s.radius);
        }

        public static void DrawEnemyGenerator(EnemyGenerator e)
        {
            GL.Color4(Color.FromArgb(200, Color.Orange));
            foreach (LVDShape s in e.spawns)
                DrawShape(s);

            GL.Color4(Color.FromArgb(200, Color.White));
            foreach (LVDShape s in e.zones)
                DrawShape(s);
        }

        public static void DrawSpawn(Spawn s, bool isRespawn)
        {
            GL.LineWidth(2);

            Vector3 pos = s.useStartPos ? s.startPos : new Vector3(s.x, s.y, 0);
            float x = pos[0], y = pos[1], z = pos[2];

            //Draw quad
            float width = 3.0f;
            float height = 10.0f;

            GL.Color4(Color.FromArgb(100, Color.Blue));
            GL.Begin(PrimitiveType.QuadStrip);

            GL.Vertex3(x - width, y, z);
            GL.Vertex3(x + width, y, z);
            GL.Vertex3(x - width, y + height, z);
            GL.Vertex3(x + width, y + height, z);

            GL.End();

            //Respawn platform
            if (isRespawn)
            {
                float scale = 5.0f;

                //Draw arrow
                GL.Color4(Color.FromArgb(200, Color.Gray));
                GL.Begin(PrimitiveType.Triangles);

                GL.Vertex3(x - scale, y, z);
                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y, z + scale);

                GL.Vertex3(x - scale, y, z);
                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y, z - scale);

                GL.Vertex3(x - scale, y, z);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x, y, z + scale);

                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x, y, z - scale);

                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x, y, z + scale);

                GL.Vertex3(x - scale, y, z);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x, y, z - scale);

                GL.End();

                //Draw wireframe
                GL.Color4(Color.FromArgb(200, Color.Black));
                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(x - scale, y, z);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y - scale, z);

                GL.Vertex3(x, y, z - scale);
                GL.Vertex3(x, y - scale, z);
                GL.Vertex3(x, y, z + scale);
                GL.Vertex3(x, y - scale, z);

                GL.Vertex3(x, y, z - scale);
                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y, z - scale);
                GL.Vertex3(x - scale, y, z);

                GL.Vertex3(x, y, z + scale);
                GL.Vertex3(x + scale, y, z);
                GL.Vertex3(x, y, z + scale);
                GL.Vertex3(x - scale, y, z);

                GL.End();
            }
        }

        public static void DrawBounds(Bounds b, Color color)
        {
            GL.LineWidth(2);

            Vector3 sPos = b.useStartPos ? b.startPos : new Vector3(0, 0, 0);

            GL.Color4(Color.FromArgb(128, color));
            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(b.left + sPos.X, b.top + sPos.Y, sPos.Z);
            GL.Vertex3(b.right + sPos.X, b.top + sPos.Y, sPos.Z);
            GL.Vertex3(b.right + sPos.X, b.bottom + sPos.Y, sPos.Z);
            GL.Vertex3(b.left + sPos.X, b.bottom + sPos.Y, sPos.Z);

            GL.End();
        }

        public void DrawItemSpawners()
        {
            foreach (ItemSpawner c in itemSpawns)
            {
                Vector3 sPos = c.useStartPos ? c.startPos : new Vector3(0, 0, 0);
                foreach (LVDShape s in c.sections)
                {
                    // draw the item spawn quads
                    GL.LineWidth(2);

                    // draw outside borders
                    GL.Color3(Color.Black);

                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (Vector2 vi in s.points)
                        GL.Vertex3(vi.X + sPos[0], vi.Y + sPos[1], 5 + sPos[2]);
                    GL.End();

                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (Vector2 vi in s.points)
                        GL.Vertex3(vi.X + sPos[0], vi.Y + sPos[1], -5 + sPos[2]);
                    GL.End();


                    // draw vertices
                    GL.Color3(Color.White);
                    GL.Begin(PrimitiveType.Lines);
                    foreach (Vector2 vi in s.points)
                    {
                        GL.Vertex3(vi.X + sPos[0], vi.Y + sPos[1], 5 + sPos[2]);
                        GL.Vertex3(vi.X + sPos[0], vi.Y + sPos[1], -5 + sPos[2]);
                    }
                    GL.End();
                }
            }
        }

        public void DrawCollisions()
        {
            bool blink = DateTime.UtcNow.Second % 2 == 1;
            Color color;
            GL.LineWidth(4);
            Matrix4 transform = Matrix4.Identity;
            foreach (Collision c in collisions)
            {
                bool colSelected = (LVDSelection == c);
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
                    foreach (ModelContainer m in MeshList.filesTreeView.Nodes)
                    {
                        if (m.Text.Equals(c.Subname))
                        {
                            riggedModel = m;
                            if (m.VBN != null)
                            {
                                foreach (Bone b in m.VBN.bones)
                                {
                                    if (b.Text.Equals(c.BoneName))
                                    {
                                        riggedBone = b;
                                    }
                                }
                            }
                        }
                    }
                    if (riggedModel != null)
                    {
                        if (riggedBone == null && riggedModel.VBN != null && riggedModel.VBN.bones.Count > 0)
                        {
                            riggedBone = riggedModel.VBN.bones[0];
                        }
                        if (riggedBone != null)
                            transform = riggedBone.invert * riggedBone.transform;
                    }
                }

                for (int i = 0; i < c.verts.Count - 1; i++)
                {
                    Vector3 v1Pos = Vector3.TransformPosition(new Vector3(c.verts[i].X + addX, c.verts[i].Y + addY, addZ + 5), transform);
                    Vector3 v1Neg = Vector3.TransformPosition(new Vector3(c.verts[i].X + addX, c.verts[i].Y + addY, addZ - 5), transform);
                    Vector3 v1Zero = Vector3.TransformPosition(new Vector3(c.verts[i].X + addX, c.verts[i].Y + addY, addZ), transform);
                    Vector3 v2Pos = Vector3.TransformPosition(new Vector3(c.verts[i + 1].X + addX, c.verts[i + 1].Y + addY, addZ + 5), transform);
                    Vector3 v2Neg = Vector3.TransformPosition(new Vector3(c.verts[i + 1].X + addX, c.verts[i + 1].Y + addY, addZ - 5), transform);
                    Vector3 v2Zero = Vector3.TransformPosition(new Vector3(c.verts[i + 1].X + addX, c.verts[i + 1].Y + addY, addZ), transform);

                    Vector3 normals = Vector3.TransformPosition(new Vector3(c.normals[i].X, c.normals[i].Y, 0), transform);

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
                            GL.Vertex3(v.X + (c.normals[i].X * 5), v.Y + (c.normals[i].Y * 5), v.Z);
                            GL.End();
                            GL.Begin(PrimitiveType.Quads);
                        }

                        float angle = (float)(Math.Atan2(normals.Y, normals.X) * 180 / Math.PI);

                        if (c.flag4)
                            color = Color.FromArgb(128, Color.Yellow);
                        else if (c.materials[i].GetFlag(4) && ((angle <= 0 && angle >= -70) || (angle <= -110 && angle >= -180) || angle == 180))
                            color = Color.FromArgb(128, Color.Purple);
                        else if ((angle <= 0 && angle >= -70) || (angle <= -110 && angle >= -180) || angle == 180)
                            color = Color.FromArgb(128, Color.Lime);
                        else if (normals.Y < 0)
                            color = Color.FromArgb(128, Color.Red);
                        else
                            color = Color.FromArgb(128, Color.Cyan);

                        if ((colSelected || (LVDSelection != null && LVDSelection.Equals(c.normals[i]))) && blink)
                            color = ColorUtils.InvertColor(color);

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
                        if (c.materials[i].GetFlag(6) || (i > 0 && c.materials[i - 1].GetFlag(7)))
                            color = Color.Purple;
                        else
                            color = Color.Orange;

                        if ((colSelected || (LVDSelection != null && LVDSelection.Equals(c.verts[i]))) && blink)
                            color = ColorUtils.InvertColor(color);
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
                            if (c.materials[i].GetFlag(7))
                                color = Color.Purple;
                            else
                                color = Color.Orange;

                            if (LVDSelection != null && LVDSelection.Equals(c.verts[i + 1]) && blink)
                                color = ColorUtils.InvertColor(color);
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
                for (int i = 0; i < c.cliffs.Count; i++)
                {
                    Vector3 pos = c.cliffs[i].useStartPos ? Vector3.TransformPosition(new Vector3(c.cliffs[i].startPos.X, c.cliffs[i].startPos.Y, c.cliffs[i].startPos.Z), transform) : Vector3.TransformPosition(new Vector3(c.cliffs[i].pos.X, c.cliffs[i].pos.Y, 0), transform);

                    GL.Color3(Color.White);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(pos[0], pos[1], pos[2] + 10);
                    GL.Vertex3(pos[0], pos[1], pos[2] - 10);
                    GL.End();

                    GL.LineWidth(2);
                    GL.Color3(Color.Blue);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(pos);
                    GL.Vertex3(pos[0] + (c.cliffs[i].angle * 10), pos[1], pos[2]);
                    GL.End();

                    GL.LineWidth(4);
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
            float scale = m.DatMelee.stageScale;
            List<int> ledges = new List<int>();
            foreach (DAT.COLL_DATA.Link link in m.DatMelee.collisions.links)
            {

                GL.Begin(PrimitiveType.Quads);
                GL.Color4(getLinkColor(link));
                Vector2 vi = m.DatMelee.collisions.vertices[link.vertexIndices[0]];
                GL.Vertex3(vi.X * scale, vi.Y * scale, 5);
                GL.Vertex3(vi.X * scale, vi.Y * scale, -5);
                vi = m.DatMelee.collisions.vertices[link.vertexIndices[1]];
                GL.Vertex3(vi.X * scale, vi.Y * scale, -5);
                GL.Vertex3(vi.X * scale, vi.Y * scale, 5);
                GL.End();

                if ((link.flags & 2) != 0)
                {
                    ledges.Add(link.vertexIndices[0]);
                    ledges.Add(link.vertexIndices[1]);
                }
            }

            GL.LineWidth(4);
            for (int i = 0; i < m.DatMelee.collisions.vertices.Count; i++)
            {
                Vector2 vi = m.DatMelee.collisions.vertices[i];
                if (ledges.Contains(i))
                    GL.Color3(Color.Purple);
                else
                    GL.Color3(Color.Tomato);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(vi.X * scale, vi.Y * scale, 5);
                GL.Vertex3(vi.X * scale, vi.Y * scale, -5);
                GL.End();
            }
        }

    }

    #endregion

}



