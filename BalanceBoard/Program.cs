using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

using PIDLibrary;
using MicroLiquidCrystal;

namespace BalanceBoard
{
    public class Program
    {
        static AnalogIn[] aPin = new AnalogIn[6];

        static Accelerometer Acc = new Accelerometer();
        static Gyroscope Gyro = new Gyroscope();
        static IMU Imu = new IMU(Acc,Gyro);

        static PID Pid1 = new PID(1, 0.01, 0, -90, 90, -90, 90);
        static PID Pid2 = new PID(1, 0.01, 0, -90, 90, -90, 90);

        static Lcd myLcd;

        static TimeSpan duration;

        
        static int[] adcAcc = new int[] { 512, 512, 618 };       // inserimento dati ADC
        static int[] adcGyro = new int[] { 416, 416 }; 

        public static void Main()
        {
            // Inizializzazione pin AnalogIn
            aPin[0] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An0);
            aPin[1] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An1);
            aPin[2] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An2);
            aPin[3] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An3);
            aPin[4] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An4);
            aPin[5] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An5);

            // Inizializzazione LCD
            var lcdProvider = new GpioLcdTransferProvider((Cpu.Pin)FEZ_Pin.Digital.Di2, (Cpu.Pin)FEZ_Pin.Digital.Di3,
                (Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5, (Cpu.Pin)FEZ_Pin.Digital.Di6,
                (Cpu.Pin)FEZ_Pin.Digital.Di7);

            myLcd = new Lcd(lcdProvider);
            myLcd.Begin(16, 2);

            // Definizione timer
            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 20);
            Timer display_timer = new Timer(new TimerCallback(Display), null, 0, 250);

            //while (true)
            //{
            //    myLcd.SetCursorPosition(0, 1);
            //    myLcd.Write(DateTime.Now.Millisecond.ToString());
            //    Thread.Sleep(250);
            //}

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Control(object state)
        {
            DateTime begin = DateTime.Now;

            // Algoritmo angolo
            Acc.Raw = adcAcc;       // inserimento dati ADC
            Gyro.Raw = adcGyro;      // ...

            Acc.compute();
            Gyro.compute();
            Imu.compute();

            // Algoritmo PID
            Pid1.SetPoint = 0;
            Pid1.ProcessVariable = Imu.AngleXZ;
            Pid2.SetPoint = -1;
            Pid2.ProcessVariable = Imu.AngleYZ;

            Pid1.Compute();
            Pid2.Compute();

            duration = (DateTime.Now - begin);

        }

        private static void Display(object state)    
        {
            Debug.Print("Angles: " + Imu.AngleXZ.ToString("f3") + "," + Imu.AngleYZ.ToString("f3"));
            Debug.Print("Output variables: " + Pid1.OutputValue.ToString("f3") + "," + Pid2.OutputValue.ToString("f3"));
            Debug.Print("Duration: " + duration.Milliseconds);

            myLcd.SetCursorPosition(0, 0);
            myLcd.Write(Imu.AngleXZ.ToString("f3") + "," + Imu.AngleYZ.ToString("f3"));
            myLcd.SetCursorPosition(0,1);
            myLcd.Write("PID:" + Pid1.OutputValue.ToString("f2") + "," + Pid2.OutputValue.ToString("f2"));
        }
    }
}
