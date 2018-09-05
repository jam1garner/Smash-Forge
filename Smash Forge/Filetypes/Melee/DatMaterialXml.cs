using MeleeLib.DAT;
using System.Xml;

namespace Smash_Forge.Filetypes.Melee
{
    public static class DatMaterialXml
    {

        public static XmlDocument CreateMaterialXml(MeleeRootNode rootNode)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode mainNode = doc.CreateElement("DAT");
            doc.AppendChild(mainNode);

            DatDOBJ[] dOBJs = rootNode.Root.GetDataObjects();
            for (int i = 0; i < dOBJs.Length; i++)
            {
                DatDOBJ d = dOBJs[i];

                string name = $"DataObject{ i }";
                XmlNode dobjNode = doc.CreateElement(name);
                AppendAttributes(doc, dobjNode, d);

                XmlNode texturesListNode = doc.CreateElement("Textures");
                foreach (var tex in d.Material.Textures)
                {
                    XmlNode texNode = doc.CreateElement("Texture");
                    AppendTexAttributes(doc, tex, texNode);
                    texturesListNode.AppendChild(texNode);
                }
                dobjNode.AppendChild(texturesListNode);

                mainNode.AppendChild(dobjNode);
            }

            return doc;
        }

        private static void AppendTexAttributes(XmlDocument doc, DatTexture tex, XmlNode texNode)
        {
            AppendXmlAttribute(doc, texNode, "Flags", tex.UnkFlags.ToString("X"));
            AppendXmlAttribute(doc, texNode, "WrapS", tex.WrapS.ToString());
            AppendXmlAttribute(doc, texNode, "WrapT", tex.WrapT.ToString());
            AppendXmlAttribute(doc, texNode, "MagFilter", tex.MagFilter.ToString());
        }

        private static void AppendXmlAttribute(XmlDocument doc, XmlNode node, string name, string value)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            node.Attributes.Append(attribute);
        }

        private static void AppendAttributes(XmlDocument doc, XmlNode dobjNode, DatDOBJ d)
        {
            XmlAttribute flagsAttribute = doc.CreateAttribute("Flags");
            flagsAttribute.Value = d.Material.Flags.ToString("X");
            dobjNode.Attributes.Append(flagsAttribute);
        }
    }
}
