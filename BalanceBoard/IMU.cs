using System;
using Microsoft.SPOT;

namespace BalanceBoard
{
    public class IMU
    {
    }

    class Accelerometer
    {
        #region Fields

        static const int d_sens = 340;
        static const int min_sens = 0;
        static const int max_sens = 3000;

        static const int d_offset = 1650;
        static const int min_offset = 0;
        static const int max_offset = 3000;

        static const int d_vRef = 3300;
        static const int min_vRef = 1500;
        static const int max_vRef = 5000;

        static const int d_invert = 1;
        static const int ADC_resolution = 1023;

        int _sens;           // mV / g
        int _offset;         // zero level (mV) @ 0g
        int _vRef;           // ADC voltage reference
        int[] _invert = new int[3];      // -1 if inverted, 1 otherwise

        int[] _rawData = new int[3];     // data from the ADC
        float[] _acc = new float[3];       // readings in g
        float[] _accNorm = new float[3];   // normalized 3D vector of acc[3]

        #endregion

        #region Properties

        public int Sensivity
        {
            get { return _sens; }
            set
            {
                if (value >= min_sens && value <= max_sens) _sens = value;
                else _sens = d_sens;
            }
        }

        public int Offset
        {
            get { return _offset; }
            set
            {
                if (value >= min_offset && value <= max_offset) _offset = value;
                else _offset = d_offset;
            }
        }



        public int VRef
        {
            get { return _vRef; }
            set
            {
                if (value > min_vRef && value < max_vRef) _vRef = value;
                else _vRef = d_vRef;
            }
        }

        public int[] Invert
        {
            get { return _invert; }
            set
            {
                _invert = value;
            }
        }

        #endregion



        public Accelerometer(int sens, int offset, int vref, int[] invert)
        {

        }
    }

    class Gyroscope
    {
    }
}
