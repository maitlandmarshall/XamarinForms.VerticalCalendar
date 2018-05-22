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
		public MainPage()
		{
			InitializeComponent();
            this.Title = "Vertical Calendar Demo";

            VerticalCalendar.VerticalCalendar cal;
            this.Content = cal = new VerticalCalendar.VerticalCalendar() { MaximumDate = DateTime.Now.Date, AlternativeMonthView = true};
            cal.VisibleWeeksAfterScroll += Cal_VisibleWeeksAfterScroll;
		}

        private void Cal_VisibleWeeksAfterScroll(object sender, IEnumerable<VerticalCalendar.VerticalCalendarRowViewModel> e)
        {
            List<DateTime> rows = e.ToList().Select(y => y.FirstDayOfWeek).ToList();

            Debug.WriteLine($"MIN DATE: {rows.Min()}");
            Debug.WriteLine($"MAX DATE: {rows.Max()}");
        }
    }
}
