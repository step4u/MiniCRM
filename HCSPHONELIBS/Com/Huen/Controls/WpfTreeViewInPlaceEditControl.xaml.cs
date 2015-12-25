// example for an article on www.codeproject.com
// by Yury.Vetyukov@tuwien.ac.at

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Com.Huen.DataModel;

namespace Com.Huen.Controls
{
    public delegate void CommitEditedTextEventHandler(object sender, KeyEventArgs e);
    public delegate void CancelEditEventHandler(object sender, KeyEventArgs e);
    public delegate void DeleteItemEventHandler(object sender, KeyEventArgs e);
    public delegate void LostFocusItemEventHandler(object sender, RoutedEventArgs e);

    /// <summary>
    /// Interaction logic for WpfTreeViewInPlaceEditControl.xaml
    /// </summary>
    public partial class WpfTreeViewInPlaceEditControl : TreeView, INotifyPropertyChanged
    {
        public event CommitEditedTextEventHandler CommitEditedTextEvent;
        public event CancelEditEventHandler CancelEditEvent;
        public event DeleteItemEventHandler DeleteItemEvent;
        public event LostFocusItemEventHandler LostFocusItemEvent;

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This flag indicates whether the tree view items shall (if possible) open in edit mode
        bool isInEditMode = false;
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set
            {
                isInEditMode = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if(handler != null)
                    handler(this, new PropertyChangedEventArgs("IsInEditMode"));
            }
        }

        public WpfTreeViewInPlaceEditControl()
        {
            InitializeComponent();
        }

        // text in a text box before editing - to enable cancelling changes
        string oldText;

        // if a text box has just become visible, we give it the keyboard input focus and select contents
        private void editableTextBoxHeader_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if(tb.IsVisible)
            {
                tb.Focus();
                tb.SelectAll();
                oldText = tb.Text;      // back up - for possible cancelling
            }
        }

        // stop editing on Enter or Escape (then with cancel)
        private void editableTextBoxHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var tb = sender as TextBox;
                tb.Foreground = Brushes.Black;
                tb.Background = Brushes.White;
                IsInEditMode = false;

                if (CommitEditedTextEvent != null)
                    CommitEditedTextEvent(this, e);
            }
            if(e.Key == Key.Escape)
            {
                var tb = sender as TextBox;
                tb.Text = oldText;
                IsInEditMode = false;

                if (CancelEditEvent != null)
                    CancelEditEvent(this, e);
            }
        }

        // stop editing on lost focus
        private void editableTextBoxHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsInEditMode)
            {
                if (LostFocusItemEvent != null)
                    LostFocusItemEvent(this, e);
            }

            IsInEditMode = false;
        }

        // it might happen, that the user pressed F2 while a non-editable item was selected
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IsInEditMode = false;

            var newval = e.NewValue as GroupList;
            if (string.IsNullOrEmpty(newval.Name) && newval.IsSelected == true)
            {
                IsInEditMode = true;
            }
        }
        

        // we (possibly) switch to edit mode when the user presses F2
        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F2)
            {
                var view = sender as WpfTreeViewInPlaceEditControl;
                var item = view.SelectedItem as GroupList;

                if (item.Idx < 1)
                {
                    e.Handled = true;
                    return;
                }

                IsInEditMode = true;
            }

            if (e.Key == Key.Delete)
            {
                var view = sender as WpfTreeViewInPlaceEditControl;
                var item = view.SelectedItem as GroupList;

                if (item.Idx < 1)
                {
                    e.Handled = true;
                    return;
                }

                if (DeleteItemEvent != null)
                    DeleteItemEvent(this, e);
            }
        }

        // the user has clicked a header - proceed with editing if it was selected
        private void textBlockHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return;

            //if (FindTreeItem(e.OriginalSource as DependencyObject).IsSelected)
            //{
            //    IsInEditMode = true;
            //    e.Handled = true;       // otherwise the newly activated control will immediately loose focus
            //}
        }

        // searches for the corresponding TreeViewItem,
        // based on http://stackoverflow.com/questions/592373/select-treeview-node-on-right-click-before-displaying-contextmenu
        static TreeViewItem FindTreeItem(DependencyObject source)
        {
            while(source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }
    }
}
