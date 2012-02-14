using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.USBClient;

using PIDLibrary;
using MicroLiquidCrystal;

namespace BalanceBoard
{
    public class Program
    {
        // Definizione ingressi                
        static AnalogIn[] aPin = new AnalogIn[6];
        static InterruptPort menuBut = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di11, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
        static InterruptPort enterBut = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di34, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
        static InterruptPort upBut = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di32, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
        static InterruptPort downBut = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di30, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

        // Definizione uscite
        static OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);

        // Definizioni oggetti
        static Accelerometer Acc = new Accelerometer();
        static Gyroscope Gyro = new Gyroscope();
        static IMU Imu = new IMU(Acc,Gyro);

        static PID Pid1 = new PID(1, 0.01, 0, -90, 90, -90, 90);
        static PID Pid2 = new PID(1, 0.01, 0, -90, 90, -90, 90);

        // Definizioni per LCD
        static Lcd myLcd;
        static byte currentMenu;

        // Definizioni var. globali
        static TimeSpan duration;
        static DateTime begin;

        static int[] adcAcc = new int[] { 512, 512, 618 };       // inserimento dati ADC
        static int[] adcGyro = new int[] { 500, 416 };

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
            myLcd.ShowCursor = true;

            // Eventi interrupt
            menuBut.OnInterrupt += new NativeEventHandler(menuBut_OnInterrupt);
            enterBut.OnInterrupt += new NativeEventHandler(enterBut_OnInterrupt);

            // Definizione timer
            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 20);
            Timer display_timer = new Timer(new TimerCallback(Display), null, 0, 500);

            Thread.Sleep(Timeout.Infinite);
        }


        private static void Control(object state)
        {
            begin = DateTime.Now;

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

            //Debug.Assert(Imu.AngleXZ >= 1.9);

            // In teoria velocizza lo scheduling...
            Thread.Sleep(0);

        }

        private static void Display(object state)    
        {
            //Debug.Print("Angles: " + Imu.AngleXZ.ToString("f3") + "," + Imu.AngleYZ.ToString("f3"));
            //Debug.Print("Output variables: " + Pid1.OutputValue.ToString("f3") + "," + Pid2.OutputValue.ToString("f3"));
            //Debug.Print("Control run time: " + duration.Milliseconds);

            switch (currentMenu)
            {
                case 0:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("Angles");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write(Imu.AngleXZ.ToString("f5") + "," + Imu.AngleYZ.ToString("f5"));
                    break;
                case 1:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("PID values");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write(Pid1.OutputValue.ToString("f4") + "," + Pid2.OutputValue.ToString("f4"));
                    break;
                case 2:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("ADC readings");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write("                ");
                    break;
                default:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("Errore");
                    break;
            }

            // In teoria velocizza lo scheduling...
            Thread.Sleep(0);
        }

        static void menuBut_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            //Debug.Print("menuBut pressed!");
            currentMenu++;
            if (currentMenu > 2) currentMenu = 0;
            menuBut.ClearInterrupt();
        }

        static void enterBut_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            //Debug.Print("enterBut pressed!");
            Thread test = new Thread(blink);
            test.Priority = ThreadPriority.BelowNormal;
            test.Start();
        }

        static void blink()
        {
            led.Write(true);
            Thread.Sleep(1000);
            led.Write(false);
            Thread.Sleep(0);
        }
    }
}
