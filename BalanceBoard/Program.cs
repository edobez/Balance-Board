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
        static Accelerometer Acc = new Accelerometer();
        static Gyroscope Gyro = new Gyroscope();
        static IMU Imu = new IMU(Acc,Gyro);

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
            //Acc = new Accelerometer();
            //Gyro = new Gyroscope();
            //Imu = new IMU(Acc, Gyro);

            Timer control_timer = new Timer(new TimerCallback(Control), null, 0, 500);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Control(object state)
        {
            DateTime begin = DateTime.Now;

            // Inserimento dati ADC nell'algoritmo
            Acc.Raw = new int[] { 512, 512, 618 };
            Gyro.Raw = new int[] { 416, 416 };

            Acc.compute();
            Gyro.compute();
            Imu.compute();

            TimeSpan duration = (DateTime.Now - begin);

            Debug.Print("Angles: " + Imu.AngleXZ + "," + Imu.AngleYZ);
            Debug.Print("Duration: " + duration.Milliseconds);

        }
    }
}
