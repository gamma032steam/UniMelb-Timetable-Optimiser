using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
// ReSharper disable All

namespace Timetable_Optimiser
{
    public partial class Timetabler : Form
    {
        public Timetabler()
        {
            InitializeComponent();
        }

        public bool DoesClash(Class a, Class b)
        {
            return (a.Day == b.Day) && (a.Start < b.End && b.Start < a.End);
        }

        private void Timetabler_Load(object sender, EventArgs e)
        {
            List<string> subjectCodes = new List<string> { "COMP20003", "ELEN20005", "ENGR20004", "SWEN20003"};
            List<Subject> allSubjects = LoadSubjects(subjectCodes);
            var cleansedSubjects = ApplyRestrictions(allSubjects);
            GeneratePermutations(cleansedSubjects);
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

        private List<List<Class>> GeneratePermutations(List<Subject> subjects)
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
                    var typeClasses = sub.Classes.Where(x => x.TypeCode == classType.Key).ToList();
                    setPool.Add(typeClasses);
                }   
            }

            int count = 1;
            setPool.ForEach(x => count *= x.Count);
            Console.WriteLine($"This number: {count} should match ^ one.");
            var combinations = AllCombinationsOf(setPool.ToArray());
            Console.WriteLine($"Thanks to the help of StackOverflow, namely https://stackoverflow.com/questions/545703/combination-of-listlistint, I just generated {combinations.Count()} permutations.");
            return null;
        }

        private List<Subject> LoadSubjects(List<string> subjectCodes)
        {
            List<Subject> allSubjects = new List<Subject>();
            subjectCodes.ForEach(x => allSubjects.Add(LoadSubject(x)));
            return allSubjects;
        }

        /* Applying Restrictions */
            private List<Subject> ApplyRestrictions(List<Subject> subjects)
        {
            List<Subject> restrictedSubjects = subjects.Clone().ToList();
            TimeSpan MorningStart = new TimeSpan(9, 0, 0);
            TimeSpan AfternoonFinish = new TimeSpan(17, 0, 0);
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
                    if (deleteNames.Contains(c.TypeCode))
                    {
                        sub.Classes.Remove(c);
                    }
                    else
                    if (typeCounts[c.TypeCode] == 1)
                    {
                        tempSubject.AddClass(c);
                        sub.Classes.Remove(c);
                        Console.WriteLine(c.ShortDescription + " is mandatory.");
                    }
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
                    if (c.Start < MorningStart || c.Start > AfternoonFinish)
                    {
                        //DumpClasses(new List<Class> {c});
                        sub.Classes.Remove(c);
                    }
                }

                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassTypes.Count;
                Console.WriteLine($"\nRemoved {beforeClassCount - afterClassCount} classes from {sub.Code} due to restrictions.");
                Console.WriteLine($"Removed {afterClassTypeCount - beforeClassTypeCount} unique class types from {sub.Code} due to restrictions.");
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
                    mandatorySubjectClasses.ForEach(x=>allMandatoryClasses.AddRange(x.Classes));
                    foreach (var mand in allMandatoryClasses)
                    {
                        if (DoesClash(var, mand))
                        {
                            Console.WriteLine($"\nVariable Class: {var.ShortDescription} clashes with Mandatory Class: {mand.ShortDescription}");
                            sub.Classes.Remove(var);
                        }
                    }
                }
                int afterClassCount = sub.Classes.Count;
                int afterClassTypeCount = sub.ClassTypes.Count;
                Console.WriteLine($"\nRemoved {beforeClassCount - afterClassCount} classes from {sub.Code} due to clashes.");
                Console.WriteLine($"Removed {afterClassTypeCount - beforeClassTypeCount} unique class types from {sub.Code} due to clashes."); 
            }

            classCount = 0;
            restrictedSubjects.ForEach(x => classCount += x.Classes.Count());
            Console.WriteLine($"\nAfter to pruning there were {classCount} variable classes in total.\n");
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
            return restrictedSubjects;
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
            Console.WriteLine("Total Permutations: " + permutations);
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
                Console.WriteLine(c.TypeCode);
                Console.WriteLine(c.Name);
                Console.WriteLine(c.Location);
                Console.WriteLine(c.Day);
                Console.WriteLine(c.Start);
                Console.WriteLine(c.End);
                Console.WriteLine("--------------------------");
            }
        }
    }
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

    }
}
