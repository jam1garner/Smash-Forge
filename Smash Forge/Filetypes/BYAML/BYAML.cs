using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using Syroot.NintenTools.Byaml.Dynamic;
using OpenTK;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class BYAML
    {
        public dynamic byaml_file;

        public BYAML(string fname)
        {
        }

        public List<ObjPath> objPaths = new List<ObjPath>();

        public bool hasEnemyPath;
        public bool hasItemPath;
        public bool hasLapPath;
        public bool hasIntroCamera;
        public bool hasPath;
        public bool hasObjPath;
        public bool hasGravityPath;
        public bool hasGravityCameraPath;
        public bool hasObj;
        public bool hasArea;
        public bool hasSoundObj;
        public bool hasClip;
        public bool hasClipArea;
        public bool hasClipPattern;
        public bool hasFirstCurve;
        public bool hasJugemPath;
        public bool hasReplayCamera;
        public bool HasGliderPath;

        public TreeNode Areas = new TreeNode() { Text = "Areas", Checked = true };
        public TreeNode Clips = new TreeNode() { Text = "Clips", Checked = true };
        public TreeNode ClipAreas = new TreeNode() { Text = "Clip Areas", Checked = true };
        public TreeNode ClipPatterns = new TreeNode() { Text = "Clip Patterns", Checked = true };
        public TreeNode EnemyPaths = new TreeNode() { Text = "Enemy Paths", Checked = true };
        public TreeNode FirstCurve = new TreeNode() { Text = "FirstCurve", Checked = true };
        public TreeNode LapPaths = new TreeNode() { Text = "Lap Paths", Checked = true };
        public TreeNode GravityPaths = new TreeNode() { Text = "Gravity Paths", Checked = true };
        public TreeNode GravityCamPaths = new TreeNode() { Text = "Gravity Camera Paths", Checked = true };
        public TreeNode GliderPaths = new TreeNode() { Text = "Glide Paths", Checked = true };
        public TreeNode ItemPaths = new TreeNode() { Text = "Item Paths", Checked = true };
        public TreeNode Paths = new TreeNode() { Text = "Paths", Checked = true };
        public TreeNode ObjPaths = new TreeNode() { Text = "Objs", Checked = true };
        public TreeNode Objs = new TreeNode() { Text = "ObjPaths", Checked = true };
        public TreeNode IntroCameras = new TreeNode() { Text = "IntroCameras", Checked = true };
        public TreeNode JugemPaths = new TreeNode() { Text = "JugemPaths", Checked = true };

        public class IntroCamera : TreeNode
        {
            public int CameraNum = 0;
            public int CameraTime = 0;
            public int CameraType = 6;
            public int Camera_AtPath = -1; //These index "Paths" list in the Path Array class
            public int Camera_Path = -1;
            public int Fovy = -1;
            public int Fovy2 = -1;
            public int FovySpeed = 0;
            public int UnitIdNum = 0;
            public Vector3 rotate;
            public Vector3 scale;
            public Vector3 translate;
        }
        public class Path : TreeNode
        {
            public bool IsClosed = false;
            public bool Delete = false;
            public int RailType = 1;
            public int UintIdNum = 0;

            public class ControlPoint
            {
                public Vector3 point;
            }
            public class PathPoint : TreeNode
            {
                public List<ControlPoint> controlPoints = new List<ControlPoint>();

                public float param1 = 0f;
                public float param2 = 0f;

                public Vector3 rotate;
                public Vector3 scale;
                public Vector3 translate;
            }
        }
        public class ObjPath
        {
            public List<PathPoint> objPoints = new List<PathPoint>();

            public bool IsClosed = false;
            public int PtNum = 1;
            public float SplitWidth = 0f;
            public int UnitIdNum = 0;

            public class ControlPoint
            {
                public Vector3 point;
            }
            public class ObjPoint
            {
                public Vector3 position;
                public Vector3 normal;
                public int val;
            }
            public class PathPoint
            {
                public int Index = 0;
                public float param1 = 0f;
                public float param2 = 0f;

                public Vector3 rotate;
                public Vector3 translate;
            }
        }

        //Common arrays used in mutliple classes

        public class GenericPathGroup : TreeNode
        {
            public List<PathPoint> pathPoints = new List<PathPoint>();
        }

        public class PathPoint : TreeNode
        {
            public List<NextPt> nextPts = new List<NextPt>();
            public List<PrevPt> prevPts = new List<PrevPt>();

            public Vector3 translate;
            public Vector3 rotate;
            public Vector3 scale;
        }

        public class NextPt
        {
            public int PathId;
            public int PtId;
        }
        public class PrevPt
        {
            public int PathId;
            public int PtId;
        }

        public void Rebuild(string fileName)
        {
           // byaml_file = byaml_file.Save(fileName);
        }
    }
}
