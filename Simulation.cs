using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wave_Diffraction_Project
{
    class Simulation
    {
        private int _speed; // wave speed in ps^-1
        private double _period; // period between wavefronts /s
        private int _maxDistance; // maximum radius of wavefronts
        private int _maxWavefronts; // the max number of wavefronts which each wave centre can have
        private int _waveLimit; // the maximum number of waves which can be displayed on the screen
        private int _barrierLimit; // the maximum number of barriers

        private List<Wavefront> _wavefronts; // all wavefronts on screen
        private List<Wave> _waves; // each wave adds wave fronts per period
        private List<Barrier> _barriers; // list of all barriers
        private List<Placer> _placers; // placers show where waves will spawn
        
        private double _deltaTime; // change in time each frame
        private TextBlock _backText; // the background text
        private Canvas _canvas;
        private double _time; // the total time while the simulation is running

        private bool _activeBarrier; // whether a barrier is currently being drawn
        private bool _started; // whether the user has started the simulation
        private bool _sliderShowing;

        private Slider _speedSlider;
        private TextBlock _speedText;
        private Slider _periodSlider;
        private TextBlock _periodText;
        private Slider _maxDistanceSlider;
        private TextBlock _maxDistanceText;
        private Slider _maxWavefrontsSlider;
        private TextBlock _maxWavefrontsText;
        private Slider _waveLimitSlider;
        private TextBlock _waveLimitText;
        private Slider _barrierLimitSlider;
        private TextBlock _barrierLimitText;

        public Simulation(Canvas canvas)
        {
            _canvas = canvas;
            _wavefronts = new List<Wavefront>();
            _deltaTime = 0;
            _waves = new List<Wave>();
            _barriers = new List<Barrier>();
            _activeBarrier = false;
            _started = false;
            _placers = new List<Placer>();
            _time = 0;
            _sliderShowing = false;
            SetBackText();
            CreateSliders();
            UpdateVariables();
            SetSliderPositions();
            SetEdges();
        }
        
        public void Tick(object sender, EventArgs e, double deltaTime)
        {
            UpdateUI();
            CheckRestart();
            _deltaTime = deltaTime;

            UpdateWaveLogic();
            if (_sliderShowing)
            {
                UpdateSliderText();
            }
            if (_started)
            {
                _time += _deltaTime;
                UpdateWavefrontLogic();
                UpdateWavefrontVisuals();
            }
            else if (!_sliderShowing)
            {
                UpdateBarriers();
            }

        } // updates all the objects each frame based on the constants and difference in time

        private void UpdateUI()
        {
            if (Input.tab && !Input.lastTab && !_started)
            {
                if (_sliderShowing)
                {

                    UpdateVariables();
                    HideSliders();
                    ShowAll();
                }
                else
                {
                    ShowSliders();
                    HideAll();
                    _canvas.Children.Remove(_backText);
                }
                _sliderShowing = !_sliderShowing;
            }
            if (Input.Space && !Input.LastSpace && !_sliderShowing)
            {
                _started = true;
                _canvas.Children.Remove(_backText);
                foreach (var placer in _placers)
                {
                    placer.Remove();
                }
            }
        } // updates the UI based on user inputs

        private void CheckRestart()
        {
            if (Input.Delete && !Input.LastDelete)
            {
                RemoveAll();
                _time = 0;
                if (_started)
                {
                    _canvas.Children.Add(_backText);
                }
                _started = false;
                SetEdges();
            }

            if ((_wavefronts.Count == 0 && _time >= _maxWavefronts * _period - _period) || (_started && _waves.Count == 0))
            {
                _time = 0;

                _canvas.Children.Add(_backText);
                foreach (var wave in _waves)
                {
                    wave.numOfWavefronts = 0;
                    wave.time = _period;
                }
                foreach (var placer in _placers)
                {
                    placer.Show();
                }
                _started = false;
            }
        } // checks whether to return to the starting screen 

        private void UpdateBarriers()
        {
            if (Input.MouseRight && !Input.LastMouseRight)
            {
                if (_barriers.Count < _barrierLimit + 4)
                {
                    _barriers.Add(new Barrier(_canvas)); // Adds a barrier 
                    _activeBarrier = true;
                }
            }
            if (!Input.MouseRight)
            {
                _activeBarrier = false;
            }
            if (_barriers.Count > 4 && _activeBarrier)
            {
                _barriers[_barriers.Count - 1].BarrierUpdate(); // updates barrier end points
            }
        } // updates the barrier being drawn or adds new barrier if the user right clicks

        private void UpdateWaveLogic()
        {
            if (!_started)
            {
                if (Input.MouseLeft && !Input.LastMouseLeft && !_sliderShowing)
                {
                    if (_waves.Count < _waveLimit)
                    {
                        if (Input.MousePosition.X > 2 && Input.MousePosition.X < 1918 && Input.MousePosition.Y > 2 && Input.MousePosition.Y < 1078) // cant place on edges
                        {
                            _waves.Add(new Wave(Input.MousePosition.X, Input.MousePosition.Y, _period, _speed, _maxDistance)); // adds a new wave
                            _placers.Add(new Placer(_canvas));
                            _placers[_placers.Count - 1].Show();
                        }
                    }
                }
            }

            if (_started)
            {
                foreach (var wave in _waves)
                {
                    wave.UpdateLogic(_canvas, _maxWavefronts, _deltaTime, _wavefronts, _barriers.Count); // every wave is updated
                }
            }
        } // updates the logic for each wave

        private void UpdateWavefrontLogic()
        {
            int wfCount = _wavefronts.Count();

            for (int i = 0; i < wfCount; i++)
            {
                _wavefronts[i].UpdateLogic(_barriers, ref _wavefronts, _deltaTime);
            }

            for (int i = 0; i < _wavefronts.Count; i++)
            {
                if (_wavefronts[i].CheckDead())
                {
                    _wavefronts.RemoveAt(i);
                }
            }

        } // updates the logic for each wavefront

        private void UpdateWavefrontVisuals()
        {
            foreach (var wavefront in _wavefronts)
            {
                wavefront.UpdateVisuals(_barriers);
            }
        } // updates the visual logic for each wavefront (redraws)

        private void RemoveAll()
        {
            HideAll();

            _wavefronts.Clear();
            _barriers.Clear();
            _placers.Clear();
            _waves.Clear();
        } // removes everything from the canvas and resets lists

        private void SetBackText()
        {
            _backText = new TextBlock
            {
                Text = "Press space to start",
                
                TextWrapping = TextWrapping.WrapWithOverflow,
                Width = 1920,
                Height = 540,
                Foreground = new SolidColorBrush(Color.FromRgb(25, 25, 25)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                FontSize = 200
            };

            Panel.SetZIndex(_backText, -2);
            Canvas.SetTop(_backText, 420);
            _canvas.Children.Add(_backText);
        } // draws the background text

        private void SetEdges()
        {
            Barrier edge1 = new Barrier(_canvas);
            edge1.EdgeRedraw(new Vector(1, 2), new Vector(1919, 2));
            Barrier edge2 = new Barrier(_canvas);
            edge2.EdgeRedraw(new Vector(1918, 1), new Vector(1918, 1079));
            Barrier edge3 = new Barrier(_canvas);
            edge3.EdgeRedraw(new Vector(1919, 1078), new Vector(1, 1078));
            Barrier edge4 = new Barrier(_canvas);
            edge4.EdgeRedraw(new Vector(2, 1079), new Vector(2, 1));
            _barriers.Add(edge1);
            _barriers.Add(edge2);
            _barriers.Add(edge3);
            _barriers.Add(edge4);
        } // sets the edge barriers to kill off screen wavefronts

        private void HideAll()
        {
            foreach (var barrier in _barriers)
            {
                barrier.Remove();   
            }

            foreach (var wavefront in _wavefronts)
            {
                wavefront.Remove();
            }

            foreach (var placer in _placers)
            {
                placer.Remove();
            }
        } // removes all from canvas

        private void UpdateVariables()
        {
            _speed = (int)_speedSlider.Value;
            _period = _periodSlider.Value;
            _maxDistance = (int)_maxDistanceSlider.Value;
            _maxWavefronts = (int)_maxWavefrontsSlider.Value;
            _waveLimit = (int)_waveLimitSlider.Value;
            _barrierLimit = (int)_barrierLimitSlider.Value;

            foreach (var wave in _waves)
            {
                wave.UpdateVariables(_period, _speed, _maxDistance);
            }

            while (_waves.Count > _waveLimit)
            {
                _placers[_placers.Count - 1].Remove();
                _placers.RemoveAt(_placers.Count - 1);
                _waves.RemoveAt(_waves.Count - 1);
            }

            while (_barriers.Count > _barrierLimit + 4)
            {
                _barriers[_barriers.Count - 1].Remove();
                _barriers.RemoveAt(_barriers.Count - 1);
            }
        } // updates variables to those set by sliders

        private void ShowAll()
        {
            foreach (var barrier in _barriers)
            {
                barrier.Show();
            }

            foreach (var placer in _placers)
            {
                placer.Show();
            }

            _canvas.Children.Add(_backText);
        } // adds every object to the screen

        private void UpdateSliderText()
        {
            _speedText.Text = "speed = " + Convert.ToString(_speedSlider.Value) + " p/s";
            _periodText.Text = "period = " + Convert.ToString(_periodSlider.Value) + " s";
            _maxDistanceText.Text = "maximum distance = " + Convert.ToString(_maxDistanceSlider.Value) + " pixels";
            _maxWavefrontsText.Text = "wavefronts spawned = " + Convert.ToString(_maxWavefrontsSlider.Value);
            _waveLimitText.Text = "maximum waves = " + Convert.ToString(_waveLimitSlider.Value);
            _barrierLimitText.Text = "maximum barriers = " + Convert.ToString(_barrierLimitSlider.Value);
        } // updates text on sliders to updated values

        private void CreateSliders()
        {
            _speedSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 50,
                Maximum = 500,
                TickFrequency = 50,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 200
            };
            _speedText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };

            _periodSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 0.1,
                Maximum = 5,
                TickFrequency = 0.1,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 1
            };
            _periodText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };

            _maxDistanceSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 500,
                Maximum = 2500,
                TickFrequency = 100,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 1500
            };
            _maxDistanceText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };

            _maxWavefrontsSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 1,
                Maximum = 20,
                TickFrequency = 1,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 10
            };
            _maxWavefrontsText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };

            _waveLimitSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 1,
                Maximum = 5,
                TickFrequency = 1,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 3
            };
            _waveLimitText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };

            _barrierLimitSlider = new Slider
            {
                Width = 1500,
                Height = 100,
                BorderBrush = Brushes.Black,
                Minimum = 1,
                Maximum = 20,
                TickFrequency = 1,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight,
                IsSnapToTickEnabled = true,
                Value = 10
            };
            _barrierLimitText = new TextBlock
            {
                Width = 500,
                Height = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("EucrosiaUPC"),
                FontWeight = FontWeights.Bold,
                Text = ""
            };
        } // creates sliders with base values

        private void ShowSliders()
        {
            _canvas.Children.Add(_speedSlider);
            _canvas.Children.Add(_speedText);

            _canvas.Children.Add(_periodSlider);
            _canvas.Children.Add(_periodText);

            _canvas.Children.Add(_maxDistanceSlider);
            _canvas.Children.Add(_maxDistanceText);

            _canvas.Children.Add(_maxWavefrontsSlider);
            _canvas.Children.Add(_maxWavefrontsText);

            _canvas.Children.Add(_waveLimitSlider);
            _canvas.Children.Add(_waveLimitText);

            _canvas.Children.Add(_barrierLimitSlider);
            _canvas.Children.Add(_barrierLimitText);
        } // adds sliders to canvas

        private void HideSliders()
        {
            _canvas.Children.Remove(_speedSlider);
            _canvas.Children.Remove(_speedText);

            _canvas.Children.Remove(_periodSlider);
            _canvas.Children.Remove(_periodText);

            _canvas.Children.Remove(_maxDistanceSlider);
            _canvas.Children.Remove(_maxDistanceText);

            _canvas.Children.Remove(_maxWavefrontsSlider);
            _canvas.Children.Remove(_maxWavefrontsText);

            _canvas.Children.Remove(_waveLimitSlider);
            _canvas.Children.Remove(_waveLimitText);

            _canvas.Children.Remove(_barrierLimitSlider);
            _canvas.Children.Remove(_barrierLimitText);
        } // removes sliders from canvas

        private void SetSliderPositions()
        {
            double distanceFromTop = 125;
            double width = 1920;

            Canvas.SetLeft(_speedSlider, (width - _speedSlider.Width)/2);
            Canvas.SetTop(_speedSlider, distanceFromTop);
            Canvas.SetLeft(_speedText, width / 2 - _speedText.Width / 2);
            Canvas.SetTop(_speedText, distanceFromTop + _speedSlider.Height/2);

            distanceFromTop += _speedSlider.Height + 50;

            Canvas.SetLeft(_periodSlider, (width - _periodSlider.Width) / 2);
            Canvas.SetTop(_periodSlider, distanceFromTop);
            Canvas.SetLeft(_periodText, width / 2 - _periodText.Width / 2);
            Canvas.SetTop(_periodText, distanceFromTop + _periodSlider.Height / 2);

            distanceFromTop += _periodSlider.Height + 50;

            Canvas.SetLeft(_maxDistanceSlider, (width - _maxDistanceSlider.Width) / 2);
            Canvas.SetTop(_maxDistanceSlider, distanceFromTop);
            Canvas.SetLeft(_maxDistanceText, width / 2 - _maxDistanceText.Width / 2);
            Canvas.SetTop(_maxDistanceText, distanceFromTop + _maxDistanceSlider.Height / 2);

            distanceFromTop += _maxDistanceSlider.Height + 50;

            Canvas.SetLeft(_maxWavefrontsSlider, (width - _maxWavefrontsSlider.Width) / 2);
            Canvas.SetTop(_maxWavefrontsSlider, distanceFromTop);
            Canvas.SetLeft(_maxWavefrontsText, width / 2 - _maxWavefrontsText.Width / 2);
            Canvas.SetTop(_maxWavefrontsText, distanceFromTop + _maxWavefrontsSlider.Height / 2);

            distanceFromTop += _maxWavefrontsSlider.Height + 50;

            Canvas.SetLeft(_waveLimitSlider, (width - _waveLimitSlider.Width) / 2);
            Canvas.SetTop(_waveLimitSlider, distanceFromTop);
            Canvas.SetLeft(_waveLimitText, width / 2 - _waveLimitText.Width / 2);
            Canvas.SetTop(_waveLimitText, distanceFromTop + _waveLimitSlider.Height / 2);

            distanceFromTop += _waveLimitSlider.Height + 50;

            Canvas.SetLeft(_barrierLimitSlider, (width - _barrierLimitSlider.Width) / 2);
            Canvas.SetTop(_barrierLimitSlider, distanceFromTop);
            Canvas.SetLeft(_barrierLimitText, width / 2 - _barrierLimitText.Width / 2);
            Canvas.SetTop(_barrierLimitText, distanceFromTop + _barrierLimitSlider.Height / 2);

            distanceFromTop += _barrierLimitSlider.Height + 50;
        } // positions the sliders on canvas 
    }
}