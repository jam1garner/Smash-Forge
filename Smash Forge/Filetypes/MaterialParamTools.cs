using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge.Params
{
    public class MatParam
    {
        public string name = "";

        public string generalDescription = "";
        public string param1Description = "";
        public string param2Description = "";
        public string param3Description = "";
        public string param4Description = "";

        public string[] paramLabels = new string[4];

        // Users can still manually enter a value higher than max.
        public float[] maxValues = new float[] { 10, 10, 10, 10 };

        public bool useTrackBar = true;
    }

    public class MaterialParamTools
    {
        public static Dictionary<string, MatParam> GetMatParamsFromFile()
        {
            Dictionary<string, MatParam> propList = new Dictionary<string, MatParam>();
            if (File.Exists("param_labels\\material_params.ini"))
            {
                try
                {
                    MatParam matParam = new MatParam();
                    using (StreamReader sr = new StreamReader("param_labels\\material_params.ini"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string[] args = sr.ReadLine().Split('=');
                            string line = args[0];
                            switch (line)
                            {
                                case "[Param]":
                                    if (!matParam.name.Equals("") && !propList.ContainsKey(matParam.name))
                                        propList.Add(matParam.name, matParam); matParam = new MatParam();
                                    break;
                                case "name":
                                    matParam.name = args[1]; Console.WriteLine(matParam.name);
                                    break;
                                case "description":
                                    matParam.generalDescription = args[1];
                                    break;
                                case "descriptionX":
                                    matParam.param1Description = args[1];
                                    break;
                                case "descriptionY":
                                    matParam.param2Description = args[1];
                                    break;
                                case "descriptionZ":
                                    matParam.param3Description = args[1];
                                    break;
                                case "descriptionW":
                                    matParam.param4Description = args[1];
                                    break;
                                case "param1":
                                    matParam.paramLabels[0] = args[1];
                                    break;
                                case "param2":
                                    matParam.paramLabels[1] = args[1];
                                    break;
                                case "param3":
                                    matParam.paramLabels[2] = args[1];
                                    break;
                                case "param4":
                                    matParam.paramLabels[3] = args[1];
                                    break;
                                case "max1":
                                    float.TryParse(args[1], out matParam.maxValues[0]);
                                    break;
                                case "max2":
                                    float.TryParse(args[1], out matParam.maxValues[1]); break;
                                case "max3":
                                    float.TryParse(args[1], out matParam.maxValues[2]);
                                    break;
                                case "max4": float.TryParse(args[1], 
                                    out matParam.maxValues[3]);
                                    break;
                                case "useTrackBar":
                                    bool.TryParse(args[1], out matParam.useTrackBar);
                                    break;
                            }
                        }
                    }
                    if (!matParam.name.Equals("") && !propList.ContainsKey(matParam.name)) propList.Add(matParam.name, matParam);
                }
                catch (Exception)
                {
                    // Who knows what actually went wrong. Pros don't need labels anyway...
                }
            }

            return propList;
        }

    }
}
