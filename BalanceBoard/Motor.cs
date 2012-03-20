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

        /// <summary>
        /// Dead zone of the motor. Quantity of duty cycle that is unseen by the motor. The interval is symmetrical so this is the semi-amplitude.
        /// </summary>
        int deadzone;

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
            if (enablePin.Read())  // Se il pin enable e' alto (quindi enable spento) la funzione mette la PWM a 50%.
            {
                pwmPin.Set(frequency, 50);
            }
            else
            {
                float newDutyCycle;
                if (dutyCycle >= 50) newDutyCycle = dutyCycle + deadzone;
                else newDutyCycle = dutyCycle - deadzone;

                if (newDutyCycle > 100 || newDutyCycle < 0) throw new ArgumentOutOfRangeException();
                else
                {
                    uint period = (uint)(1e9 / frequency);
                    uint highTime = (uint)(period / 100 * newDutyCycle);

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
        }
    }
}
