using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Com.Huen.Commands
{
    public class DialCommands
    {
        public static RoutedUICommand DialNum1;
        public static RoutedUICommand DialNum2;
        public static RoutedUICommand DialNum3;
        public static RoutedUICommand DialNum4;
        public static RoutedUICommand DialNum5;
        public static RoutedUICommand DialNum6;
        public static RoutedUICommand DialNum7;
        public static RoutedUICommand DialNum8;
        public static RoutedUICommand DialNum9;
        public static RoutedUICommand DialNum0;
        public static RoutedUICommand DialCall;
        public static RoutedUICommand DialClear;
        public static RoutedUICommand DialHangup;
        public static RoutedUICommand DialAsterisk;
        public static RoutedUICommand DialSharp;
        public static RoutedUICommand DialBackSpace;

        static DialCommands()
        {
            InputGestureCollection inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad1, ModifierKeys.None, "1"));
            DialNum1 = new RoutedUICommand("1", "DialNum1", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad2, ModifierKeys.None, "2"));
            DialNum2 = new RoutedUICommand("2", "DialNum2", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad3, ModifierKeys.None, "3"));
            DialNum3 = new RoutedUICommand("3", "DialNum3", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad4, ModifierKeys.None, "4"));
            DialNum4 = new RoutedUICommand("4", "DialNum3", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad5, ModifierKeys.None, "5"));
            DialNum5 = new RoutedUICommand("5", "DialNum5", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad6, ModifierKeys.None, "6"));
            DialNum6 = new RoutedUICommand("6", "DialNum6", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad7, ModifierKeys.None, "7"));
            DialNum7 = new RoutedUICommand("7", "DialNum7", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad8, ModifierKeys.None, "8"));
            DialNum8 = new RoutedUICommand("8", "DialNum8", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad9, ModifierKeys.None, "9"));
            DialNum9 = new RoutedUICommand("9", "DialNum9", typeof(DialCommands), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.NumPad0, ModifierKeys.None, "0"));
            DialNum0 = new RoutedUICommand("0", "DialNum0", typeof(DialCommands), inputs);

            //inputs = new InputGestureCollection();
            //inputs.Add(new KeyGesture(Key.Back, ModifierKeys.None, "←"));
            //DialBackSpace = new RoutedUICommand("←", "DialBackSpace", typeof(DialCommands), inputs);

            //inputs = new InputGestureCollection();
            //inputs.Add(new KeyGesture(Key.D8, ModifierKeys.Shift, "*"));
            //DialAsterisk = new RoutedUICommand("*", "DialAsterisk", typeof(DialCommands), inputs);

            //inputs = new InputGestureCollection();
            //inputs.Add(new KeyGesture(Key.D3, ModifierKeys.Shift, "#"));
            //DialSharp = new RoutedUICommand("#", "DialSharp", typeof(DialCommands), inputs);

            //inputs = new InputGestureCollection();
            //inputs.Add(new KeyGesture(Key.Delete, ModifierKeys.None, "CLR"));
            //DialClear = new RoutedUICommand("CLR", "DialClear", typeof(DialCommands), inputs);

            //inputs = new InputGestureCollection();
            //inputs.Add(new KeyGesture(Key.Q, ModifierKeys.Alt, "Hangup"));
            //DialHangup = new RoutedUICommand("Hangup", "DialHangup", typeof(DialCommands), inputs);
        }
    }
}
