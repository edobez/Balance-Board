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

            menu.addMenu(new MenuItem("ADC"), 10);
            menu.addSubMenu(new MenuItem("CH0:", pippo, true), 11);
            menu.addSubMenu(new MenuItem("CH1:", tizio, true), 12);
            menu.addSubMenu(new MenuItem("CH2:", caio, true), 13);

            menu.addMenu(new MenuItem("PID"), 20);
            menu.addSubMenu(new MenuItem("CH0:", pippo, true), 21);

            menu.addMenu(new MenuItem("Output"), 30);
            menu.addSubMenu(new MenuItem("CH0:", pippo, true), 31);

            while (true)
            {
                menu.display();
            }
        }

    }
}
