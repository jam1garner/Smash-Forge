using System;
using System.Windows.Forms;
using System.IO;

namespace SmashForge
{
    class DRP : TreeNode
    {

        public DRP(string fname)
        {
            Text = Path.GetFileNameWithoutExtension(fname);
            var o = new FileOutput();
            o.WriteBytes(Decrypt(new FileData(fname)));
            o.Save(fname + "_dec");
            Read(new FileData(Decrypt(new FileData(fname))));
        }

        public void Read(FileData d)
        {
            d.endian = Endianness.Big;

            d.Seek(0x16);
            int count = d.ReadUShort();

            d.Seek(0x60);

            for(int i = 0; i < count; i++)
            {
                string name = d.ReadString(d.Pos(), -1);
                d.Skip(0x40);
                int unk = d.ReadInt();
                int nextFile = d.ReadInt();
                int c2 = d.ReadUShort();
                int c1 = d.ReadUShort();
                d.Skip(4); // padding?

                int[] partsizes = new int[4];
                for (int j = 0; j < 4; j++)
                    partsizes[j] = d.ReadInt();

                TreeNode part = new TreeNode();
                part.Text = name;
                Nodes.Add(part);

                int off = 0;
                for (int j = 0; j < c1; j++)
                {
                    TreeNode t = new TreeNode();
                    part.Nodes.Add(t);
                    int decompressedSize = d.ReadInt();
                    byte[] dat = FileData.InflateZlib(d.GetSection(d.Pos(), partsizes[j] - 4 - off));
                    d.Skip(partsizes[j] - 4);
                    off += partsizes[j];
                    string mag = new FileData(dat).Magic();
                    t.Text = name + "." + mag;

                    if(mag.Equals("NTWD"))
                        Runtime.textureContainers.Add(new NUT(new FileData(dat)));

                    if (mag.Equals("OMO "))
                    {
                        Runtime.Animations.Add(t.Text, OMOOld.read(new FileData(dat)));
                        MainForm.Instance.animList.treeView1.Nodes.Add(t.Text);
                    }
                    
                }
            }
        }

        private byte[] Decrypt(FileData d)
        {
            int[] filedata = null;
            filedata = new int[d.Size() / 4];
            int size = (int)d.Size();
            int words = size >> 2;
            int xorval = 0;
            d.Seek(0x1C);
            d.endian = Endianness.Big;
            int SEED = d.ReadInt();
            RandomXS _rand = new RandomXS(SEED);
            d.endian = Endianness.Big;
            d.Seek(0);
            if (size % 8 == 0) goto Dword_loop;
            else if (size == 0) return null;
            else if (size / 8 <= 0) goto BYTE_loop;
            else;//goto default_loop

            #region DWORD Loop
            Dword_loop:
            if (words == 0)
                goto loops_end;
            else if (words / 8 <= 0)
                goto Word_Loop;
            else
                for (int i = 0; i < (words >> 3); i++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        var randInt = _rand.GetInt();
                        var XOR = d.ReadInt() ^ randInt;
                        var val = XOR ^ xorval;
                        xorval = (randInt << 13) & unchecked((int)0x80000000);
                        filedata[x + (i * 8)] = val.Reverse();
                    }
                }
            #endregion
            #region Word Loop
            Word_Loop:
            if ((words & 7) <= 0)
                goto loops_end;
            int offset = (size >> 2 >> 3 << 3 << 2);
            d.Seek(offset);

            for (int i = 0; i < (words & 7); i++)
            {
                var randInt = _rand.GetInt();
                var XOR = d.ReadInt() ^ randInt;
                var val = XOR ^ xorval;
                xorval = (randInt << 13) & unchecked((int)0x80000000);
                filedata[offset / 4 + i] = val.Reverse();
            }
            goto loops_end;
        #endregion
        #region BYTE Loop
        BYTE_loop:
            if ((size & 7) == 0)
                goto func_end;
            d.Seek(4);
            for (int i = 4; i < (size & 7); i++)
            {
                byte[] data = BitConverter.GetBytes(filedata[i.RoundDown(4)]);
                var b = d.ReadByte();
                var shifted = (b >> 3) | (b << 32 - 3);
                var val = b ^ shifted;
                data[i] = (byte)val;
            }
        #endregion
        loops_end:
            filedata[7] = SEED.Reverse();
        func_end:
            byte[] result = new byte[filedata.Length * sizeof(int)];
            Buffer.BlockCopy(filedata, 0, result, 0, result.Length);
            return result;
        }

        private void Encrypt(byte[] b)
        {

        }
        
        public class RandomXS
        {
            public RandomXS(int seed)
            {
                int init = 0x41C64E6D;
                _data = new int[4];
                _data[0] = (seed * init) + 0x3039;
                _data[1] = (_data[0] * init) + 0x3039;
                _data[2] = (_data[1] * init) + 0x3039;
                _data[3] = (_data[2] * init) + 0x3039;
            }
            private int[] _data;

            public int GetInt()
            {
                int last = _data[3];
                int first = _data[0];
                int third = _data[2];
                int second = _data[1];

                int XOR1 = last ^ (last << 11);
                int XOR2 = first ^ (first >> 0x13);
                int XOR3 = XOR1 ^ (XOR1 >> 8);
                int FINAL_XOR = XOR2 ^ XOR3;

                _data[1] = first;
                _data[3] = third;
                _data[2] = second;
                _data[0] = FINAL_XOR;
                return FINAL_XOR & 0x7FFFFFFF;
            }
        }
    }
}
