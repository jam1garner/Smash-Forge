using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace Smash_Forge
{
    class DRP : TreeNode
    {

        public DRP(string fname)
        {
            Text = Path.GetFileNameWithoutExtension(fname);
            var o = new FileOutput();
            o.writeBytes(Decrypt(new FileData(fname)));
            o.save(fname + "_dec");
            Read(new FileData(Decrypt(new FileData(fname))));
        }

        public void Read(FileData d)
        {
            d.Endian = Endianness.Big;

            d.seek(0x16);
            int count = d.readShort();

            d.seek(0x60);

            for(int i = 0; i < count; i++)
            {
                string name = d.readString(d.pos(), -1);
                d.skip(0x40);
                int unk = d.readInt();
                int nextFile = d.readInt();
                int c2 = d.readShort();
                int c1 = d.readShort();
                d.skip(4); // padding?

                int[] partsizes = new int[4];
                for (int j = 0; j < 4; j++)
                    partsizes[j] = d.readInt();

                TreeNode part = new TreeNode();
                part.Text = name;
                Nodes.Add(part);

                int off = 0;
                for (int j = 0; j < c1; j++)
                {
                    TreeNode t = new TreeNode();
                    part.Nodes.Add(t);
                    int decompressedSize = d.readInt();
                    byte[] dat = FileData.InflateZLIB(d.getSection(d.pos(), partsizes[j] - 4 - off));
                    d.skip(partsizes[j] - 4);
                    off += partsizes[j];
                    string mag = new FileData(dat).Magic();
                    t.Text = name + "." + mag;

                    if(mag.Equals("NTWD"))
                        Runtime.TextureContainers.Add(new NUT(new FileData(dat)));

                    if (mag.Equals("OMO "))
                    {
                        Runtime.Animations.Add(t.Text, OMOOld.read(new FileData(dat)));
                        MainForm.animNode.Nodes.Add(t.Text);
                    }
                    
                }
            }
        }

        private byte[] Decrypt(FileData d)
        {
            int[] filedata = null;
            filedata = new int[d.size() / 4];
            int size = (int)d.size();
            int words = size >> 2;
            int xorval = 0;
            d.seek(0x1C);
            d.Endian = Endianness.Big;
            int SEED = d.readInt();
            RandomXS _rand = new RandomXS(SEED);
            d.Endian = Endianness.Big;
            d.seek(0);
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
                        var XOR = d.readInt() ^ randInt;
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
            d.seek(offset);

            for (int i = 0; i < (words & 7); i++)
            {
                var randInt = _rand.GetInt();
                var XOR = d.readInt() ^ randInt;
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
            d.seek(4);
            for (int i = 4; i < (size & 7); i++)
            {
                byte[] data = BitConverter.GetBytes(filedata[i.RoundDown(4)]);
                var b = d.readByte();
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
