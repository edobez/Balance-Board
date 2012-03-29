using System;
using System.Threading;

namespace edobezLib
{
    public class PID
    {

        public enum Mode
        {
            Manual,
            Automatic
        }

        #region Fields

        //Settings
        bool inAuto = false; // inizia spento
        int deadzone;

        //Gains
        float kp;
        float ki;
        float kd;

        //Running Values
        float pTerm;
        float iTerm;
        float dTerm;

        DateTime lastUpdate;
        float lastInput;
        float errSum;
        float lastErr;

        //Storing PV,SP,OV
        float input;
        float setpoint;
        float output;

        //Max/Min Calculation
        float inputMax;
        float inputMin;
        float outputMax;
        float outputMin;

        #endregion

        #region Properties

        public int Deadzone
        {
            get { return deadzone; }
            set
            {
                if (value >= 0 && value <= (OutMax - OutMin)/2)
                {
                    deadzone = value;
                }
                else throw new ArgumentOutOfRangeException();
            }
        }

        public float Input
        {
            get { return input; }
            set { input = value; }
        }

        public float SetPoint
        {
            get { return setpoint; }
            set { setpoint = value; }
        }

        public float OutputValue
        {
            get { return output; }
        }

        public float PGain
        {
            get { return kp; }
            set { kp = value; }
        }

        public float IGain
        {
            get { return ki; }
            set { ki = value; }
        }

        public float DGain
        {
            get { return kd; }
            set { kd = value; }
        }

        public float InputMin
        {
            get { return inputMin; }
            set { inputMin = value; }
        }

        public float InputMax
        {
            get { return inputMax; }
            set { inputMax = value; }
        }

        public float OutMin
        {
            get { return outputMin; }
            set { outputMin = value; }
        }

        public float OutMax
        {
            get { return outputMax; }
            set { outputMax = value; }
        }

        #endregion

        #region Construction / Deconstruction

        public PID(float pG, float iG, float dG,
            float pMin, float pMax, float oMin, float oMax)
        {
            kp = pG;
            ki = iG;
            kd = dG;
            inputMax = pMax;
            inputMin = pMin;
            outputMax = oMax;
            outputMin = oMin;

            iTerm = 0.0f;

            //TODO: inizializzare a metà dell'uscita possibile (outmax - outmin)/2
            //setpoint = 0;
            output = 50;  
        }

        #endregion

        #region Public Methods

        public void Compute()
        {
            if (!inAuto) return; // Se la modalita' e' manuale non parte

            //We need to scale the pv to +/- 100%, but first clamp it
            input = Clamp(input, inputMin, inputMax);
            input = ScaleValue(input, inputMin, inputMax, -1.0f, 1.0f);

            //We also need to scale the setpoint
            float spScaled;
            spScaled = Clamp(setpoint, inputMin, inputMax);
            spScaled = ScaleValue(spScaled, inputMin, inputMax, -1.0f, 1.0f);

            //Now the error is in percent...
            float error = spScaled - input;

            //pTerm = error * kp;

            DateTime nowTime = DateTime.Now;

            if (lastUpdate != DateTime.MinValue)
            {
                float timeChange = (nowTime - lastUpdate).Milliseconds / 1000.0f;
                float dInput = (input - lastInput) / timeChange;

                pTerm = kp * error;
                iTerm += ki * (error * timeChange); // correzione per cambi ai parametri
                //TODO: provare correzione suggerita da Will, su un commento della pagina di Brett
                if (iTerm > 1.0f) iTerm = 1.0f;         // correzione windup
                else if (iTerm < -1.0f) iTerm = -1.0f;  // ^^
                dTerm = -kd * dInput; //correzione per Derivative Kick
    
            }

            float outReal = pTerm + iTerm + dTerm; 

            lastUpdate = nowTime;
            lastInput = input;

            outReal = Clamp(outReal, -1.0f, 1.0f);
            outReal = ScaleValue(outReal, -1.0f, 1.0f, (outputMin + deadzone), (outputMax - deadzone));

            //ATTENZIONE: il valore a 50 è fissato in modo hardcoded
            if (outReal >= 50) output = outReal + deadzone;
            else output = outReal - deadzone;
        }

        public void SetMode(Mode mode)
        {
            bool newAuto = (mode == Mode.Automatic);
            if (newAuto && !inAuto)
            {   // andato da manuale ad automatico
                Init();
            }
            inAuto = newAuto;
        }

        //TODO: verificare questa funzione! il settaggio dell'iTerm non mi piace..
        public void Init()
        {
            input = Clamp(input, inputMin, inputMax);
            input = ScaleValue(input, inputMin, inputMax, -1.0f, 1.0f);
            lastInput = input;

            float temp;
            // qui forse bisogna aggiungere il clamping dell'ingresso
            temp = ScaleValue(output, outputMin, outputMax, -1.0f, 1.0f);
            iTerm = temp;
            if (iTerm > 1.0f) iTerm = 1.0f;         // correzione windup
            else if (iTerm < -1.0f) iTerm = -1.0f;  // ^^

        }

        #endregion

        #region Private Methods

        private float ScaleValue(float value, float valuemin, float valuemax, float scalemin, float scalemax)
        {
            float vPerc = (value - valuemin) / (valuemax - valuemin);
            float bigSpan = vPerc * (scalemax - scalemin);

            float retVal = scalemin + bigSpan;

            return retVal;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        #endregion



    }
}
