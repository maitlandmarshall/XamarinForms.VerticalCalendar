using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace VerticalCalendar
{
    public class VerticalCalendarRow : ViewCell
    {
        VerticalCalendarRowViewModel Model
        {
            get => this.BindingContext as VerticalCalendarRowViewModel;
        }

        VerticalCalendar Calendar { get; set; }
        List<VerticalCalendarCell> Cells { get; set; } = new List<VerticalCalendarCell>();

        public VerticalCalendarRow(VerticalCalendar calendar)
        {
            this.Calendar = calendar;

            Grid row = new Grid();
            row.ColumnSpacing = 0;
            row.RowSpacing = 0;

            for (int i = 0; i < 7; i++)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                VerticalCalendarCell cell = new VerticalCalendarCell(this.Calendar);
                Grid.SetColumn(cell, i);
                row.Children.Add(cell);

                this.Cells.Add(cell);
            }

            this.View = row;

            calendar.RefreshCallbacks.Add(new Action(this.OnBindingContextChanged));
        }

        protected override void OnBindingContextChanged()
        {
            if (this.Model == null) return;

            DateTime 
                periodStartDate = this.Model.FirstDayOfWeek,
                firstDayOfWeek = this.Calendar.GetFirstDayOfWeek(this.Model.FirstDayOfWeek);

            if(periodStartDate.Month == firstDayOfWeek.Month)
            {
                periodStartDate = firstDayOfWeek;
            }

            int dayOfWeekMatch = 0;
            for(int i = 0; i < this.Cells.Count; i++)
            {
                VerticalCalendarCell cell = this.Cells[i];
                DateTime newDate = periodStartDate.AddDays(dayOfWeekMatch);

                if(this.Calendar.AlternativeMonthView)
                {
                    if(newDate.Month != periodStartDate.Month)

                    {
                        cell.BindingContext = null;
                    } else
                    {
                        if(firstDayOfWeek.AddDays(i).DayOfWeek != newDate.DayOfWeek)
                        {
                            cell.BindingContext = null;
                        } else
                        {
                            cell.BindingContext = newDate;
                            dayOfWeekMatch++;
                        }
                    }
                } else
                {
                    cell.BindingContext = newDate;
                    dayOfWeekMatch++;
                }
            }
        }

    }
}
