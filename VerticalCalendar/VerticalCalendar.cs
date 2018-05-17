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
        public event EventHandler<List<DateTime>> MonthsBecameVisible;

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
        ListView Calendar { get; set; }

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
            this.Calendar = new ListView(ListViewCachingStrategy.RecycleElement);

            this.Calendar.ItemAppearing += Calendar_ItemAppearing;
            this.Calendar.ItemDisappearing += Calendar_ItemDisappearing;

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
                this.Calendar.ScrollTo(group.FirstOrDefault(), group, ScrollToPosition.End, true);

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
                this.Calendar.ScrollTo(weeks.FirstOrDefault(y => y.FirstDayOfWeek == startDate), ScrollToPosition.End, true);
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                this.Loaded = true;
                this.HandleMonthVisiblity();
            });
        }


        private void Calendar_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            VerticalCalendarRowViewModel item = e.Item as VerticalCalendarRowViewModel;
            if (item == null) return;

            DateTime currentMonth = item.FirstDayOfWeek;

            if (this.CurrentDateAppearing == currentMonth) return;
            this.CurrentDateAppearing = currentMonth;

            this.HandleMonthVisiblity();
        }


        private void Calendar_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            VerticalCalendarRowViewModel item = e.Item as VerticalCalendarRowViewModel;
            if (item == null) return;

            DateTime currentMonth = item.FirstDayOfWeek;

            if (this.CurrentDateDisappearing == currentMonth) return;
            this.CurrentDateDisappearing = currentMonth;

            this.HandleMonthVisiblity();
        }


        void HandleMonthVisiblity()
        {
            if (!this.Loaded) return;
            if (this.MonthsBecameVisible == null) return;
            if (this.CurrentDateAppearing == null || this.CurrentDateDisappearing == null) return;

            List<DateTime> range = this.GetVisibleMonthRange();
            if(range.Count <= 1)
            {
                return;
            }

            this.MonthsBecameVisible.Invoke(this, range);
        }

        public List<DateTime> GetVisibleMonthRange()
        {
            if (!this.CurrentDateDisappearing.HasValue && this.CurrentDateAppearing.HasValue)
            {
                this.CurrentDateDisappearing = this.CurrentDateAppearing.Value.AddMonths(-1);
            }

            if (this.CurrentDateAppearing == null || this.CurrentDateDisappearing == null) return null;

            DateTime startMonth, endMonth;
            if (this.CurrentDateAppearing > this.CurrentDateDisappearing)
            {
                startMonth = this.CurrentDateDisappearing.Value;
                endMonth = this.CurrentDateAppearing.Value;
            }
            else
            {
                startMonth = this.CurrentDateAppearing.Value;
                endMonth = this.CurrentDateDisappearing.Value;
            }

            List<DateTime> monthRange = new List<DateTime>();
            DateTime iteratorDate = startMonth;

            while (iteratorDate <= endMonth)
            {
                monthRange.Add(iteratorDate);
                iteratorDate = iteratorDate.AddMonths(1);
            }

            return monthRange;
        }

        void BuildContentGrid()
        {
            this.ContentGrid = new Grid();
            this.ContentGrid.BackgroundColor = Color.Transparent;
            this.ContentGrid.ColumnSpacing = 0;
            this.ContentGrid.RowSpacing = 0;
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid.SetRow(this.Header, 0);
            this.ContentGrid.Children.Add(this.Header);

            Grid.SetRow(this.Calendar, 1);
            this.ContentGrid.Children.Add(this.Calendar);
        }
        #endregion

        internal List<Action> RefreshCallbacks = new List<Action>();
        public void Refresh()
        {
            RefreshCallbacks.ForEach(y => y());
        }

    }
}
