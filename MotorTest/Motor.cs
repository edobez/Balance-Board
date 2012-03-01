using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Hardware;

namespace BalanceBoard
{
    public sealed class Motor
    {
        /// <summary>
        /// PWM port.
        /// </summary>
        private PWM m_pin;

        /// <summary>
        /// Frequency of the PWM.
        /// </summary>
        private int m_frequency;

        public Motor(PWM.Pin pin, int frequency)
        {
            m_pin = new PWM(pin);
            m_frequency = frequency;
        }

        ~Motor()
        {
            m_pin.Dispose();
            Debug.Print("PWM pin disposed");
        }

        public void set(byte dutyCycle)
        {
            if (dutyCycle > 100) throw new ArgumentOutOfRangeException();
            else m_pin.Set(m_frequency, dutyCycle);
        }

        public void set(double dutyCycle)
        {
            if (dutyCycle > 100 || dutyCycle < 0) throw new ArgumentOutOfRangeException();
            else
            {
                uint period = (uint) (1e9 / m_frequency);
                uint highTime = (uint) (period / 100 * dutyCycle);

                Debug.Print("Periodo: " + period.ToString());
                Debug.Print("High time: " + highTime.ToString());
                Debug.Print("Ratio: " + ((float)highTime / period).ToString());

                m_pin.SetPulse(period, highTime);
            }
        }

        
    }
}
