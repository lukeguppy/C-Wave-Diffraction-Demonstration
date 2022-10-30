using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wave_Diffraction_Project
{
    static class Input // Handles user input (key presses + mouse movement)
    {
        public static bool MouseLeft { get; private set; } // whether the left mouse button is clicked
        public static bool LastMouseLeft { get; private set; } // the state of the left mouse button last frame
        public static bool MouseRight { get; private set; } // whether the right mouse button is clicked
        public static bool LastMouseRight { get; private set; } // the state of the right mouse button last frame
        public static bool Escape { get; private set; } // whether the escape key has been clicked
        public static bool Delete { get; private set; } // whether the delete key has been clicked
        public static bool LastDelete { get; private set; } // the state of the delete key last frame
        public static bool Space { get; private set; } // whether the space key has been clicked
        public static bool LastSpace { get; private set; } // the state of the space key last frame
        public static bool tab { get; private set; }
        public static bool lastTab { get; private set; }
        public static Vector MousePosition { get; private set; } // the current mouse position

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key); // lower level access to the key states
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT point); // lower level access to the mouse position 
        [StructLayout(LayoutKind.Sequential)]

        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        } // a point structure for the mouse position on the screen

        private static Canvas Canvas;

        public static void SetCanvas(Canvas canvas)
        {
            Canvas = canvas;
        } // sets the canvas variable

        public static void Update()
        {
            GetCursorPos(out POINT temp);
            Point MousePositionPoint = Canvas.PointFromScreen(new Point(temp.X, temp.Y));
            MousePosition = new Vector(MousePositionPoint.X, MousePositionPoint.Y);
            
            LastMouseLeft = MouseLeft;
            LastMouseRight = MouseRight;
            lastTab = tab;
            tab = KeyDown(Key.Tab);
            MouseLeft = IsDown(1);
            MouseRight = IsDown(2);
            Escape = KeyDown(Key.Escape);
            LastDelete = Delete;
            Delete = KeyDown(Key.Delete);
            LastSpace = Space;
            Space = KeyDown(Key.Space);
        } // sets all the boolean variables to their key states

        private static bool KeyDown(Key key)
        {
            return IsDown(KeyInterop.VirtualKeyFromKey(key));
        } // returns whether a given key is pressed
        
        private static bool IsDown(int key)
        {
            return GetAsyncKeyState(key) != 0;
        } // returns whether a key (without a set key name) is pressed
    }
}