using System;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
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
