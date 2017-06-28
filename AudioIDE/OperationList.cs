using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AudioIDE
{
    public class OperationList : DragableList
    {
        public OperationList() : base()
        {
            ListViewItem ImmediateItem = new ListViewItem();
            ImmediateItem.Content = "[Constant]";
            AddChild(ImmediateItem);

            Type_ = EOperand.Operation;
        }

        protected override void ListMouseMove(object sender, MouseEventArgs e)
        {
            base.ListMouseMove(sender, e);
        }

        protected override short ItemValue(string Item)
        {
            return 0;
        }

        protected override EOperand ItemType(string Item)
        {
            if(Item == "[Constant]")
                return EOperand.Immediate;

            return Type_;
        }
    }
}
