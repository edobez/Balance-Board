using System;
using Microsoft.SPOT;

using GHIElectronics.NETMF.System;
using ElzeKool;

namespace BalanceBoard
{
    public sealed class IMU
    {
        #region Fields

        const int D_WGYRO = 5;

        Accelerometer oAcc;
        Gyroscope oGyro;

        bool _firstSample = true;
        int _wGyro = D_WGYRO;
        int _signRzGyro;

        float[] _RwAcc = new float[3];
        float[] _RwGyro = new float[3];
        float[] _RwEst = new float[3];
        float[] _Awz = new float[2];

        DateTime _currentTime, _lastTime;
        TimeSpan _deltaTime;

        #endregion


        #region Properties

        public float AngleXZ
        {
            get { return _Awz[0]; }
        }

        public float AngleYZ
        {
            get { return _Awz[1]; }
        }

        public int WGyro
        {
            get { return _wGyro; }
            set
            {
                if (value > 0 && value <= 100) _wGyro = value;
                else throw new ArgumentOutOfRangeException("value", "Argomento fuori dai limiti");
            }
        }

        #endregion


        #region Constructor / Deconstructor

        public IMU(Accelerometer a, Gyroscope g)
        {
            oAcc = a;
            oGyro = g;
        }

        #endregion


        #region Public Methods

        public void compute()
        {
            

            float temp;

            // Gestione tempo per l'integrazione
            _currentTime = DateTime.Now;
            _deltaTime = _currentTime - _lastTime;
            _lastTime = _currentTime;
            
            // Prende i dati dall'oggetto accelerometro
            for(int i=0;i<3;i++)
            {
                 _RwAcc[i] = oAcc.AccNorm[i];
            }
    
            // Se è il primo campione, il vettore stimato è uguale alla lettura dell'acc. Si fa questo per non incorrere in NaN
            if (_firstSample == true)
            {
                for(int w = 0;w <= 2; w++)
                {
                    _RwEst[w] = _RwAcc[w];    //initialize with accelerometer readings
                }
            }
            else // Valuta il vettore RwGyro
            {
        
                if(exMath.Abs(_RwEst[2]) < 0.1)
                {
                    //Rz è troppo piccolo per calcolare gli angoli. In questo caso il nuovo 
                    // vettore RwGyro è uguale al precedente
                    for(int w=0; w<=2; w++)
                    {
                    _RwGyro[w] = _RwEst[w];
                    }
                }
                else 
                {
                    // Calcola gli angoli dalle proiezioni del vettore stimato RwEst
                    for(int i=0;i<2;i++)
                    {
                        temp = oGyro.Gyro[i];
                        temp *= _deltaTime.Milliseconds / (float)1000;

                        _Awz[i] = (float) MathEx.Atan2(_RwEst[i],_RwEst[2]);
                        _Awz[i] = _Awz[i] * 180 / (float)System.Math.PI;
                        _Awz[i] += temp;
                    }
            
                    //Stima del segno della componente z del vettore RwGyro
                    // RzGyro è positivo se l'angolo Axz va da -90 a 90
                    if (_Awz[0] >= -90 && _Awz[0] <= 90) _signRzGyro = 1;
                    else _signRzGyro = -1;
            
                    //Calcoli inversi per determinare RwGyro dagli angoli Awz
                    _RwGyro[0] = (float)MathEx.Sin(_Awz[0] * (float)System.Math.PI / 180);
                    _RwGyro[0] /= (float)MathEx.Sqrt( 1 + (float)MathEx.Pow((float)MathEx.Cos(_Awz[0] * (float)System.Math.PI / 180),2) * (float)MathEx.Pow((float)MathEx.Tan(_Awz[1] * (float)System.Math.PI / 180),2) );
                    _RwGyro[1] = (float)MathEx.Sin(_Awz[1] * (float)System.Math.PI / 180);
                    _RwGyro[1] /= (float)MathEx.Sqrt( 1 + (float)MathEx.Pow((float)MathEx.Cos(_Awz[1] * (float)System.Math.PI / 180),2) * (float)MathEx.Pow((float)MathEx.Tan(_Awz[0] * (float)System.Math.PI / 180),2) );
                    _RwGyro[2] = _signRzGyro * (float)MathEx.Sqrt(1 - (float)MathEx.Pow(_RwGyro[0],2) - (float)MathEx.Pow(_RwGyro[1],2) );
            
                }
        
                //Combina i dati dell'accelerometro e giroscopio per formare il vettore stimato
                for (int w = 0; w < 3; w++)
                {
                    _RwEst[w] = (_RwAcc[w] + _wGyro*_RwGyro[w]) / (1 + _wGyro);
                }
        
                //Normalizza il vettore stimato
                _RwEst = normalize3DVector(_RwEst);
            }
    
            _firstSample = false;
        }

        public static float[] normalize3DVector(float[] vector)
        {
            float R = (float)MathEx.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);

            vector[0] /= R;
            vector[1] /= R;
            vector[2] /= R;

            return vector;
        }

        #endregion
    }

    public class Accelerometer
    {
        #region Fields

        const float D_SENS = 300;
        const float MIN_SENS = 250;
        const float MAX_SENS = 350;

        const float D_OFFSET = 1500;
        const float MIN_OFFSET = 1200;
        const float MAX_OFFSET = 1800;

        const int D_VREF = 3300;
        const int MIN_VREF = 1500;
        const int MAX_VREF = 5000;

        const int D_INVERT = 1;
        const int ADC_RES = 1023;

        float _sens;           // mV / g
        float _offset;         // zero level (mV) @ 0g
        int _vRef;           // ADC voltage reference
        int[] _invert = new int[3];      // -1 if inverted, 1 otherwise

        int[] _raw = new int[3];     // data from the ADC
        float[] _acc = new float[3];       // readings in g
        float[] _accNorm = new float[3];   // normalized 3D vector of acc[3]

        #endregion


        #region Properties

        public float Sensivity
        {
            get { return _sens; }
            set
            {
                if (value >= MIN_SENS && value <= MAX_SENS) _sens = value;
                else _sens = D_SENS;
            }
        }

        public float Offset
        {
            get { return _offset; }
            set
            {
                if (value >= MIN_OFFSET && value <= MAX_OFFSET) _offset = value;
                else _offset = D_OFFSET;
            }
        }



        public int VRef
        {
            get { return _vRef; }
            set
            {
                if (value > MIN_VREF && value < MAX_VREF) _vRef = value;
                else _vRef = D_VREF;
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
                    if (value[i] >= 0 && value[i] <= ADC_RES) _raw[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Valore deve essere fra 0 e ADC_resolution");
                }
            }
        }

        public float[] RawMV
        {
            get
            {
                return new float[] { (float)Raw[0] / 1024 * 3300, (float)Raw[1] / 1024 * 3300, (float)Raw[2] / 1024 * 3300 };
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

        public Accelerometer(float sens, float offset, int vref, int[] invert)
        {
            Sensivity = sens;
            Offset = offset;
            VRef = vref;
            invert.CopyTo(Invert, 0);

            // I valori di default sono assegnati nella creazione degli array.
            //for (int i = 0; i < 3; i++)
            //{
            //    _raw[i] = 0;
            //    _acc[i] = 0;
            //    _accNorm[i] = 0;
            //}
        }

        //Non sono sicuro che funzioni, soprattutto la parte new int....
        public Accelerometer()
            : this(D_SENS, D_OFFSET, D_VREF, new int[3] { 1, 1, 1 })
        { }

        #endregion


        #region Public Methods

        public void compute()
        {
            // Converte i dati grezzi dell'ADC in g
            for (int i = 0; i < 3; i++)
            {
                _acc[i] = _raw[i] * _vRef / ADC_RES;
                _acc[i] -= _offset;
                _acc[i] /= _sens;
                _acc[i] *= _invert[i];
            }

            // Normalizza il vettore
            _acc.CopyTo(_accNorm, 0);
            IMU.normalize3DVector(_accNorm);
        }

        #endregion
    }

    public class Gyroscope
    {
        #region Fields

        const int D_SENS = 2000;
        const int MIN_SENS = 0;
        const int MAX_SENS = 3000;

        const int D_OFFSET = 1340;
        const int MIN_OFFSET = 0;
        const int MAX_OFFSET = 3000;

        const int D_VREF = 3300;
        const int MIN_VREF = 1500;
        const int MAX_VREF = 5000;

        const int D_INVERT = 1;
        const int ADC_RESOLUTION = 1023;

        float _sens;                          // mV / deg/sec
        float _offset;                        // zero level (mV) @ 0 deg/sec
        int _vRef;                          // ADC voltage reference
        int[] _invert = new int[2];         // -1 if inverted, 1 otherwise

        int[] _raw = new int[2];            // data from the ADC
        float[] _gyro = new float[2];          // readings in deg/sec


        #endregion


        #region Properties

        public float Sensivity
        {
            get { return _sens; }
            set
            {
                if (value >= MIN_SENS && value <= MAX_SENS) _sens = value;
                else _sens = D_SENS;
            }
        }

        public float Offset
        {
            get { return _offset; }
            set
            {
                if (value >= MIN_OFFSET && value <= MAX_OFFSET) _offset = value;
                else _offset = D_OFFSET;
            }
        }



        public int VRef
        {
            get { return _vRef; }
            set
            {
                if (value > MIN_VREF && value < MAX_VREF) _vRef = value;
                else _vRef = D_VREF;
            }
        }

        public int[] Invert
        {
            get { return _invert; }
            set
            {
                for (int i = 0; i < 2; i++)
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
                for (int i = 0; i < 2; i++)
                {
                    if (value[i] >= 0 && value[i] <= ADC_RESOLUTION) _raw[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Valore deve essere fra 0 e ADC_resolution");
                }
            }
        }

        public float[] RawMV
        {
            get
            {
                return new float[] { (float)Raw[0] / 1024 * 3300, (float)Raw[1] / 1024 * 3300 };
            }
        }

        public float[] Gyro
        {
            get { return _gyro; }
        }

        #endregion


        #region Construction / Deconstruction

        public Gyroscope(int sens, int offset, int vref, int[] invert)
        {
            Sensivity = sens;
            Offset = offset;
            VRef = vref;
            invert.CopyTo(Invert, 0);

            // Non serve inizializzare
            //for (int i = 0; i < 2; i++)
            //{
            //    _raw[i] = 0;
            //    _gyro[i] = 0;
            //}
        }

        //Non sono sicuro che funzioni, soprattutto la parte new int....
        public Gyroscope()
            : this(D_SENS, D_OFFSET, D_VREF, new int[2] { 1, 1 })
        { }

        #endregion


        #region Public Methods

        public void compute()
        {
            // Converte i dati grezzi dell'ADC in g
            for (int i = 0; i < 2; i++)
            {
                _gyro[i] = _raw[i] * _vRef / ADC_RESOLUTION;
                _gyro[i] -= _offset;
                _gyro[i] /= _sens;
                _gyro[i] *= _invert[i];
            }
        }

        #endregion
    }

    public abstract class Sensor
    {
        #region Fields

        /// <summary>
        /// Permette l'inversione degli assi del sensore.
        /// </summary>
        private int[] invert;

        /// <summary>
        /// Offset del sensore a riposo/vuoto.
        /// </summary>
        private double[] offset;

        /// <summary>
        /// Valori letti dall'ADC e immagazzinati nell'oggetto.
        /// </summary>
        private int[] raw;

        /// <summary>
        /// Sensibilita' del sensore per unita' presa in considerazione.
        /// </summary>
        private double sens;

        /// <summary>
        /// Tensione di riferimento dell'ADC.
        /// </summary>
        private int vRef;

        /// <summary>
        /// Numero dei gradi di liberta' del sensore.
        /// </summary>
        private int dofCount;

        #endregion

        #region Properties

        public int[] Invert
        {
            get { return invert; }
            set
            {
                for (int i = 0; i < dofCount; i++)
                {
                    if (value[i] == 0 && value[i] == 1) invert[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Invert deve essere 1 o -1");
                }
            }
        }

        public double[] Offset
        {
            get { return offset; }
            set
            {
                for (int i = 0; i < dofCount; i++)
                {
                    if (value[i] > 0) offset[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Invert deve essere maggiore di 0");
                }
            }
        }

        public int[] Raw
        {
            get { return raw; }
            set
            {
                for (int i = 0; i < dofCount; i++)
                {
                    // TODO: cambiare il valore 1024 con una costante o variabile
                    if (value[i] >= 0 && value[i] <= 1024) raw[i] = value[i];
                    else throw new ArgumentOutOfRangeException("value[i]", "Valore deve essere fra 0 e ADC_resolution");
                }
            }
        }

        public double Sensivity
        {
            get { return sens; }
            set
            {
                if (value > 0) sens = value;
                else throw new ArgumentOutOfRangeException("value", "Valore deve essere maggiore di 0");
            }
        }

        /// <summary>
        /// Lettura dati grezzi convertiti in millivolt.
        /// </summary>
        public double[] RawMV
        {
            get
            {
                double[] temp = new double[dofCount];
                for (int i = 0; i < dofCount; i++)
                {
                    temp[i] = raw[i];
                    temp[i] *= 3300;
                    temp[i] /= 1024;
                }
                return temp;
            }
        }

        public int VRef
        {
            get { return vRef; }
            set
            {
                if (value > 0) vRef = value;
                else throw new ArgumentOutOfRangeException("value", "Valore deve essere maggiore di 0");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Metodo che calcola i valori i valori di uscita.
        /// </summary>
        public abstract void compute();

        #endregion
    }
}
