using System.Windows;

namespace Launcher.Common
{
    /// <summary>
    /// https://stackoverflow.com/questions/24778677/how-to-hide-datagrid-column-in-wpf-automatically-using-mvvm
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object),
                typeof(BindingProxy));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion
    }
}