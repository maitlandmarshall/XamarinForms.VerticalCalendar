using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VerticalCalendar
{
    public class VerticalCalendar : ContentView
    {
        public delegate View CustomViewForDateCellDelegate(DateTime date);

        public event CustomViewForDateCellDelegate CustomViewForDateCell;
        public event EventHandler<DateTime> DateTapped;
        public event EventHandler<IEnumerable<VerticalCalendarRowViewModel>> VisibleWeeksAfterScroll;

        internal Func<List<VerticalCalendarRowViewModel>> GetVisibleRows;
        public List<VerticalCalendarRowViewModel> VisibleRows
        {
            get => this.GetVisibleRows?.Invoke();
        }

        internal View OnCustomViewForDateCell(DateTime date)
        {
            return this.CustomViewForDateCell?.Invoke(date);
        }

        internal void OnDateTapped(DateTime date)
        {
            this.DateTapped?.Invoke(this, date);
        }

        Grid ContentGrid { get; set; }
        Grid Header { get; set; }
        VerticalCalendarListView Calendar { get; set; }

        DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        public DateTime? MinimumDate { get; set; }
        public DateTime? MaximumDate { get; set; }

        public Color? CellBackgroundColor { get; set; }
        public Color? CellDisabledBackgroundColor { get; set; }

        bool alternativeMonthView = false;
        public bool AlternativeMonthView
        {
            get
            {
                return this.alternativeMonthView;
            }
            set
            {
                this.alternativeMonthView = value;
                this.Build();
            }
        }

        DateTime? CurrentDateAppearing { get; set; }
        DateTime? CurrentDateDisappearing { get; set; }

        bool Loaded { get; set; } = false;

        public VerticalCalendar(bool isAlternativeMonthView)
        {
            this.AlternativeMonthView = isAlternativeMonthView;
        }

        public VerticalCalendar()
        {
            this.Build();
        }

        void Build()
        {
            BuildHeader();
            BuildCalendar();
            BuildContentGrid();

            this.Content = this.ContentGrid;
        }

        #region BUILDING CALENDAR
        void BuildHeader()
        {
            this.Header = new Grid();
            DateTime startDate = this.GetFirstDayOfWeek();

            for(var i = 0; i < 7; i++)
            {
                string dayOfWeek = startDate.AddDays(i).ToString("ddd");

                // build the column definitions
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                this.Header.ColumnDefinitions.Add(col);

                Label lblDay = new Label();
                lblDay.Text = dayOfWeek;
                lblDay.HorizontalTextAlignment = TextAlignment.Center;
                lblDay.VerticalTextAlignment = TextAlignment.Center;
                lblDay.FontSize = 16;

                Grid.SetColumn(lblDay, i);

                this.Header.Children.Add(lblDay);
            }
        }

        internal DateTime GetFirstDayOfWeek(DateTime? start = null)
        {
            DateTime startDate = start ?? DateTime.Now.Date;
            while (startDate.DayOfWeek != this.FirstDayOfWeek)
            {
                startDate = startDate.AddDays(-1);
            }

            return startDate;
        }

        DateTime GetFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        void BuildCalendar()
        {
            this.Calendar = new VerticalCalendarListView(this);
            this.Calendar.RowsVisibleAfterScroll += Calendar_RowsVisibleAfterScroll;

            if (this.AlternativeMonthView)
            {
                this.Calendar.IsGroupingEnabled = true;
                this.Calendar.GroupDisplayBinding = new Binding("Title");
            }

            this.Calendar.BackgroundColor = Color.Transparent;
            this.Calendar.SeparatorVisibility = SeparatorVisibility.None;

            this.Calendar.ItemTemplate = new DataTemplate(() =>
            {                
                return new VerticalCalendarRow(this);
            });

            DateTime startDate = GetFirstDayOfWeek();

            // generate x years worth of rows
            const int yearsToGenerate = 5;
            if(this.AlternativeMonthView)
            {
                DateTime genDate = this.GetFirstDayOfMonth(startDate.AddYears(-((int)yearsToGenerate / 2)));
                DateTime endDate = startDate.AddYears((int)yearsToGenerate / 2);

                List<VerticalCalendarRowGroupedViewModel> months = new List<VerticalCalendarRowGroupedViewModel>();
                VerticalCalendarRowGroupedViewModel vm = new VerticalCalendarRowGroupedViewModel();

                while (genDate <= endDate)
                {
                    vm.Add(new VerticalCalendarRowViewModel(genDate));

                    DateTime newDate = genDate.AddDays(7);
                    if (newDate.Month != genDate.Month)
                    {
                        DateTime newDateFirstDayOfWeek = this.GetFirstDayOfWeek(newDate);

                        if(newDateFirstDayOfWeek.Month == genDate.Month)
                        {
                            vm.Add(new VerticalCalendarRowViewModel(newDateFirstDayOfWeek));
                        }
                        
                        months.Add(vm);

                        newDate = this.GetFirstDayOfMonth(newDate);
                        vm = new VerticalCalendarRowGroupedViewModel();
                    }

                    genDate = newDate;
                }

                if (!months.Contains(vm)) months.Add(vm);
                this.Calendar.ItemsSource = months;

                VerticalCalendarRowGroupedViewModel group = months.FirstOrDefault(y => y.FirstOrDefault().FirstDayOfWeek >= startDate);
                this.Calendar.ScrollTo(group.FirstOrDefault(), group, ScrollToPosition.End, false);

            } else
            {
                List<VerticalCalendarRowViewModel> weeks = new List<VerticalCalendarRowViewModel>();
                DateTime genDate = this.GetFirstDayOfWeek(startDate.AddYears(-((int)yearsToGenerate / 2)));
                DateTime endDate = startDate.AddYears((int)yearsToGenerate / 2);

                while (genDate <= endDate)
                {
                    genDate = genDate.AddDays(7);
                    weeks.Add(new VerticalCalendarRowViewModel(genDate));
                }

                this.Calendar.ItemsSource = weeks;
                this.Calendar.ScrollTo(weeks.FirstOrDefault(y => y.FirstDayOfWeek == startDate), ScrollToPosition.End, false);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                this.Loaded = true;
            });
        }

        private void Calendar_RowsVisibleAfterScroll(object sender, IEnumerable<VerticalCalendarRowViewModel> e)
        {
            this.VisibleWeeksAfterScroll?.Invoke(this, e);
        }

        void BuildContentGrid()
        {
            this.ContentGrid = new Grid();
            this.ContentGrid.BackgroundColor = Color.Transparent;
            this.ContentGrid.ColumnSpacing = 0;
            this.ContentGrid.RowSpacing = 0;
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetRow(this.Calendar, 1);
            this.ContentGrid.Children.Add(this.Calendar);

            Grid.SetRow(this.Header, 0);
            this.ContentGrid.Children.Add(this.Header);
        }
        #endregion

        internal List<Action> RefreshCallbacks = new List<Action>();
        public void Refresh()
        {
            RefreshCallbacks.ForEach(y => y());
        }

    }
}
