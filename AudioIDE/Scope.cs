using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioIDE
{
    public class Scope
    {
        private List<Point> Points_;
        private double MaxWidth_;
        private double MaxHeight_;

        public const double PointDistance_ = 60.0;

        public Scope(double MaxWidth, double MaxHeight)
        {
            Points_ = new List<Point>();
            MaxWidth_ = MaxWidth;
            MaxHeight_ = MaxHeight;

            //Leftmost point
            Points_.Add(new Point(0, MaxHeight_ / 2.0));

            //Point near first point to make a small starting straight line
            //Points_.Add(new Point(PointDistance_, MaxHeight_ / 2.0));

            Points_.Add(new Point(PointDistance_, MaxHeight_ / 2.0));
            Points_.Add(new Point(MaxWidth_, MaxHeight_ / 2.0));
        }

        private double YPointPosition(short Y)
        {
            //Convert param 'Y' into a position usable for the visual scope
            double OldRange = short.MaxValue - short.MinValue;
            double NewRange = MaxHeight_;
            double YInRange = (double)(((Y - short.MinValue) * NewRange) / OldRange);
            YInRange = MaxHeight_ - YInRange;

            return YInRange;
        }

        private double XPointPosition()
        {
            //Calculate X position of the new point, will be 'PointDistance_' to the right of the last point
            return (Points_.Count - 2) * PointDistance_; //Minus two to point count to disregard the flattening points at the end
        }

        public void AddPoint(short Y)
        {
            Points_.Insert(Points_.Count - 2, new Point(XPointPosition(), YPointPosition(Y))); //Insert before the first end flattening point

            //Move the first end flatten point to the right to make room for the new point
            Point FlattenPoint = Points_[Points_.Count - 2];
            FlattenPoint.X += PointDistance_;
            Points_[Points_.Count - 2] = FlattenPoint;
        }

        public void PositionPoints(List<short> Ys)
        {
            for(int i = 0 ; i < Ys.Count ; ++i)
            {
                Point point = Points_[Points_.Count - 3 - i];
                point.Y = YPointPosition(Ys[i]);
                Points_[Points_.Count - 3 - i] = point;
            }
        }

        public void RemoveLastPoint()
        {
            if(Points_.Count <= 3)
                return;

            Points_.RemoveAt(Points_.Count - 3);
        }

        public List<Point> Resize(double NewMaxWidth, double NewMaxHeight)
        {
            int InitalPointSize = Points_.Count;
            double WidthRatio = NewMaxWidth / MaxWidth_;
            double HeightRatio = NewMaxHeight / MaxHeight_;

            Points_.Add(new Point(0, NewMaxHeight / 2.0));
            //Points_.Add(new Point(PointDistance_, NewMaxHeight / 2.0));

            for(int i = 0 ; i < InitalPointSize - 3 ; ++i)
            {
                Points_.Add(new Point(Points_[i + 1].X, Points_[i + 1].Y * HeightRatio));
            }
            
            Points_.RemoveRange(0, InitalPointSize);

            Points_.Add(new Point(PointDistance_ * Points_.Count, NewMaxHeight / 2.0));
            Points_.Add(new Point(MaxWidth_, NewMaxHeight / 2.0));

            MaxWidth_ = NewMaxWidth;
            MaxHeight_ = NewMaxHeight;

            return new List<Point>(Points_);
        }

        public List<Point> Points()
        {
            return new List<Point>(Points_);
        }
    }
}
