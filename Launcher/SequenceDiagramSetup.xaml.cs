using System.ComponentModel;
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


        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBoxBase.TextChanged="ComboBox_TextChanged" 

            // Collection view filters the visible items in the combobox.
            _cv = CollectionViewSource.GetDefaultView(_comboFunctions.ItemsSource);

            var text = _comboFunctions.Text.ToUpper();
            if (string.IsNullOrEmpty(text))
            {
                // switch off
                _cv.Filter = null;
            }
            else
            {
                // switch filter on
                _cv.Filter = obj => { return (obj as FunctionInfo).Name.ToUpper().Contains(text); };
            }

            e.Handled = false;
        }

        private void _comboFunctions_OnTextChanged(object sender, TextChangedEventArgs e)
        {
          

            // Pass character to Text
            e.Handled = false;
        }

        private void FilterAvailableFunctions()
        {
            // Collection view filters the visible items in the combobox.
            _cv = CollectionViewSource.GetDefaultView(_comboFunctions.ItemsSource);

            var text = _comboFunctions.Text.ToUpper();
            if (string.IsNullOrEmpty(text))
            {
                // switch off
                _cv.Filter = null;
            }
            else
            {
                // switch filter on
                _cv.Filter = obj => { return (obj as FunctionInfo).Name.ToUpper().Contains(text); };
            }
        }

        private void _comboFunctions_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key!= Key.Left && e.Key!= Key.Right)
            {
                FilterAvailableFunctions();
            }

            e.Handled = false;
        }
    }
}