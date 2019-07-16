using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;
using SALT.PARAMS;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace SmashForge
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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Edited)
            {
                var window = MessageBox.Show(
                    "Are you sure you want to close this window?\nYou have unsaved changes that will be lost.",
                    "Close",
                    MessageBoxButtons.YesNo);

                e.Cancel = (window == DialogResult.No);
            }
        }
    }
}
