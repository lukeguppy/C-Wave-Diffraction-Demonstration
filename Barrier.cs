using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wave_Diffraction_Project
{
    class Barrier
    {
        public Vector offset { get; private set; } // the offset of the start from the end
        public Vector start { get; private set; } // the starting point of the barrier
        public Vector end { get; private set; } // the end point of the barrier
        private Line _body; // the line representing the barrier
        private Canvas _canvas; // the main window canvas

        public Barrier(Canvas canvas)
        {
            _canvas = canvas;
            start = new Vector(Input.MousePosition.X, Input.MousePosition.Y);
            end = new Vector(Input.MousePosition.X, Input.MousePosition.Y);
            _body = new Line { X1 = start.X, X2 = end.X, Y1 = start.Y, Y2 = end.Y, StrokeThickness = 2, Stroke = Brushes.Red };
            offset = end - start;
            Show();
        }

        public void EdgeRedraw(Vector start, Vector end)
        {
            _body = new Line { X1 = start.X, X2 = end.X, Y1 = start.Y, Y2 = end.Y, StrokeThickness = 0 };
            this.start = start;
            this.end = end;
            offset = end - start;
            _canvas.Children.Add(_body);
        } // resets the start and end points for and edge barrier and updates the offset

        public void Remove()
        {
            _canvas.Children.Remove(_body);
        } // removes the barrier from the screen

        public void Show()
        {
            _canvas.Children.Add(_body);
        }

        public void BarrierUpdate()
        {
            end = Input.MousePosition;
            _body.X2 = end.X;
            _body.Y2 = end.Y;
            offset = end - start;
        } // updates the end point of the barrier to the current mouse pos and updates the offset
    }
}