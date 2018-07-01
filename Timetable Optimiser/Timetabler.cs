using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Calendar;
using HtmlAgilityPack;
// ReSharper disable All

namespace Timetable_Optimiser
{
    public partial class Timetabler : Form
    {
        List<Appointment> m_Appointments;

        public Timetabler()
        {
            InitializeComponent();
            m_Appointments = new List<Appointment>();
            dayView1.ResolveAppointments += DayView1_ResolveAppointments;
        }

        private List<Timetable> generatedTables = new List<Timetable>();
        private int currentTable = 0;

        private void DayView1_ResolveAppointments(object sender, ResolveAppointmentsEventArgs args)
        {
            args.Appointments = m_Appointments;
        }

        private void Timetabler_Load(object sender, EventArgs e)
        {
            List<string> rohylCodes = new List<string> { "COMP20003", "ELEN20005", "ENGR20004", "SWEN20003" };
            List<string> leeleeCodes = new List<string> { "ARCH20002 ", "ABPL20036", "ABPL20033", "PLAN20002" };
            List<string> mahaCodes = new List<string> { "ECON20001 ", "MGMT20001", "SPAN10002" };
            List<string> davidCodes = new List<string> { "CONS20002", "ABPL20053", "PLAN20002", "GEOM20015" };
            List<string> naufalCodes = new List<string> { "COMP30020", "COMP30026", "COMP30022", "ECOM30004" };
            List<string> habibCodes = new List<string> { "MCEN30020", "ENGR30003", "MCEN30019", "ELEN90066" };
            List<Subject> allSubjects = LoadSubjects(naufalCodes);
            var cleansedSubjects = ApplyRestrictions(allSubjects);
            switch (cleansedSubjects.Item2) 
            {
                case RestrictionOutcome.Invalid:
                    label2.Text = "The restrictions in place are INVALID!";
                    label2.ForeColor = Color.Red;
                    break;
                case RestrictionOutcome.Valid:
                    label2.Text = "The restrictions in place are VALID :)";
                    label2.ForeColor = Color.DarkGreen;
                    break;
            }
            var generatedTimeTables = GeneratePermutations(cleansedSubjects.Item1);

            var optimisedTimeTables = generatedTimeTables.OrderBy(x => x.VariableClassDaySpan).OrderBy(x => x.ClashCount)
                .OrderBy(x=>
                {
                    if (x._daySpans.ContainsKey(DayOfWeek.Wednesday))
                    {
                        Timetable.ClassSpan span = x._daySpans[DayOfWeek.Wednesday];
                        return (span.Last - span.First).TotalHours;
                    }
                    else return 0;
                }).ToList();

            TimetableStatistics(optimisedTimeTables);
            generatedTables = optimisedTimeTables;
            ShowTimetable(optimisedTimeTables[0]);
        }

        public static List<List<T>> AllCombinationsOf<T>(params List<T>[] sets)
        {
            // need array bounds checking etc for production
            var combinations = new List<List<T>>();

            // prime the data
            foreach (var value in sets[0])
                combinations.Add(new List<T> { value });

            foreach (var set in sets.Skip(1))
                combinations = AddExtraSet(combinations, set);

            return combinations;
        }

        private static List<List<T>> AddExtraSet<T>
            (List<List<T>> combinations, List<T> set)
        {
            var newCombinations = from value in set
                                  from combination in combinations
                                  select new List<T>(combination) { value };

            return newCombinations.ToList();
        }

        private List<Timetable> GeneratePermutations(List<Subject> subjects)
        {
            /* Stores all variable classes by type
             i.e:
                Lecture 1 { Possible Class A }
                Workshop 1 { Possible Class A, Possible Class B, Possible Class C}
            Can't think of a more innovative name.
            */
            List<List<Class>> setPool = new List<List<Class>>();
            foreach (var sub in subjects)
            {
                foreach (var classType in sub.ClassTypes)
                {
                    var typeClasses = sub.Classes.Where(x => x.ClassCode == classType.Key).ToList();
                    setPool.Add(typeClasses);
                }
            }

            int count = 1;
            setPool.ForEach(x => count *= x.Count);
            Console.WriteLine($"This number: {count} should match ^ one.");
            var combinations = AllCombinationsOf(setPool.ToArray());
            List<Timetable> loadedTimetables = new List<Timetable>();
            combinations.ForEach(classList => loadedTimetables.Add(new Timetable(classList)));
            Console.WriteLine($"Thanks to the help of StackOverflow, namely Garry Shutler, I just generated {combinations.Count()} permutations.");
            return loadedTimetables;
        }


        private void TimetableStatistics(List<Timetable> tables)
        {
            /* Thanks, https://stackoverflow.com/questions/17434119/how-to-get-frequency-of-elements-from-list-in-c-sharp */
            var clashFrequencies = tables.GroupBy(x => x.ClashCount).ToDictionary(x => x.Key, x => x.Count());
            Console.WriteLine("\nClash Statistics:");
            foreach (var clashType in clashFrequencies.OrderBy(x => x.Key))
            {
                Console.WriteLine($"There are {clashType.Value} timetables with {clashType.Key} clashes.");
            }

            var dayOffFrequencies = tables.GroupBy(x => x.DaysOff).ToDictionary(x => x.Key, x => x.Count());

            Console.WriteLine("\nDays Off Statistics:");
            foreach (var dayOffCount in dayOffFrequencies.OrderBy(x => x.Key))
            {
                Console.WriteLine($"There are {dayOffCount.Value} timetables with {dayOffCount.Key} days off.");
                if (dayOffCount.Key == 0 && dayOffFrequencies.Count == 1)
                {
                    Console.WriteLine("Oof, that must suck :/");
                }
            }

            Console.WriteLine("\nLongest Day Statistics:");
            var longestDayFrequency = tables.GroupBy(x => x.LongestDay).ToDictionary(x => x.Key, x => x.Count());
            foreach (var longestDay in longestDayFrequency.OrderBy(x => x.Key))
            {
                Console.WriteLine(
                    $"There are {longestDay.Value} timetables with the longest day being {longestDay.Key} hours.");
            }

            Console.WriteLine("\nShortest Day Statistics:");
            var shortestDayFrequency = tables.GroupBy(x => x.ShortestDay).ToDictionary(x => x.Key, x => x.Count());
            foreach (var shortestDay in shortestDayFrequency.OrderBy(x => x.Key))
            {
                Console.WriteLine(
                    $"There are {shortestDay.Value} timetables with the shortest day being {shortestDay.Key} hours.");
            }

            Console.WriteLine("\nVariable Class Statistics:");
            var variableSpanCounts = tables.GroupBy(x => x.VariableClassDaySpan).ToDictionary(x => x.Key, x => x.Count());
            foreach (var variableSpanCount in variableSpanCounts.OrderBy(x => x.Key))
            {
                Console.WriteLine(
                    $"There are {variableSpanCount.Value} timetables with variable classes spanning {variableSpanCount.Key} days.");
            }
        }

        private void ShowTimetable(Timetable timetable)
        {
            m_Appointments.Clear();
            label1.Text = $"Timetable {currentTable}/{generatedTables.Count}";
            foreach (var c in timetable.Classes)
            {
                RenderClass(c);
            }
            dayView1.Renderer = new Office11Renderer();
        }

        private void RenderClass(Class c)
        {
            Appointment m_Appointment = new Appointment();
            DateTime startOfWeek = DateTime.Today;
            int increment = (int)c.Day - 1;
            DateTime classDay = startOfWeek.AddDays(increment);
            m_Appointment.StartDate = classDay + c.Start;
            m_Appointment.EndDate = classDay + c.End;
            switch (c.Type)
            {
                case Class.ClassType.Variable:
                    m_Appointment.Color = Color.CadetBlue;
                    break;
                case Class.ClassType.Mandatory:
                    m_Appointment.Color = Color.Crimson;
                    break;
            }
            m_Appointment.Title = c.Type + Environment.NewLine + c.Name + Environment.NewLine + c.SubjectCode + Environment.NewLine + c.Location;
            m_Appointments.Add(m_Appointment);
        }

        private List<Subject> LoadSubjects(List<string> subjectCodes)
        {
            List<Subject> allSubjects = new List<Subject>();
            subjectCodes.ForEach(x => allSubjects.Add(LoadSubject(x)));
            return allSubjects;
        }

        enum RestrictionOutcome
        {
            Valid, Invalid
        }
        /* Applying Restrictions */
        private Tuple<List<Subject>, RestrictionOutcome>ApplyRestrictions(List<Subject> subjects)
        {
            List<Subject> restrictedSubjects = subjects.Clone().ToList();
            RestrictionOutcome restOut = RestrictionOutcome.Valid;
            TimeSpan MorningStart = new TimeSpan(10, 0, 0);
            TimeSpan AfternoonFinish = new TimeSpan(20, 0, 0);
            List<string> deleteNames = new List<string> { "Breakout", "Breakout01", "Breakout1" };

            PermutationAnalysis(restrictedSubjects);

            /* Compile a list of mandatory, one-time classes that can't be varied */
            var mandatorySubjectClasses = new List<Subject>();
            foreach (var sub in subjects)
            {
                var typeCounts = sub.ClassTypes;
                Subject tempSubject = new Subject() { Code = sub.Code };
                for (var index = sub.Classes.Count - 1; index >= 0; index--)
                {
                    Class c = sub.Classes[index];
                    /* Remove weird classes like "breakout" */
                    if (deleteNames.Contains(c.ClassCode))
                    {
                        sub.Classes.Remove(c);
                    }
                    else if (typeCounts[c.ClassCode] == 1)
                    {
                        c.Type = Class.ClassType.Mandatory;
                        tempSubject.AddClass(c);
                        sub.Classes.Remove(c);
                        Console.WriteLine(c.ShortDescription + " is mandatory.");
                    }
                    else
                        c.Type = Class.ClassType.Variable;
                }

                mandatorySubjectClasses.Add(tempSubject);
            }
            /* We now only have variable classes, but there can still be pruning :) */
            int classCount = 0;
            restrictedSubjects.ForEach(x => classCount += x.Classes.Count());
            Console.WriteLine($"\nPrior to pruning there were {classCount} variable classes in total.\n");
            /* Apply custom date/time restrictions */
            foreach (Subject sub in restrictedSubjects)
            {
                int beforeClassCount = sub.Classes.Count;
                int beforeClassTypeCount = sub.ClassTypes.Count;
                for (var index = sub.Classes.Count - 1; index >= 0; index--)
                {
                    Class c = sub.Classes[index];
                    if (c.Start < MorningStart || c.End > AfternoonFinish)
                    {
                        //DumpClasses(new List<Class> {c});
                        sub.Classes.Remove(c);
                    }
                }

                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassTypes.Count;
                Console.WriteLine($"\nRemoved {beforeClassCount - afterClassCount} classes from {sub.Code} due to morning/afternoon time restrictions.");
                if (afterClassTypeCount - beforeClassCount > 0)
                    Console.WriteLine($"[ALERT!] Removed {afterClassTypeCount - beforeClassTypeCount} unique class types from {sub.Code} due to morning/afternoon time restrictions.");
            }

            /* Removing classes that clash with mandatory classes */
            foreach (var sub in restrictedSubjects)
            {
                Console.WriteLine("--------------------------");
                int beforeClassCount = sub.Classes.Count;
                int beforeClassTypeCount = sub.ClassTypes.Count;
                for (int i = sub.Classes.Count - 1; i >= 0; i--)
                {
                    Class var = sub.Classes[i];
                    var allMandatoryClasses = new List<Class>();
                    mandatorySubjectClasses.ForEach(x => allMandatoryClasses.AddRange(x.Classes));
                    foreach (var mand in allMandatoryClasses)
                    {
                        if (mand.DoesClash(var))
                        {
                            Console.WriteLine($"\nVariable Class: {var.ShortDescription} clashes with Mandatory Class: {mand.ShortDescription}");
                            sub.Classes.Remove(var);
                        }
                    }
                }
                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassTypes.Count;
                Console.WriteLine($"\nRemoved {beforeClassCount - afterClassCount} classes from {sub.Code} due to clashes.");
                if (afterClassTypeCount - beforeClassTypeCount > 0)
                {
                    restOut = RestrictionOutcome.Invalid;
                    Console.WriteLine(
                        $"[ALERT!] Removed {afterClassTypeCount - beforeClassTypeCount} unique class types from {sub.Code} due to clashes. This is bad.");
                }
            }

            classCount = 0;
            restrictedSubjects.ForEach(x => classCount += x.Classes.Count());
            Console.WriteLine($"\nAfter pruning there are {classCount} variable classes in total.\n");
            /* Re-merge mandatory classes with pruned variable classes */
            foreach (var sub in mandatorySubjectClasses)
            {
                foreach (var sub2 in restrictedSubjects)
                {
                    if (sub.Code == sub2.Code)
                    {
                        foreach (var subClass in sub.Classes)
                        {
                            sub2.AddClass(subClass);
                        }
                    }
                }
            }
            PermutationAnalysis(restrictedSubjects);
            return new Tuple<List<Subject>, RestrictionOutcome>(restrictedSubjects, restOut);
        }

        private void PermutationAnalysis(List<Subject> subjects)
        {
            int permutations = 1;
            foreach (Subject sub in subjects)
            {
                Console.WriteLine($"{sub.Code}");
                foreach (var types in sub.ClassTypes)
                {
                    permutations *= types.Value;
                    Console.WriteLine($"-> {types.Value} possibilities for {types.Key}");
                }
            }
            Console.WriteLine("Total Permutations: " + permutations + Environment.NewLine);
        }

        private Subject LoadSubject(string subjectCode)
        {
            /* Pretty clean method */
            var html = $"https://sws.unimelb.edu.au/2018/Reports/List.aspx?objects={subjectCode}&weeks=1-52&days=1-7&periods=1-56&template=module_by_group_list";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);
            var classNodes = htmlDoc.DocumentNode.SelectNodes("//*[@class=\"cyon_table\"]/tbody/tr");
            List<Class> allClasses = new List<Class>();
            foreach (var childNode in classNodes)
            {
                var informationNodes = childNode.SelectNodes(".//td");
                DayOfWeek convertedDay;
                Enum.TryParse(informationNodes[2].InnerText.Trim(), out convertedDay);
                string fullCode = informationNodes[0].InnerText.Trim();
                if (fullCode.Contains("SM2"))
                    allClasses.Add(new Class
                    {
                        FullCode = fullCode,
                        Name = informationNodes[1].InnerText.Trim(),
                        Day = convertedDay,
                        Start = TimeSpan.Parse(informationNodes[3].InnerText.Trim()),
                        End = TimeSpan.Parse(informationNodes[4].InnerText.Trim()),
                        Location = informationNodes[7].InnerText.Trim()
                    });
            }
            Subject sub = new Subject { Code = subjectCode };
            allClasses.ForEach(x => sub.AddClass(x));
            return sub;
        }

        private void DumpClasses(List<Class> Classes)
        {
            foreach (Class c in Classes)
            {
                Console.WriteLine("--------------------------");
                Console.WriteLine(c.FullCode);
                Console.WriteLine(c.ClassCode);
                Console.WriteLine(c.Name);
                Console.WriteLine(c.Location);
                Console.WriteLine(c.Day);
                Console.WriteLine(c.Start);
                Console.WriteLine(c.End);
                Console.WriteLine("--------------------------");
            }
        }

        private bool stop = false;
        private void btn_Next_Click(object sender, EventArgs e)
        {
            stop = false;
            new Thread(() =>
            {
                int max = generatedTables.Count;
                int i = 0;
                while (!stop && i++ < max)
                {
                    Invoke((MethodInvoker)delegate { ShowTimetable(generatedTables[++currentTable]); });
                    Thread.Sleep(500);
                }
            }).Start();
        }

        private void btn_Prev_Click(object sender, EventArgs e)
        {
            if (currentTable != 0)
            {
                ShowTimetable(generatedTables[--currentTable]);
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            stop = true;

        }

        private void Btn_Next_Click_1(object sender, EventArgs e)
        {
            if (currentTable < generatedTables.Count)
                ShowTimetable(generatedTables[++currentTable]);
        }
    }
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
