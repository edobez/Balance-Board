using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace BalanceBoard
{
    public class Program
    {
        static AnalogIn[] aPin = new AnalogIn[6];


        public static void Main()
        {
            // Inizializzazione pin AnalogIn
            aPin[0] = new AnalogIn(AnalogIn.Pin.Ain0);
            aPin[1] = new AnalogIn(AnalogIn.Pin.Ain1);
            aPin[2] = new AnalogIn(AnalogIn.Pin.Ain2);
            aPin[3] = new AnalogIn(AnalogIn.Pin.Ain3);
            aPin[4] = new AnalogIn(AnalogIn.Pin.Ain4);
            aPin[5] = new AnalogIn(AnalogIn.Pin.Ain5);

            Thread control_thread = new Thread(Control);
            control_thread.Start();
        }

        private static void Control()
        {
            int[] AdcValues = new int[6];

            for (int i = 0; i < AdcValues.Length; i++)
            {
                AdcValues[i] = aPin[i].Read();
            }




        }
    }
}
