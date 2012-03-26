using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;

namespace BalanceBoard
{
    public sealed class Motor
    {
        /// <summary>
        /// PWM port.
        /// </summary>
        PWM pwmPin;

        /// <summary>
        /// Frequency of the PWM.
        /// </summary>
        int frequency;

        /// <summary>
        /// Enable pin of the motor (of the L298 IC).
        /// </summary>
        OutputPort enablePin;

        public Motor(PWM.Pin pPin,Cpu.Pin ePin, int freq)
        {
            pwmPin = new PWM(pPin);
            frequency = freq;
            enablePin = new OutputPort(ePin, true);
        }

        ~Motor()
        {
            pwmPin.Dispose();
            enablePin.Dispose();
            Debug.Print("PWM pin disposed");
        }

        /// <summary>
        /// Assegna il duty cycle.
        /// </summary>
        /// <param name="dutyCycle">Duty cycle in double</param>
        public void Set(float dutyCycle)
        {
            if (enablePin.Read()) return; // Se il pin enable e' alto (quindi enable spento) la funzione mette la PWM a 50%.
            else
            {
                if (dutyCycle > 100 || dutyCycle < 0) throw new ArgumentOutOfRangeException();
                else
                {
                    uint period = (uint)(1e9 / frequency);
                    uint highTime = (uint)(period / 100 * dutyCycle);

                    pwmPin.SetPulse(period, highTime);
                }
            }
        }

        public void Enable()
        {
            enablePin.Write(false);
        }

        public void Disable()
        {
            enablePin.Write(true);
            Set(50);
        }
    }
}
