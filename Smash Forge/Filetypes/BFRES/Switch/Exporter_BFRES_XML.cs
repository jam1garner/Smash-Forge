using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syroot.NintenTools.Bfres;
using Newtonsoft.Json;
using System.IO;

namespace Smash_Forge
{
    public partial class BFRES
    {
        public static void WriteFMATXML(MaterialData mat, Mesh msh)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "JSON|*.json|All files(*.*)|*.*";
            sfd.FileName = mat.Name;

       

         

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter file = new StreamWriter(sfd.FileName);
                using (JsonWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();
                    writer.WritePropertyName("Material");
                    writer.WriteValue(mat.Name);
                    writer.WritePropertyName("TextureArray");
                    writer.WriteStartArray();
                    int s = 0;
                    foreach (var tex in mat.textures)
                    {
                        writer.WritePropertyName("TextureArray");
                        writer.WriteValue(mat.Samplers.Keys.ElementAt(s));
                        s++;
                    }
                    writer.WriteEnd();
                    writer.WriteEndObject();

                }



                /*    writer.WriteStartElement("SamplerArray");
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
                        writer.WriteAttributeString("Type", rnd.Type.ToString());

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
                        writer.WriteAttributeString("Index", p.ToString());

                        switch (param.Type)
                        {
                            case ShaderParamType.Float:
                                writer.WriteAttributeString(prm.Key, param.Value_float.ToString());
                                break;
                            case ShaderParamType.Float2:
                                writer.WriteAttributeString(prm.Key, param.Value_float2.ToString());
                                break;
                            case ShaderParamType.Float3:
                                writer.WriteAttributeString(prm.Key, param.Value_float3.ToString());
                                break;
                            case ShaderParamType.Float4:
                                writer.WriteAttributeString(prm.Key, param.Value_float4.ToString());
                                break;
                            case ShaderParamType.TexSrt:
                                writer.WriteAttributeString(prm.Key, param.Value_TexSrt.scale.ToString() +
                                    " " + param.Value_TexSrt.rotate.ToString() +
                                    " " + param.Value_TexSrt.translate.ToString());
                                break;
                            case ShaderParamType.Bool:
                                writer.WriteAttributeString(prm.Key, param.Value_bool.ToString());
                                break;
                        }
                        writer.WriteAttributeString("Type", param.Type.ToString());

                        writer.WriteEndElement();
                        p++;
                    }
                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                    writer.Close();*/
            }
        }
    }
}
