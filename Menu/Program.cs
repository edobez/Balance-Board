using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;

namespace Menu
{
    public class Program
    {
        public static void Main()
        {
            int pippo = 4;
            int tizio = 3;
            int caio = 7;

            MenuManager menu = new MenuManager();
            MenuItem item0 = new MenuItem("ADC");
            MenuItem item1 = new MenuItem("Angoli");
            MenuItem item2 = new MenuItem("PID");

            MenuItem item00 = new MenuItem("CH0:", pippo, true);
            MenuItem item01 = new MenuItem("CH1:", tizio, true);
            MenuItem item02 = new MenuItem("CH1:", caio, true);

            menu.Root.addMenu(item0);
            menu.Root.addMenu(item1);
            menu.Root.addMenu(item2);
            item0.addMenu(item00).addMenu(item01).addMenu(item02);

            while (true)
            {
                menu.display();
                menu.butMenu();
            }
        }

    }
}
