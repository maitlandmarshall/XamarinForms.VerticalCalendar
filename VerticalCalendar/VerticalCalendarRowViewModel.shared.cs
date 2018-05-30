using System;
using System.Collections.Generic;
using System.Text;

namespace VerticalCalendar
{
    public class VerticalCalendarRowViewModel
    {
        public DateTime FirstDayOfWeek { get; set; }

#if ANDROID
        public string GroupHeaderTitle { get; set; }
#endif

        public VerticalCalendarRowViewModel(DateTime date)
        {
            this.FirstDayOfWeek = date;
        }

        public override string ToString()
        {
            return this.FirstDayOfWeek.ToString("yyyy-MM-dd");
        }
    }
}
