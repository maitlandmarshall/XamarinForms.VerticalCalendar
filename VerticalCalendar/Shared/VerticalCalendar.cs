using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.VerticalCalendar.Shared
{
    public class VerticalCalendar : ContentView
    {
        Grid ContentGrid { get; set; }
        Grid Header { get; set; }
        ListView Calendar { get; set; }

        DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        public VerticalCalendar()
        {
            Build();
        }

        void Build()
        {
            BuildHeader();
            BuildContentGrid();

            this.Content = this.ContentGrid;
        }

        void BuildHeader()
        {
            this.Header = new Grid();
            for(var i = 0; i < 7; i++)
            {
                // build the column definitions
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(0, GridUnitType.Star);
                this.Header.ColumnDefinitions.Add(col);

                // build the labels and place them in the column
                DayOfWeek day = (DayOfWeek)i;

                Label lblDay = new Label();
                lblDay.Text = day.ToString();
                lblDay.HorizontalTextAlignment = TextAlignment.Center;
                lblDay.VerticalTextAlignment = TextAlignment.Center;

                Grid.SetColumn(lblDay, i);

                this.Header.Children.Add(lblDay);
            }
        }

        void BuildCalendar()
        {
            this.Calendar = new ListView();

            DateTime startDate = DateTime.Now.Date;
            while(startDate.DayOfWeek != this.FirstDayOfWeek)
            {
                startDate = startDate.AddDays(-1);
            }

            // generate 100 years worth of rows
            List<DateTime> weeks = new List<DateTime>();

            DateTime genDate = startDate;
            while(genDate >= startDate.AddYears(-50))
            {
                weeks.Add(genDate);
                genDate = genDate.AddDays(-7);
            }

            genDate = startDate.AddDays(7);
            while(genDate <= startDate.AddYears(50))
            {
                weeks.Add(genDate);
                genDate = genDate.AddDays(7);
            }

            this.Calendar.ItemsSource = weeks;
        }

        void BuildContentGrid()
        {
            this.ContentGrid = new Grid();
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            this.ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });

            Grid.SetRow(this.Header, 0);
            this.ContentGrid.Children.Add(this.Header);
        }

        
    }
}
