using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Drawing;

namespace Smash_Forge
{
    public class FormBase : DockContent
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            /*
            Queue<Control> cs = new Queue<Control>();
            cs.Enqueue(this);
            while(cs.Count > 0)
            {
                Control c = cs.Dequeue();
                foreach (Control cc in c.Controls)
                    cs.Enqueue(cc);
                c.BackColor = Color.Black;
                c.ForeColor = Color.Wheat;
                if(c is Button)
                {
                    c.BackColor = Color.AliceBlue;
                }
            }*/
        }

    }
}
