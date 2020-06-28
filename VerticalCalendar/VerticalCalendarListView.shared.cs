using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace VerticalCalendar
{
    internal class VerticalCalendarScrollToRequestedEventArgs : EventArgs
    {
        public object Item { get; set; }
        public object Group { get; set; }
        public ScrollToPosition Position { get; set; }
        public bool Animated { get; set; }
    }

    public class VerticalCalendarListView : ContentView
    {
        public bool IsGroupingEnabled { get; set; }
        public Binding GroupDisplayBinding { get; set; }

        public SeparatorVisibility SeparatorVisibility { get; set; }

        public DataTemplate ItemTemplate { get; set; }
        public IEnumerable<object> ItemsSource { get; set; }

        public VerticalCalendar Calendar { get; set; }

        List<EventHandler<VerticalCalendarScrollToRequestedEventArgs>> scrollToRequested = new List<EventHandler<VerticalCalendarScrollToRequestedEventArgs>>();
        internal event EventHandler<VerticalCalendarScrollToRequestedEventArgs> ScrollToRequested {
            add
            {
                this.scrollToRequested.Add(value);
                if(this.scrollToDelayed != null)
                {
                    value.Invoke(this, scrollToDelayed);
                    this.scrollToDelayed = null;
                }
            }
            remove
            {
                this.scrollToRequested.Remove(value);
            }
        }

        public event EventHandler<IEnumerable<VerticalCalendarRowViewModel>> RowsVisibleAfterScroll;
        internal void OnRowsVisibleAfterScroll(IEnumerable<VerticalCalendarRowViewModel> rows)
        {
            this.RowsVisibleAfterScroll?.Invoke(this, rows);
        }

        // scrollToDelayed is used if ScrollTo is called before the Renderer is attached to the list view, then invoke the handler as soon as the Renderer subscribes to it
        VerticalCalendarScrollToRequestedEventArgs scrollToDelayed;


        public VerticalCalendarListView(VerticalCalendar calendar)
        {
            this.Calendar = calendar;
        }

        public void ScrollTo(object item, ScrollToPosition position, bool animated) => this.ScrollTo(item, null, position, animated);
        public void ScrollTo(object item, object group, ScrollToPosition position, bool animated)
        {
            VerticalCalendarScrollToRequestedEventArgs e = new VerticalCalendarScrollToRequestedEventArgs
            {
                Item = item,
                Group = group,
                Position = position,
                Animated = animated
            };

            if (this.scrollToRequested.Count == 0)
            {
                scrollToDelayed = e;
            } else
            {
                this.scrollToRequested.ForEach(y=> y.Invoke(this, e));
            }
        }
    }
}
