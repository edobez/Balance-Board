using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

using BalanceBoard;

namespace TestPWM
{
    public class Program
    {
        public static void Main()
        {
            // Blink board LED

            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            Motor motor1 = new Motor(PWM.Pin.PWM1, 20000);

            double i = 0.5;

            while (true)
            {
                motor1.set(i++);
                if (i > 100) i = 0.75;
                Thread.Sleep(700);
            }
        }

    }
}




