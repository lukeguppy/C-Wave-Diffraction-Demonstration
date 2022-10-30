using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wave_Diffraction_Project
{
    class Placer
    {
        private Ellipse _body1; // smallest circle
        private Ellipse _body2; // middle circle
        private Ellipse _body3; // outermost circle
        private Canvas _canvas; 

        public Placer(Canvas canvas)
        {
            _canvas = canvas;
            _body1 = new Ellipse { Height = 5, Width = 5, Fill = Brushes.Blue, Stroke = Brushes.Blue };
            Canvas.SetTop(_body1, Mouse.GetPosition(_canvas).Y - _body1.Height / 2);
            Canvas.SetLeft(_body1, Mouse.GetPosition(_canvas).X - _body1.Width / 2);

            _body2 = new Ellipse { Height = 25, Width = 25, Stroke = Brushes.Blue, StrokeThickness = 2 };
            Canvas.SetTop(_body2, Mouse.GetPosition(_canvas).Y - _body2.Height / 2);
            Canvas.SetLeft(_body2, Mouse.GetPosition(_canvas).X - _body2.Width / 2);

            _body3 = new Ellipse { Height = 45, Width = 45, Stroke = Brushes.Blue, StrokeThickness = 2 };
            Canvas.SetTop(_body3, Mouse.GetPosition(_canvas).Y - _body3.Height / 2);
            Canvas.SetLeft(_body3, Mouse.GetPosition(_canvas).X - _body3.Width / 2);
        }

        public void Show()
        {
            _canvas.Children.Add(_body1);
            _canvas.Children.Add(_body2);
            _canvas.Children.Add(_body3);
        }

        public void Remove()
        {
            _canvas.Children.Remove(_body1);
            _canvas.Children.Remove(_body2);
            _canvas.Children.Remove(_body3);
        } // removes the placer from the screen
    }
}