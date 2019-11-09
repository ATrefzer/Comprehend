using System.Windows;

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