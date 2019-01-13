using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LuteScribe.View
{
    //adapted from http://dirk.schuermans.me/?p=585
    //traverses the visual tree from some element
    //and returns all items on the tree
    class ChildControls
    {
        private List<object> _children;

        public List<object> GetChildren(Visual parent, int searchLevel)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("Element {0} is null!", parent.ToString());
            }

            this._children = new List<object>();

            this.GetChildControls(parent, searchLevel);

            return this._children;

        }

        private void GetChildControls(Visual parent, int searchLevel)
        {
            int nChildCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i <= nChildCount - 1; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);

                _children.Add((object)v);

                if (VisualTreeHelper.GetChildrenCount(v) > 0)
                {
                    GetChildControls(v, searchLevel + 1);
                }
            }
        }
    }
}
