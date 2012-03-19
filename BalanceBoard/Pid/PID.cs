using System;
using System.Threading;

namespace PIDLibrary
{
    public class PID
    {

        #region Fields

        //Gains
        private float kp;
        private float ki;
        private float kd;

        //Running Values
        private DateTime lastUpdate;
        private float lastPV;
        private float errSum;

        //Reading/Writing Values
        //private Getfloat readPV;
        //private Getfloat readSP;
        //private Setfloat writeOV;

        //Storing PV,SP,OV
        private float pv;
        private float sp;
        private float ov;

        //Max/Min Calculation
        private float pvMax;
        private float pvMin;
        private float outMax;
        private float outMin;

        //Threading and Timing
        //private float computeHz = 1.0f;
        //private Thread runThread;

        #endregion

        #region Properties

        public float ProcessVariable
        {
            get { return pv; }
            set { pv = value; }
        }

        public float SetPoint
        {
            get { return sp; }
            set { sp = value; }
        }

        public float OutputValue
        {
            get { return ov; }
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
            get { return pvMin; }
            set { pvMin = value; }
        }

        public float PVMax
        {
            get { return pvMax; }
            set { pvMax = value; }
        }

        public float OutMin
        {
            get { return outMin; }
            set { outMin = value; }
        }

        public float OutMax
        {
            get { return outMax; }
            set { outMax = value; }
        }

        //public bool PIDOK
        //{
        //    get { return runThread != null; }
        //}

        #endregion

        #region Construction / Deconstruction

        public PID(float pG, float iG, float dG,
            float pMin, float pMax, float oMin, float oMax)
        {
            kp = pG;
            ki = iG;
            kd = dG;
            pvMax = pMax;
            pvMin = pMin;
            outMax = oMax;
            outMin = oMin;
            //readPV = pvFunc;
            //readSP = spFunc;
            //writeOV = outFunc;
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
            //if (readPV == null || readSP == null || writeOV == null)
            //    return;

            //float pv = readPV();
            //float sp = readSP();

            //We need to scale the pv to +/- 100%, but first clamp it
            pv = Clamp(pv, pvMin, pvMax);
            pv = ScaleValue(pv, pvMin, pvMax, -1.0f, 1.0f);

            //We also need to scale the setpoint
            sp = Clamp(sp, pvMin, pvMax);
            sp = ScaleValue(sp, pvMin, pvMax, -1.0f, 1.0f);

            //Now the error is in percent...
            float err = sp - pv;

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
                if (pv >= pvMin && pv <= pvMax)
                {
                    partialSum = errSum + dT * err;
                    iTerm = ki * partialSum;
                }

                if (dT != 0.0f)
                    dTerm = kd * (pv - lastPV) / dT;
            }

            lastUpdate = nowTime;
            errSum = partialSum;
            lastPV = pv;

            //Now we have to scale the output value to match the requested scale
            float outReal = pTerm + iTerm + dTerm;

            outReal = Clamp(outReal, -1.0f, 1.0f);
            outReal = ScaleValue(outReal, -1.0f, 1.0f, outMin, outMax);

            //Write it out to the world
            //writeOV(outReal);
            ov = outReal;
        }

        //public void Enable()
        //{
        //    if (runThread != null)
        //        return;

        //    Reset();

        //    runThread = new Thread(new ThreadStart(Run));
        //    runThread.Start();
        //}

        //public void Disable()
        //{
        //    if (runThread == null)
        //        return;

        //    runThread.Abort();
        //    runThread = null;
        //}

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
