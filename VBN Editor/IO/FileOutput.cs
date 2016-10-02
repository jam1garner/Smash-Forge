using System;
using System.IO;
using System.Collections.Generic;

namespace VBN_Editor
{
	public class FileOutput
	{

		List<byte> data = new List<byte>();

		public bool littleEndian = false;

        public byte[] getBytes()
        {
            return data.ToArray();
        }

        public void writeString(String s){
			char[] c = s.ToCharArray();
			for(int i = 0; i < c.Length ; i++)
				data.Add((byte)c[i]);
		}

		public int size(){
			return data.Count;
		}

		public void writeOutput(FileOutput d){
			foreach(byte b in d.data)
				data.Add(b);
		}

		public void writeInt(int i){
			if(!littleEndian){
				data.Add((byte)((i)&0xFF));
				data.Add((byte)((i>>8)&0xFF));
				data.Add((byte)((i>>16)&0xFF));
				data.Add((byte)((i>>24)&0xFF));
			}else{
				data.Add((byte)((i>>24)&0xFF));
				data.Add((byte)((i>>16)&0xFF));
				data.Add((byte)((i>>8)&0xFF));
				data.Add((byte)((i)&0xFF));
			}
		}



		public void writeIntAt(int i, int p){
			if(!littleEndian){
				data[p++] = (byte)((i)&0xFF);
				data[p++] = (byte)((i>>8)&0xFF);
				data[p++] = (byte)((i>>16)&0xFF);
				data[p++] = (byte)((i>>24)&0xFF);
			}else{
				data[p++] = (byte)((i>>24)&0xFF);
				data[p++] = (byte)((i>>16)&0xFF);
				data[p++] = (byte)((i>>8)&0xFF);
				data[p++] =  (byte)((i)&0xFF);
			}
		}
		public void writeShortAt(int i, int p){
			if(!littleEndian){
				data[p++] =  (byte)((i)&0xFF);
				data[p++] = (byte)((i>>8)&0xFF);
			}else{
				data[p++] = (byte)((i>>8)&0xFF);
				data[p++] = (byte)((i)&0xFF);
			}
		}

		/*public void align(int i){
			while(data.size() % i != 0)
				writeByte(0);
		}

		public void align(int i, int value){
			while(data.size() % i != 0)
				writeByte(value);
		}*/


		public void writeFloat(float f){
			int i = SingleToInt32Bits (f, littleEndian);
				data.Add((byte)((i)&0xFF));
				data.Add((byte)((i>>8)&0xFF));
				data.Add((byte)((i>>16)&0xFF));
				data.Add((byte)((i>>24)&0xFF));
		}

		public static int SingleToInt32Bits(float value, bool littleEndian) {
			byte[] b = BitConverter.GetBytes (value);
			int p = 0;

			if (!littleEndian) {
				return (b [p++]&0xFF) | ((b [p++] & 0xFF) << 8) | ((b [p++] & 0xFF) << 16) | ((b [p++] & 0xFF) << 24);
			}else
				return ((b [p++] & 0xFF) << 24) | ((b [p++] & 0xFF) << 16) | ((b [p++] & 0xFF) << 8) | (b [p++]&0xFF);
		}

		public void writeHalfFloat(float f){
			int i = FileData.fromFloat(f, littleEndian);
			data.Add((byte)((i)&0xFF));
			data.Add((byte)((i>>8)&0xFF));
		}

		public void writeShort(int i){
			if(!littleEndian){
				data.Add((byte)((i)&0xFF));
				data.Add((byte)((i>>8)&0xFF));
			} else {
				data.Add((byte)((i>>8)&0xFF));
				data.Add((byte)((i)&0xFF));
			}
		}

		public void writeByte(int i){
			data.Add((byte)((i)&0xFF));
		}

		public void save(String fname){

			File.WriteAllBytes (fname, data.ToArray());
		}
	}
}

