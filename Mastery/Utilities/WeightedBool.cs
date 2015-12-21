using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mastery.Utilities
{
    /// <summary>
    /// A bool that observes instances of true and false. 
    /// Returns true when the required number of consistant true observations has been met. Otherwise false. 
    /// </summary>
    public class WeightedBool
    {
        private int m_weight;
        private int m_True = 10;
        private const int m_False = 0;
        private bool currentState;

        public bool State { get { return currentState; } }
        public int TrueRange { get { return m_True; } set { m_True = value; } }

        /// <summary>
        /// Influence the weight by passing in the observed state
        /// </summary>
        public void AddWeight(bool state)
        {
            if (state)
            {
                m_weight++;
                m_weight = Clamp(m_weight, m_False, m_True);
            }
            else
            {
                m_weight--;
                m_weight = Clamp(m_weight, m_False, m_True);
            }

            if (m_weight >= m_True)
            {
                currentState = true;
            }
            if (m_weight <= m_False)
            {
                currentState = false;
            }
        }

        /// <summary>
        /// Influence the weight directly by passing in a value.
        /// </summary>
        public void AddWeight(int value)
        {
            m_weight = Clamp(m_weight + value, m_False, m_True);

            if (m_weight >= m_True)
            {
                currentState = true;
            }
            if (m_weight <= m_False)
            {
                currentState = false;
            }
        }

        /// <summary>
        /// Force the bool into a true state
        /// </summary>
        public void SetTrue()
        {
            currentState = true;
            m_weight = m_True;
        }

        /// <summary>
        /// Force the bool into a true state
        /// </summary>
        public void SetFalse()
        {
            currentState = false;
            m_weight = m_False;
        }

        /// <summary>
        /// Clamp value to range using min max.
        /// </summary>
        private int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
