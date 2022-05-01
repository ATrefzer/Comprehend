using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Launcher.Models;

namespace Launcher
{
    /// <summary>
    ///     Interaction logic for CallTreeExplorerView.xaml
    /// </summary>
    public partial class CallTreeExplorerView
    {
        public CallTreeExplorerView()
        {
            InitializeComponent();
        }

        public Action<FunctionCall, bool> ExportAction { get; set; }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is FunctionCallViewModel vm)
                {
                    Deselect(vm);
                }
            }
        }

        /// <summary>
        ///     Deselecting the view model propagates to the FunctionCall instancew.
        /// </summary>
        private void Deselect(FunctionCallViewModel vm)
        {
            if (vm == null)
            {
                return;
            }


            vm.IsIncluded = false;

            // Ensure placeholder is resolved
            vm.Load();
            foreach (var child in vm.Children)
            {
                Deselect(child as FunctionCallViewModel);
            }
        }

        private void MenuItem_OnExport(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is FunctionCallViewModel dc)
                {
                    ExportAction?.Invoke(dc.Call, false);
                }
            }
        }

        private void Search_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is CallTreeExplorerViewModel vm && e.Key == Key.Enter)
            {
                vm.SearchCommand.Execute(Search.Text);
            }
        }

        private void MenuItem_OnExportSimplified(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is FunctionCallViewModel dc)
                {
                    ExportAction?.Invoke(dc.Call, true);
                }
            }
        }
    }
}