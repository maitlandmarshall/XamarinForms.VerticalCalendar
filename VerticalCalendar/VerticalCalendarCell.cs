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

        Grid Cell { get; set; }
        Label DayLabel { get; set; }
        Label MonthLabel { get; set; }

        View CustomOverlay { get; set; }

        public VerticalCalendarCell(VerticalCalendar calendar)
        {
            this.Calendar = calendar;

            this.Cell = new Grid();
            Cell.BackgroundColor = calendar.CellBackgroundColor ?? DefaultBackgroundColor;
            Cell.Margin = new Thickness(1);

            this.DayLabel = new Label();
            this.DayLabel.HorizontalTextAlignment = TextAlignment.End;
            this.DayLabel.VerticalTextAlignment = TextAlignment.End;
            this.DayLabel.FontSize = 8;

            Cell.Children.Add(this.DayLabel);

            this.Content = this.Cell;

            this.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(this.CellTapped) });
        }

        void CellTapped()
        {
            if (this.BindingContext == null) return;            

            DateTime bindingContext = (DateTime)this.BindingContext;
            if (this.IsDisabled(bindingContext)) return;

            this.Calendar.OnDateTapped(bindingContext);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            this.RemoveMonthLabel();
            this.RemoveCustomOverlay();

            if (this.BindingContext == null)
            {
                if(this.Calendar.AlternativeMonthView)
                {
                    this.DayLabel.IsVisible = false;
                }

                return;
            }

            DateTime bindingContext = (DateTime)this.BindingContext;

            // set the cells day of month
            this.DayLabel.Text = bindingContext.Day.ToString();

            this.SetCellBackgroundColor(bindingContext);
            this.SetCellMonthLabel(bindingContext);
            this.SetCellCustomView(bindingContext);

            this.DayLabel.IsVisible = true;
        }

        bool IsDisabled(DateTime bindingContext)
        {
            return bindingContext > this.Calendar.MaximumDate || bindingContext < this.Calendar.MinimumDate;
        }

        void SetCellBackgroundColor(DateTime bindingContext)
        {
            if (this.IsDisabled(bindingContext))
            {
                this.Cell.BackgroundColor = this.Calendar.CellDisabledBackgroundColor ?? DefaultDisabledBackgroundColor;
            }
            else
            {
                this.Cell.BackgroundColor = DefaultBackgroundColor;
            }
        }

        void SetCellMonthLabel(DateTime bindingContext)
        {
            if (this.Calendar.AlternativeMonthView) return;

            if (bindingContext.Day == 1)
            {
                string monthName = bindingContext.ToString("MMM");

                this.MonthLabel = new Label();
                this.MonthLabel.Text = monthName;
                this.Cell.Children.Add(this.MonthLabel);
            }
        }

        void SetCellCustomView(DateTime bindingContext)
        {
            View customOverlay = this.Calendar.OnCustomViewForDateCell(bindingContext);
            if (customOverlay == null) return   ;

            this.Cell.Children.Add(customOverlay);
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
                this.Cell.Children.Remove(this.CustomOverlay);
                this.CustomOverlay = null;
            }
        }
    }
}
