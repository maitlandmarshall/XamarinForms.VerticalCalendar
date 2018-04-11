using System;
using System.Collections.Generic;
using System.Text;

namespace VerticalCalendar
{
    public class VerticalCalendarRowViewModel
    {
        public DateTime FirstDayOfWeek { get; set; }

        public VerticalCalendarRowViewModel(DateTime date)
        {
            this.FirstDayOfWeek = date;
        }
    }
}
