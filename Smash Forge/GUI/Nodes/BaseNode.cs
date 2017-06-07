using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class BaseNode : TreeNode
    {
        public static ContextMenuStrip _menu;
        static BaseNode()
        {
            _menu = new ContextMenuStrip();
        }
        public BaseNode()
        {
            this.ContextMenuStrip = _menu;
        }

        public virtual void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {

        }

    }
}