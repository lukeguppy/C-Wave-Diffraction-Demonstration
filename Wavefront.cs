using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wave_Diffraction_Project
{
    class Wavefront
    {

        private int _maxDistance; // max distance before the wave "runs out of energy"
        private int _speed; // speed constant
        private int _diffractBarrierOrder; // the barrier the wavefront diffracted around if diffracted (else = -1)
        private int _barrierCount; // the number of barriers (needed for the diffracted bool)

        private double _startAngle; // the beginning angle of the range
        private double _endAngle; // the end angle (draws anticlockwise)
        private double _timeAlive; // time since spawned by wave
        private double _radius; // the radius from the centre

        private bool[] _diffractedBool; // states whether the wavefront has diffracted around each barrier so it doesnt diffract every tick 

        private List<Path> _arcs; // list of arcs making wavefront
        private List<double> _angles; // a list of angles of collisions

        private Vector _centre; // the centre 
        private GeometryGroup _lineData; // general line data needed for the arc drawing (reducing repeated calculation)
        private Canvas _canvas; // so canvas doesnt need to repeatedly be passed in as a parameter

        public Wavefront(Vector centre, Canvas canvas, double initialRadius, double startAngle, double endAngle, int diffractBarrierOrder, int barrierCount, double timeAlive, int speed, int maxDistance)
        {
            _speed = speed;
            _timeAlive = timeAlive;
            _barrierCount = barrierCount;
            _diffractedBool = new bool[2 * _barrierCount];
            _centre = centre;
            _canvas = canvas;
            _radius = initialRadius;
            _diffractBarrierOrder = diffractBarrierOrder;
            _startAngle = PositiveAngle(startAngle);
            _endAngle = PositiveAngle(endAngle);
            _arcs = new List<Path>();
            _angles = new List<double>();
            _lineData = new GeometryGroup();
            _angles.Add(_startAngle); // initial angle
            _angles.Add(_endAngle);
            _maxDistance = maxDistance;
            _lineData = new GeometryGroup { Children = { new EllipseGeometry((Point)centre, _radius, _radius) } };
            _arcs.Add(new Path { Stroke = Brushes.White, StrokeThickness = AlphaValue(), Data = _lineData }); // starts as ellipse
        }

        public void UpdateLogic(List<Barrier> barriers, ref List<Wavefront> wavefronts, double deltaTime)
        {
            IncreaseProperties(deltaTime);
            UpdateIntersections(barriers, ref wavefronts);
        } // updates the logic each tick

        private void UpdateIntersections(List<Barrier> barriers, ref List<Wavefront> wavefronts)
        {
            Barrier barrier;
            Vector meet1;
            Vector meet2;
            bool meet1Use; //Whether to use this point as an intersect
            bool meet2Use;
            double angle1;
            double angle2;
            Vector centreOffset;
            double lengthSquared;
            double temp;
            double temp2;
            //variables to reduce repeated calculations 
            double discriminant;
            double weight1;
            double weight2;

            for (int i = 0; i < barriers.Count; i++)
            {
                if (i != _diffractBarrierOrder)
                {
                    barrier = barriers[i];
                    centreOffset = barrier.start - _centre;
                    lengthSquared = barrier.offset.X * barrier.offset.X + barrier.offset.Y * barrier.offset.Y;
                    temp = barrier.offset.X * centreOffset.Y - barrier.offset.Y * centreOffset.X;
                    temp2 = barrier.offset.X * centreOffset.X + barrier.offset.Y * centreOffset.Y;
                    //variables to reduce repeated calculations 
                    discriminant = _radius * _radius * lengthSquared - temp * temp;
                    weight1 = (-temp2 - Math.Sqrt(discriminant)) / lengthSquared;
                    weight2 = (-temp2 + Math.Sqrt(discriminant)) / lengthSquared;

                    if (discriminant > 0 && !((weight1 < 0 && weight2 < 0) || (weight1 > 1 && weight2 > 1))) //intersects barrier twice
                    {
                        meet1Use = true; //Whether to use this point as an intersect
                        meet2Use = true;

                        if (weight1 < 0) // if 1st intersect is not on line end points are used
                        {
                            angle1 = Math.Atan2(barrier.start.Y - _centre.Y, barrier.start.X - _centre.X);
                            meet1 = FindCollisionPoint(angle1);
                        } // find new point and angle if the wave is no longer in contact with barrier on start side
                        else
                        {
                            meet1 = new Vector(barrier.start.X + weight1 * barrier.offset.X, barrier.start.Y + weight1 * barrier.offset.Y);
                            angle1 = Math.Atan2(meet1.Y - _centre.Y, meet1.X - _centre.X);
                        } // otherwise use intersection point on barrier

                        if (weight2 > 1) // if 2nd intersect is not on line end points are used
                        {
                            angle2 = Math.Atan2(barrier.end.Y - _centre.Y, barrier.end.X - _centre.X);
                            meet2 = FindCollisionPoint(angle2);
                        } // find new point and angle if the wave is no longer in contact with barrier on end side
                        else
                        {
                            meet2 = new Vector(barrier.start.X + weight2 * barrier.offset.X, barrier.start.Y + weight2 * barrier.offset.Y);
                            angle2 = Math.Atan2(meet2.Y - _centre.Y, meet2.X - _centre.X);
                        } // otherwise use intersection point on barrier

                        for (int j = 0; j < barriers.Count; j++)
                        {
                            if (i != j) //so it doesnt compare to itself
                            {
                                CheckBarrierBlock(ref meet1Use, meet1, _centre, barriers[j]);
                                CheckBarrierBlock(ref meet2Use, meet2, _centre, barriers[j]);
                            }
                        }// checking if any barrier blocks the angle

                        if (BarriersIntersect(FindCollisionPoint(_startAngle), _centre, barrier.start, barrier.end))
                        {
                            _angles.Remove(_startAngle);
                        } // remove the inital start angle if it is blocked

                        if (BarriersIntersect(FindCollisionPoint(_endAngle), _centre, barrier.start, barrier.end))
                        {
                            _angles.Remove(_endAngle);
                        } // remove the inital end angle if it is blocked

                        angle1 = PositiveAngle(angle1); // make angles between 0 and 2 pi
                        angle2 = PositiveAngle(angle2);

                        if (!InRange(angle1))
                        {
                            meet1Use = false;
                        } // dont use angle if it isnt between the inital angles

                        if (!InRange(angle2))
                        {
                            meet2Use = false;
                        } // dont use angle if it isnt between the inital angles

                        if (weight1 <= 0 && meet1Use) // if 1st intersect is not on line end points are used
                        {
                            if (!_diffractedBool[2 * i])
                            {
                                _diffractedBool[2 * i] = true;
                                Diffract(barrier.start, i, barrier, angle1, ref wavefronts, true);
                            }

                        } // if the intersection reaches the barrier end it begins to diffract

                        if (weight2 >= 1 && meet2Use) // if 1st intersect is not on line end points are used
                        {
                            if (!_diffractedBool[2 * i + 1])
                            {
                                _diffractedBool[2 * i + 1] = true;
                                Diffract(barrier.end, i, barrier, angle2, ref wavefronts, false);
                            }
                        } // if the intersection reaches the barrier end it begins to diffract

                        if (meet1Use)
                        {
                            _angles.Add(angle1); // adds angles
                        } // if the angle should be used, add it to angle list

                        if (meet2Use)
                        {
                            _angles.Add(angle2);
                        } // if the angle should be used, add it to angle list
                    } // if the wavefront has reached the barrier
                } // so it doesnt compare to a barrier it diffracted around if it is a diffracted wave
            }
        } //find the angles with each intersected barrier and adds them to the list (also diffracts at the end of barriers)

        private void CheckBarrierBlock(ref bool meetUse, Vector linePoint, Vector centre, Barrier barrier)
        {
            if (meetUse)
            {
                meetUse = !BarriersIntersect(linePoint, centre, barrier.start, barrier.end);
            }
        } // stops an angle from being used if it is blocked by another barrier

        private void Diffract(Vector centre, int diffractedBarrierOrder, Barrier barrier, double startAngle, ref List<Wavefront> wavefronts, bool isStart)
        {
            double endAngle;
            double temp;

            if (isStart) // if the diffraction occurs at the start point of the barrier
            {
                endAngle = PositiveAngle(Math.Atan2(barrier.offset.Y, barrier.offset.X)); // finds the angle from end to start
            }
            else
            {
                endAngle = PositiveAngle(Math.Atan2(-barrier.offset.Y, -barrier.offset.X)); // finds the angle from start to end
            }

            if ((startAngle > endAngle && startAngle - endAngle < Math.PI) || (startAngle < endAngle && endAngle - startAngle > Math.PI))
            {
                endAngle = PositiveAngle(endAngle + (Math.Pow(NonReflexDifference(endAngle, startAngle), 2) / Math.PI));
            }
            else
            {
                endAngle = PositiveAngle(endAngle - (Math.Pow(NonReflexDifference(endAngle, startAngle), 2) / Math.PI));
            }

            if ((startAngle > endAngle && startAngle - endAngle < Math.PI) || (startAngle < endAngle && endAngle - startAngle > Math.PI))
            {
                    temp = startAngle;
                    startAngle = endAngle;
                    endAngle = temp; // switching the start and end angles
            }

            Vector centreToBarrierStart = barrier.start - _centre;
            Vector centreToBarrierEnd = barrier.end - _centre;

            if (isStart)
            {
                wavefronts.Add(new Wavefront(centre, _canvas, _radius - centreToBarrierStart.Length, startAngle, endAngle, diffractedBarrierOrder, _barrierCount, _timeAlive, _speed, _maxDistance));
            }
            else
            {
                wavefronts.Add(new Wavefront(centre, _canvas, _radius - centreToBarrierEnd.Length, startAngle, endAngle, diffractedBarrierOrder, _barrierCount, _timeAlive, _speed, _maxDistance));
            }
        } // creates a new wavefront with the diffracted angles spawining at the barrier point

        private bool InRange(double angle)
        {
            if ((PositiveAngle(angle) < _endAngle && PositiveAngle(angle) > _startAngle && _endAngle > _startAngle) // this works (when start < end)
             || ((PositiveAngle(angle) < _endAngle || PositiveAngle(angle) > _startAngle) && _endAngle < _startAngle)) // WHY DOESNT THIS WORK
            {
                return true;
            }
            return false;
        } // returns whether an angle is between the initial angles

        private Vector FindCollisionPoint(double angle)
        {
            return new Vector
            {
                X = _centre.X + _radius * Math.Cos(angle),
                Y = _centre.Y + _radius * Math.Sin(angle)
            }; // finds the point where the angle of intersection meets the wavefront
        } // finds the point on the wavefront given the angle to the point

        private bool BarriersIntersect(Vector start1, Vector end1, Vector start2, Vector end2)
        {
            Vector offset1 = end1 - start1;
            Vector offset2 = end2 - start2; // repeated calculations reduced

            if (((start2.X - start1.X) * offset1.Y - (start2.Y - start1.Y) * offset1.X) *
               ((end2.X - start1.X) * offset1.Y - (end2.Y - start1.Y) * offset1.X) < 0
               &&
               ((start1.X - start2.X) * offset2.Y - (start1.Y - start2.Y) * offset2.X) *
               ((end1.X - start2.X) * offset2.Y - (end1.Y - start2.Y) * offset2.X) < 0)
            //compares if two lines are touching using an xY-yX < 0 formula for the two different lines and the four different points (start and end points)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // returns whether two lines intersect

        private double NonReflexDifference(double angle1, double angle2)
        {
            double difference;

            if (angle1 < angle2)
            {
                difference = angle2 - angle1;
            }
            else
            {
                difference = angle1 - angle2;
            }

            if (difference > Math.PI)
            {
                difference = 2 * Math.PI - difference;
            }

            return difference;
        }

        private double PositiveAngle(double angle)
        {
            if (angle < 0)
            {
                return angle += 2 * Math.PI;
            }
            if (angle  > 2*Math.PI)
            {
                return angle % (2*Math.PI);
            }
            return angle;
        } // makes a given angle in the range 0 to 2pi

        private void IncreaseProperties(double deltaTime)
        {
            _angles.Clear(); // removes all angles
            _angles.Add(_startAngle);
            _angles.Add(_endAngle); // sets default angles
            _timeAlive += deltaTime;
            _radius += _speed * deltaTime; // increases the wavefront radius by the change in pixels
        } // increases the radius and alive time (also sets initial angles)

        public void UpdateVisuals(List<Barrier> barriers)
        {
            Remove();
            DrawArcs(barriers); // draws all of the arcs to the canvas
        } // removes all arcs and redraws them in their new positions

        public bool CheckDead()
        {
            if (_angles.Count == 0 || _timeAlive >= _maxDistance / _speed)
            {
                Remove();
                return true;
            }
            else
            {
                return false;
            }
        } // returns if the wavefront is "dead" and removes it if it is

        private void DrawArcs(List<Barrier> barriers)
        {
            if (_startAngle > _endAngle)
            {
                _angles.Add(0);
                _angles.Add(2 * Math.PI);
            }

            for (int j = 0; j < barriers.Count; j++)
            {
                if (BarriersIntersect(_centre, FindCollisionPoint(0), barriers[j].start, barriers[j].end))
                {
                    _angles.Remove(0);
                    _angles.Remove(2 * Math.PI);
                }
            }// checking if any barrier blocks the angle

            for (int j = 0; j < _angles.Count / 2; j++)
            {
                _arcs.Add(new Path { Stroke = Brushes.White, StrokeThickness = 1, Data = _lineData });
            }

            foreach (var arc in _arcs)
            {
                _canvas.Children.Remove(arc); // removes all the arcs
            }

            _angles.Sort(); // sorts all the angles in size order

            int i = 0;
            while (i < _angles.Count)
            {
                Arc(i, _angles[i], _angles[(i + 1) % _angles.Count], _radius); // draws each arc to alternating angles
                i += 2;
            }

           Show();

        } // draws all of the arcs in arc list

        public void Remove()
        {
            foreach (var arc in _arcs)
            {
                _canvas.Children.Remove(arc); // removes all arcs from canvas
            }
        } // removes all arcs from the screen

        public void Show()
        {
            int i = 0;
            while (i < _angles.Count)
            {
                _canvas.Children.Add(_arcs[i / 2]);
                i += 2;
            }
        } // adds all arcs from the screen

        private byte AlphaValue()
        {
            if (_timeAlive * _speed < _maxDistance)
            {
                return (byte)(255 - 255 * _timeAlive * _speed / _maxDistance);
            }
            else
            {
                return 0;
            }
        } // gives the alpha value as a function of time to represent energy loss

        private void Arc(int angleOrder, double startAngle, double endAngle, double radius)
        {

            if ((startAngle + 2 * Math.PI) % (2 * Math.PI) != (endAngle + 2 * Math.PI) % (2 * Math.PI))
            {
                Vector start = _centre + new Vector(Math.Cos(startAngle) * radius, Math.Sin(startAngle) * radius); // finding start coords for arc segment
                Vector finish = _centre + new Vector(Math.Cos(endAngle) * radius, Math.Sin(endAngle) * radius); // finding end coords for arc segment
                PathSegmentCollection arcCollection = new PathSegmentCollection
                {
                    new ArcSegment((Point)finish, new Size(radius, radius), 0, ((endAngle - startAngle + 3 * Math.PI) % (2 * Math.PI)) < Math.PI, SweepDirection.Clockwise, true)
                }; // creating a collection of paths for the arc segment with all the data needed
                PathFigure arcFig = new PathFigure { Segments = arcCollection, StartPoint = (Point)start, IsClosed = false };
                PathFigureCollection arcFigCollection = new PathFigureCollection { arcFig };
                PathGeometry arcGeom = new PathGeometry { Figures = arcFigCollection };
                _arcs[angleOrder / 2] = new Path() { Stroke = new SolidColorBrush(Color.FromArgb(AlphaValue(), 255, 255, 255)), StrokeThickness = 2, Data = arcGeom }; // converting to path so it can be drawn
            }
            else
            {
                GeometryGroup ellipseData = new GeometryGroup { Children = { new EllipseGeometry((Point)_centre, radius, radius) } }; // drawing an ellipse if there are two equal angles
                _arcs[angleOrder / 2] = new Path { Stroke = new SolidColorBrush(Color.FromArgb(AlphaValue(), 255, 255, 255)), StrokeThickness = 2, Data = ellipseData }; // converting the ellipse to a path to be drawn
            }
            Panel.SetZIndex(_arcs[angleOrder / 2], -1);
        } // creates arc from start to end angle with set radius
    }
}