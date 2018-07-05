using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
// ReSharper disable All

namespace UoM_Timetable_Optimiser
{
    public enum Uni
    {
        Melbourne,
        Monash
    }

    class TimetableGenerator
    {
        private List<Subject> Subjects { get; set; }
        private List<Subject> restrictedSubjects { get; set; }

        public TimetableGenerator(List<Subject> subjectInfo, Uni university)
        {
            Subjects = subjectInfo;
        }

        public List<Timetable> GeneratePermutations()
        {
            /* Stores all variable classes by type
             i.e:
                Lecture 1 { Possible Class A }
                Workshop 1 { Possible Class A, Possible Class B, Possible Class C}
            Can't think of a more innovative name.
            */
            List<List<Class>> setPool = new List<List<Class>>();
            foreach (var sub in restrictedSubjects)
            {
                foreach (var classType in sub.ClassCodeTypes)
                {
                    var typeClasses = sub.Classes.Where(x => x.ClassCode == classType.Key).ToList();
                    setPool.Add(typeClasses);
                }
            }

            List<List<List<Class>>> streamPool = new List<List<List<Class>>>();
            foreach (var sub in restrictedSubjects)
            {
                foreach (var streamTypes in sub.StreamContainers)
                {
                    var strms = streamTypes.Value.Streams;
                    List<List<Class>> streamOptions = new List<List<Class>>();
                    if (strms.Count > 0)
                    {
                        foreach (var stream in strms)
                        {
                            streamOptions.Add(stream.Classes);

                        }
                        streamPool.Add(streamOptions);
                    }
                }

            }

            var combinations = new List<List<Class>>();
            if (streamPool.Count > 0)
            {
                var streamCombinations = AllCombinationsOf(streamPool.ToArray());

                foreach (var strmComb in streamCombinations)
                {
                    List<List<Class>> tempclasses = new List<List<Class>>();
                    foreach (var x in strmComb)
                    {
                        foreach (var y in x)
                        {
                            var lol = new List<Class> { y };
                            setPool.Add(lol);
                            tempclasses.Add(lol);
                        }
                    }

                    combinations.AddRange(AllCombinationsOf(setPool.ToArray()));
                    tempclasses.ForEach(x => setPool.Remove(x));
                }
            }
            else
                combinations.AddRange(AllCombinationsOf(setPool.ToArray()));

            List<Timetable> loadedTimetables = new List<Timetable>();
            combinations.ForEach(classList => loadedTimetables.Add(new Timetable(classList)));
            Console.WriteLine($"Thanks to the help of StackOverflow, namely Garry Shutler, I just generated {combinations.Count()} permutations.");
            return loadedTimetables;
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

        public OptionManager.RestrictionOutcome CheckRestrictions(OptionManager.BasicRestrictions restrictions)
        {
            var cleansedSubjects = ApplyRestrictions(Subjects, restrictions);
            double finalPerms = PermutationAnalysis(cleansedSubjects.Item1);
            if (finalPerms > 8000000)
            {
                return OptionManager.RestrictionOutcome.Invalid;
            }
            return cleansedSubjects.Item2;
        }

        public OptionManager.RestrictionOutcome ApplyRestrictions(OptionManager.BasicRestrictions restrictions)
        {
            PermutationAnalysis(Subjects);
            restrictedSubjects = Subjects.Clone().ToList();
            var cleansedSubjects = ApplyRestrictions(Subjects, restrictions);
            restrictedSubjects = cleansedSubjects.Item1;
            double finalPerms = PermutationAnalysis(restrictedSubjects);
            if (finalPerms > 8000000)
            {
                return OptionManager.RestrictionOutcome.Invalid;
            }
            return cleansedSubjects.Item2;
        }

        private double PermutationAnalysis(List<Subject> subjects, bool log = true, bool deeper = false)
        {
            double permutations = 1;
            foreach (Subject sub in subjects)
            {
                foreach (var streamType in sub.StreamContainers)
                {
                    int streamCount = streamType.Value.Streams.Count;
                    if (streamCount > 0)
                        permutations *= streamCount;
                }
                if (log)
                    Console.WriteLine($"{sub.Code}");

                var streamClasses = new List<Class>();
                foreach (var streamType in sub.StreamContainers)
                {
                    streamType.Value.Streams.ForEach(x => streamClasses.AddRange(x.Classes));
                }
                if (log)
                    foreach (var cls in streamClasses.GroupBy(x => x.ClassCode))
                    {
                        Console.WriteLine($"-> {cls.Count()} possibilities for {cls.Key}");
                    }

                foreach (var types in sub.ClassCodeTypes)
                {
                    permutations *= types.Value;
                    if (!log)
                    {
                        continue;
                    }
                    Console.WriteLine($"-> {types.Value} possibilities for {types.Key}");
                    var typeClasses = sub.Classes.Where(x => x.ClassCode == types.Key);
                    if (deeper && typeClasses.Count() < 5)
                    {
                        foreach (var cls in typeClasses.OrderBy(x => x.Day))
                        {
                            Console.WriteLine($"  ->{cls.Start}  on {cls.Day}");
                        }
                    }
                }
            }
            if (log)
                Console.WriteLine("Total Permutations: " + permutations + Environment.NewLine);
            return permutations;
        }

        private List<Subject> MandatoryClasses(List<Subject> subjects)
        {
            var mandatorySubjectClasses = new List<Subject>();
            /* List of substitute classes, secondary lectures, e.t.c. */
            List<string> deleteNames = new List<string> { "Breakout", "Breakout01", "Breakout1","On hold", "On hold 1", "L04", "L05", "L06" };
            foreach (var sub in subjects)
            {
                var typeCounts = sub.ClassCodeTypes;
                Subject tempSubject = new Subject(sub.Code);
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
            return mandatorySubjectClasses;
        }

        private int TotalStreams(List<Subject> subjects)
        {
            int count = 0;
            foreach (var subject in subjects)
            {
                int currentCount = 0;
                foreach (var streamType in subject.StreamContainers.Values)
                {
                    currentCount += streamType.Streams.Count;
                }
                count += currentCount;
            }

            return count;
        }

        private Tuple<List<Subject>, OptionManager.RestrictionOutcome> ApplyRestrictions(List<Subject> subjects, OptionManager.BasicRestrictions rest)
        {
            /* Clone subjects to a new list to perform restriction operations on */
            List<Subject> restrictedSubjects = subjects.Clone().ToList();
            OptionManager.RestrictionOutcome restOut = OptionManager.RestrictionOutcome.Valid;
            /* Compile a list of mandatory, one-time classes that can't be varied */
            var mandatorySubjectClasses = MandatoryClasses(restrictedSubjects);

            /* We now only have variable classes, but there can still be pruning :) */
            int classCount = 0;
            restrictedSubjects.ForEach(x => classCount += x.Classes.Count());

            /* Count number of streams */
            Console.WriteLine($"\nPrior to pruning there were {TotalStreams(subjects)} streams and {classCount} variable classes in total.\n");


            /* Apply custom date/time restrictions */
            foreach (var sub in restrictedSubjects)
            {
                int beforeClassCount = sub.Classes.Count;
                int beforeClassTypeCount = sub.ClassCodeTypes.Count;
                int beforeStreamCount = 0;
                sub.StreamContainers.Values.ToList().ForEach(x => beforeStreamCount += x.Streams.Count);
                for (var index = sub.Classes.Count - 1; index >= 0; index--)
                {
                    Class c = sub.Classes[index];
                    if (c.Start < rest.MinStart || c.End > rest.MaxFinish)
                    {
                        //DumpClasses(new List<Class> {c});
                        sub.Classes.Remove(c);
                    }
                }

                foreach (var streamType in sub.StreamContainers)
                {
                    var streamList = streamType.Value.Streams;
                    for (var index = streamList.Count - 1; index >= 0; index--)
                    {
                        var stream = streamList[index];
                        foreach (Class c in stream.Classes)
                        {
                            if (c.Start < rest.MinStart || c.End > rest.MaxFinish)
                            {
                                streamList.Remove(stream);
                            }
                        }
                    }
                }

                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassCodeTypes.Count;
                int afterStreamCount = 0;
                sub.StreamContainers.Values.ToList().ForEach(x => afterStreamCount += x.Streams.Count);
                Console.WriteLine(
                    $"\nRemoved {beforeStreamCount - afterStreamCount} streams from {sub.Code} due to morning/afternoon restrictions.");
                Console.WriteLine(
                    $"Removed {beforeClassCount - afterClassCount} classes from {sub.Code} due to morning/afternoon time restrictions.");
                if (afterClassTypeCount - beforeClassTypeCount < 0)
                {
                    restOut = OptionManager.RestrictionOutcome.Invalid;
                    Console.WriteLine(
                        $"[ALERT!] Removed {afterClassTypeCount - beforeClassTypeCount} unique class types from {sub.Code} due to morning/afternoon time restrictions.");
                }
            }

            /* Removing classes that clash with mandatory classes */
            foreach (var sub in restrictedSubjects)
            {
                Console.WriteLine("--------------------------");
                int beforeClassCount = sub.Classes.Count;
                int beforeClassTypeCount = sub.ClassCodeTypes.Count;

                for (int i = sub.Classes.Count - 1; i >= 0; i--)
                {
                    Class var = sub.Classes[i];
                    var allMandatoryClasses = new List<Class>();
                    mandatorySubjectClasses.ForEach(x => allMandatoryClasses.AddRange(x.Classes));
                    foreach (var mand in allMandatoryClasses)
                    {
                        if (mand.ClashesWith(var))
                        {
                            Console.WriteLine($"\nVariable Class: {var.ShortDescription} clashes with Mandatory Class: {mand.ShortDescription}");
                            if (sub.ClassCodeTypes[var.ClassCode] != 1)
                                sub.Classes.Remove(var);
                            else Console.WriteLine("Cannot remove the last class! Clash must persist.");
                            break;
                        }
                    }
                }
                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassCodeTypes.Count;
                Console.WriteLine($"\nRemoved {beforeClassCount - afterClassCount} classes from {sub.Code} due to clashes.");
                if (afterClassTypeCount - beforeClassTypeCount < 0)
                {
                    restOut = OptionManager.RestrictionOutcome.Invalid;
                    Console.WriteLine(
                        $"[ALERT!] Removed {beforeClassTypeCount - afterClassTypeCount} unique class types from {sub.Code} due to clashes. This is bad.");
                }
            }

            /* Removing classes that clash with ALL streams lol */
            foreach (var sub in restrictedSubjects)
            {
                foreach (var streamType in sub.StreamContainers)
                {
                    foreach (Class c1 in sub.Classes)
                    {
                        int badStreamCount = 0;
                        foreach (Stream s in streamType.Value.Streams)
                        {
                            foreach (Class c2 in s.Classes)
                            {
                                if (c1.ClashesWith(c2))
                                {
                                    badStreamCount++;
                                }
                            }
                        }
                    }

                }
            }

            classCount = 0;
            restrictedSubjects.ForEach(x => classCount += x.Classes.Count());
            Console.WriteLine($"\nAfter pruning there are {TotalStreams(restrictedSubjects)} streams and {classCount} variable classes in total.\n");
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
            return new Tuple<List<Subject>, OptionManager.RestrictionOutcome>(restrictedSubjects, restOut);

        }
    }
}
