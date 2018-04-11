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
                return this[0].FirstDayOfWeek.ToString("MMMM");
            }
        }
    }
}
