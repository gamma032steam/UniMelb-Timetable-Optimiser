using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timetable_Optimiser
{

    public class Class
    {
        public enum ClassType
        {
            Mandatory, Variable
        }

        public ClassType Type { get; set; }
        public string SubjectCode { get; set; }
        public string FullCode { get; set; }

        public string ClassCode
        {
            get
            {
                try
                {
                    return FullCode.Split('/')[4].Trim();
                }
                catch
                {
                    return "Breakout";
                }
            }
        }

        public string ShortDescription => $"{Name} for {SubjectCode} at {Start.ToString()} on {Day.ToString()}";
        public string Name { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public TimeSpan Length => End - Start;
        public string Location { get; set; }
    }

    public static class ClassExtensions
    {
        public static bool DoesClash(this Class a, Class b)
        {
            return (a.Day == b.Day) && (a.Start < b.End && b.Start < a.End);
        }
    }
}
