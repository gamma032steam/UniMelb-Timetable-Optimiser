using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Extensions;

namespace UoM_Timetable_Optimiser
{
    public enum OptimisationType
    {
        Cram, /* Fit classes into as little days as possible */
        LeastClashes, /* Minimise the number of overall clashes */
        LongestRun, /* Minimise the number of consecutive classes */
        DayOptimisation /* Avoid classes on specified days */
    }

    static class TimetableOptimiser
    {
        /* Given a list of optimisations IN ORDER, optimisations will be applied */
        private static List<Timetable> ApplyOptimisation(List<Timetable> tables, List<OptimisationType> opt, double longestAllowedRun, List<DayOfWeek> avoid)
        {
            var cleansed = tables.OrderBy(x => true);
            int avoidIndex = 0;
            for (var index = 0; index < opt.Count; index++)
            {
                var optimisation = opt[index];
                switch (optimisation)
                {
                    case OptimisationType.Cram:
                        cleansed = cleansed.ThenBy(x => x.VariableClassDaySpan);
                        break;
                    case OptimisationType.LeastClashes:
                        cleansed = cleansed.ThenBy(x => x.ClashCount);
                        break;
                    case OptimisationType.DayOptimisation:
                        var avoidIndex1 = avoidIndex;
                        cleansed = cleansed.ThenBy(x =>
                        {
                            double totalHrs = 0;
                            var index1 = avoidIndex1;
                            var dayClasses = x.Classes.Where(y => y.Day == avoid[index1]);
                            foreach (var cls in dayClasses)
                            {
                                totalHrs += cls.Length.TotalHours;
                            }
                            return totalHrs;
                        });
                        break;
                }

                if (optimisation == OptimisationType.DayOptimisation)
                    avoidIndex++;
            }

            if (opt.Contains(OptimisationType.LongestRun))
            {
                return cleansed.Where(x => x.LongestRun <= longestAllowedRun).ToList();
            }
            return cleansed.ToList();
        }

        /* Public method making avoidDays optional */
        public static List<Timetable> Optimise(this List<Timetable> tables, List<OptimisationType> optimisations, double longestRun, List<DayOfWeek> avoidDays = null)
        {
            avoidDays = avoidDays ?? new List<DayOfWeek>();
            return ApplyOptimisation(tables, optimisations, longestRun, avoidDays);
        }
    }
}
