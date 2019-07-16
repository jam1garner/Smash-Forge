using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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
            o.align(4);
            o.writeByte(0x21); // version stuffs
            o.writeByte(0x21); // version stuffs
            o.Endian = System.IO.Endianness.Little;
            o.writeShort(0xA755); // version

            FileOutput d_Main = new FileOutput();
            d_Main.Endian = System.IO.Endianness.Little;
            FileOutput d_Main2 = new FileOutput();
            d_Main2.Endian = System.IO.Endianness.Little;
            FileOutput d_Main3 = new FileOutput();
            d_Main3.Endian = System.IO.Endianness.Little;
            FileOutput d_String = new FileOutput();
            d_String.Endian = System.IO.Endianness.Little;
            FileOutput d_GPU = new FileOutput();
            d_GPU.Endian = System.IO.Endianness.Little;
            FileOutput d_Data = new FileOutput();
            d_Data.Endian = System.IO.Endianness.Little;

            FileOutput Reloc = new FileOutput();
            Reloc.Endian = System.IO.Endianness.Little;

            //Offsets
            o.writeInt(0); //main
            o.writeInt(0); //string
            o.writeInt(0); //gpu
            o.writeInt(0); //data
            o.writeInt(0); //dataext
            o.writeInt(0); //relocationtable

            //Length
            o.writeInt(0); //main
            o.writeInt(0); //string
            o.writeInt(0); //gpu
            o.writeInt(0); //data
            o.writeInt(0); //dataext
            o.writeInt(0); //relocationtable

            o.writeInt(0); //datasection
            o.writeInt(0); //

            o.writeShort(1); //flag
            o.writeShort(0); //addcount

            //Contents in the main header......
            
            d_Main.writeInt(0); // Model
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Material
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Shader
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Texture
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // MaterialLUT
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Lights
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Camera
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Fog
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            // SkeAnim
            {
                // Names need to be in patricia tree.......
                Dictionary<string, int> NameBank = new Dictionary<string, int>();

                NameBank.Add("BustN", d_String.Size());
                d_String.WriteString("BustN");
                d_String.writeByte(0);

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
                    d_Main2.writeInt((int)node.ReferenceBit);
                    d_Main2.writeShort(node.LeftNodeIndex);
                    d_Main2.writeShort(node.RightNodeIndex);
                    if (node.Name.Equals(""))
                    {
                        d_Main2.writeInt(0);
                    }
                    else
                    {
                        NameBank.Add(node.Name, d_String.Size());
                        d_Main2.WriteOffset(d_String.Size(), d_String);
                        d_String.WriteString(node.Name);
                        d_String.writeByte(0);
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
                    d_Main3.writeInt(0x2); // Flags TODO: What are these
                    d_Main3.writeFloat(a.FrameCount + 1);
                    d_Main3.WriteOffset(d_Main3.Size() + 12, d_Main3); // bone offset
                    d_Main3.writeInt(a.Bones.Count); // bonecount
                    d_Main3.writeInt(0); // metadata nonsense
                    
                    FileOutput boneHeader = new FileOutput();
                    boneHeader.Endian = System.IO.Endianness.Little;
                    FileOutput keyData = new FileOutput();
                    keyData.Endian = System.IO.Endianness.Little;
                    int start = d_Main3.Size() + (a.Bones.Count * 4);
                    int track = 0;
                    foreach (Animation.KeyNode node in a.Bones)
                    {
                        d_Main3.WriteOffset(start + boneHeader.Size(), d_Main3); // bone offset
                        // name type and flags
                        if (!NameBank.ContainsKey(node.Text))
                        {
                            NameBank.Add(node.Text, d_String.Size());
                            d_String.WriteString(node.Text);
                            d_String.writeByte(0);
                        }
                        boneHeader.WriteOffset(NameBank[node.Text], d_String); // name offset
                        boneHeader.writeInt(0x040000); // animation type flags, default is just simply transform
                        // Actual Flags
                        int flags = 0;
                        flags |= (((node.XSCA.Keys.Count > 0) ? 0 : 1) << (16 + 0)); 
                        flags |= (((node.YSCA.Keys.Count > 0) ? 0 : 1) << (16 + 1)); 
                        flags |= (((node.ZSCA.Keys.Count > 0) ? 0 : 1) << (16 + 2));
                        flags |= (((node.XROT.Keys.Count > 0) ? 0 : 1) << (16 + 3));
                        flags |= (((node.YROT.Keys.Count > 0) ? 0 : 1) << (16 + 4));
                        flags |= (((node.ZROT.Keys.Count > 0) ? 0 : 1) << (16 + 5));
                        flags |= (((node.XPOS.Keys.Count > 0) ? 0 : 1) << (16 + 6));
                        flags |= (((node.YPOS.Keys.Count > 0) ? 0 : 1) << (16 + 7));
                        flags |= (((node.ZPOS.Keys.Count > 0) ? 0 : 1) << (16 + 8));
                        
                        flags |= (((node.XSCA.Keys.Count == 1) ? 1 : 0) << (6 + 0));
                        flags |= (((node.YSCA.Keys.Count == 1) ? 1 : 0) << (6 + 1));
                        flags |= (((node.ZSCA.Keys.Count == 1) ? 1 : 0) << (6 + 2));
                        flags |= (((node.XROT.Keys.Count == 1) ? 1 : 0) << (6 + 3));
                        flags |= (((node.YROT.Keys.Count == 1) ? 1 : 0) << (6 + 4));
                        flags |= (((node.ZROT.Keys.Count == 1) ? 1 : 0) << (6 + 5));
                        flags |= (((node.XPOS.Keys.Count == 1) ? 1 : 0) << (6 + 7));
                        flags |= (((node.YPOS.Keys.Count == 1) ? 1 : 0) << (6 + 8));
                        flags |= (((node.ZPOS.Keys.Count == 1) ? 1 : 0) << (6 + 9));
                        boneHeader.writeInt(flags);

                        // Create KeyFrame Data
                        int sta = start + (a.Bones.Count * 12 * 4);
                        WriteKeyData(node.XSCA, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.YSCA, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.ZSCA, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.XROT, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.YROT, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.ZROT, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.XPOS, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.YPOS, boneHeader, keyData, d_Main3, sta, ref track);
                        WriteKeyData(node.ZPOS, boneHeader, keyData, d_Main3, sta, ref track);
                    }
                    d_Main3.WriteOutput(boneHeader);
                    d_Main3.WriteOutput(keyData);

                }
                
                d_Main.WriteOffset(dataOff, d_Main);
                d_Main.writeInt(animations.Count); //
                d_Main.WriteOffset(nameOff, d_Main); //
            }
            

            d_Main.writeInt(0); // MaterialAnim
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // VisAnim
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // LightAnim
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // CameraAnim
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // FogAnim
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.writeInt(0); // Scene
            d_Main.writeInt(0); //
            d_Main.WriteOffset(0xB4 + d_Main2.Size(), d_Main); //

            d_Main2.writeInt(0);
            d_Main2.writeInt(0);
            d_Main2.writeInt(0);

            d_Main.WriteOutput(d_Main2);
            d_Main.WriteOutput(d_Main3);

            int headSize = o.Size();
            o.writeIntAt(headSize, 0x08);
            o.writeIntAt(d_Main.Size(), 0x20);
            o.WriteOutput(d_Main);
            o.align(4);

            int stringSize = o.Size();
            o.writeIntAt(stringSize, 0x0C);
            o.writeIntAt(d_String.Size(), 0x24);
            o.WriteOutput(d_String);
            o.align(4);

            int gpuSize = o.Size();
            o.writeIntAt(d_GPU.Size() > 0 ? gpuSize : 0, 0x10);
            o.writeIntAt(d_GPU.Size(), 0x28);
            o.WriteOutput(d_GPU);
            o.align(0x100);

            int dataSize = o.Size();
            o.writeIntAt(dataSize, 0x14);
            o.writeIntAt(dataSize, 0x18);
            o.writeIntAt(d_Data.Size(), 0x2C);
            o.writeIntAt(d_Data.Size(), 0x30);
            o.WriteOutput(d_Data);

            //Create Relocation Table
            // Flag is 7 bits
            // 0 - main 1 - string 2 - gpu 3 - data
            foreach (FileOutput.RelocOffset off in o.Offsets)
            {
                int size = 0;
                int code = 0;
                int div = 4;
                if(off.output == d_Main || off.output == d_Main2 || off.output == d_Main3)
                {
                    size = headSize;
                    code = 0;
                    if (off.output == d_Main3)
                        off.Value += headSize;
                    if (off.output == d_Main2)
                        off.Value += d_Main2.Size()+headSize;
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

                o.writeIntAt(off.Value - size, off.Position);
                int reloc = (code << 25) | (((off.Position - headSize) / div) &0x1FFFFFF);
                Reloc.writeInt(reloc);
            }

            int relocSize = o.Size();
            o.writeIntAt(relocSize, 0x1C);
            o.writeIntAt(Reloc.Size(), 0x34);
            o.WriteOutput(Reloc);

            o.save(fname);
        }

        public static void WriteKeyData(Animation.KeyGroup group, FileOutput boneHeader, FileOutput keyData, FileOutput d_Main3, int start, ref int track)
        {
            if (group.Keys.Count == 1)
                boneHeader.writeFloat(group.Keys[0].Value);
            else
            if (group.Keys.Count == 0)
                boneHeader.writeInt(0);
            else
            {
                int off = (group.Keys.Count * 4);
                boneHeader.WriteOffset(start + keyData.Size(), d_Main3); // bone offset

                keyData.writeFloat(0);
                keyData.writeFloat(group.FrameCount);
                keyData.writeInt(track++ << 16); // track
                keyData.writeInt((group.Keys.Count << 16) | 0x0701); // 7 is quantinization and 1 is linear interpolation

                float minv = 999, maxv = -999;
                float minf = 999, maxf = -999;
                foreach (Animation.KeyFrame key in group.Keys)
                {
                    minv = Math.Min(key.Value, minv);
                    maxv = Math.Max(key.Value, maxv);
                    minf = Math.Min(key.Frame, minf);
                    maxf = Math.Max(key.Frame, maxf);
                }
                maxv -= minv;
                keyData.writeFloat(maxv / 0xFFFFF); // value scale
                keyData.writeFloat(minv); // value offset
                keyData.writeFloat(1f); // frame scale
                keyData.writeFloat(minf); // frame offset

                keyData.WriteOffset(start + keyData.Size() + 4, d_Main3); // useless flags

                foreach (Animation.KeyFrame key in group.Keys)
                {
                    keyData.writeInt((((int)(((key.Value - minv) / (maxv)) * 0xFFFFF)) << 12) | (((int)(key.Frame - minf)) & 0xFFF));
                    
                }
            }
            //------
            
        }
    }
}
