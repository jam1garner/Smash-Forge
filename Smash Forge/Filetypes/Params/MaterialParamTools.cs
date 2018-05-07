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
        public string description = "";
        public string[] paramLabels = new string[4];

        // Users can still manually enter a value higher than max.
        public float max1 = 10.0f;
        public float max2 = 10.0f;
        public float max3 = 10.0f;
        public float max4 = 10.0f;

        public bool useTrackBar = true;
    }

    public class MaterialParamTools
    {
        // TODO: Store and read the information from the material_params.ini
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
                                    matParam.description = args[1];
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
                                    float.TryParse(args[1], out matParam.max1);
                                    break;
                                case "max2":
                                    float.TryParse(args[1], out matParam.max2); break;
                                case "max3":
                                    float.TryParse(args[1], out matParam.max3);
                                    break;
                                case "max4": float.TryParse(args[1], 
                                    out matParam.max4);
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
                }
            }

            return propList;
        }

    }
}
