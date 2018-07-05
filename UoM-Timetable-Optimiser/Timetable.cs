using System;
using System.Collections.Generic;
using System.Linq; 

namespace UoM_Timetable_Optimiser
{
    class Timetable
    {
        public struct ClassSpan
        {
            public TimeSpan First, Last;
        }

        public List<DayOfWeek> VariableClassDays;

        public List<Class> Classes { get; private set; }
        private readonly List<DayOfWeek> _daysPresent;
        public readonly Dictionary<DayOfWeek, ClassSpan> DaySpans;
        public Dictionary<DayOfWeek, double> LongestRuns;

        public Timetable(List<Class> classes)
        {
            Classes = new List<Class>();
            _daysPresent = new List<DayOfWeek>();
            DaySpans = new Dictionary<DayOfWeek, ClassSpan>();
            VariableClassDays = new List<DayOfWeek>();

            classes.ForEach(AddClass);
        }

        public int ClashCount { get; private set; }

        public int DaysOff => 5 - _daysPresent.Count;

        public int VariableClassDaySpan => VariableClassDays.Count;

        public double TotalRunningHours
        {
            get
            {
                if (LongestRuns == null)
                {
                    double fixthis = LongestRun;
                }
                double sum = 0;
                foreach (var keyValuePair in LongestRuns)
                {
                    sum += keyValuePair.Value;
                }
                return sum;
            }
        }

        public double LongestRun
        {
            get
            {
                var dayGroups = Classes.GroupBy(x => x.Day);
                double longestRun = 0;
                Dictionary<DayOfWeek, double> dictLongestRun = new Dictionary<DayOfWeek, double>();
                foreach (var day in dayGroups)
                {
                    var timeSortedClasses = day.OrderBy(x => x.Start);
                    Class lastClass = timeSortedClasses.ToList()[0]; 
                    double currentRun = 0;
                    foreach (var cls in timeSortedClasses)
                    {
                        bool clashesWithLast = lastClass.ClashesWith(cls);
                        double classBreak = (cls.Start - lastClass.End).TotalMinutes;
                        if (currentRun > 1)
                        {

                        }
                        if (!clashesWithLast && classBreak <= 15)
                        {
                            currentRun += cls.Length.TotalHours;
                        }
                        lastClass = cls;

                        if (currentRun > longestRun)
                            longestRun = currentRun;
                    }
                    dictLongestRun.Add(day.Key, currentRun);
                }

                LongestRuns = dictLongestRun;
                return longestRun;
            }

        }

        public double LongestDay
        {
            get
            {
                double longest = 0;
                foreach (var daySpan in DaySpans)
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
                foreach (var daySpan in DaySpans)
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
            if (!_daysPresent.Contains(newClass.Day))
                _daysPresent.Add(newClass.Day);
            if (newClass.Type == Class.ClassType.Variable && !VariableClassDays.Contains(newClass.Day))
            {
                VariableClassDays.Add(newClass.Day);
            }
            if (!DaySpans.ContainsKey(newClass.Day))
            {
                DaySpans.Add(newClass.Day, new ClassSpan { First = newClass.Start, Last = newClass.End });
            }
            else
            {
                ClassSpan cSpan = DaySpans[newClass.Day];
                if (newClass.Start < cSpan.First)
                {
                    cSpan.First = newClass.Start;
                }

                if (newClass.End > cSpan.Last)
                {
                    cSpan.Last = newClass.End;
                }

                DaySpans[newClass.Day] = cSpan;
            }
            foreach (var currClass in Classes)
            {
                if (currClass.ClashesWith(newClass))
                {
                    ClashCount++;
                }
            }
            Classes.Add(newClass);
        }
    }
}
