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
            OutputPort enableA = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di11,true);

            Motor motor1 = new Motor(PWM.Pin.PWM1, 20000);

            double i = 0.5;

            enableA.Write(false);
            while (true)
            {
                motor1.set(100);
                Thread.Sleep(200);
                motor1.set(0);
                Thread.Sleep(200);
            }
        }

    }
}




