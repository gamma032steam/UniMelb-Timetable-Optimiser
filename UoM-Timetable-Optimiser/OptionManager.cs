using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable All

namespace UoM_Timetable_Optimiser
{
    public class OptionManager
    {
        public struct BasicRestrictions
        {
            public TimeSpan MinStart;
            public TimeSpan MaxFinish; 

            public BasicRestrictions(TimeSpan minStart, TimeSpan maxFinish)
            {
                MinStart = minStart;
                MaxFinish = maxFinish; 
            }
        }

        public enum RestrictionOutcome
        {
            Invalid,
            Valid
        }
        
    }
}
