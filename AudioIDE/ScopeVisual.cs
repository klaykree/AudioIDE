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
        public static ConsoleOutput ConsoleOut;

        public ScopeVisual()
        {
            Children_ = new VisualCollection(this);
            Instructions_ = new Instructions();

            SizeChanged += Resize;

            PreviewDrop += DropItem;
            PreviewDragOver += DragItem;
            PreviewDragEnter += DragEnterItem;
        }

        public void Init()
        {
            Scope_ = new Scope(ActualWidth, ActualHeight);

            ColorRect BlackScopeBackground = new ColorRect(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));

            Draw();

            Grid FirstGridParent = Parent as Grid;
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

            Width = ActualWidth;
        }

        private void AddPoint(short InstructionValue)
        {
            Scope_.AddPoint(InstructionValue);
        }

        private void Resize(Object sender, RoutedEventArgs e)
        {
            if(Scope_ == null)
                return;

            Scope_.Resize(ActualWidth, ActualHeight);

            Draw();
        }

        private void Draw()
        {
            if(Scope_ == null)
                return;

            Children_.Clear();

            List<Point> points = Scope_.Points();

            DrawingVisual BlackBackground = new DrawingVisual();
            Children_.Add(BlackBackground);
            using(DrawingContext dc = BlackBackground.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));
            }

            for(int i = 0 ; i < points.Count - 1 ; ++i)
            {
                DrawingVisual visual = new DrawingVisual();
                Children_.Add(visual);

                Pen LinePen = new Pen(Brushes.LawnGreen, 1.15);
                using(DrawingContext dc = visual.RenderOpen())
                {
                    Point FirstPoint = points[i];
                    Point SecondPoint = points[i + 1];
                    dc.DrawLine(LinePen, FirstPoint, SecondPoint);
                }
            }
        }

        private void FinishInstruction()
        {
            Scope_.PositionPoints(Instructions_.LastInstruction().OperandValues().ToList());

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

            double WidthMultiplyer = PointCount;
            Instruction LastInst = Instructions_.LastInstruction();
            if(LastInst != null)
                WidthMultiplyer += LastInst.OperandCount() + 1;

            return new Thickness(WidthMultiplyer * Scope.PointDistance_, HalfHeight, 0, 0);
        }

        private Label CreateDragDropLabel(string Text)
        {
            Label OPLabel = CreateLabel(Text);

            //Move the text label by changing its margin
            Thickness Margin = LastPointPosition();
            Margin.Top -= (OPLabel.DesiredSize.Height / 2.0);
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
        private void TextBoxEnterKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
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
            Label DisplayLabel = CreateDragDropLabel(ImmediateValue.ToString());

            short LowWord = (short)(ImmediateValue & 0xFF);
            short HighWord = (short)(ImmediateValue >> 8);

            Operand ImmediateLowWord = new Operand("[Constant]", EOperand.Immediate, LowWord, DisplayLabel);
            AddOperand(ImmediateLowWord);

            Operand ImmediateHighWord = new Operand("[Constant]", EOperand.Immediate, HighWord, DisplayLabel);
            AddOperand(ImmediateHighWord);
        }

        private void AddOperand(Operand Op)
        {
            bool InstructionCompleted = Instructions_.AddOp(Op.OP, Op.Type, Op.Value, Op.DisplayLabel);

            Scope_.AddPoint(Op.Value);

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

        private void DragEnterItem(object sender, DragEventArgs e)
        {
            Operand operand = (Operand)e.Data.GetData(typeof(Operand));

            if(operand.Type == EOperand.Immediate)
            {
                TextBox test = new TextBox();
                test.HorizontalAlignment = HorizontalAlignment.Left;
                test.VerticalAlignment = VerticalAlignment.Top;
                //test.VerticalContentAlignment = VerticalAlignment.Center;
                //test.Width = 60;
                //test.Height = 22;
                Thickness Margin = LastPointPosition();
                Margin.Top -= (test.FontSize / 2.0);
                test.Margin = Margin;
                
                Grid FirstGridParent = Parent as Grid;
                FirstGridParent.Children.Add(test);

                test.PreviewTextInput += PreviewEnterText;
                test.KeyUp += TextBoxEnterKeyUp;

                test.Focus();
            }
            else
            {
                Label DisplayLabel = CreateDragDropLabel(operand.OP);
                operand.DisplayLabel = DisplayLabel;
                
                AddOperand(operand);
            }
        }
        
        private void DragItem(object sender, DragEventArgs e)
        {
        }

        private void DropItem(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(Operand)))
            {
                Operand Op = (Operand)e.Data.GetData(typeof(Operand));
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
    }
}
