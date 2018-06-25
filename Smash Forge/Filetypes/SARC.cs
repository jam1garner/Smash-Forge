using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Syroot.BinaryData;

/* exelix's/MasterF0X's SARC library. Packing is not yet supported. */

namespace Smash_Forge
{
    class SARC
    {
        static uint NameHash(string name)
        {
            uint result = 0;
            for (int i = 0; i < name.Length; i++)
            {
                result = name[i] + result * 0x00000065;
            }
            return result;
        }

        public static byte[] pack(Dictionary<string, byte[]> files)
        {
            MemoryStream o = new MemoryStream();
            BinaryDataWriter bw = new BinaryDataWriter(o, false);
            bw.ByteOrder = ByteOrder.LittleEndian;
            bw.Write("SARC", BinaryStringFormat.NoPrefixOrTermination);
            bw.Write((UInt16)0x14); // Chunk length
            bw.Write((UInt16)0xFEFF); // BOM
            bw.Write((UInt32)0x00); //filesize update later
            bw.Write((UInt32)0x00); //Beginning of data
            bw.Write((UInt32)0x01000000);
            bw.Write("SFAT", BinaryStringFormat.NoPrefixOrTermination);
            bw.Write((UInt16)0xc);
            bw.Write((UInt16)files.Keys.Count);
            bw.Write((UInt32)0x00000065);
            List<uint> offsetToUpdate = new List<uint>();
            foreach (string k in files.Keys)
            {
                bw.Write(NameHash(k));
                bw.Write((byte)0x1);
                bw.Write((byte)0); //this should be part of the name index, but u16 will work too
                offsetToUpdate.Add((uint)bw.BaseStream.Position);
                bw.Write((UInt16)0);
                bw.Write((UInt32)0);
                bw.Write((UInt32)0);
            }
            bw.Write("SFNT", BinaryStringFormat.NoPrefixOrTermination);
            bw.Write((UInt16)0x8);
            bw.Write((UInt16)0);
            List<uint> StringOffsets = new List<uint>();
            foreach (string k in files.Keys)
            {
                StringOffsets.Add((uint)bw.BaseStream.Position);
                bw.Write(k, BinaryStringFormat.ZeroTerminated);
                bw.Align(4);
            }
            List<uint> FileOffsets = new List<uint>();
            foreach (string k in files.Keys)
            {
                FileOffsets.Add((uint)bw.BaseStream.Position);
                bw.Write(files[k]);
                bw.Align(4);
            }
            for (int i = 0; i < offsetToUpdate.Count; i++)
            {
                bw.BaseStream.Position = offsetToUpdate[i];
                bw.Write((UInt16)((StringOffsets[i] - StringOffsets[0]) / 4));
                bw.Write((UInt32)(FileOffsets[i] - FileOffsets[0]));
                bw.Write((UInt32)(FileOffsets[i] + files.Values.ToArray()[i].Length - FileOffsets[0]));
            }
            bw.BaseStream.Position = 0x08;
            bw.Write((uint)bw.BaseStream.Length);
            bw.Write((uint)FileOffsets[0]);
            return o.ToArray();
        }

        public Dictionary<string, byte[]> unpackRam(byte[] src)
        {
            return unpackRam(new MemoryStream(src));
        }

        public Dictionary<string, byte[]> unpackRam(Stream src)
        {
            Dictionary<string, byte[]> res = new Dictionary<string, byte[]>();
            BinaryDataReader br = new BinaryDataReader(src, false);
            br.BaseStream.Position = 0;
            br.ByteOrder = ByteOrder.LittleEndian;
            br.ReadUInt32(); // Header
            br.ReadUInt16(); // Chunk length
            br.ReadUInt16(); // BOM
            br.ReadUInt32(); // File size
            UInt32 startingOff = br.ReadUInt32();
            br.ReadUInt32(); // Unknown;
            SFAT sfat = new SFAT();
            sfat.parse(br, (int)br.BaseStream.Position);
            SFNT sfnt = new SFNT();
            sfnt.parse(br, (int)br.BaseStream.Position, sfat, (int)startingOff);

            for (int m = 0; m < sfat.nodeCount; m++)
            {
                br.Seek(sfat.nodes[m].nodeOffset + startingOff, 0);
                byte[] temp;
                if (m == 0)
                {
                    temp = br.ReadBytes((int)sfat.nodes[m].EON);
                }
                else
                {
                    int tempInt = (int)sfat.nodes[m].EON - (int)sfat.nodes[m].nodeOffset;
                    temp = br.ReadBytes(tempInt);
                }
                res.Add(sfnt.fileNames[m], temp);
            }
            new SARC();
            return res;
        }

        public void unpack(string file)
        {
            try
            {
                Stream src = new MemoryStream(File.ReadAllBytes(file + ".sarc"));
                var files = unpackRam(src);
                write(files.Keys.ToList(), files.Values.ToList(), file);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void readFiles(string dir, List<string> flist, List<byte[]> fdata)
        {
            processDirectory(dir, flist, fdata);
        }

        public void processDirectory(string targetDirectory, List<string> flist, List<byte[]> fdata)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                processFile(fileName, fdata);
                char[] sep = { '\\' };
                string[] fn = fileName.Split(sep);
                string tempf = "";
                for (int i = 1; i < fn.Length; i++)
                {
                    tempf += fn[i];
                    if (fn.Length > 2 && (i != fn.Length - 1))
                    {
                        tempf += "/";
                    }
                }
                flist.Add(tempf);
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                processDirectory(subdirectory, flist, fdata);
        }

        public void processFile(string path, List<byte[]> fdata)
        {
            byte[] temp = File.ReadAllBytes(path);
            fdata.Add(temp);
        }

        public void write(List<string> fileNames, List<byte[]> files, string file)
        {
            Directory.CreateDirectory(file);

            for (int s = 0; s < fileNames.Count; s++)
            {
                if (fileNames[s].Contains("/"))
                {
                    char[] sep = { '/' };
                    string[] p = fileNames[s].Split(sep);
                    string fullDir = file + "/";
                    for (int r = 0; r < p.Length - 1; r++)
                    {
                        fullDir += p[r] + "/";
                        Directory.CreateDirectory(fullDir);
                    }
                }
                FileStream fs = File.Create(file + "\\" + fileNames[s]);
                fs.Write(files[s], 0, files[s].Length);
                fs.Close();
            }
        }

        public class SFAT
        {
            public List<Node> nodes = new List<Node>();

            public UInt16 chunkSize;
            public UInt16 nodeCount;
            public UInt32 hashMultiplier;

            public struct Node
            {
                public UInt32 hash;
                public byte fileBool;
                public byte unknown1;
                public UInt16 fileNameOffset;
                public UInt32 nodeOffset;
                public UInt32 EON;
            }

            public void parse(BinaryDataReader br, int pos)
            {
                br.ReadUInt32(); // Header;
                chunkSize = br.ReadUInt16();
                nodeCount = br.ReadUInt16();
                hashMultiplier = br.ReadUInt32();
                for (int i = 0; i < nodeCount; i++)
                {
                    Node node;
                    node.hash = br.ReadUInt32();
                    node.fileBool = br.ReadByte();
                    node.unknown1 = br.ReadByte();
                    node.fileNameOffset = br.ReadUInt16();
                    node.nodeOffset = br.ReadUInt32();
                    node.EON = br.ReadUInt32();
                    nodes.Add(node);
                }
            }

        }

        public class SFNT
        {
            public List<string> fileNames = new List<string>();

            public UInt32 chunkID;
            public UInt16 chunkSize;
            public UInt16 unknown1;

            public void parse(BinaryDataReader br, int pos, SFAT sfat, int start)
            {
                chunkID = br.ReadUInt32();
                chunkSize = br.ReadUInt16();
                unknown1 = br.ReadUInt16();

                char[] temp = br.ReadChars(start - (int)br.BaseStream.Position);
                string temp2 = new string(temp);
                char[] splitter = { (char)0x00 };
                string[] names = temp2.Split(splitter);
                for (int j = 0; j < names.Length; j++)
                {
                    if (names[j] != "")
                    {
                        fileNames.Add(names[j]);
                    }
                }
            }
        }
    }
}
