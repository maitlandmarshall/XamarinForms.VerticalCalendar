using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(VerticalCalendar.VerticalCalendarListView), typeof(VerticalCalendar.VerticalCalendarListViewRenderer))]
namespace VerticalCalendar
{
    internal class VerticalCalendarListViewRenderer : ViewRenderer<VerticalCalendarListView, UITableView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<VerticalCalendarListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null) return;

            SetNativeControl(new UITableView());
            this.Control.RegisterClassForCellReuse(typeof(UITableViewCell), "row");
            this.Control.Source = new VerticalCalendarTableViewSource(this.Element, this.Control);

            this.Element.ScrollToRequested += Element_ScrollToRequested;
        }

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            if(disposing)
            {
                this.Element.ScrollToRequested -= Element_ScrollToRequested;
            }

            disposed = true;

            base.Dispose(disposing);
        }

        private void Element_ScrollToRequested(object sender, VerticalCalendarScrollToRequestedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                int section, row;

                if (this.Element.IsGroupingEnabled)
                {
                    section = this.Element.ItemsSource.IndexOf(e.Group);
                    row = (this.Element.ItemsSource.ElementAt(section) as IEnumerable<object>).IndexOf(e.Item);
                }
                else
                {
                    section = 0;
                    row = this.Element.ItemsSource.IndexOf(e.Item);
                }

                UITableViewScrollPosition position = 0;
                switch (e.Position)
                {
                    case ScrollToPosition.Center:
                        position = UITableViewScrollPosition.Middle;
                        break;
                    case ScrollToPosition.End:
                        position = UITableViewScrollPosition.Bottom;
                        break;
                    case ScrollToPosition.MakeVisible:
                        position = UITableViewScrollPosition.None;
                        break;
                    case ScrollToPosition.Start:
                        position = UITableViewScrollPosition.Top;
                        break;
                }

                await Task.Delay(1);

                this.Control.ScrollToRow(NSIndexPath.FromRowSection(row, section), position, e.Animated);
            });
        }
    }

    class VerticalCalendarTableViewSource : UITableViewSource
    {
        const int RowHeight = 44;

        VerticalCalendarListView ListView;
        UITableView TableView;

        public VerticalCalendarTableViewSource(VerticalCalendarListView listView, UITableView tableView)
        {
            this.ListView = listView;
            this.TableView = tableView;

            this.ListView.Calendar.GetVisibleRows = VisibleRows;
        }

        public static UIView ConvertFormsToNative(Xamarin.Forms.View view, CGRect size)
        {
            var renderer = Platform.CreateRenderer(view);

            renderer.NativeView.Frame = size;

            renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

            renderer.Element.Layout(size.ToRectangle());

            var nativeView = renderer.NativeView;

            nativeView.SetNeedsLayout();

            return nativeView;
        }

        Dictionary<UITableViewCell, VerticalCalendarRow> cache = new Dictionary<UITableViewCell, VerticalCalendarRow>();
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell("row", indexPath);

            VerticalCalendarRow row;
            if(!cache.TryGetValue(cell, out row))
            {
                row = this.ListView.ItemTemplate.CreateContent() as VerticalCalendarRow;
                cell.ContentView.AddSubview(ConvertFormsToNative(row.View, new CGRect(0, 0, tableView.Frame.Width, RowHeight)));
                cache.Add(cell, row);
            }

            object data;
            if(this.ListView.IsGroupingEnabled)
            {
                data = (ListView.ItemsSource.ElementAt(indexPath.Section) as IEnumerable<object>).ElementAt(indexPath.Row);
            } else
            {
                data = ListView.ItemsSource.ElementAt(indexPath.Section);
            }

            row.BindingContext = data;

            return cell;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            if (!this.ListView.IsGroupingEnabled) return "";

            VerticalCalendarRowGroupedViewModel group = (ListView.ItemsSource.ElementAt((int)section) as IEnumerable<object>) as VerticalCalendarRowGroupedViewModel;
            return group.Title;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if(this.ListView.IsGroupingEnabled)
            {
                return (ListView.ItemsSource.ElementAt((int)section) as IEnumerable<object>).Count();
            } else
            {
                return this.ListView.ItemsSource.Count();
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return RowHeight;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            object dataItem = this.ListView.ItemsSource.FirstOrDefault();
            if (dataItem is IEnumerable<object>)
            {
                return this.ListView.ItemsSource.Count();
            }
            else
            {
                return 1;
            }
        }

        CGPoint? lastScrollOffset;
        public override void Scrolled(UIScrollView scrollView)
        {
            if (lastScrollOffset.HasValue)
            {
                if (Math.Abs(scrollView.ContentOffset.Y - lastScrollOffset.Value.Y) < RowHeight / 2) return;
            }

            lastScrollOffset = scrollView.ContentOffset;
            this.ListView.OnRowsVisibleAfterScroll(this.VisibleRows());
        }

        public List<VerticalCalendarRowViewModel> VisibleRows()
        {
            List<VerticalCalendarRowViewModel> visibleResult = new List<VerticalCalendarRowViewModel>();

            NSIndexPath[] visibleRows = this.TableView.IndexPathsForVisibleRows;
            foreach (NSIndexPath index in visibleRows)
            {
                if (this.ListView.IsGroupingEnabled)
                {
                    visibleResult.Add((this.ListView.ItemsSource.ElementAt(index.Section) as IEnumerable<VerticalCalendarRowViewModel>).ElementAt(index.Row));
                }
                else
                {
                    visibleResult.Add(this.ListView.ItemsSource.ElementAt(index.Row) as VerticalCalendarRowViewModel);
                }
            }

            return visibleResult;
        }
    }
}
