using System;
using System.Threading;
using System.IO.Ports;
using System.Text;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.USBClient;

using PIDLibrary;
using MicroLiquidCrystal;
using edobezLib;

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
        static Motor Motor1, Motor2;

        // Definizioni oggetti
        static Accelerometer Acc;
        static Gyroscope Gyro;
        static IMU Imu;

        static PID Pid1;
        static PID Pid2;

        static SerialPort UART;
        static StringParser Parser;

        // Definizioni per LCD
        //static Lcd myLcd;
        //static byte currentMenu;

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

            //button[(int)Button.menu] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di11, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            //button[(int)Button.enter] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di34, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            //button[(int)Button.up] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di32, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            //button[(int)Button.down] = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.Di30, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            // Init uscite
            led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);
            Motor1 = new Motor(PWM.Pin.PWM2, (Cpu.Pin)FEZ_Pin.Digital.Di8, 20000);
            Motor2 = new Motor(PWM.Pin.PWM1, (Cpu.Pin)FEZ_Pin.Digital.Di11, 20000);

            // Inizializzazione LCD
            //var lcdProvider = new GpioLcdTransferProvider((Cpu.Pin)FEZ_Pin.Digital.Di2, (Cpu.Pin)FEZ_Pin.Digital.Di3,
            //    (Cpu.Pin)FEZ_Pin.Digital.Di4, (Cpu.Pin)FEZ_Pin.Digital.Di5, (Cpu.Pin)FEZ_Pin.Digital.Di6,
            //    (Cpu.Pin)FEZ_Pin.Digital.Di7);
            //myLcd = new Lcd(lcdProvider);
            //myLcd.Begin(16, 2);

            // Init sensore e PID
            Acc = new Accelerometer();
            Gyro = new Gyroscope();
            Imu = new IMU(Acc,Gyro);
            Pid1 = new PID(1, 0 , 0, -90, 90, 0, 100);
            Pid2 = new PID(1, 0, 0, -90, 90, 15, 85);

            Acc.Invert = new int[] { -1, -1, -1 };
            Gyro.Invert = new int[] { -1, -1 };
            Acc.Offset = new float[] { 1656, 1579, 1650 };      // forse sono da mettere come sopra...
            Gyro.Offset = new float[] { 1328, 1331 };

            // Init porta seriale e parser
            UART = new SerialPort("COM2", 57600);
            UART.ReadTimeout = 200;
            UART.Open();
            UART.DataReceived += new SerialDataReceivedEventHandler(UART_DataReceived);

            Parser = new StringParser();
            Parser.addCommand("ping", Parser_onPing);
            Parser.addCommand("setpoint", Parser_onSetPoint);
            Parser.addCommand("pid", Parser_onPid);
            Parser.addCommand("mt", Parser_onMotorTest);
            Parser.addCommand("pidstop", Parser_onPidStop);
            Parser.addCommand("pidstart", Parser_onPidStart);
            Parser.addCommand("rq", Parser_onSerialMonitor);

            // Eventi interrupt
            //button[(int)Button.menu].OnInterrupt += new NativeEventHandler(menuBut_OnInterrupt);
            //button[(int)Button.enter].OnInterrupt += new NativeEventHandler(enterBut_OnInterrupt);

            // Definizione timer
            Timer sensacq_timer = new Timer(new TimerCallback(SensAcq), null, 0, 10);
            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 25);
            Timer display_timer = new Timer(new TimerCallback(Display), null, 0, 500);

            Thread.Sleep(Timeout.Infinite);
        }

        static void SensAcq(object state)
        {
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
        }

        static void Control(object state)
        {
            led.Write(!led.Read());

            //begin = DateTime.Now;

            // Algoritmo PID
            Pid1.ProcessVariable = Imu.AngleXZ;
            Pid2.ProcessVariable = Imu.AngleYZ;

            Pid1.Compute();
            Pid2.Compute();

            // Invio PID output values ai motori
            Motor1.Set(Pid1.OutputValue);
            Motor2.Set(Pid2.OutputValue);

            //duration = (DateTime.Now - begin);

            // In teoria velocizza lo scheduling...
            Thread.Sleep(0);

        }

        static void Display(object state)
        {
            Debug.Print("Angles: " + Imu.AngleXZ.ToString("f0") + "," + Imu.AngleYZ.ToString("f0"));
            Debug.Print("Output variables: " + Pid1.OutputValue.ToString("f1") + "," + Pid2.OutputValue.ToString("f1"));
            Debug.Print("Control run time: " + duration.Milliseconds);

            Parser_onSerialMonitor(null, 0);

            //switch (currentMenu)
            //{
            //    case 0:
            //        myLcd.SetCursorPosition(0, 0);
            //        myLcd.Write2("Angoli");
            //        myLcd.SetCursorPosition(0, 1);
            //        myLcd.Write2(Imu.AngleXZ.ToString("f0") + "," + Imu.AngleYZ.ToString("f0"));
            //        break;
            //    case 1:
            //        myLcd.SetCursorPosition(0, 0);
            //        myLcd.Write2("PID values");
            //        myLcd.SetCursorPosition(0, 1);
            //        myLcd.Write2(Pid1.OutputValue.ToString("f0") + "," + Pid2.OutputValue.ToString("f0"));
            //        break;
            //    case 2:
            //        myLcd.SetCursorPosition(0, 0);
            //        myLcd.Write2("ADC readings 1");
            //        myLcd.SetCursorPosition(0, 1);
            //        myLcd.Write2(Acc.RawMV[0].ToString("f0") + "," + Acc.RawMV[1].ToString("f0") + "," + Acc.RawMV[2].ToString("f0"));
            //        break;
            //    case 3:
            //        myLcd.SetCursorPosition(0, 0);
            //        myLcd.Write2("ADC readings 2");
            //        myLcd.SetCursorPosition(0, 1);
            //        myLcd.Write2(Gyro.RawMV[0].ToString("f0") + "," + Gyro.RawMV[1].ToString("f0"));
            //        break;
            //    default:
            //        myLcd.SetCursorPosition(0, 0);
            //        myLcd.Write2("Errore");
            //        break;
            //}

            // In teoria velocizza lo scheduling...
            Thread.Sleep(0);
        }

        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Debug.Print("Serial data recieved!");

            Thread.Sleep(5);    // Serve?

            byte[] rxData = new byte[32];
            UART.Read(rxData, 0, rxData.Length);

            //Debug.Print("Received: " + new String(Encoding.UTF8.GetChars(rxData)));

            if (rxData.Length != 0)
            {
                if (Parser.parse(rxData) == false)
                {
                    string message = "Comando sconosciuto";
                    Debug.Print(message);
                    //UART_PrintString(message);
                }
            }
        }

        static void UART_PrintString(string s)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s + "\n");
            UART.Write(buffer, 0, buffer.Length);
        }

        static void Parser_onPing(string[] args, int argNum)
        {
            Debug.Print("Pong!");
            UART_PrintString("Pong!");
        }

        static void Parser_onPid(string[] args, int argNum)
        {
            if (argNum == 4)
            {
                int ch = int.Parse(args[0]);
                if (ch == 1)
                {
                    Pid1.PGain = (float)Double.Parse(args[1]);
                    Pid1.IGain = (float)Double.Parse(args[2]);
                    Pid1.DGain = (float)Double.Parse(args[3]);
                    string message = "Nuovi parametri del canale " + ch + ": " + Pid1.PGain.ToString("f2") + "," + Pid1.IGain.ToString("f2") + "," + Pid1.DGain.ToString("f2");
                    Debug.Print(message);
                    UART_PrintString(message);
                }
                else if (ch == 2)
                {
                    Pid2.PGain = (float)Double.Parse(args[1]);
                    Pid2.IGain = (float)Double.Parse(args[2]);
                    Pid2.DGain = (float)Double.Parse(args[3]);
                    string message = "Nuovi parametri del canale " + ch + ": " + Pid2.PGain + "," + Pid2.IGain + "," + Pid2.DGain;
                    Debug.Print(message);
                    UART_PrintString(message);
                }
                else
                {
                    string message = "Argomento 1 puo' essere 1 o 2";
                    Debug.Print(message);
                    UART_PrintString(message);
                }
            }
            else
            {
                string message = "Numero degli argomenti deve essere uguale a 4";
                Debug.Print(message);
                UART_PrintString(message);
            }
        }

        static void Parser_onPidStart(string[] args, int argNum)
        {
            Motor1.Enable();
            Motor2.Enable();
            Pid1.Enable();
            Pid2.Enable();

            string message = "Pid Started!";
            //Debug.Print(message);
            UART_PrintString(message);
        }

        static void Parser_onPidStop(string[] args, int argNum)
        {
            Motor1.Disable();
            Motor2.Disable();
            Pid1.Disable();
            Pid2.Disable();

            string message = "Pid Stopped!";
            //Debug.Print(message);
            UART_PrintString(message);
        }

        static void Parser_onSetPoint(string[] args, int argNum)
        {
            if (argNum == 2)
            {
                int ch = int.Parse(args[0]);
                if (ch == 1)
                {
                    Pid1.SetPoint = (float) Double.Parse(args[1]);
                    string message = "Nuovo setpoint del canale " + ch + ": " + Pid1.SetPoint;
                    Debug.Print(message);
                    UART_PrintString(message);
                }
                else if (ch == 2)
                {
                    Pid2.SetPoint = (float) Double.Parse(args[1]);
                    string message = "Nuovo setpoint del canale " + ch + ": " + Pid2.SetPoint;
                    Debug.Print(message);
                    UART_PrintString(message);
                }
                else
                {
                    string message = "Argomento 1 puo' essere 1 o 2";
                    Debug.Print(message);
                    UART_PrintString(message);
                }
            }
            else
            {
                string message = "Numero degli argomenti deve essere uguale a 2";
                Debug.Print(message);
                UART_PrintString(message);
            }
        }

        static void Parser_onSerialMonitor(string[] args, int argNum)
        {
            string message;
            message = Imu.AngleXZ.ToString("f1") + "," + Imu.AngleYZ.ToString("f1");
            UART_PrintString(message);
        }

        static void Parser_onMotorTest(string[] args, int argNum)
        {
            Motor motor;
            if (argNum == 3)
            {
                int motorNum = int.Parse(args[0]);
                int testNum = int.Parse(args[1]);

                if (motorNum == 1)
                {
                    motor = Motor1;
                }
                else if (motorNum == 2)
                {
                    motor = Motor2;
                }
                else
                {
                    string message = "Errore parametro motore";
                    Debug.Print(message);
                    UART_PrintString(message);
                    return;
                }

                switch (testNum)
                {
                    case 1: // Motore avanti al 50% per 1 sec.
                        motor.Set(75);
                        Thread.Sleep(1000);
                        motor.Set(50);
                        break;
                    case 2: // Motore indietro al 50% per 1 sec.
                        motor.Set(25);
                        Thread.Sleep(1000);
                        motor.Set(50);
                        break;
                    default:
                        string message = "Numero test errato";
                        Debug.Print(message);
                        UART_PrintString(message);
                        break;
                }

            }
            else
            {
                string message = "Numero degli argomenti deve essere uguale a 3";
                Debug.Print(message);
                UART_PrintString(message);
            }
        }

        //static void menuBut_OnInterrupt(uint data1, uint data2, DateTime time)
        //{
        //    //Debug.Print("menuBut pressed!");
        //    //menuBut.DisableInterrupt();
        //    currentMenu++;
        //    if (currentMenu > 3) currentMenu = 0;
        //    //menuBut.EnableInterrupt();
        //}

        //static void enterBut_OnInterrupt(uint data1, uint data2, DateTime time)
        //{
        //    //Debug.Print("enterBut pressed!");
        //    Thread test = new Thread(blink);
        //    test.Priority = ThreadPriority.BelowNormal;
        //    test.Start();
        //}
    }
}
