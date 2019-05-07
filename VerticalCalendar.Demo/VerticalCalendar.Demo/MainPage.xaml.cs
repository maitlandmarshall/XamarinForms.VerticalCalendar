using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VerticalCalendarDemo
{
	public partial class MainPage : ContentPage
	{
        VerticalCalendar.VerticalCalendar Calendar;

		public MainPage()
		{
			InitializeComponent();
            this.Title = "Vertical Calendar Demo";

            this.Calendar = new VerticalCalendar.VerticalCalendar(true);
            this.Calendar.BackgroundColor = Color.Transparent;

            this.GridContent.Children.Add(this.Calendar);
        }

        private void Cal_VisibleWeeksAfterScroll(object sender, IEnumerable<VerticalCalendar.VerticalCalendarRowViewModel> e)
        {
            List<DateTime> rows = e.ToList().Select(y => y.FirstDayOfWeek).ToList();

            Debug.WriteLine($"MIN DATE: {rows.Min()}");
            Debug.WriteLine($"MAX DATE: {rows.Max()}");
        }
    }
}
