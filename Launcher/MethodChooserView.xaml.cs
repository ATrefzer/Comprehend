using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace Launcher
{
    /// <summary>
    /// Interaction logic for MethodChooser.xaml
    /// </summary>
    public sealed partial class MethodChooserView : Window
    {
        private ICollectionView _cv;

        public MethodChooserView()
        {
            InitializeComponent();
        }

        private void FilterAvailableFunctions()
        {
            // Collection view filters the visible items 
            var cv = CollectionViewSource.GetDefaultView(_dataGrid.ItemsSource);

            var text = _filterText.Text.ToUpper();
            var hideExcluded = _hideExcludedCheck.IsChecked.HasValue? _hideExcludedCheck.IsChecked.Value : false;

            if (string.IsNullOrEmpty(text) && !hideExcluded)
            {
                // switch off
                cv.Filter = null;
            }
            else
            {
                // switch filter on
                cv.Filter = obj =>
                            {
                                var vm = (obj as FunctionInfoViewModel);
                                if (vm == null) return false;

                                return vm.FullName.ToUpper().Contains(text) && (!hideExcluded || vm.Included);
                            };
            
            }
        }


        private void _filterText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAvailableFunctions();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            FilterAvailableFunctions();
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            FilterAvailableFunctions();
        }

        //private void _dataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var row = (DataGridRow)sender;
        //    var vm = DataContext as MethodChooserViewModel;
        //    var rowVm = (FunctionInfoViewModel) row.DataContext;

        //    // Clicking the check box for including a row would be recognized as double click.
        //    if (vm.StartFunction == null)
        //    {
        //        vm.StartFunction = rowVm.Model;
        //    }
        //}
    }
}