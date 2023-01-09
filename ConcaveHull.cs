using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleCodes
{
    public class ConcaveHull
    {
        public class Point
        {
            private readonly Double x;
            private readonly Double y;
            public Point(Double x, Double y)
            {
                this.x = x;
                this.y = y;
            }

            public virtual Double X()
            {
                return x;
            }

            public virtual Double Y()
            {
                return y;
            }

            public virtual string ToString()
            {
                return "(" + x + " " + y + ")";
            }

            public override bool Equals(object obj)
            {
                if (obj is Point)
                {
                    if (x.Equals(((Point)obj).X()) && y.Equals(((Point)obj).Y()))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {

                // http://stackoverflow.com/questions/22826326/good-hashcode-function-for-2d-coordinates
                // http://www.cs.upc.edu/~alvarez/calculabilitat/enumerabilitat.pdf
                int tmp = (int)(y + ((x + 1) / 2));
                return Math.Abs((int)(x + (tmp * tmp)));
            }
        }

        private Double EuclideanDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X() - b.X(), 2) + Math.Pow(a.Y() - b.Y(), 2));
        }

        private List<Point> KNearestNeighbors(List<Point> l, Point q, int k)
        {
            List<KeyValuePair<Double, Point>> nearestList = new List<KeyValuePair<double, Point>>();
            foreach (Point o in l)
            {
                nearestList.Add(new KeyValuePair<double, Point>(EuclideanDistance(q, o), o));
            }
            //someList.Sort((x, y) => x.Value.Length.CompareTo(y.Value.Length));
            nearestList.Sort((x, y) => x.Key.CompareTo(y.Key));
            
            List<Point> result = new List<Point>(0);
            for (int i = 0; i < Math.Min(k, nearestList.Count); i++)
            {
                result.Add(nearestList[i].Value);
            }

            return result;
        }

        

        private Point FindMinYPoint(List<Point> l)
        {
            double minY = l.Min(p => p.Y());
            Point minPoint = l.FirstOrDefault(p => p.Y() == minY);
            return minPoint;
        }
        private Double CalculateAngle(Point o1, Point o2)
        {
            return Math.Atan2(o2.Y() - o1.Y(), o2.X() - o1.X());
        }

        private Double AngleDifference(Double a1, Double a2)
        {

            // calculate angle difference in clockwise directions as radians
            if ((a1 > 0 && a2 >= 0) && a1 > a2)
            {
                return Math.Abs(a1 - a2);
            }
            else if ((a1 >= 0 && a2 > 0) && a1 < a2)
            {
                return 2 * Math.PI + a1 - a2;
            }
            else if ((a1 < 0 && a2 <= 0) && a1 < a2)
            {
                return 2 * Math.PI + a1 + Math.Abs(a2);
            }
            else if ((a1 <= 0 && a2 < 0) && a1 > a2)
            {
                return Math.Abs(a1 - a2);
            }
            else if (a1 <= 0 && 0 < a2)
            {
                return 2 * Math.PI + a1 - a2;
            }
            else if (a1 >= 0 && 0 >= a2)
            {
                return a1 + Math.Abs(a2);
            }
            else
            {
                return 0;
            }
        }

        private List<Point> SortByAngle(List<Point> l, Point q, Double a)
        {
            List<Point> sortedPoints = new List<Point>(0);
            Dictionary<double, Point> dictOnAngle = new Dictionary<double, Point>(0);
            foreach (Point pntO1 in l)
            {
                Double a1 = AngleDifference(a, CalculateAngle(q, pntO1));
                dictOnAngle.Add(a1,pntO1);
            }
            List<KeyValuePair<double, Point>> keyValuePairs = dictOnAngle.OrderBy(x => x.Value).ToList();
            foreach (var keyValuePair in keyValuePairs)
            {
                sortedPoints.Add(keyValuePair.Value);
            }
            //Collections.Sort(l, new AnonymousComparator2(this));
            return sortedPoints;
        }
        private int Compare1(Point o1, Point o2, Point q, Double a)
        {
            Double a1 = AngleDifference(a, CalculateAngle(q, o1));
            Double a2 = AngleDifference(a, CalculateAngle(q, o2));
            return a2.CompareTo(a1);
        }
        //private sealed class AnonymousComparator2 : Comparator
        //{
        //    public AnonymousComparator2(Point parent)
        //    {
        //        this.parent = parent;
        //    }

        //    private readonly Point parent;
        //    public int Compare(Point o1, Point o2)
        //    {
        //        Double a1 = AngleDifference(a, CalculateAngle(q, o1));
        //        Double a2 = AngleDifference(a, CalculateAngle(q, o2));
        //        return a2.CompareTo(a1);
        //    }
        //}

        private bool Intersect(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {

            // calculate part equations for line-line intersection
            Double a1 = l1p2.Y() - l1p1.Y();
            Double b1 = l1p1.X() - l1p2.X();
            Double c1 = a1 * l1p1.X() + b1 * l1p1.Y();
            Double a2 = l2p2.Y() - l2p1.Y();
            Double b2 = l2p1.X() - l2p2.X();
            Double c2 = a2 * l2p1.X() + b2 * l2p1.Y();

            // calculate the divisor
            Double tmp = (a1 * b2 - a2 * b1);

            // calculate intersection point x coordinate
            Double pX = (c1 * b2 - c2 * b1) / tmp;

            // check if intersection x coordinate lies in line line segment
            if ((pX > l1p1.X() && pX > l1p2.X()) || (pX > l2p1.X() && pX > l2p2.X()) || (pX < l1p1.X() && pX < l1p2.X()) || (pX < l2p1.X() && pX < l2p2.X()))
            {
                return false;
            }


            // calculate intersection point y coordinate
            Double pY = (a1 * c2 - a2 * c1) / tmp;

            // check if intersection y coordinate lies in line line segment
            if ((pY > l1p1.Y() && pY > l1p2.Y()) || (pY > l2p1.Y() && pY > l2p2.Y()) || (pY < l1p1.Y() && pY < l1p2.Y()) || (pY < l2p1.Y() && pY < l2p2.Y()))
            {
                return false;
            }

            return true;
        }

        private bool PointInPolygon(Point p, List<Point> pp)
        {
            bool result = false;
            for (int i = 0, j = pp.Count - 1; i < pp.Count; j = i++)
            {
                if ((pp[i].Y() > p.Y()) != (pp[j].Y() > p.Y()) && (p.X() < (pp[j].X() - pp[i].X()) * (p.Y() - pp[i].Y()) / (pp[j].Y() - pp[i].Y()) + pp[i].X()))
                {
                    result = !result;
                }
            }

            return result;
        }

        public ConcaveHull()
        {
        }

        public virtual List<Point> CalculateConcaveHull(List<Point> pointArrayList, int k)
        {

            // the resulting concave hull
            List<Point> concaveHull = new List<Point>();

            // optional remove duplicates
            HashSet<Point> set = new HashSet<Point>(pointArrayList);
            List<Point> pointArraySet = new List<Point>(set);

            // k has to be greater than 3 to execute the algorithm
            int kk = Math.Max(k, 3);

            // return Points if already Concave Hull
            if (pointArraySet.Count < 3)
            {
                return pointArraySet;
            }


            // make sure that k neighbors can be found
            kk = Math.Min(kk, pointArraySet.Count - 1);

            // find first point and remove from point list
            Point firstPoint = FindMinYPoint(pointArraySet);
            concaveHull.Add(firstPoint);
            Point currentPoint = firstPoint;
            pointArraySet.Remove(firstPoint);
            Double previousAngle = 0;
            int step = 2;
            while ((currentPoint != firstPoint || step == 2) && pointArraySet.Count > 0)
            {

                // after 3 steps add first point to dataset, otherwise hull cannot be closed
                if (step == 5)
                {
                    pointArraySet.Add(firstPoint);
                }


                // get k nearest neighbors of current point
                List<Point> kNearestPoints = KNearestNeighbors(pointArraySet, currentPoint, kk);

                // sort points by angle clockwise
                List<Point> clockwisePoints = SortByAngle(kNearestPoints, currentPoint, previousAngle);

                // check if clockwise angle nearest neighbors are candidates for concave hull
                bool its = true;
                int ii = -1;
                while (its && ii < clockwisePoints.Count - 1)
                {
                    ii++;
                    int lastPoint = 0;
                    if (clockwisePoints[ii] == firstPoint)
                    {
                        lastPoint = 1;
                    }


                    // check if possible new concave hull point intersects with others
                    int j = 2;
                    its = false;
                    while (!its && j < concaveHull.Count - lastPoint)
                    {
                        its = Intersect(concaveHull[step - 2], clockwisePoints[ii], concaveHull[step - 2 - j], concaveHull[step - 1 - j]);
                        j++;
                    }
                }


                // if there is no candidate increase k - try again
                if (its)
                {
                    return CalculateConcaveHull(pointArrayList, k + 1);
                }


                // add candidate to concave hull and remove from dataset
                currentPoint = clockwisePoints[ii];
                concaveHull.Add(currentPoint);
                pointArraySet.Remove(currentPoint);

                // calculate last angle of the concave hull line
                previousAngle = CalculateAngle(concaveHull[step - 1], concaveHull[step - 2]);
                step++;
            }


            // Check if all points are contained in the concave hull
            bool insideCheck = true;
            int i = pointArraySet.Count - 1;
            while (insideCheck && i > 0)
            {
                insideCheck = PointInPolygon(pointArraySet[i], concaveHull);
                i--;
            }


            // if not all points inside -  try again
            if (!insideCheck)
            {
                return CalculateConcaveHull(pointArrayList, k + 1);
            }
            else
            {
                return concaveHull;
            }
        }
    }
}