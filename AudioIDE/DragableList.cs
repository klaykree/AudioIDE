using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace AudioIDE
{
    abstract public class DragableList : ListView
    {
        protected Point Start_;
        protected EOperand Type_;

        public DragableList()
        {
            MouseMove += ListMouseMove;
            MouseLeftButtonDown += ListLeftMouseButtonDown;
        }

        protected virtual void ListMouseMove(object sender, MouseEventArgs e)
        {
            Point MousePos = e.GetPosition(null);
            Vector FromMouseStart = MousePos - Start_;

            if(e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(FromMouseStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(FromMouseStart.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                ListViewItem Item = null;
                DependencyObject Current = (DependencyObject)e.OriginalSource;
                while(Current != null)
                {
                    if(Current is ListViewItem)
                    {
                        Item = (ListViewItem)Current;
                        break;
                    }

                    Current = VisualTreeHelper.GetParent(Current);
                }

                BeginDragDrop(Item);
            }
        }

        protected void ListLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            Start_ = e.GetPosition(null);
        }

        protected void BeginDragDrop(ListViewItem Item)
        {
            if (Item == null)
                return;

            Operand DragDropInst;
            string Op = (string)Item.Content;
            DragDropInst.OP = Op;
            DragDropInst.Type = ItemType(Op);
            DragDropInst.Value = ItemValue(Op);
            DragDropInst.DisplayLabel = null; //Label has not been made at this point

            //Begin drag drop for the ScopeVisual
            DataObject DragDropObj = new DataObject(DragDropInst);
            DragDrop.DoDragDrop(this, DragDropObj, DragDropEffects.Copy | DragDropEffects.Move);
        }

        protected abstract short ItemValue(string Item);
        protected abstract EOperand ItemType(string Item);
    }
}
