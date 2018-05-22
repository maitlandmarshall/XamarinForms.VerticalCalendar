using System;
using System.Collections.Generic;
using System.Text;

namespace VerticalCalendar
{
    public class VerticalCalendarRowGroupedViewModel : List<VerticalCalendarRowViewModel>
    {
        public string Title
        {
            get
            {
                if (this.Count == 0) return "";
                VerticalCalendarRowViewModel first = this[0];
                if(first.FirstDayOfWeek.Month == 1)
                {
                    return first.FirstDayOfWeek.ToString("MMMM yyyy");
                }
                return first.FirstDayOfWeek.ToString("MMMM");
            }
        }
    }
}
