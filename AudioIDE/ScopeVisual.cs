using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioIDE
{
    public class ScopeVisual : FrameworkElement
    {
        private struct ColorRect
        {
            public ColorRect(Brush brush, Pen pen, Rect rect)
            {
                this.brush = brush;
                this.pen = pen;
                this.rect = rect;
            }

            public Brush brush;
            public Pen pen;
            public Rect rect;
        }

        private VisualCollection Children_;
        private Scope Scope_;
        private Instructions Instructions_;
        public static ConsoleOutput ConsoleOut = null;

        public ScopeVisual()
        {
            Children_ = new VisualCollection(this);
            Instructions_ = new Instructions();
        }

        public void Init()
        {
            Grid FirstGridParent = Parent as Grid;

            FirstGridParent.SizeChanged += Resize;
            FirstGridParent.PreviewDrop += DropItem;
            FirstGridParent.PreviewDragOver += DragItem;
            FirstGridParent.PreviewDragEnter += DragEnterItem;

            MinWidth = FirstGridParent.ActualWidth;
            Width = FirstGridParent.ActualWidth;
            MinHeight = FirstGridParent.ActualHeight;
            Height = FirstGridParent.ActualHeight;

            Scope_ = new Scope(FirstGridParent.ActualWidth, FirstGridParent.ActualHeight);
            
            Draw();

            ScrollViewer Scroll = FirstGridParent.Parent as ScrollViewer;
            Grid GridParent = Scroll.Parent as Grid;

            //Find and set the console out from the main grid
            foreach(UIElement Element in GridParent.Children)
            {
                ConsoleOutput ConsoleChild = Element as ConsoleOutput;
                if(ConsoleChild != null)
                {
                    ConsoleOut = ConsoleChild;
                    break;
                }
            }
        }

        private void Resize(Object sender, RoutedEventArgs e)
        {
            if(Scope_ == null)
                return;

            Grid FirstGridParent = Parent as Grid;
            Scope_.Resize(FirstGridParent.ActualWidth, FirstGridParent.ActualHeight);
            
            Draw();
        }

        private void Draw()
        {
            if(Scope_ == null)
                return;

            Children_.Clear();

            List<Scope.ScopePoint> points = Scope_.Points();

            /*DrawingVisual BlackBackground = new DrawingVisual();
            Children_.Add(BlackBackground);
            using(DrawingContext dc = BlackBackground.RenderOpen())
            {
                Grid FirstGridParent = Parent as Grid;
                dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, FirstGridParent.ActualWidth, FirstGridParent.ActualHeight));
            }*/

            for(int i = 0 ; i < points.Count - 1 ; ++i)
            {
                DrawingVisual visual = new DrawingVisual();
                Children_.Add(visual);

                Pen LinePen;
                if(i == 0 || i == points.Count - 1 || i == points.Count - 2) //Control points are blue
                    LinePen = new Pen(Brushes.RoyalBlue, 1.15);
                else
                    LinePen = new Pen(Brushes.LawnGreen, 1.15);

                using(DrawingContext dc = visual.RenderOpen())
                {
                    Point FirstPoint = points[i].point;

                    Point SecondPoint = points[i + 1].point;
                    dc.DrawLine(LinePen, FirstPoint, SecondPoint);

                    if(points[i].EndPoint)
                    {
                        FirstPoint.X = SecondPoint.X;
                        FirstPoint.Y = 0;
                        SecondPoint.Y = ActualHeight;
                        dc.DrawLine(new Pen(Brushes.LightSlateGray, 0.5), FirstPoint, SecondPoint);
                    }
                }
            }
        }

        private void FinishInstruction()
        {
            Instruction Inst = Instructions_.LastInstruction();
            List<short> Ys = Inst.OperandValues().ToList();
            Scope_.PositionPoints(Ys);

            Label[] Labels = Inst.DisplayLabels();
            for(int i = 0 ; i < Inst.OperandCount() + 1; ++i)
            {
                double LabelHeight = Scope_.YPointPosition(Ys[i]);
                Thickness Margin = Labels[i].Margin;
                Margin.Top = LabelHeight;
                Labels[i].Margin = Margin;
            }

            Draw();
        }

        public List<short> InstructionsValues()
        {
            return Instructions_.InstructionsValuesWithEnd();
        }

        private Label CreateLabel(string Text)
        {
            Label TextLabel = new Label();
            TextLabel.Content = Text;
            TextLabel.Foreground = Brushes.Black;
            TextLabel.Background = Brushes.Gray;
            TextLabel.HorizontalAlignment = HorizontalAlignment.Left;
            TextLabel.VerticalAlignment = VerticalAlignment.Top;
            TextLabel.AllowDrop = false;
            TextLabel.IsHitTestVisible = false;
            TextLabel.Opacity = 0.91;

            TextLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));

            return TextLabel;
        }

        private Thickness LastPointPosition()
        {
            //Minus 2 to discount the two points at the end
            int PointCount = Scope_.Points().Count - 2;

            double HalfHeight = ActualHeight / 2;
            
            return new Thickness(PointCount * Scope.PointDistance_, HalfHeight, 0, 0);
        }

        private Label CreateDragDropLabel(String Text, short Value)
        {
            Label OPLabel = CreateLabel(Text);

            //Move the text label by changing its margin
            Thickness Margin = LastPointPosition();
            Margin.Top = Scope_.YPointPosition(Value);
            Margin.Top -= (OPLabel.DesiredSize.Height / 2.0);
            Margin.Left -= (OPLabel.DesiredSize.Width / 3.0);
            OPLabel.Margin = Margin;

            Grid FirstGridParent = Parent as Grid;
            FirstGridParent.Children.Add(OPLabel);
            
            return OPLabel;
        }

        //Event fires when text is entered to the textbox but before the text is 'accepted' by the text box
        private void PreviewEnterText(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if(e.Text.Length == 1 && e.Text[0] == '-') //Entered value is a hyphen
            {
                TextBox NumericTextBox = sender as TextBox;
                if(NumericTextBox.Text.Length == 0) //Numeric input box is empty
                    return; //Allow text enter to continue past preview
            }
            
            if(!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true; //Stop event here to prevent non digit char from going through
        }

        //Event fires on key up with the text box in focus
        private void TextBoxEnterKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBox NumericTextBox = sender as TextBox;

                if(NumericTextBox.Text.Length > 0)
                {
                    Grid FirstGridParent = Parent as Grid;
                    FirstGridParent.Children.Remove(NumericTextBox);
                    AddImmediate(int.Parse(NumericTextBox.Text));
                }
            }
        }

        private void AddImmediate(int ImmediateValue)
        {
            short LowWord = (short)(ImmediateValue & 0xFFFF);
            short HighWord = (short)(ImmediateValue >> 16);

            Label DisplayLabelLow = CreateDragDropLabel(ImmediateValue.ToString() + "\n(" + LowWord + ")", LowWord);
            
            Operand ImmediateLowWord = new Operand("[Constant]", EOperand.Immediate, LowWord, DisplayLabelLow);
            AddOperand(ImmediateLowWord);

            Label DisplayLabelHigh = CreateDragDropLabel(ImmediateValue.ToString() + "\n(" + HighWord + ")", HighWord);

            Operand ImmediateHighWord = new Operand("[Constant]", EOperand.Immediate, HighWord, DisplayLabelHigh);
            AddOperand(ImmediateHighWord);
        }

        private void AddOperand(Operand Op)
        {
            bool InstructionCompleted = Instructions_.AddOp(Op.OP, Op.Type, Op.Value, Op.DisplayLabel);

            Scope_.AddPoint(Op.Value, InstructionCompleted);

            if(InstructionCompleted)
            {
                FinishInstruction();
            }
        }

        public void RemoveLastOperand()
        {
            if(Instructions_.InstCount() == 0)
                return;

            Label RemoveLabel = Instructions_.RemoveLastOperand();

            Grid FirstGridParent = Parent as Grid;
            FirstGridParent.Children.Remove(RemoveLabel);

            Scope_.RemoveLastPoint();
            Draw();
        }

        public void DragOperand(Operand operand)
        {
            if(operand.Type == EOperand.Immediate)
            {
                TextBox ImmediateInput = new TextBox();
                ImmediateInput.HorizontalAlignment = HorizontalAlignment.Left;
                ImmediateInput.VerticalAlignment = VerticalAlignment.Top;
                //ImmediateInput.VerticalContentAlignment = VerticalAlignment.Center;
                //ImmediateInput.Width = 60;
                //ImmediateInput.Height = 22;
                Thickness Margin = LastPointPosition();
                Margin.Top -= (ImmediateInput.FontSize / 2.0);
                ImmediateInput.Margin = Margin;

                Grid FirstGridParent = Parent as Grid;
                FirstGridParent.Children.Add(ImmediateInput);

                ImmediateInput.PreviewTextInput += PreviewEnterText;
                ImmediateInput.KeyUp += TextBoxEnterKeyUp;

                ImmediateInput.Focus();
            }
            else
            {
                Label DisplayLabel = CreateDragDropLabel(operand.OP, operand.Value);
                operand.DisplayLabel = DisplayLabel;

                AddOperand(operand);
            }
        }

        private void DragEnterItem(object sender, DragEventArgs e)
        {
            //Operand operand = (Operand)e.Data.GetData(typeof(Operand));

            //DragOperand(operand);
        }
        
        private void DragItem(object sender, DragEventArgs e)
        {
        }

        private void DropItem(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(Operand)))
            {
                Operand Op = (Operand)e.Data.GetData(typeof(Operand));

                DragOperand(Op);
            }
        }
        
        protected override int VisualChildrenCount
        {
            get { return Children_.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if(index < 0 || index >= Children_.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Children_[index];
        }

        //DEBUG printing
        public void WriteDebug()
        {
        }
    }
}
