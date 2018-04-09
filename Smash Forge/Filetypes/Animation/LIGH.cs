using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge.LIGH
{
    public class LighBin : FileBase
    {
        public LighBin(string filename) : this()
        {
            Read(filename);
        }
        public LighBin()
        {
            version = 5;
            frameCount = 0;
            frameDuration = 1;
            lightFrames = new List<LightFrame>();
            rgbProperties = new List<RgbProperty>();
            for (int i = 0; i < 5; i++)
                rgbProperties.Add(new RgbProperty());
        }
        public int version;
        public int frameCount;
        public int frameDuration;
        public List<LightFrame> lightFrames;
        public List<RgbProperty> rgbProperties; //There are 5 of these; in order: fighter fresnel (sky), fighter fresnel (ground), fighter ambient (sky), fighter ambient (ground), reflection color

        public override Endianness Endian { get; set; }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);

            //Header
            f.skip(0x4);
            version = f.readInt();
            frameCount = f.readInt();
            if (version == 4)
                frameDuration = 1;
            else if (version == 5)
                frameDuration = f.readInt();
            else
                throw new NotImplementedException($"Unknown light.bin version {version}");

            int[] offsets = new int[6];
            for (int i = 0; i < 6; i++)
                offsets[i] = f.readInt();

            //RGB properties
            for (int i = 0; i < 5; i++)
            {
                rgbProperties[i].enabled = (offsets[i+1] != 0);
                if (rgbProperties[i].enabled)
                {
                    f.seek(offsets[i+1]);
                    for (int j = 0; j < frameCount; j++)
                    {
                        byte[] frame = new byte[3];
                        for (int k = 0; k < 3; k++)
                            frame[k] = (byte)f.readByte();
                        rgbProperties[i].frames.Add(frame);
                    }
                }
            }

            //Light data
            f.seek(offsets[0]);
            for (int i = 0; i < frameCount; i++)
            {
                LightFrame temp = new LightFrame();

                for (int j = 0; j < 17; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        temp.lightSets[j].lights[k].enabled = f.readInt();
                        for (int l = 0; l < 3; l++)
                            temp.lightSets[j].lights[k].angle[l] = f.readFloat();
                        temp.lightSets[j].lights[k].colorHue = f.readFloat();
                        temp.lightSets[j].lights[k].colorSat = f.readFloat();
                        temp.lightSets[j].lights[k].colorVal = f.readFloat();
                    }
                    temp.lightSets[j].fog.unknown = (byte)f.readByte();
                    for (int k = 0; k < 3; k++)
                        temp.lightSets[j].fog.color[k] = (byte)f.readByte();
                }
                temp.effect.unknown = (byte)f.readByte();
                for (int j = 0; j < 3; j++)
                    temp.effect.color[j] = (byte)f.readByte();
                for (int j = 0; j < 3; j++)
                    temp.effect.position[j] = f.readFloat();

                lightFrames.Add(temp);
            }

        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeHex("4C494748"); //LIGH
            f.writeInt(version);
            f.writeInt(frameCount);
            if (version == 5)
                f.writeInt(frameDuration);

            //Offsets
            int padding = 0;
            while ( (((frameCount*3) + padding)%4) != 0 ) //All offsets must be multiples of 4 or the game won't use the file properly
                padding++;

            int[] offsets = new int[6];
            int currOff = 0x24;
            if (version == 4)
                currOff = 0x24;
            else if (version == 5)
                currOff = 0x28;

            for (int i = 0; i < 5; i++)
            {
                offsets[i+1] = (rgbProperties[i].enabled) ? currOff : 0x0;
                if (rgbProperties[i].enabled)
                    currOff += (frameCount*3) + padding;
            }
            offsets[0] = currOff;

            for (int i = 0; i < 6; i++)
                f.writeInt(offsets[i]);

            //RGB properties
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < frameCount; j++)
                    for (int k = 0; k < 3; k++)
                        f.writeByte(rgbProperties[i].frames[j][k]);

                for (int j = 0; j < padding; j++)
                    f.writeByte(0);
            }

            //Light data
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        f.writeInt(lightFrames[i].lightSets[j].lights[k].enabled);
                        for (int l = 0; l < 3; l++)
                            f.writeFloat(lightFrames[i].lightSets[j].lights[k].angle[l]);
                        f.writeFloat(lightFrames[i].lightSets[j].lights[k].colorHue);
                        f.writeFloat(lightFrames[i].lightSets[j].lights[k].colorSat);
                        f.writeFloat(lightFrames[i].lightSets[j].lights[k].colorVal);
                    }
                    f.writeByte(lightFrames[i].lightSets[j].fog.unknown);
                    for (int k = 0; k < 3; k++)
                        f.writeByte(lightFrames[i].lightSets[j].fog.color[k]);
                }
                f.writeByte(lightFrames[i].effect.unknown);
                for (int j = 0; j < 3; j++)
                    f.writeByte(lightFrames[i].effect.color[j]);
                for (int j = 0; j < 3; j++)
                    f.writeFloat(lightFrames[i].effect.position[j]);
            }

            return f.getBytes();
        }
    }

    public class RgbProperty
    {
        public bool enabled;
        public List<byte[]> frames;

        public RgbProperty()
        {
            enabled = false;
            frames = new List<byte[]>();
        }
    }

    public class LightFrame
    {
        public List<LightSet> lightSets;
        public Effect effect;

        public LightFrame()
        {
            lightSets = new List<LightSet>();
            for (int i = 0; i < 17; i++)
                lightSets.Add(new LightSet());
            effect = new Effect();
        }
    }
    public class LightSet
    {
        public List<Light> lights;
        public Fog fog;

        public LightSet()
        {
            lights = new List<Light>();
            for (int i = 0; i < 4; i++)
                lights.Add(new Light());
            fog = new Fog();
        }
    }
    public class Light
    {
        public int enabled;
        public Vector3 angle;
        public float colorHue, colorSat, colorVal;

        public Light()
        {
            enabled = 0;
            angle = new Vector3();
            colorHue = 0;
            colorSat = 0;
            colorVal = 0;
        }
    }
    public class Fog
    {
        public byte unknown;
        public byte[] color;

        public Fog()
        {
            unknown = 0;
            color = new byte[3];
        }
    }
    public class Effect
    {
        public byte unknown;
        public byte[] color;
        public Vector3 position;

        public Effect()
        {
            unknown = 0;
            color = new byte[3];
            position = new Vector3();
        }
    }
}