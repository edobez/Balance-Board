using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace TestPWM
{
    public class Program
    {
        public static void Main()
        {
            // Blink board LED

            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);
            PWM pwm = new PWM(PWM.Pin.PWM1);

            while (true)
            {
                //for (byte i = 0; i < 100; i+= 2)
                //{
                //    pwm.Set(20000, i);
                //    Debug.Print("Duty: " + i);
                //    Thread.Sleep(200);
                //}
                //for (byte i = 100; i > 0; i-=2)
                //{
                //    pwm.Set(20000, i);
                //    Debug.Print("Duty: " + i);
                //    Thread.Sleep(200);
                //}
                pwm.Set(20000, 100);
                Thread.Sleep(20);
                pwm.Set(20000, 0);
                Thread.Sleep(20);
            }
        }

    }
}
