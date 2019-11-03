using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Launcher.Profiler;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MethodChooser.xaml
    /// </summary>
    public partial class SequenceDiagramSetup : Window
    {
        private ICollectionView _cv;

        public SequenceDiagramSetup()
        {
            InitializeComponent();
        }

        private void FilterAvailableFunctions()
        {
            // Collection view filters the visible items 
            var cv = CollectionViewSource.GetDefaultView(_dataGrid.ItemsSource);

            var text = _filterText.Text.ToUpper();
            var hideExcluded = _hideExcludedCheck.IsChecked.HasValue? _hideExcludedCheck.IsChecked.Value : false;

            if (string.IsNullOrEmpty(text))
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
    }
}