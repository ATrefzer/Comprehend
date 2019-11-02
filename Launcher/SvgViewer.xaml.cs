using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for SvgViewer.xaml
    /// </summary>
    public partial class SvgViewer : Window
    {
        public SvgViewer()
        {
            InitializeComponent();
        }

        public void LoadImage(string file)
        {
            var uri = new System.Uri(file);
            var converted = uri.AbsoluteUri;
            _viewBox.Source = uri;
        }
    }
}
