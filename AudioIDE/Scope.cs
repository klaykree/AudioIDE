using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AudioIDE
{
    public class Scope
    {
        public class ScopePoint
        {
            public ScopePoint(Point point, bool EndPoint)
            {
                this.point = point;
                this.EndPoint = EndPoint;
            }

            public ScopePoint(Point point)
            {
                this.point = point;
                EndPoint = false;
            }

            public Point point;
            public bool EndPoint;
        }

        private List<ScopePoint> Points_;
        private double MaxWidth_;
        private double MaxHeight_;

        public const double PointDistance_ = 65.0;

        public Scope(double MaxWidth, double MaxHeight)
        {
            Points_ = new List<ScopePoint>();
            MaxWidth_ = MaxWidth;
            MaxHeight_ = MaxHeight;

            //Leftmost point
            Points_.Add(new ScopePoint(new Point(0, MaxHeight_ / 2.0)));

            //Point near first point to make a small starting straight line
            //Points_.Add(new Point(PointDistance_, MaxHeight_ / 2.0));

            Points_.Add(new ScopePoint(new Point(PointDistance_, MaxHeight_ / 2.0)));
            Points_.Add(new ScopePoint(new Point(MaxWidth_, MaxHeight_ / 2.0)));
        }

        public double YPointPosition(short Y)
        {
            //Convert param 'Y' into a position usable for the visual scope
            double OldRange = short.MaxValue - short.MinValue;
            double NewRange = MaxHeight_;
            double YInRange = (((Y - short.MinValue) * NewRange) / OldRange);
            YInRange = MaxHeight_ - YInRange;

            return YInRange;
        }

        private double XPointPosition()
        {
            //Calculate X position of the new point, will be 'PointDistance_' to the right of the last point
            return (Points_.Count - 2) * PointDistance_; //Minus two to point count to disregard the flattening points at the end
        }

        public void AddPoint(short Y, bool InstructionEnd)
        {
            //Insert before the first end flattening points
            Points_.Insert(Points_.Count - 2, new ScopePoint(new Point(XPointPosition(), YPointPosition(Y)), InstructionEnd));

            //Move the first end flatten point to the right to make room for the new point
            Point FlattenPoint = Points_[Points_.Count - 2].point;
            FlattenPoint.X += PointDistance_;
            Points_[Points_.Count - 2].point = FlattenPoint;
        }

        public void PositionPoints(List<short> Ys)
        {
            for(int i = 0 ; i < Ys.Count ; ++i)
            {
                //Minus 3 to ignore the last 2 control points, plus 1 to ignore the first control point
                int PointIndex = Points_.Count - 3 - Ys.Count + i + 1;

                Point point = Points_[PointIndex].point;
                point.Y = YPointPosition(Ys[i]);
                Points_[PointIndex].point = point;
            }
        }

        public void RemoveLastPoint()
        {
            if(Points_.Count <= 3)
                return;

            Points_.RemoveAt(Points_.Count - 3);

            Point SecondLast = Points_[Points_.Count - 2].point;
            SecondLast.X -= PointDistance_;
            Points_[Points_.Count - 2].point = SecondLast;
        }

        public void Resize(double NewMaxWidth, double NewMaxHeight)
        {
            int InitalPointSize = Points_.Count;
            double WidthRatio = NewMaxWidth / MaxWidth_;
            double HeightRatio = NewMaxHeight / MaxHeight_;

            Points_.Add(new ScopePoint(new Point(0, NewMaxHeight / 2.0)));

            for(int i = 0 ; i < InitalPointSize - 3 ; ++i)
            {
                Points_.Add(new ScopePoint(new Point(Points_[i + 1].point.X, Points_[i + 1].point.Y * HeightRatio), Points_[i + 1].EndPoint));
            }
            
            Points_.RemoveRange(0, InitalPointSize);

            Points_.Add(new ScopePoint(new Point(PointDistance_ * Points_.Count, NewMaxHeight / 2.0)));
            Points_.Add(new ScopePoint(new Point(MaxWidth_, NewMaxHeight / 2.0)));

            MaxWidth_ = NewMaxWidth;
            MaxHeight_ = NewMaxHeight;
        }

        public List<ScopePoint> Points()
        {
            return new List<ScopePoint>(Points_);
        }
    }
}
