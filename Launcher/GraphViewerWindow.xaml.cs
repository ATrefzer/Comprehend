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
    /// Interaction logic for GraphViewerWindow.xaml
    /// </summary>
    public partial class GraphViewerWindow : Window
    {
        public GraphViewerWindow()
        {
            InitializeComponent();
        }

        private void GraphViewerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //GraphViewer graphViewer = new GraphViewer();
            //graphViewer.BindToPanel(_panel);
            //Graph graph = new Graph();

            //graph.AddEdge("A", "B");
            //graph.Attr.LayerDirection = LayerDirection.LR;
            //graphViewer.Graph = graph; // throws exception
        }
    }
}
