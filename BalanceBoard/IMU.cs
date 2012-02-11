using System;
using Microsoft.SPOT;

using GHIElectronics.NETMF.System;

namespace BalanceBoard
{
    public class IMU
    {
        #region Public Methods

        public static float[] normalize3DVector(float[] vector)
        {
            float[] outArray = new float[3];
            double R;
            R = (vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            R = MathEx.Sqrt(R);

            outArray[0] /= (float)R;
            outArray[1] /= (float)R;
            outArray[2] /= (float)R;

            return outArray;
        }

        #endregion
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

        int[] _raw = new int[3];     // data from the ADC
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
                for (int i = 0; i < 3; i++)
                {
                    if (value[i] == 0 && value[i] == 1) _invert[i] = value[i];
                    else _invert[i] = 0;
                }
            }
        }

        public int[] Raw
        {
            get { return _raw; }
            set
            {
                for (int i = 0; i < 3; i++)
                {
                    if (value[i] >= 0 && value[i] <= ADC_resolution) _raw[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Valore deve essere fra 0 e ADC_resolution");
                }
            }
        }

        public float[] Acc
        {
            get { return _acc; }
        }

        public float[] AccNorm
        {
            get { return _accNorm; }
        }

        #endregion


        #region Construction / Deconstruction

        public Accelerometer(int sens, int offset, int vref, int[] invert)
        {
            this.Sensivity = sens;
            this.Offset = offset;
            this.VRef = vref;
            this.Invert = invert;

            // Non so se serve e se bisogna mettere this.xxxx
            for (int i = 0; i < 3; i++)
            {
                _raw[i] = 0;
                _acc[i] = 0;
                _accNorm[i] = 0;
            }
        }

        //Non sono sicuro che funzioni, soprattutto la parte new int....
        public Accelerometer()
            : this(d_sens, d_offset, d_vRef, new int[3]{1,1,1})
        { }

        #endregion


        #region Private Methods

        private void compute()
        {
            // Converte i dati grezzi dell'ADC in g
            for(int i = 0; i < 3; i++)  
            {
                _acc[i] = _raw[i] * _vRef / ADC_resolution;
                _acc[i] -= _offset;
                _acc[i] /= _sens;
                _acc[i] *= _invert[i];
            }
    
            // Normalizza il vettore
            _accNorm = IMU.normalize3DVector(_acc);
        }

        #endregion
    }

    class Gyroscope
    {
    }
}
