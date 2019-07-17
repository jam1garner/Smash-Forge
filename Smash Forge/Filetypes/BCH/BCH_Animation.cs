using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmashForge
{
    class BCH_Animation : TreeNode
    {

        public static void Rebuild(string fname, List<Animation> animations)
        {

            // poopity doo da
            // headery deadery
            FileOutput o = new FileOutput();
            o.WriteString("BCH");
            o.Align(4);
            o.WriteByte(0x21); // version stuffs
            o.WriteByte(0x21); // version stuffs
            o.endian = System.IO.Endianness.Little;
            o.WriteShort(0xA755); // version

            FileOutput d_Main = new FileOutput();
            d_Main.endian = System.IO.Endianness.Little;
            FileOutput d_Main2 = new FileOutput();
            d_Main2.endian = System.IO.Endianness.Little;
            FileOutput d_Main3 = new FileOutput();
            d_Main3.endian = System.IO.Endianness.Little;
            FileOutput d_String = new FileOutput();
            d_String.endian = System.IO.Endianness.Little;
            FileOutput d_GPU = new FileOutput();
            d_GPU.endian = System.IO.Endianness.Little;
            FileOutput d_Data = new FileOutput();
            d_Data.endian = System.IO.Endianness.Little;

            FileOutput Reloc = new FileOutput();
            Reloc.endian = System.IO.Endianness.Little;

            //Offsets
            o.WriteInt(0); //main
            o.WriteInt(0); //string
            o.WriteInt(0); //gpu
            o.WriteInt(0); //data
            o.WriteInt(0); //dataext
            o.WriteInt(0); //relocationtable

            //Length
            o.WriteInt(0); //main
            o.WriteInt(0); //string
            o.WriteInt(0); //gpu
            o.WriteInt(0); //data
            o.WriteInt(0); //dataext
            o.WriteInt(0); //relocationtable

            o.WriteInt(0); //datasection
            o.WriteInt(0); //

            o.WriteShort(1); //flag
            o.WriteShort(0); //addcount

            //Contents in the main header......
            
            d_Main.WriteInt(0); // Model
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Material
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Shader
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Texture
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // MaterialLUT
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Lights
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Camera
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Fog
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            // SkeAnim
            {
                // Names need to be in patricia tree.......
                Dictionary<string, int> NameBank = new Dictionary<string, int>();

                NameBank.Add("BustN", d_String.Size());
                d_String.WriteString("BustN");
                d_String.WriteByte(0);

                List<PatriciaTree.PatriciaTreeNode> Nodes = new List<PatriciaTree.PatriciaTreeNode>();
                int maxlength = 0;
                foreach (Animation a in animations)
                    maxlength = Math.Max(maxlength, a.Text.Length);

                Nodes.Add(new PatriciaTree.PatriciaTreeNode() { ReferenceBit = uint.MaxValue });
                foreach (Animation a in animations)
                    PatriciaTree.Insert(Nodes, new PatriciaTree.PatriciaTreeNode() { Name = a.Text }, maxlength);

                int nameOff = 0xb4 + d_Main2.Size();
                foreach (PatriciaTree.PatriciaTreeNode node in Nodes)
                {
                    d_Main2.WriteInt((int)node.ReferenceBit);
                    d_Main2.WriteShort(node.LeftNodeIndex);
                    d_Main2.WriteShort(node.RightNodeIndex);
                    if (node.Name.Equals(""))
                    {
                        d_Main2.WriteInt(0);
                    }
                    else
                    {
                        NameBank.Add(node.Name, d_String.Size());
                        d_Main2.WriteOffset(d_String.Size(), d_String);
                        d_String.WriteString(node.Name);
                        d_String.WriteByte(0);
                    }
                }
                // bones
                

                // Okay, first create the animation data then create the table pointng to it side by side 

                int dataOff = 0xb4 + d_Main2.Size();
                foreach (Animation a in animations)
                {
                    d_Main2.WriteOffset(d_Main3.Size(), d_Main2);

                    // now create the actual animation data I guess
                    d_Main3.WriteOffset(NameBank[a.Text], d_String); // name offset
                    d_Main3.WriteInt(0x2); // Flags TODO: What are these
                    d_Main3.WriteFloat(a.frameCount + 1);
                    d_Main3.WriteOffset(d_Main3.Size() + 12, d_Main3); // bone offset
                    d_Main3.WriteInt(a.bones.Count); // bonecount
                    d_Main3.WriteInt(0); // metadata nonsense
                    
                    FileOutput boneHeader = new FileOutput();
                    boneHeader.endian = System.IO.Endianness.Little;
                    FileOutput keyData = new FileOutput();
                    keyData.endian = System.IO.Endianness.Little;
                    int start = d_Main3.Size() + (a.bones.Count * 4);
                    int track = 0;
                    foreach (Animation.KeyNode node in a.bones)
                    {
                        d_Main3.WriteOffset(start + boneHeader.Size(), d_Main3); // bone offset
                        // name type and flags
                        if (!NameBank.ContainsKey(node.Text))
                        {
                            NameBank.Add(node.Text, d_String.Size());
                            d_String.WriteString(node.Text);
                            d_String.WriteByte(0);
                        }
                        boneHeader.WriteOffset(NameBank[node.Text], d_String); // name offset
                        boneHeader.WriteInt(0x040000); // animation type flags, default is just simply transform
                        // Actual Flags
                        int flags = 0;
                        flags |= (((node.xsca.keys.Count > 0) ? 0 : 1) << (16 + 0)); 
                        flags |= (((node.ysca.keys.Count > 0) ? 0 : 1) << (16 + 1)); 
                        flags |= (((node.zsca.keys.Count > 0) ? 0 : 1) << (16 + 2));
                        flags |= (((node.xrot.keys.Count > 0) ? 0 : 1) << (16 + 3));
                        flags |= (((node.yrot.keys.Count > 0) ? 0 : 1) << (16 + 4));
                        flags |= (((node.zrot.keys.Count > 0) ? 0 : 1) << (16 + 5));
                        flags |= (((node.xpos.keys.Count > 0) ? 0 : 1) << (16 + 6));
                        flags |= (((node.ypos.keys.Count > 0) ? 0 : 1) << (16 + 7));
                        flags |= (((node.zpos.keys.Count > 0) ? 0 : 1) << (16 + 8));
                        
                        flags |= (((node.xsca.keys.Count == 1) ? 1 : 0) << (6 + 0));
                        flags |= (((node.ysca.keys.Count == 1) ? 1 : 0) << (6 + 1));
                        flags |= (((node.zsca.keys.Count == 1) ? 1 : 0) << (6 + 2));
                        flags |= (((node.xrot.keys.Count == 1) ? 1 : 0) << (6 + 3));
                        flags |= (((node.yrot.keys.Count == 1) ? 1 : 0) << (6 + 4));
                        flags |= (((node.zrot.keys.Count == 1) ? 1 : 0) << (6 + 5));
                        flags |= (((node.xpos.keys.Count == 1) ? 1 : 0) << (6 + 7));
                        flags |= (((node.ypos.keys.Count == 1) ? 1 : 0) << (6 + 8));
                        flags |= (((node.zpos.keys.Count == 1) ? 1 : 0) << (6 + 9));
                        boneHeader.WriteInt(flags);

                        // Create KeyFrame Data
                        int sta = start + (a.bones.Count * 12 * 4);
                        WriteKeyData(node.xsca, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.ysca, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.zsca, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.xrot, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.yrot, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.zrot, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.xpos, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.ypos, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.zpos, boneHeader, keyData, d_Main3, sta, ref track);
                    }
                    d_Main3.WriteOutput(boneHeader);
                    d_Main3.WriteOutput(keyData);

                }
                
                d_Main.WriteOffset(dataOff, d_Main);
                d_Main.WriteInt(animations.Count); //
                d_Main.WriteOffset(nameOff, d_Main); //
            }
            

            d_Main.WriteInt(0); // MaterialAnim
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // VisAnim
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // LightAnim
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // CameraAnim
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // FogAnim
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteInt(0); // Scene
            d_Main.WriteInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);
            d_Main2.WriteInt(0);

            d_Main.WriteOutput(d_Main2);
            d_Main.WriteOutput(d_Main3);

            int headSize = o.Size();
            o.WriteIntAt(headSize, 0x08);
            o.WriteIntAt(d_Main.Size(), 0x20);
            o.WriteOutput(d_Main);
            o.Align(4);

            int stringSize = o.Size();
            o.WriteIntAt(stringSize, 0x0C);
            o.WriteIntAt(d_String.Size(), 0x24);
            o.WriteOutput(d_String);
            o.Align(4);

            int gpuSize = o.Size();
            o.WriteIntAt(d_GPU.Size() > 0 ? gpuSize : 0, 0x10);
            o.WriteIntAt(d_GPU.Size(), 0x28);
            o.WriteOutput(d_GPU);
            o.Align(0x100);

            int dataSize = o.Size();
            o.WriteIntAt(dataSize, 0x14);
            o.WriteIntAt(dataSize, 0x18);
            o.WriteIntAt(d_Data.Size(), 0x2C);
            o.WriteIntAt(d_Data.Size(), 0x30);
            o.WriteOutput(d_Data);

            //Create Relocation Table
            // Flag is 7 bits
            // 0 - main 1 - string 2 - gpu 3 - data
            foreach (FileOutput.RelocOffset off in o.offsets)
            {
                int size = 0;
                int code = 0;
                int div = 4;
                if(off.output == d_Main || off.output == d_Main2 || off.output == d_Main3)
                {
                    size = headSize;
                    code = 0;
                    if (off.output == d_Main3)
                        off.value += headSize;
                    if (off.output == d_Main2)
                        off.value += d_Main2.Size()+headSize;
                }
                if (off.output == d_String)
                {
                    size = stringSize;
                    code = 1;
                    div = 1;
                }
                if (off.output == d_GPU)
                {
                    size = gpuSize;
                    code = 2;
                }
                if (off.output == d_Data)
                {
                    size = dataSize;
                    code = 3;
                }

                o.WriteIntAt(off.value - size, off.position);
                int reloc = (code << 25) | (((off.position - headSize) / div) &0x1FFFFFF);
                Reloc.WriteInt(reloc);
            }

            int relocSize = o.Size();
            o.WriteIntAt(relocSize, 0x1C);
            o.WriteIntAt(Reloc.Size(), 0x34);
            o.WriteOutput(Reloc);

            o.Save(fname);
        }

        public static void WriteKeyData(Animation.KeyGroup group, FileOutput boneHeader, FileOutput keyData, FileOutput d_Main3, int start, ref int track)
        {
            if (group.keys.Count == 1)
                boneHeader.WriteFloat(group.keys[0].Value);
            else
            if (group.keys.Count == 0)
                boneHeader.WriteInt(0);
            else
            {
                int off = (group.keys.Count * 4);
                boneHeader.WriteOffset(start + keyData.Size(), d_Main3); // bone offset

                keyData.WriteFloat(0);
                keyData.WriteFloat(group.FrameCount);
                keyData.WriteInt(track++ << 16); // track
                keyData.WriteInt((group.keys.Count << 16) | 0x0701); // 7 is quantinization and 1 is linear interpolation

                float minv = 999, maxv = -999;
                float minf = 999, maxf = -999;
                foreach (Animation.KeyFrame key in group.keys)
                {
                    minv = Math.Min(key.Value, minv);
                    maxv = Math.Max(key.Value, maxv);
                    minf = Math.Min(key.Frame, minf);
                    maxf = Math.Max(key.Frame, maxf);
                }
                maxv -= minv;
                keyData.WriteFloat(maxv / 0xFFFFF); // value scale
                keyData.WriteFloat(minv); // value offset
                keyData.WriteFloat(1f); // frame scale
                keyData.WriteFloat(minf); // frame offset

                keyData.WriteOffset(start + keyData.Size() + 4, d_Main3); // useless flags

                foreach (Animation.KeyFrame key in group.keys)
                {
                    keyData.WriteInt((((int)(((key.Value - minv) / (maxv)) * 0xFFFFF)) << 12) | (((int)(key.Frame - minf)) & 0xFFF));
                    
                }
            }
            //------
            
        }
    }
}
