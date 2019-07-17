using System;
using System.Collections.Generic;

namespace SmashForge
{
    // Taken from SPICA by gdkchan
    // https://github.com/gdkchan/SPICA/blob/f76384f673a7206858fb3c766f891ec9cc9977ee/SPICA/Formats/CtrH3D/H3DPatriciaTree.cs
    

    public class PatriciaTree
    {
        public class PatriciaTreeNode
        {
            public string Name = "";
            public uint ReferenceBit;
            public ushort LeftNodeIndex = 0;
            public ushort RightNodeIndex = 0;
        }

        private const string DuplicateKeysEx = "Tree shouldn't contain duplicate keys!";

        public static void Insert(List<PatriciaTreeNode> Nodes, PatriciaTreeNode Value, int MaxLength)
        {
            uint Bit = (uint)((MaxLength << 3) - 1);
            PatriciaTreeNode Root = Nodes[0];

            int Index = Traverse(Value.Name, Nodes, out Root);

            while (GetBit(Nodes[Index].Name, Bit) == GetBit(Value.Name, Bit))
            {
                if (--Bit == uint.MaxValue) throw new InvalidOperationException(DuplicateKeysEx);
            }

            Value.ReferenceBit = Bit;

            if (GetBit(Value.Name, Bit))
            {
                Value.LeftNodeIndex = (ushort)Traverse(Value.Name, Nodes, out Root, Bit);
                Value.RightNodeIndex = (ushort)Nodes.Count;
            }
            else
            {
                Value.LeftNodeIndex = (ushort)Nodes.Count;
                Value.RightNodeIndex = (ushort)Traverse(Value.Name, Nodes, out Root, Bit);
            }

            int RootIndex = Nodes.IndexOf(Root);

            if (GetBit(Value.Name, Root.ReferenceBit))
                Root.RightNodeIndex = (ushort)Nodes.Count;
            else
                Root.LeftNodeIndex = (ushort)Nodes.Count;

            Nodes.Add(Value);

            Nodes[RootIndex] = Root;
        }

        public static int Traverse(string Name, List<PatriciaTreeNode> Nodes, out PatriciaTreeNode Root, uint Bit = 0) 
        {
            Root = Nodes[0];

            int Output = Root.LeftNodeIndex;

            PatriciaTreeNode Left = Nodes[Output];

            while (Root.ReferenceBit > Left.ReferenceBit && Left.ReferenceBit > Bit)
            {
                if (GetBit(Name, Left.ReferenceBit))
                    Output = Left.RightNodeIndex;
                else
                    Output = Left.LeftNodeIndex;

                Root = Left;
                Left = Nodes[Output];
            }

            return Output;
        }

        private static bool GetBit(string Name, uint Bit)
        {
            int Position = (int)(Bit >> 3);
            int CharBit = (int)(Bit & 7);

            if (Name != null && Position < Name.Length)
                return ((Name[Position] >> CharBit) & 1) != 0;
            else
                return false;
        }
    }
}
