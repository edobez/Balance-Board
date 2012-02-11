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
        static Accelerometer Acc;
        static Gyroscope Gyro = new Gyroscope();
        static IMU Imu;

        static AnalogIn[] aPin = new AnalogIn[6];


        public static void Main()
        {
            // Inizializzazione pin AnalogIn
            aPin[0] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An0);
            aPin[1] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An1);
            aPin[2] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An2);
            aPin[3] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An3);
            aPin[4] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An4);
            aPin[5] = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An5);

            // Inizializzazione sensore
            Acc = new Accelerometer();
            // La instanza del gyro l'ho lasciata fuori dal main per vedere che succede
            Imu = new IMU(Acc, Gyro);

            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 500);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Control(object state)
        {
            DateTime begin = DateTime.Now;

            // Acquisizione dati ADC
            //int[] adcValues = new int[6];
            //for (int i = 0; i < adcValues.Length; i++) adcValues[i] = aPin[i].Read();
            Acc.Raw[0] = aPin[0].Read()*10;
            Acc.Raw[1] = aPin[1].Read()*10;
            Acc.Raw[2] = aPin[2].Read()*10;

            Gyro.Raw[0] = aPin[3].Read()*10;
            Gyro.Raw[1] = aPin[4].Read()*10;

            Acc.compute();
            Gyro.compute();
            Imu.compute();
            TimeSpan duration = (DateTime.Now - begin);

            Debug.Print("Angles: " + Imu.AngleXZ + "," + Imu.AngleYZ);
            Debug.Print("Duration: " + duration.Ticks);

        }
    }
}
