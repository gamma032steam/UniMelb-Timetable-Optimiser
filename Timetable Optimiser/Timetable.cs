using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timetable_Optimiser
{
    class Timetable
    {
        public struct ClassSpan
        {
            public TimeSpan First, Last;
        }

        private List<DayOfWeek> _variableClassDays;

        public List<Class> Classes { get; private set; }
        private readonly List<DayOfWeek> _daysPresent;
        public readonly Dictionary<DayOfWeek, ClassSpan> _daySpans;

        public Timetable(List<Class> classes)
        {
            Classes = new List<Class>();
            _daysPresent = new List<DayOfWeek>();
            _daySpans = new Dictionary<DayOfWeek, ClassSpan>();
            _variableClassDays = new List<DayOfWeek>();

            classes.ForEach(AddClass);
        }

        public int ClashCount { get; private set; }

        public int DaysOff => 5 - _daysPresent.Count;

        public int VariableClassDaySpan => _variableClassDays.Count;

        public double LongestDay
        {
            get
            {
                double longest = 0;
                foreach (var daySpan in _daySpans)
                {
                    ClassSpan cSpan = daySpan.Value;
                    double length = (cSpan.Last - cSpan.First).TotalHours;
                    if (length > longest)
                    {
                        longest = length;
                    }
                }
                return longest;
            }
        }

        public double ShortestDay
        {
            get
            {
                double shortest = int.MaxValue;
                foreach (var daySpan in _daySpans)
                {
                    ClassSpan cSpan = daySpan.Value;
                    double length = (cSpan.Last - cSpan.First).TotalHours;
                    if (length < shortest)
                    {
                        shortest = length;
                    }
                }
                return shortest;
            }
        }

        public void AddClass(Class newClass)
        {
            foreach (var currClass in Classes)
            {
                if (!_daysPresent.Contains(currClass.Day))
                    _daysPresent.Add(currClass.Day);
                if (currClass.Type == Class.ClassType.Variable && !_variableClassDays.Contains(currClass.Day))
                {
                    _variableClassDays.Add(currClass.Day);
                }
                if (!_daySpans.ContainsKey(currClass.Day))
                {
                    _daySpans.Add(currClass.Day, new ClassSpan{First = currClass.Start, Last=currClass.End});
                }
                else
                {
                    ClassSpan cSpan = _daySpans[currClass.Day];
                    if (currClass.Start < cSpan.First)
                    {
                        cSpan.First = currClass.Start;
                    }

                    if (currClass.End > cSpan.Last)
                    {
                        cSpan.Last = currClass.End;
                    }
                } 
                if (currClass.DoesClash(newClass))
                {
                    ClashCount++;
                }
            }
            Classes.Add(newClass);
        }
    }
}
