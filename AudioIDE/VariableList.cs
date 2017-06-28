using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioIDE
{
    public class VariableList : DragableList
    {
        public VariableList() : base()
        {
            Type_ = EOperand.Variable;
            
            //for(short i = short.MinValue ; i <= short.MaxValue ; ++i)
            for(short i = 0 ; i < 200 ; ++i)
            {
                ListViewItem VariableItem = new ListViewItem();
                VariableItem.Content = i.ToString();
                AddChild(VariableItem);
            }
        }

        protected override void ListMouseMove(object sender, MouseEventArgs e)
        {
            base.ListMouseMove(sender, e);
        }

        protected override short ItemValue(string Item)
        {
            return short.Parse(Item);
        }

        protected override EOperand ItemType(string Item)
        {
            return Type_;
        }
    }
}
