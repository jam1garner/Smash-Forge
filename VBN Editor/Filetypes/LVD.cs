using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBN_Editor
{
    public struct Vector2D
    {
        public float x;
        public float y;
    }

    public struct Point
    {
        public string name;
        public string subname;
        public float x;
        public float y;
    }

    public struct Bounds //Either Camera bounds or Blast zones
    {
        public string name;
        public string subname;
        public float top;
        public float bottom;
        public float left;
        public float right;
    }

    public struct CollisionMat
    {
        public bool leftLedge;
        public bool rightLedge;
        public bool noWallJump;
        public byte physicsType;
    }

    public struct ItemSection
    {
        public List<Vector2D> points;
    }

    public class ItemSpawner
    {
        public string name;
        public string subname;
        public List<ItemSection> sections = new List<ItemSection>();

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
                ItemSection temp;
                temp.points = new List<Vector2D>();
                int vertCount = f.readInt();
                for(int j = 0; j < vertCount; j++)
                {
                    f.skip(1);//Seperation char
                    Vector2D point;
                    point.x = f.readFloat();
                    point.y = f.readFloat();
                    temp.points.Add(point);
                }
                sections.Add(temp);
            }
        }
    }

    public class Collision
    {
        public string name;
        public string subname;
        public List<Vector2D> verts = new List<Vector2D>();
        public List<Vector2D> normals = new List<Vector2D>();
        public List<CollisionMat> materials = new List<CollisionMat>();

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
            f.skip(0xAA);//TODO: Read in collision header

            int vertCount = f.readInt();
            for(int i = 0; i < vertCount; i++)
            {
                f.skip(1);//Seperation char
                Vector2D temp;
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                verts.Add(temp);
            }
            f.skip(1);//Seperation char

            int normalCount = f.readInt();
            for(int i = 0; i < normalCount; i++)
            {
                f.skip(1);//Seperation char
                Vector2D temp;
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
                CollisionMat temp;
                byte[] mat = f.read(0xC);//Temporary, will work on fleshing out material more later
                temp.noWallJump = ((mat[10] & 0x10) != 0);
                temp.leftLedge = ((mat[10] & 0x40) != 0);
                temp.rightLedge = ((mat[10] & 0x80) != 0);
                temp.physicsType = mat[4];
                materials.Add(temp);
            }

        }
    }

    public class LVD
    {
        public List<Collision> collisions = new List<Collision>();
        public List<Point> spawns = new List<Point>();
        public List<Point> respawns = new List<Point>();
        public List<Bounds> cameraBounds = new List<Bounds>();
        public List<Bounds> blastzones = new List<Bounds>();
        public List<Point> generalPoints = new List<Point>();
        public List<ItemSpawner> items = new List<ItemSpawner>();

        public LVD()
        {

        }

        public void read(FileData f)
        {
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
                Point temp;
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
                Point temp;
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
                Bounds temp;
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
                Bounds temp;
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

            if (f.readInt() != 0)//1
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//2
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//3
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//4
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//5
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//6
                throw new NotImplementedException();
            f.skip(1);

            if (f.readInt() != 0)//7
                throw new NotImplementedException();
            f.skip(1);

            int itemCount = f.readInt();
            for(int i = 0; i < itemCount; i++)
            {
                ItemSpawner temp = new ItemSpawner();
                temp.read(f);
                items.Add(temp);
            }
            f.skip(1);//Seperation char

            if (f.readInt() != 0)//8
                throw new NotImplementedException();
            f.skip(1);
            
            int generalPointCount = f.readInt();
            for(int i = 0; i < generalPointCount; i++)
            {
                Point temp;
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
            //LVD doesn't end here and neither does my confusion, will update this part later
        }
    }
}
