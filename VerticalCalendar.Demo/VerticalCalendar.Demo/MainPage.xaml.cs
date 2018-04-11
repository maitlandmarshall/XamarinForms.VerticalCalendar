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
            this.Content = cal = new VerticalCalendar.VerticalCalendar { MaximumDate = DateTime.Now.Date, AlternativeMonthView = true };

            cal.MonthsBecameVisible += Cal_MonthBecameVisible;
		}

        private void Cal_MonthBecameVisible(object sender, List<DateTime> e)
        {
            if(e.Count == 1)
            {
                if(1==1)
                {

                }
            }

            foreach(DateTime dte in e)
            {
                Debug.WriteLine(dte.ToString("MMMM"));
            }
            
        }
    }
}
