using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;

namespace UoM_Timetable_Optimiser
{
    static class UniMelbScraper
    {
        private static bool IsPotentialStream(string classCode)
        {
            return classCode.StartsWith("L") || classCode.StartsWith("P") && int.TryParse(classCode.Remove(0, 1), out int temp);
        }

        public static Subject LoadSubject(string subjectCode)
        {
            /* Pretty clean method */
            var html = $"https://sws.unimelb.edu.au/2018/Reports/List.aspx?objects={subjectCode}&weeks=1-52&days=1-7&periods=1-56&template=module_by_group_list";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);
            if (htmlDoc.Text.Contains("Error processing page"))
            {
                MessageBox.Show($"There was an error trying to retrieve subject with code {subjectCode}.");
                return null;
            }
            var classNodes = htmlDoc.DocumentNode.SelectNodes("//*[@class=\"cyon_table\"]/tbody/tr");
            List<Class> allClasses = new List<Class>();
            Dictionary<char, StreamContainer> streamContainer = new Dictionary<char, StreamContainer>();
            foreach (var childNode in classNodes)
            {
                var informationNodes = childNode.SelectNodes(".//td");
                Enum.TryParse(informationNodes[2].InnerText.Trim(), out DayOfWeek convertedDay);
                string fullCode = informationNodes[0].InnerText.Trim();
                if (!fullCode.Contains("SM2"))
                    continue;
                Class c = new Class
                {
                    FullCode = fullCode,
                    SubjectCode = subjectCode,
                    Name = informationNodes[1].InnerText.Trim(),
                    Day = convertedDay,
                    Start = TimeSpan.Parse(informationNodes[3].InnerText.Trim()),
                    End = TimeSpan.Parse(informationNodes[4].InnerText.Trim())
                };
                c.AddLocation(new List<string>() { informationNodes[7].InnerText.Trim() });

                /* Look to merge locations for non-stream classes */
                if (!IsPotentialStream(c.ClassCode))
                {
                    var sameClassesOnDay = allClasses.Where(x => x != c && x.Day == c.Day && x.ClassCode == c.ClassCode).ToList();
                    for (var j = sameClassesOnDay.Count() - 1; j >= 0; j--)
                    {
                        var c2 = sameClassesOnDay[j];
                        if (c.Start == c2.Start && c.End == c2.End)
                        {
                            c.AddLocation(c2.Locations);
                            allClasses.Remove(c2);
                        }
                    }
                }
                allClasses.Add(c);
            }

            /* Look for possible streams and add them to a separate container */
            for (var index = allClasses.Count - 1; index >= 0; index--)
            {
                var c = allClasses[index];
                if (IsPotentialStream(c.ClassCode))
                {
                    char streamType = c.StreamType;
                    if (!streamContainer.ContainsKey(streamType))
                    {
                        streamContainer.Add(streamType, new StreamContainer());
                    }
                    streamContainer[streamType].AddStreamClass(c.StreamType, c.StreamCode, c);
                    allClasses.Remove(c);
                }
            }

            /* Analyse streams, grouping the ones that start at the same times */
            foreach (var streamType in streamContainer)
            {
                for (var i = streamType.Value.Streams.Count - 1; i >= 0; i--)
                {
                    var stream = streamType.Value.Streams[i];
                    var candidateStreams =
                        streamType.Value.Streams.Where(x => streamType.Value.Streams.IndexOf(x) != i && x.Classes.Count == stream.Classes.Count).ToList();
                    for (var j = candidateStreams.Count - 1; j >= 0; j--)
                    {
                        var compStream = candidateStreams[j];
                        bool doMerge = true;
                        for (var k = stream.Classes.Count - 1; k >= 0; k--)
                        {
                            var c1 = stream.Classes[k];
                            var c2 = compStream.Classes[k];
                            if (c1.Day != c2.Day || c1.Start != c2.Start || c1.End != c2.End)
                            {
                                doMerge = false;
                            }
                        }
                        if (doMerge)
                        {
                            for (var k = stream.Classes.Count - 1; k >= 0; k--)
                            {
                                var c1 = stream.Classes[k];
                                var c2 = compStream.Classes[k];
                                c1.AddLocation(c2.Locations);
                            }

                            streamType.Value.Streams.Remove(compStream);
                            i--;
                        }
                    }
                }
            }


            /* Analyser and see if there are some unneeded streams added */
            for (int i = streamContainer.Count - 1; i >= 0; i--)
            {
                var streamType = streamContainer.ElementAt(i);
                var type = streamType.Key;
                var currStreams = streamType.Value.Streams;
                bool onlyOneClassInStreams = true;
                currStreams.ForEach(x => { onlyOneClassInStreams = x.Classes.Count == 1; });
                if (currStreams.Count == 1 || onlyOneClassInStreams)
                {
                    /* Only 1 stream! */
                    var mandClasses = currStreams[0].Classes;
                    mandClasses.ForEach(x => x.Type = Class.ClassType.Mandatory);
                    allClasses.AddRange(mandClasses);
                    streamContainer.Remove(type);
                }
                else
                {
                    var numberOfStreams = streamType.Value.Streams.Count;
                    Console.WriteLine($"{subjectCode} has {numberOfStreams} streams for type: {type}");
                }
            }


            Subject sub = new Subject(subjectCode, streamContainer);
            allClasses.ForEach(x => sub.AddClass(x));
            return sub;
        }

    }
}

