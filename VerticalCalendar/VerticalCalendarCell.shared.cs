using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace VerticalCalendar
{
    public class VerticalCalendarCell : ContentView
    {
        static Color DefaultBackgroundColor = Color.FromHex("#FAF9FA");
        static Color DefaultDisabledBackgroundColor = Color.FromHex("#CCCEDB");

        VerticalCalendar Calendar { get; set; }

        Grid Container { get; set; }
        Grid Cell { get; set; }

        Label DayLabel { get; set; }
        Label MonthLabel { get; set; }

        Grid CurrentDate { get; set; }

        View CustomOverlay { get; set; }

        DateTime? Date
        {
            get => this.BindingContext as DateTime?;
        }

        public VerticalCalendarCell(VerticalCalendar calendar)
        {
            this.Calendar = calendar;

            this.Container = new Grid();
            this.Cell = new Grid();
            Cell.BackgroundColor = calendar.CellBackgroundColor ?? DefaultBackgroundColor;
            Cell.Margin = new Thickness(1);
            this.Container.Children.Add(this.Cell);

            this.DayLabel = new Label();
            this.DayLabel.HorizontalTextAlignment = TextAlignment.Center;
            this.DayLabel.VerticalTextAlignment = TextAlignment.Center;
            this.DayLabel.FontSize = 14;

            Cell.Children.Add(this.DayLabel);

            this.Content = this.Container;

            this.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(this.CellTapped) });
        }

        void CellTapped()
        {
            if (!this.Date.HasValue) return;            
            if (this.IsDisabled()) return;

            this.Calendar.OnDateTapped(this.Date.Value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            this.RefreshCell();
        }

        public void RefreshCell()
        {
            this.RemoveMonthLabel();
            this.RemoveCustomOverlay();
            this.RemoveCurrentDate();

            if (!this.Date.HasValue)
            {
                if (this.Calendar.AlternativeMonthView)
                {
                    this.DayLabel.IsVisible = false;
                    this.SetCellBackgroundColor();
                }

                return;
            }


            // set the cells day of month
            this.DayLabel.Text = this.Date.Value.Day.ToString();

            this.SetCellBackgroundColor();
            this.SetCellMonthLabel();
            this.SetCellCustomView();

            this.SetCurrentDate();

            this.DayLabel.IsVisible = true;
        }

        bool IsDisabled()
        {
            if (!this.Date.HasValue) return true;
            return this.Date.Value > this.Calendar.MaximumDate || this.Date.Value < this.Calendar.MinimumDate;
        }

        void SetCurrentDate()
        {
            if (this.Date.Value == DateTime.Now.Date)
            {
                this.CurrentDate = new Grid();
                this.CurrentDate.BackgroundColor = Color.FromHex("#2CB044");
                this.CurrentDate.Opacity = 0.4;

                this.Cell.Children.Add(this.CurrentDate);
                this.Cell.LowerChild(this.CurrentDate);
            }
        }

        void SetCellBackgroundColor()
        {
            if (this.IsDisabled())
            {
                this.Cell.BackgroundColor = this.Calendar.CellDisabledBackgroundColor ?? DefaultDisabledBackgroundColor;
            }
            else
            {
                this.Cell.BackgroundColor = DefaultBackgroundColor;
            }
        }

        void SetCellMonthLabel()
        {
            if (this.Calendar.AlternativeMonthView) return;

            if (this.Date.Value.Day == 1)
            {
                string monthName = this.Date.Value.ToString("MMM");

                this.MonthLabel = new Label();
                this.MonthLabel.Text = monthName;
                this.Cell.Children.Add(this.MonthLabel);
            }
        }

        void SetCellCustomView()
        {
            View customOverlay = this.Calendar.OnCustomViewForDateCell(this.Date.Value);
            if (customOverlay == null) return   ;

            this.Container.Children.Add(customOverlay);
            this.CustomOverlay = customOverlay;   
        }

        void RemoveMonthLabel()
        {
            if (this.MonthLabel != null)
            {
                this.Cell.Children.Remove(this.MonthLabel);
                this.MonthLabel = null;
            }
        }

        void RemoveCustomOverlay()
        {
            if(this.CustomOverlay != null)
            {
                this.Container.Children.Remove(this.CustomOverlay);
                this.CustomOverlay = null;
            }
        }

        void RemoveCurrentDate()
        {
            if(this.CurrentDate != null)
            {
                this.Cell.Children.Remove(this.CurrentDate);
                this.CurrentDate = null;
            }
        }
    }
}
