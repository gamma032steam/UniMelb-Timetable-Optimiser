using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Syncfusion.Windows.Controls;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Syncfusion.Data.Extensions;

namespace UoM_Timetable_Optimiser
{
    public class SubjectListInformation
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public string FullName => Code + " - " + Name;

        public SubjectListInformation(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }

    public static class SubjectListUpdater
    {
        public static List<SubjectListInformation> subjectInformation = new List<SubjectListInformation>();
        /* https://social.msdn.microsoft.com/Forums/vstudio/en-US/6768f963-a568-468f-a0a5-b8841e13ffcd/c-display-week-of-the-year-in-a-label?forum=winforms */
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo
                .InvariantCulture
                .Calendar
                .GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /* This is the week number SWOTVAC is likely to fall under */
        private const int Sem1Swotvac = 22;
        private const int Sem2Swotvac = 43;

        private static int SemesterIdentifier(DateTime date)
        {
            int weekOfYear = GetIso8601WeekOfYear(date);
            if (weekOfYear < Sem1Swotvac)
            {
                return 1;
            }
            else if (weekOfYear < Sem2Swotvac)
            {
                return 2;
            }
            else return 3;
        }

        private const int PageNumbers = 118;

        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UoM Timetable Optimiser");
        private static void SaveSubjectList(int year, int semester, string json)
        {
            string location = Path.Combine(FolderPath, $"{year}_{semester}.json");
            Directory.CreateDirectory(FolderPath);
            File.WriteAllText(location, json);
            Console.WriteLine("JSON exported to " + location);
        }

        private static bool TryLoad(int year, int semester)
        {
            string location = Path.Combine(FolderPath, $"{year}_{semester}.json");
            if (!File.Exists(location))
            {
                return false;
            }
            /* Try to deserialize stored json */
            string json = File.ReadAllText(location);
            subjectInformation = JsonConvert.DeserializeObject<List<SubjectListInformation>>(json);
            return true;

        }

        public static void UpdateCurrentSubjectList()
        {
            int year = DateTime.Now.Year;
            int semester = SemesterIdentifier(DateTime.Now);
            if (TryLoad(year, semester))
            {
                return;
            }
            List<SubjectListInformation> info = new List<SubjectListInformation>();
            if (semester == 3)
            {
                /* This is the period after semester 2 where people will plan for next year */
                semester = 1;
                /* The subject information for next year may not be added yet, so this is a WIP. */
                year += 1;
            }
            var html =
                $"https://handbook.unimelb.edu.au/search?query=&year={year}&types%5B%5D=subject&level_type%5B%5D=all&study_periods%5B%5D=semester_{semester}&study_periods%5B%5D=year_long&area_of_study=all&faculty=all&department=all";
            int subjectNumber = 1;
            for (int i = 1; i < PageNumbers; i++)
            {
                /* URL to the current page */

                var pageHtml = html + $"&page={i}";
                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(pageHtml);
                var searchResults = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class=\"search-results__accordion\"]").ChildNodes;
                foreach (HtmlNode subjectNode in searchResults)
                {
                    var subjectCode = subjectNode.SelectSingleNode(".//span[@class=\"search-results__accordion-code\"]")
                        .InnerText;
                    var title = subjectNode.SelectSingleNode(".//*[@class=\"search-results__accordion-title\"]")
                        .InnerText.Replace(subjectCode, "");
                    info.Add(new SubjectListInformation(subjectCode, title));
                    Console.WriteLine($"{subjectNumber++}: {subjectCode} - {title}");
                }
            }
            subjectInformation = info;
            var json = JsonConvert.SerializeObject(info);
            SaveSubjectList(year, semester, json);
        }
    }
}
