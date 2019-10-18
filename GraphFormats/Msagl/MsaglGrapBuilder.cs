using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using GraphLibrary;

using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;

namespace GraphFormats.Msagl
{
    public class MsaglGrapBuilder : IGraphBuilder
    {
        private readonly Graph _graph;

        private readonly HashSet<(string, string)> _edges = new HashSet<(string, string)>();

        public MsaglGrapBuilder()
        {
            _graph = new Graph();
        }

        public void AddEdge(string sourceNode, string targetNode)
        {
            // TODO Polymorphism may cause the same edge twice. But actually these are different
            // functions with the same name.
            //Debug.Assert(_edges.Add((sourceNode, targetNode)));

            _graph.AddEdge(sourceNode, string.Empty, targetNode);
        }

        public void AddEdge(string sourceNode, string targetNode, string category)
        {
            //Debug.Assert(_edges.Add((sourceNode, targetNode)));
            _graph.AddEdge(sourceNode, string.Empty, targetNode);
        }

        public void AddCategory(string category, string property, string value)
        {
        }

        public void ShowResult()
        {
            var form = new Form();
            var viewer = new GViewer();
            form.SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            form.Controls.Add(viewer);
            form.ResumeLayout();

            var settings = _graph.LayoutAlgorithmSettings as SugiyamaLayoutSettings;

            // uncomment this line to see the wide graph
            settings.MaxAspectRatioEccentricity = 100;

            // un-comment this line to us Mds
            //ss.FallbackLayoutSettings = new MdsLayoutSettings {AdjustScale = true}; 

            // or uncomment the following line to use the default layering layout with vertical layer
            _graph.Attr.LayerDirection = LayerDirection.LR;

            viewer.Graph = _graph;
            form.TopMost = true;

            //form.FormBorderStyle = FormBorderStyle.None;
            form.WindowState = FormWindowState.Maximized;
            form.ShowDialog();
        }
    }
}