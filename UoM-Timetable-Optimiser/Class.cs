using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Timetable_Optimiser
{
    [Serializable]
    public class Class
    {
        public enum ClassType
        {
            Mandatory, Variable, Stream
        }

        public ClassType Type { get; set; }
        public string SubjectCode { get; set; }
        public string FullCode { get; set; }

        public int StreamCode
        {
            get
            {
                try
                {
                    return int.Parse(FullCode.Split('/')[5].Split(' ')[0].Trim());
                }
                catch
                {
                    return -1;
                }
            }
        }
        public char StreamType => ClassCode[0];

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
        public List<string> Locations { get; private set; }

        public Class()
        {
            Locations = new List<string>();
        }
        public void AddLocation(List<string> toAdd)
        {
            Locations.AddRange(toAdd);
            Locations = Locations.Distinct().ToList();
        }
    }
}
