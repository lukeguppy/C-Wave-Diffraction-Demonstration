using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wave_Diffraction_Project
{
    class Wave
    {
        private Vector _centre; // centre point of the wave
        private double _period; // the period of the wave / s
        private int _speed; // the speed / ps^-1
        private int _maxRadius;

        public double time; // total time since the simulation started
        public int numOfWavefronts; // the number of wavefronts to be emitted

        public Wave(double x, double y, double period, int speed, int maxDistance)
        {
            _centre = new Vector(x, y); // mouse y position
            time = period;
            _period = period;
            _speed = speed;
            _maxRadius = maxDistance;
            numOfWavefronts = 0;
        }

        public void UpdateVariables(double period, int speed, int maxDistance)
        {
             _period = period;
             _speed = speed; 
             _maxRadius = maxDistance;
             time = period;
        }

        public void UpdateLogic(Canvas canvas, double wavefrontMax, double deltaTime, List<Wavefront> Wavefronts, int barrierCount)
        {
            time += deltaTime;
            if (time >= _period && numOfWavefronts < wavefrontMax)
            {
                numOfWavefronts++;
                time -= _period;
                Wavefronts.Add(new Wavefront(_centre, canvas, _speed * deltaTime, 0, 2*Math.PI, -1, barrierCount, 0, _speed, _maxRadius)); // every period another wavefront is added for each wave
            }
        } // adds a wavefront each period

    }
}