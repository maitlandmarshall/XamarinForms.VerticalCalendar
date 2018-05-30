using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

namespace VerticalCalendar
{
    internal class VerticalCalendarListViewRenderer : ViewRenderer<VerticalCalendarListView, RecyclerView>
    {
        LinearLayoutManager LayoutManager { get; set; }
        VerticalCalendarListViewAdapter Adapter { get; set; }

        public VerticalCalendarListViewRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<VerticalCalendarListView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement == null) return;

            SetNativeControl(new RecyclerView(this.Context));
            this.Control.HasFixedSize = true;

            LinearLayoutManager manager = new LinearLayoutManager(this.Context, LinearLayoutManager.Vertical, false);
            this.Adapter = new VerticalCalendarListViewAdapter(this.Element, this.Control);
            this.Control.SetLayoutManager(manager);
            this.Control.SetAdapter(this.Adapter);

            this.LayoutManager = manager;

            this.Control.AddOnScrollListener(new VerticalCalendarListViewScrollListener(this.Element, this.LayoutManager, this.Adapter, this.Context));
            this.Element.ScrollToRequested += Element_ScrollToRequested;
        }

        bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                this.Element.ScrollToRequested -= Element_ScrollToRequested;
            }

            disposed = true;
            base.Dispose(disposing);
        }

        private void Element_ScrollToRequested(object sender, VerticalCalendarScrollToRequestedEventArgs e)
        {
            int position = this.Adapter.PositionFromViewModel(e.Item as VerticalCalendarRowViewModel);
            if(e.Animated)
            {
                this.LayoutManager.SmoothScrollToPosition(this.Control, new RecyclerView.State(), position);
            } else
            {
                if(e.Position == ScrollToPosition.Start)
                {
                    this.LayoutManager.ScrollToPosition(position);
                } else
                {
                    this.LayoutManager.ScrollToPositionWithOffset(position, (int)this.Context.ToPixels(150));
                }
                
            }
        }

        internal class VerticalCalendarListViewScrollListener : RecyclerView.OnScrollListener
        {
            VerticalCalendarListView ListView { get; set; }
            LinearLayoutManager LayoutManager { get; set; }
            VerticalCalendarListViewAdapter Adapter { get; set; }
            Context Context { get; set; }

            public VerticalCalendarListViewScrollListener(VerticalCalendarListView listView, LinearLayoutManager layoutManager, VerticalCalendarListViewAdapter adapter, Context ctx)
            {
                this.ListView = listView;
                this.LayoutManager = layoutManager;
                this.Adapter = adapter;
                this.Context = ctx;

                this.ListView.Calendar.GetVisibleRows = GetVisibleRows;
            }

            private List<VerticalCalendarRowViewModel> GetVisibleRows()
            {
                return this.VisibleRows().ToList();
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                this.ListView.OnRowsVisibleAfterScroll(this.VisibleRows().ToList());
            }

            IEnumerable<VerticalCalendarRowViewModel> VisibleRows()
            {
                int startPosition = this.LayoutManager.FindFirstCompletelyVisibleItemPosition(),
                    endPosition = this.LayoutManager.FindLastCompletelyVisibleItemPosition();

                for(var i = startPosition; i <= endPosition; i++)
                {
                    yield return this.Adapter.ViewModelFromPosition(i);
                }   
            }
        }
    }

    internal class VerticalCalendarListViewAdapter : RecyclerView.Adapter
    {
        internal class VerticalCalendarListViewAdapterViewHolder : RecyclerView.ViewHolder
        {
            public VerticalCalendarRow Row { get; set; }
            public VerticalCalendarListViewAdapterViewHolder(Android.Views.View view, VerticalCalendarRow row) : base(view)
            {
                this.Row = row;
                this.IsRecyclable = true;
            }
        }
        internal class VerticalCalendarListViewCellContainer : ViewGroup, INativeElementView
        {
            VerticalCalendarRow Row { get; set; }
            IVisualElementRenderer View { get; set; }

            int RowWidth { get; set; }
            int RowHeight { get; set; }

            public Element Element
            {
                get => this.Row;
            }
            
            public VerticalCalendarListViewCellContainer(Context context, VerticalCalendarRow row, int width, int height) : base (context)
            {
                this.Row = row;
                this.RowWidth = width;
                this.RowHeight = height;

                this.View = Platform.CreateRendererWithContext(this.Row.View, this.Context);
                Platform.SetRenderer(this.Row.View, this.View);
                this.AddView(this.View.View);
                this.RefreshDrawableState();
            }
            

            protected override void OnLayout(bool changed, int l, int t, int r, int b)
            {
                double width = Context.FromPixels(r - l);
                double height = Context.FromPixels(b - t);

                Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(this.View.Element, new Rectangle(0, 0, width, height));
                this.View.UpdateLayout();
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                if (this.View == null || this.View.Element == null)
                {
                    base.SetMeasuredDimension(widthMeasureSpec, heightMeasureSpec);
                    return;
                }

                this.SetMeasuredDimension((int)Context.ToPixels(this.RowWidth), (int)Context.ToPixels(this.RowHeight));
            }
        }

        enum RowType
        {
            Header = 0,
            Row = 1
        }

        const int RowHeight = 44;

        VerticalCalendarListView ListView { get; set; }
        RecyclerView RecyclerView { get; set; }

        List<VerticalCalendarRowViewModel> allRows = null;
        List<VerticalCalendarRowViewModel> AllRows
        {
            get
            {
                if(this.allRows == null)
                {
                    if (this.ListView.IsGroupingEnabled)
                    {
                        IEnumerable<VerticalCalendarRowGroupedViewModel> groups = this.ListView.ItemsSource as IEnumerable<VerticalCalendarRowGroupedViewModel>;
                        List<VerticalCalendarRowViewModel> allRows = groups.SelectMany(y =>
                        {
                            List<VerticalCalendarRowViewModel> rows = y.ToList();
                            if (rows.Count == 0)
                            {
                                return rows;
                            }

                            VerticalCalendarRowViewModel header = new VerticalCalendarRowViewModel(rows[0].FirstDayOfWeek) { GroupHeaderTitle = y.Title };
                            rows.Insert(0, header);

                            return rows;
                        }).ToList();

                        this.allRows = allRows;
                    }
                    else
                    {
                        this.allRows = (this.ListView.ItemsSource as List<VerticalCalendarRowViewModel>).ToList();
                    }
                }

                return this.allRows;
            }
        }

        public override int ItemCount
        {
            get
            {
                if(this.ListView.IsGroupingEnabled)
                {
                    IEnumerable<VerticalCalendarRowGroupedViewModel> groups = this.ListView.ItemsSource as IEnumerable<VerticalCalendarRowGroupedViewModel>;
                    return groups.Sum(y => y.Count) + groups.Count();
                } else
                {
                    return this.ListView.ItemsSource.Count();
                }
            }
        }

        public VerticalCalendarListViewAdapter(VerticalCalendarListView listView, RecyclerView recyclerView)
        {
            this.ListView = listView;
            this.RecyclerView = recyclerView;
            
        }

       
        public VerticalCalendarRowViewModel ViewModelFromPosition(int position)
        {
            return this.AllRows[position];
        }

        public int PositionFromViewModel(VerticalCalendarRowViewModel viewModel)
        {
            return this.AllRows.IndexOf(viewModel);
        }

        public override int GetItemViewType(int position)
        {
            if (this.ListView.IsGroupingEnabled)
            {
                VerticalCalendarRowViewModel rowFromPosition = this.ViewModelFromPosition(position);
                if(!String.IsNullOrEmpty(rowFromPosition.GroupHeaderTitle))
                {
                    return (int)RowType.Header;
                } else
                {
                    return (int)RowType.Row;
                }
            }
            else
            {
                return (int)RowType.Row;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            VerticalCalendarListViewAdapterViewHolder holderCast = holder as VerticalCalendarListViewAdapterViewHolder;
            VerticalCalendarRowViewModel viewModel = this.ViewModelFromPosition(position);

            if(!String.IsNullOrEmpty(viewModel.GroupHeaderTitle))
            {
                TextView text = (holderCast.ItemView as LinearLayout).GetChildAt(0) as TextView;
                text.Text = viewModel.GroupHeaderTitle;

            } else
            {
                holderCast.Row.BindingContext = viewModel;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch(viewType)
            {
                case (int)RowType.Header:
                    LinearLayout headerView = new LinearLayout(parent.Context);
                    headerView.LayoutParameters = new ViewGroup.LayoutParams((int)parent.Context.ToPixels(this.ListView.Width), (int)parent.Context.ToPixels(RowHeight));
                    headerView.SetVerticalGravity(GravityFlags.Center);
                    headerView.SetPadding(10, 5, 5, 5);

                    headerView.AddView(new TextView(parent.Context) { TextSize = 16 });

                    return new VerticalCalendarListViewAdapterViewHolder(headerView, null);
                default:
                    VerticalCalendarRow row = this.ListView.ItemTemplate.CreateContent() as VerticalCalendarRow;
                    row.Parent = this.ListView;

                    return new VerticalCalendarListViewAdapterViewHolder(new VerticalCalendarListViewCellContainer(parent.Context, row, (int)this.ListView.Width, RowHeight), row);
            }
        }
    }
}
