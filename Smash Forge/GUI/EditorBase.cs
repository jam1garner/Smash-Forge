using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;
using SALT.PARAMS;
using System.IO;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class EditorBase : DockContent
    {
        public bool Edited
        {
            get
            {
                return _edited;
            }
            set
            {
                _edited = value;
                if (!_edited)
                    Text = Path.GetFileName(FilePath);
                else
                    Text = Path.GetFileName(FilePath) + "*";
            }
        }
        private bool _edited = false;

        public string FilePath { get; set; }

        public virtual void Save()
        {
            throw new NotImplementedException();
        }

        public virtual void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}
