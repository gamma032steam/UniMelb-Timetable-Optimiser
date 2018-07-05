using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Xsl;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Schedule;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Controls.Input;
using Syncfusion.Windows.Shared;
using Color = System.Drawing.Color;

namespace UoM_Timetable_Optimiser
{
    public class Employee
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<SubjectListInformation> SubjectList;

        private List<Timetable> _generatedTables = new List<Timetable>();
        private static Dictionary<OptimisationType, string> _optimisationStrings;
        private readonly ObservableCollection<Subject> _allSubjects;
        private readonly ObservableCollection<string> _currentOptimisationStrings;
        private int _currentTable = 0;

        public MainWindow()
        {

            _allSubjects = new ObservableCollection<Subject>();
            _currentOptimisationStrings = new ObservableCollection<string>();
            SfSkinManager.ApplyStylesOnApplication = true;

            InitializeComponent();
            dataGridClasses.ItemsSource = _allSubjects;
            lstOptimisationStrings.ItemsSource = _currentOptimisationStrings;
            _optimisationStrings = new Dictionary<OptimisationType, string>()
            {
                {OptimisationType.Cram, "Cram into less days"},
                {OptimisationType.DayOptimisation, "Avoid days specified"},
                {OptimisationType.LeastClashes, "Least Clashes"},
                {OptimisationType.LongestRun, "Longest time without a break"}
            };
            /* Activate drag drop functionality in listbox */
            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseMoveEvent,
                new MouseEventHandler(S_PreviewMouseMoveEvent)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent,
                new DragEventHandler(lstOptimisationStrings_Drop)));
            lstOptimisationStrings.ItemContainerStyle = itemContainerStyle;

            timePickerStart.Value = new TimeSpan(9, 00, 00);
            timePickerFinish.Value = new TimeSpan(17, 0, 00);

            /* List of testing codes */
            List<string> rohylCodes = new List<string> { "COMP20003", "ELEN20005", "ENGR20004", "SWEN20003" };
            List<string> leeleeCodes = new List<string> { "ARCH20002 ", "ABPL20036", "ABPL20033", "PLAN20002" };
            List<string> mahaCodes = new List<string> { "ECON20001 ", "MGMT20001", "SPAN10002" };
            List<string> davidCodes = new List<string> { "CONS20002", "ABPL20053", "PLAN20002", "GEOM20015" };
            List<string> naufalCodes = new List<string> { "COMP30020", "COMP30026", "COMP30022", "ECOM30004" };
            List<string> habibCodes = new List<string> { "MCEN30020", "ENGR30003", "MCEN30019", "ELEN90066" };
            List<string> manCodes = new List<string> { "ARCH20002", "ABPL20036", "ABPL20033", "CWRI30001" };
            List<string> danCodes = new List<string> { "BLAW10001", "ENEN20002 ", "ENGR20003", "GEOM20015" };
            List<string> mulCodes = new List<string> { "UNIB30003", "PHYC20015 ", "MAST20030", "PHYC20013" };
            List<string> hisCodes = new List<string> { "CHEN20009", "CHEN20011 ", "FNCE10002", "CHEN20010" };
            List<string> patCodes = new List<string> { "ELEN30011", "ELEN30012 ", "ECOM20001" };
            List<string> rsem2Codes = new List<string> { "COMP10001", "PHYC10004", "ENGR10003", "MAST10007" };
            List<string> kevinCodes = new List<string> { "COMP30026", "SWEN30006", "MAST20026", "MAST20005" };
            /* end list */
            //LoadSubjects(leeleeCodes, Uni.Melbourne);
            new Thread(() =>
            {
                SubjectListUpdater.UpdateCurrentSubjectList();
                Dispatcher.Invoke(() =>
                {
                    SubjectList = SubjectListUpdater.subjectInformation;
                    txt_subjectAdd.SuggestionMode = SuggestionMode.Contains;
                    txt_subjectAdd.AutoCompleteMode = AutoCompleteMode.Suggest;
                    txt_subjectAdd.AutoCompleteSource = SubjectList;
                    txt_subjectAdd.SearchItemPath = "FullName";
                });
            }).Start();


        }

        void S_PreviewMouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (sender is ListBoxItem && e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        void lstOptimisationStrings_Drop(object sender, DragEventArgs e)
        {
            string droppedData = e.Data.GetData(typeof(string)) as string;
            string target = ((ListBoxItem)(sender)).DataContext as string;

            int removedIdx = lstOptimisationStrings.Items.IndexOf(droppedData);
            int targetIdx = lstOptimisationStrings.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                _currentOptimisationStrings.Insert(targetIdx + 1, droppedData);
                _currentOptimisationStrings.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (_currentOptimisationStrings.Count + 1 > remIdx)
                {
                    _currentOptimisationStrings.Insert(targetIdx, droppedData);
                    _currentOptimisationStrings.RemoveAt(remIdx);
                }
            }
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
            var variableSpanCounts =
                tables.GroupBy(x => x.VariableClassDaySpan).ToDictionary(x => x.Key, x => x.Count());
            foreach (var variableSpanCount in variableSpanCounts.OrderBy(x => x.Key))
            {
                Console.WriteLine(
                    $"There are {variableSpanCount.Value} timetables with variable classes spanning {variableSpanCount.Key} days.");
            }
        }

        private void ShowTimetable(int index)
        {
            _currentTable = index;
            scheduleControl.Appointments.Clear();
            lbl_tableNumber.Content = $"Timetable {index + 1}/{_generatedTables.Count}";
            foreach (var c in _generatedTables[index].Classes)
            {
                RenderClass(c);
            }
        }

        private void RenderClass(Class c)
        {
            DateTime startOfWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            int increment = (int)c.Day - 1;
            DateTime classDay = startOfWeek.AddDays(increment);

            ScheduleAppointment app = new ScheduleAppointment();
            app.StartTime = classDay + c.Start;
            app.EndTime = classDay + c.End;
            int locationCount = c.Locations.Count;
            string locationInfo = locationCount > 1 ? $"{locationCount} possible locations." : c.Locations[0];
            app.Location = locationInfo;
            app.Subject = $"{c.SubjectCode}\n{c.Type}\n{c.Name}\n{locationInfo}";
            scheduleControl.Appointments.Add(app);
            Color clr = Color.White;
            switch (c.Type)
            {
                case Class.ClassType.Stream:
                    clr = Color.Green;
                    break;
                case Class.ClassType.Variable:
                    clr = Color.Orange;
                    break;
                case Class.ClassType.Mandatory:
                    clr = Color.RoyalBlue;
                    break;
            }

            app.AppointmentBackground =
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(clr.A, clr.R, clr.G, clr.B));
        }

        private void LoadSubjects(List<string> subjectCodes, Uni university)
        {
            btn_removeSubject.IsEnabled = true;
            /* I'm a UoM student so I have to be pretentious, right? */
            if (university != Uni.Melbourne)
            {
                MessageBox.Show("Re-apply to Melb <3");
                Environment.Exit(0);
            }
            new Thread(() =>
            {
                subjectCodes.ForEach(x =>
                {
                    Subject foundSubject = UniMelbScraper.LoadSubject(x);
                    if (foundSubject == null)
                    {
                        Dispatcher.Invoke(() => MessageBox.Show("There was an error trying to retrieve the subject's timetable :/\nTry again later."));
                    }
                    else
                        _allSubjects.Add(foundSubject);
                });
            }).Start();
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable < _generatedTables.Count - 1)
            {
                ShowTimetable(++_currentTable);
                btn_Prev.IsEnabled = true;
            }
            else
            {
                btn_Next.IsEnabled = false;
            }
        }

        private void btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable != 0)
            {
                ShowTimetable(--_currentTable);
                btn_Next.IsEnabled = true;
            }
            else
            {
                btn_Prev.IsEnabled = false;
            }
        }

        private void btn_addSubject_Click(object sender, RoutedEventArgs e)
        {
            string subjectCode = txt_subjectAdd.Text;
            var stringSplit = subjectCode.Split('-');
            if (stringSplit.Length > 0)
            {
                string possibleCode = stringSplit[0];
                if (SubjectList.All(x => x.Code != possibleCode.Trim()))
                {
                    MessageBox.Show("Cannot identify a subject code from what has been entered.\nTry just typing the subject code.");
                    return;
                }
                subjectCode = possibleCode;
            }
            LoadSubjects(new List<string> { subjectCode }, Uni.Melbourne);
        }

        private void btn_removeSubject_Click(object sender, RoutedEventArgs e)
        {
            Subject sub = (Subject)dataGridClasses.SelectedItem;
            if (sub == null)
            {

            }
            else
            {
                _allSubjects.Remove(sub);
                if (_allSubjects.Count == 0)
                {
                    btn_removeSubject.IsEnabled = false;
                }
            }
        }

        private void btn_generatePermutations_Click(object sender, RoutedEventArgs e)
        {
            if (_allSubjects.Count == 0)
            {
                MessageBox.Show("You must add some subjects before trying to optimise nothing.");
                return;
            }
            TimeSpan earliestStart = timePickerStart.Value.ToDateTime().TimeOfDay;
            TimeSpan latestFinish = timePickerFinish.Value.ToDateTime().TimeOfDay;
            /* TimeSpan earliestStart = new TimeSpan(9, 0, 0);
             TimeSpan latestFinish = new TimeSpan(18, 0, 0);*/
            OptionManager.BasicRestrictions restrictions =
                new OptionManager.BasicRestrictions(earliestStart, latestFinish);
            TimetableGenerator generator = new TimetableGenerator(_allSubjects.ToList(), Uni.Melbourne);
            var result = generator.ApplyRestrictions(restrictions);
            switch (result)
            {
                case OptionManager.RestrictionOutcome.Invalid:
                    lblRestrictionStatus.Content = "The restrictions in place are INVALID!";
                    lblRestrictionStatus.Foreground = Brushes.Red;
                    break;
                case OptionManager.RestrictionOutcome.Valid:
                    lblRestrictionStatus.Content = "The restrictions in place are VALID :)";
                    lblRestrictionStatus.Foreground = Brushes.DarkGreen;
                    break;
            }

            if (result == OptionManager.RestrictionOutcome.Invalid) return;

            var generatedTimeTables = generator.GeneratePermutations();
            var lolwtf = generatedTimeTables.Where(x => x.VariableClassDays.Contains(DayOfWeek.Tuesday));
            if (lolwtf.Any())
            {

            }
            List<OptimisationType> optimisations = new List<OptimisationType>();
            optimisations.Add(OptimisationType.LongestRun);

            List<DayOfWeek> avoidClasses = new List<DayOfWeek>();
            if (chk_Monday.IsChecked ?? false)
                avoidClasses.Add(DayOfWeek.Monday);
            if (chk_Tuesday.IsChecked ?? false)
                avoidClasses.Add(DayOfWeek.Tuesday);
            if (chk_Wednesday.IsChecked ?? false)
                avoidClasses.Add(DayOfWeek.Wednesday);
            if (chk_Thursday.IsChecked ?? false)
                avoidClasses.Add(DayOfWeek.Thursday);
            if (chk_Friday.IsChecked ?? false)
                avoidClasses.Add(DayOfWeek.Friday);

            optimisations = ParseOptimisations(out avoidClasses);
            var optimisedTimeTables =
                generatedTimeTables.Optimise(optimisations, longestRunUpDown.Value.Value, avoidClasses);
            TimetableStatistics(optimisedTimeTables);
            if (optimisedTimeTables.Count == 0)
            {
                lblOptimisationStatus.Content = "The optimisations in place are INVALID!";
                lblOptimisationStatus.Foreground = Brushes.Red;
                return;
            }

            _generatedTables = optimisedTimeTables;
            ShowTimetable(0);
        }

        private List<OptimisationType> ParseOptimisations(out List<DayOfWeek> DayAvoidOrder)
        {
            List<OptimisationType> optimisations = new List<OptimisationType>();
            List<DayOfWeek> dayAvoidOrder = new List<DayOfWeek>();
            if (lstOptimisationStrings.Items.Count == 0)
            {
                DayAvoidOrder = dayAvoidOrder;
                return optimisations;
            }

            foreach (string str in lstOptimisationStrings.Items)
            {
                OptimisationType type;
                if (str.StartsWith("Avoid"))
                {
                    type = OptimisationType.DayOptimisation;
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), str.Replace("Avoid ", ""));
                    dayAvoidOrder.Add(day);
                }
                else type = _optimisationStrings.FirstOrDefault(x => x.Value == str).Key;
                optimisations.Add(type);
            }
            DayAvoidOrder = dayAvoidOrder;
            return optimisations;

        }

        private void OptimisationCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox))
                return;
            CheckBox box = sender as CheckBox;
            string currentString = "Error? Contact developer.";
            Dictionary<OptimisationType, List<CheckBox>> optimisationAssociations =
                new Dictionary<OptimisationType, List<CheckBox>>()
                {
                    {OptimisationType.Cram, new List<CheckBox>() {chk_doCram}},
                    {
                        OptimisationType.DayOptimisation,
                        new List<CheckBox> {chk_Monday, chk_Tuesday, chk_Wednesday, chk_Thursday, chk_Friday}
                    },
                    {OptimisationType.LeastClashes, new List<CheckBox>() {chk_leastClashes}},
                };
            OptimisationType type = optimisationAssociations.FirstOrDefault(x => x.Value.Contains(box)).Key;
            if (type == OptimisationType.DayOptimisation)
            {
                currentString = "Avoid " + box.Name.Replace("chk_", "");
            }
            else
                currentString = _optimisationStrings[type];
            if (box.IsChecked ?? false)
            {
                if (!_currentOptimisationStrings.Contains(currentString))
                    _currentOptimisationStrings.Add(currentString);
            }
            else
                _currentOptimisationStrings.Remove(currentString);
        }

        private void longestRunUpDown_ValueChanging(object sender, ValueChangingEventArgs e)
        {
            if ((double)e.NewValue <= 1.0 || (double)e.NewValue > 24)
            {
                e.Cancel = true;
            }
        }

    }
}
