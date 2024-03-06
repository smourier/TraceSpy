using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TraceSpy
{
    public class ColumnSortHandler
    {
        public virtual GridViewColumnHeader LastHeaderClicked { get; set; }
        public virtual ListSortDirection LastDirection { get; set; }

        public virtual void HandleClick(GridViewColumnHeader header, object itemsSource)
        {
            if (header == null)
            {
                // reset
                LastDirection = ListSortDirection.Ascending;
                LastHeaderClicked = null;
                return;
            }

            ListSortDirection direction;
            if (header != null && header.Role != GridViewColumnHeaderRole.Padding)
            {
                if (header != LastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    direction = LastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }

                var binding = header.Column.DisplayMemberBinding as Binding;
                var sortBy = binding?.Path.Path ?? header.Column.Header as string;

                Sort(itemsSource, sortBy, direction);

                header.Column.HeaderTemplate = direction == ListSortDirection.Ascending ? GetAscendingTemplate() : GetDescendingTemplate();

                if (LastHeaderClicked != null && LastHeaderClicked != header)
                {
                    LastHeaderClicked.Column.HeaderTemplate = null;
                }

                LastHeaderClicked = header;
                LastDirection = direction;
            }
        }

        protected virtual DataTemplate GetAscendingTemplate() => App.Current.Resources["HeaderTemplateArrowUp"] as DataTemplate;
        protected virtual DataTemplate GetDescendingTemplate() => App.Current.Resources["HeaderTemplateArrowDown"] as DataTemplate;

        protected virtual void Sort(object itemsSource, string sortBy, ListSortDirection direction)
        {
            var view = CollectionViewSource.GetDefaultView(itemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(sortBy, direction));
            view.Refresh();
        }
    }
}
