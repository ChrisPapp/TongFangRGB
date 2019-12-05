﻿using System;
using System.Drawing;
using System.Threading;
using TongFang;

namespace Test
{
    public static class Program
    {
        public static void Main()
        {
            if (!Keyboard.Initialize())
            {
                string[,] yesno = new string[7, 21];
                Keyboard.SetColorFull(Color.Black);
                Keyboard.Update();
                Console.WriteLine("Starting mapping test program.");
                Console.WriteLine("   -Type y if the led lights up directly under a keycap");
                Console.WriteLine("   -Type n if the led lights up between two keycaps");
                Console.WriteLine("   -Type b if the no led lights up at all");

                for (byte i = 0; i < 7; i++)
                {
                    for (byte j = 0; j < 21; j++)
                    {
                        Keyboard.SetKeyWithCoords(i, j, Color.Red);
                        Keyboard.Update();
                        Console.Write($"Did row {i} and column {j} change color?: ");
                        yesno[i,j] = Console.ReadLine();
                    }
                }
                string[] lines = new string[7];
                for (byte i = 0; i < yesno.GetLength(0); i++)
                {
                    for (byte j = 0; j < yesno.GetLength(1); j++)
                    {
                        if (j != 0)
                            lines[i] += ",";

                        lines[i]+= yesno[i,j];
                    }
                }
                Console.WriteLine("Writing text file");
                System.IO.File.WriteAllLines("mapping.txt", lines);
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Could not initialize device!!");
            }
            Keyboard.SetColorFull(Color.Blue);
            Keyboard.Update();
            Keyboard.Disconnect();
        }

        public static Color ChangeHue(Color color, double offset)
        {
            if (offset == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);

            hue += offset;

            while (hue > 360) hue -= 360;
            while (hue < 0) hue += 360;

            return FromHsv(hue, saturation, value);
        }

        public static void ToHsv(Color color, out double hue, out double saturation, out double value)
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));

            var delta = max - min;

            hue = 0d;
            if (delta != 0)
            {
                if (color.R == max) hue = (color.G - color.B) / (double)delta;
                else if (color.G == max) hue = 2d + (color.B - color.R) / (double)delta;
                else if (color.B == max) hue = 4d + (color.R - color.G) / (double)delta;
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color FromHsv(double hue, double saturation, double value)
        {
            saturation = Math.Max(Math.Min(saturation, 1), 0);
            value = Math.Max(Math.Min(value, 1), 0);

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = (byte)(value);
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(v, t, p);
                case 1: return Color.FromArgb(q, v, p);
                case 2: return Color.FromArgb(p, v, t);
                case 3: return Color.FromArgb(p, q, v);
                case 4: return Color.FromArgb(t, p, v);
                default: return Color.FromArgb(v, p, q);
            }
        }
    }
}
