using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloody.NET;
using System.Drawing;
using System.Diagnostics;

namespace TestApp
{
    static class Program
    {
        static void Main()
        {
            using (BloodyKeyboard keyboard = BloodyKeyboard.Initialize())
            {
                if (keyboard == null)
                {
                    Console.WriteLine("Did not find bloody!");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("Found Bloody!");
                keyboard.SetColor(Color.Yellow);
                keyboard.SetKeyColor(Key.W, Color.Red);
                keyboard.SetKeyColor(Key.A, Color.Blue);
                keyboard.SetKeyColor(Key.S, Color.Green);
                keyboard.SetKeyColor(Key.D, Color.White);

                var watch = new Stopwatch();
                watch.Start();
                bool success = keyboard.Update();
                Console.WriteLine("Set colors: " + success + ", took :" + watch.ElapsedMilliseconds + "ms");
                watch.Stop();


                Console.ReadLine();
                Console.WriteLine("Disconnecting...");
            }
        }
    }
}