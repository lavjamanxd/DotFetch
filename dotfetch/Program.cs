using System;

namespace dotfetch
{
    class Program
    {
        private static void Main()
        {
            var renderer = new ConsoleRenderer(new SysInformationProvider());
            renderer.Render();
            Console.ReadKey();
        }
    }
}
