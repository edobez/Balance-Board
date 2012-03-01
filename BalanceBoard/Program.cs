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
    public sealed class Program
    {
        // Definizione ingressi                
        static AnalogIn[] analogIn = new AnalogIn[6];
        static InterruptPort[] button = new InterruptPort[4];
        enum Button : int
        {
            menu,
            enter,
            up,
            down
        }

        // Definizione uscite
        static OutputPort led;
        static Motor motor1, motor2;

        // Definizioni oggetti
        static Accelerometer Acc = new Accelerometer();
        static Gyroscope Gyro = new Gyroscope();
        static IMU Imu = new IMU(Acc, Gyro);

        static PID Pid1;
        static PID Pid2;

        // Definizioni per LCD
        static Lcd myLcd;
        static byte currentMenu;

        // Definizioni var. globali
        static TimeSpan duration;
        static DateTime begin;
        static int[] adc = new int[6];

        public static void Main()
        {
            // Inizializzazione ingressi
            analogIn[0] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An0);
            analogIn[1] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An1);
            analogIn[2] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An2);
            analogIn[3] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An3);
            analogIn[4] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An4);
            analogIn[5] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An5);

            button[(int)Button.menu] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di11, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            button[(int)Button.enter] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di34, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            button[(int)Button.up] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di32, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            button[(int)Button.down] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di30, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            // Init uscite
            led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);
            motor1 = new Motor(PWM.Pin.PWM1, 20000);
            motor2 = new Motor(PWM.Pin.PWM2, 20000);

            // Inizializzazione LCD
            var lcdProvider = new GpioLcdTransferProvider((Cpu.Pin)FEZ_Pin.Digital.Di2, (Cpu.Pin)FEZ_Pin.Digital.Di3,
                (Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5, (Cpu.Pin)FEZ_Pin.Digital.Di6,
                (Cpu.Pin)FEZ_Pin.Digital.Di7);
            myLcd = new Lcd(lcdProvider);
            myLcd.Begin(16, 2);

            // Init sensore e PID
            Acc = new Accelerometer();
            Gyro = new Gyroscope();
            Imu = new IMU(Acc,Gyro);
            Pid1 = new PID(1, 0 , 0, -90, 90, 0, 100);
            Pid2 = new PID(1, 0, 0, -90, 90, 0, 100);

            (new int[] { -1, -1, -1 }).CopyTo(Acc.Invert, 0);   // tutti gli invert a -1
            (new int[] { -1, -1 }).CopyTo(Gyro.Invert, 0);      // ....
            Acc.Offset = 1650;
            Gyro.Offset = 1325;

            // Eventi interrupt
            button[(int)Button.menu].OnInterrupt += new NativeEventHandler(menuBut_OnInterrupt);
            button[(int)Button.enter].OnInterrupt += new NativeEventHandler(enterBut_OnInterrupt);

            // Definizione timer
            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 15);
            Timer display_timer = new Timer(new TimerCallback(Display), null, 0, 500);

            Thread.Sleep(Timeout.Infinite);
        }


        private static void Control(object state)
        {
            begin = DateTime.Now;

            // Algoritmo angolo
            for (int i = 0; i < 6; i++)
            {
                adc[i] = analogIn[i].Read();
            }

            Array.Copy(adc, 0, Acc.Raw, 0, 3);
            Array.Copy(adc, 3, Gyro.Raw, 0, 2);

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

            // Invio PID output values ai motori
            //motor[0].Set(20000, (byte)Pid1.OutputValue);
            motor1.set(43.5);

            duration = (DateTime.Now - begin);

            // In teoria velocizza lo scheduling...
            Thread.Sleep(0);

        }

        private static void Display(object state)
        {
            //Debug.Print("Angles: " + Imu.AngleXZ.ToString("f3") + "," + Imu.AngleYZ.ToString("f3"));
            //Debug.Print("Output variables: " + Pid1.OutputValue.ToString("f3") + "," + Pid2.OutputValue.ToString("f3"));
            Debug.Print("Control run time: " + duration.Milliseconds);

            switch (currentMenu)
            {
                case 0:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("Angoli");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write2(Imu.AngleXZ.ToString("f0") + "," + Imu.AngleYZ.ToString("f0"));
                    break;
                case 1:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("PID values");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write2(Pid1.OutputValue.ToString("f0") + "," + Pid2.OutputValue.ToString("f0"));
                    break;
                case 2:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("ADC readings 1");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write2(Acc.RawMV[0].ToString("f0") + "," + Acc.RawMV[1].ToString("f0") + "," + Acc.RawMV[2].ToString("f0"));
                    break;
                case 3:
                    myLcd.SetCursorPosition(0, 0);
                    myLcd.Write2("ADC readings 2");
                    myLcd.SetCursorPosition(0, 1);
                    myLcd.Write2(Gyro.RawMV[0].ToString("f0") + "," + Gyro.RawMV[1].ToString("f0"));
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
            //menuBut.DisableInterrupt();
            currentMenu++;
            if (currentMenu > 3) currentMenu = 0;
            //menuBut.EnableInterrupt();
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
