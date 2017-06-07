using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class FormBase : Form
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            /*this.BackColor = System.Drawing.Color.Black;
            this.ForeColor = System.Drawing.Color.Wheat;

            foreach (Control c in Controls)
            {
                c.BackColor = System.Drawing.Color.Black;
                c.ForeColor = System.Drawing.Color.Wheat;
            }*/
        }

    }
}
