using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Syroot.NintenTools.Bfres;
using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace SmashForge
{
    public class BfresXML : BFRES
    {
        public static MaterialData SetMaterialToXML(string filename)
        {
            MaterialData mat = new MaterialData();

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNode main = doc.ChildNodes[1];

            foreach (XmlNode matnode in main.ChildNodes)
            {
                if (matnode.Name.Equals("SamplerArray"))
                {
                    foreach (XmlNode samp in matnode.ChildNodes)
                    {
                    }
                }
                if (matnode.Name.Equals("Sampler"))
                {

                }
                if (matnode.Name.Equals("RenderInfo"))
                {

                }
                if (matnode.Name.Equals("ShaderAssign"))
                {
                }
                if (matnode.Name.Equals("ShaderParam"))
                {
                    mat.matparam.Clear();

                    foreach (XmlNode param in matnode.ChildNodes)
                    {
                        ReadAttributes(param, mat);
                    }
                }
            }
            return mat;
        }
        private static void ReadAttributes(XmlNode materialNode, MaterialData material)
        {
            ShaderParam shaderParam = new ShaderParam();
            
            foreach (XmlAttribute attribute in materialNode.Attributes)
            {
                char[] RemoveThese = new char[] { ' ','(', ')'};
                string valueFix = string.Join("", attribute.Value.Split(RemoveThese));
                switch (attribute.Name)
                {
                    case "Name":
                        shaderParam.Name = attribute.Value;
                        break;
                    case "ValueFloat":
                        float.TryParse(attribute.Value, out shaderParam.Value_float);
                        break;
                    case "ValueFloat2":
                        float[] f2Array = Array.ConvertAll(valueFix.Split(','), float.Parse);
                        shaderParam.Value_float2 = new OpenTK.Vector2(f2Array[0], f2Array[1]);
                        break;
                    case "ValueFloat3":
                        float[] f3Array = Array.ConvertAll(valueFix.Split(','), float.Parse);
                        shaderParam.Value_float3 = new OpenTK.Vector3(f3Array[0], f3Array[1], f3Array[2]);
                        break;
                    case "ValueTexSrt":
                        float[] srt = Array.ConvertAll(valueFix.Split(','), float.Parse);
                      //  shaderParam.Value_TexSrt.scale = new OpenTK.Vector2(srt[0], srt[1]);
                     //   shaderParam.Value_TexSrt.rotate = srt[2];
                     //   shaderParam.Value_TexSrt.translate = new OpenTK.Vector2(srt[3], srt[4]);
                        break;
                }
            }

            if (shaderParam.Name != "")
                material.matparam.Add(shaderParam.Name, shaderParam);
        }
        public static void WriteMaterialXML(MaterialData mat, Mesh msh)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "XML|*.xml|All files(*.*)|*.*";
            sfd.FileName = mat.Name;

            //Todo. Redo this using XmlDocument and organise better
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                XmlWriter writer = XmlWriter.Create(sfd.FileName, settings);

                writer.WriteStartDocument();    
                writer.WriteStartElement("Materials");

                writer.WriteStartElement("SamplerArray");
                int s = 0;
                foreach (var tex in mat.textures)
                {
                    writer.WriteStartElement("Sampler");
                    writer.WriteAttributeString("Name", mat.Samplers.Keys.ElementAt(s));
                    writer.WriteAttributeString("Texture_Name", tex.Name);
                    writer.WriteAttributeString("Index", s.ToString());
                    writer.WriteStartElement("WrapMode");
                    writer.WriteAttributeString("U", wrapmode[mat.samplerinfo[s].WrapModeU].ToString());
                    writer.WriteAttributeString("V", wrapmode[mat.samplerinfo[s].WrapModeV].ToString());
                    writer.WriteAttributeString("W", wrapmode[mat.samplerinfo[s].WrapModeW].ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    s++;
                }
                writer.WriteEndElement();

                writer.WriteStartElement("Sampler");
                foreach (var smp in mat.Samplers)
                {
                    writer.WriteStartElement("SamplerInfo");
                    writer.WriteString(smp.Key);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("RenderInfo");

                int p = 0;
                foreach (var rnd in mat.renderinfo)
                {
                    writer.WriteStartElement("Info");
                    writer.WriteAttributeString("Index", p.ToString());

                    switch (rnd.Type)
                    {
                        case RenderInfoType.String:
                            writer.WriteAttributeString(rnd.Name, rnd.Value_String);
                            break;
                        case RenderInfoType.Int32:
                            writer.WriteAttributeString(rnd.Name, rnd.Value_Int.ToString());
                            break;
                        case RenderInfoType.Single:
                            writer.WriteAttributeString(rnd.Name, rnd.Value_Float.ToString());
                            break;
                    }
                    writer.WriteEndElement();
                    p++;
                }
                writer.WriteEndElement();

                writer.WriteStartElement("ShaderAssign");
                writer.WriteAttributeString("ShaderFile", mat.shaderassign.ShaderModel);
                p = 0;
                foreach (var op in mat.shaderassign.options)
                {
                    writer.WriteStartElement("Option");
                    writer.WriteAttributeString("Index", p.ToString());
                    writer.WriteAttributeString(op.Key, op.Value);
                    writer.WriteEndElement();
                    p++;
                }
                foreach (var smp in mat.shaderassign.samplers)
                {
                    writer.WriteStartElement("SamplersFragmentShader");
                    writer.WriteAttributeString(smp.Key, smp.Value);
                    writer.WriteEndElement();
                }
                foreach (var att in mat.shaderassign.attributes)
                {
                    writer.WriteStartElement("AttributesVertexShader");
                    writer.WriteAttributeString(att.Key, att.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                p = 0;
                writer.WriteStartElement("ShaderParam");
                foreach (var prm in mat.matparam)
                {
                    ShaderParam param = prm.Value;

                    writer.WriteStartElement("ShaderParam");
                    writer.WriteAttributeString("Name", param.Name);
                    string value = "Value" + param.Type.ToString();
                    switch (param.Type)
                    {
                        case ShaderParamType.UInt:
                            writer.WriteAttributeString(value, param.Value_UInt.ToString());
                            break;
                        case ShaderParamType.Float:
                            writer.WriteAttributeString(value, param.Value_float.ToString());
                            break;
                        case ShaderParamType.Float2:
                            writer.WriteAttributeString(value, param.Value_float2.ToString());
                            break;
                        case ShaderParamType.Float3:
                            writer.WriteAttributeString(value, param.Value_float3.ToString());
                            break;
                        case ShaderParamType.Float4:
                            writer.WriteAttributeString(value, param.Value_float4.ToString());
                            break;
                        case ShaderParamType.TexSrt:
                            writer.WriteAttributeString(value, param.Value_TexSrt.scale.ToString() +
                                ", (" + param.Value_TexSrt.rotate.ToString() +
                                "), " + param.Value_TexSrt.translate.ToString());
                            break;
                        case ShaderParamType.Bool:
                            writer.WriteAttributeString(value, param.Value_bool.ToString());
                            break;
                        case ShaderParamType.Float2x2:
                            writer.WriteAttributeString(value, FloatArrayToString(param.Value_float2x2));
                            break;
                        case ShaderParamType.Float2x3:
                            writer.WriteAttributeString(value, FloatArrayToString(param.Value_float2x3));
                            break;
                        case ShaderParamType.Float4x4:
                            writer.WriteAttributeString(value + param.Type.ToString(), FloatArrayToString(param.Value_float4x4));
                            break;
                        default:
                            MessageBox.Show("Cannot save undefined param type " + param.Type.ToString());
                            writer.WriteAttributeString(value, "Undefined");
                            break;
                    }
                    writer.WriteEndElement();
                    p++;
                }
                writer.WriteEndElement();

                writer.WriteEndDocument();
                writer.Close();
            }
        }
        public static string FloatArrayToString(float[] fa)
        {
            string result = String.Join(" ", fa.Select(f => f.ToString(CultureInfo.CurrentCulture)));
            return result;
        }
    }
}
