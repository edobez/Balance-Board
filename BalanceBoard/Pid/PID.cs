using System;
using System.Threading;

namespace PIDLibrary
{
    public class PID
    {

        #region Fields

        private bool mode;

        //Gains
        private float kp;
        private float ki;
        private float kd;

        //Running Values
        private DateTime lastUpdate;
        private float lastInput;
        private float errSum;

        //Storing PV,SP,OV
        private float input;
        private float sp;
        private float output;

        //Max/Min Calculation
        private float inputMax;
        private float inputMin;
        private float outputMax;
        private float outputMin;

        #endregion

        #region Properties

        public bool Mode
        {
            get;
            set;
        }

        public float ProcessVariable
        {
            get { return input; }
            set { input = value; }
        }

        public float SetPoint
        {
            get { return sp; }
            set { sp = value; }
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

        public float PVMin
        {
            get { return inputMin; }
            set { inputMin = value; }
        }

        public float PVMax
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
            sp = 0;
            output = 50;
        }

        //~PID()
        //{
        //    Disable();
        //    readPV = null;
        //    readSP = null;
        //    writeOV = null;
        //}

        #endregion

        #region Public Methods

        public void Compute()
        {
            if (!Mode) return;

            //We need to scale the pv to +/- 100%, but first clamp it
            input = Clamp(input, inputMin, inputMax);
            input = ScaleValue(input, inputMin, inputMax, -1.0f, 1.0f);

            //We also need to scale the setpoint
            float spTemp;
            spTemp = Clamp(sp, inputMin, inputMax);
            spTemp = ScaleValue(spTemp, inputMin, inputMax, -1.0f, 1.0f);

            //Now the error is in percent...
            float err = spTemp - input;

            float pTerm = err * kp;
            float iTerm = 0.0f;
            float dTerm = 0.0f;

            float partialSum = 0.0f;
            DateTime nowTime = DateTime.Now;

            // Non sono sicuro che MinValue possa essere uguale a NULL
            if (lastUpdate != DateTime.MinValue)
            {
                float dT = (float)(nowTime - lastUpdate).Milliseconds / (float)1000;

                //Compute the integral if we have to...
                if (input >= inputMin && input <= inputMax)
                {
                    partialSum = errSum + dT * err;
                    iTerm = ki * partialSum;
                    //La parte sotto e' sperimentale
                    //if (iTerm > 1.0f) iTerm = 1.0f;
                    //else if (iTerm < -1.0f) iTerm = -1.0f;
                }

                if (dT != 0.0f)
                    dTerm = kd * (input - lastInput) / dT;
            }

            lastUpdate = nowTime;
            errSum = partialSum;
            lastInput = input;

            //Now we have to scale the output value to match the requested scale
            float outReal = pTerm + iTerm + dTerm; //messo il meno all'ultimo termine ma non ne sono sicuro

            outReal = Clamp(outReal, -1.0f, 1.0f);
            outReal = ScaleValue(outReal, -1.0f, 1.0f, outputMin, outputMax);

            output = outReal;
        }

        public void Enable()
        {
            Mode = true;
            Reset();
        }

        public void Disable()
        {
            Mode = false;
        }

        public void Reset()
        {
            errSum = 0.0f;
            lastUpdate = DateTime.Now;
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

        //#region Threading

        //private void Run()
        //{

        //    while (true)
        //    {
        //        try
        //        {
        //            int sleepTime = (int)(1000 / computeHz);
        //            Thread.Sleep(sleepTime);
        //            Compute();
        //        }
        //        catch (Exception e)
        //        {

        //        }
        //    }

        //}

        //#endregion

    }
}
