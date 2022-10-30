using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wave_Diffraction_Project
{
    /// <summary>
    /// interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double deltaTime;
        Stopwatch timer;
        Simulation simulation;

        public MainWindow()
        {
            InitializeComponent();
            Initialise();
            CompositionTarget.Rendering += Tick;
        }

        private void Tick(object sender, EventArgs e)
        {
            if (IsKeyboardFocused || IsKeyboardFocusWithin)
            {
                SetDeltaTime();
                Input.Update();
                UpdateUI();
                simulation.Tick(sender, e, deltaTime);
            }
            else
            {
                timer.Restart();
                timer.Stop();
            }
        } // the tick routine which runs each frame

        private void SetDeltaTime()
        {
            deltaTime = timer.ElapsedMilliseconds / 1000d; // difference in time in seconds
            timer.Restart();
        } // sets the difference in time since last frame

        private void Initialise()
        {
            WindowStyle = WindowStyle.None;
            Width = 1920;
            Height = 1080;
            WindowState = WindowState.Maximized;

            Input.SetCanvas(Screen);

            timer = new Stopwatch();
            simulation = new Simulation(Screen);
        } // sets the initial values for variables

        private void UpdateUI()
        {
            if (Input.Escape)
            {
                Environment.Exit(0); // exits the program
            }
        } // updates the ui based on user inputs
    }
}