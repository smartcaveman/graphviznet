using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;
using GraphVizNet;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace GraphViewer
{
    public class GraphView : FrameworkElement
    {
        public GraphView()
        {
            Visuals = new VisualCollection(this);
        }

        private VisualCollection Visuals;

        private void ClearVisuals()
        {
            Visuals.Clear();
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return Visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visuals[index];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size availableSize = this.DesiredSize;
            foreach (var node in nodes.Values)
            {
                var rect = new Rect();
                rect.Size = node.DesiredSize;
                rect.X = node.Node.Pos.X - (rect.Size.Width / 2);
                rect.Y = node.Node.Pos.Y - (rect.Size.Height / 2);

                node.Arrange(rect);
            }

            foreach (var a in edges)
                a.Arrange(new Rect(new Point(0, 0), this.DesiredSize));

            return this.DesiredSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var n in nodes.Values)
            {
                n.Measure(availableSize);
            }

            foreach (var a in edges)
                a.Measure(availableSize);

            if (Graph != null && Graph.BoundingBox != null)
            {
                return Graph.BoundingBox.Size;
            }

            return Size.Empty;
        }

        public void Relayout()
        {
            this.ClearVisuals();
            this.CreateNodes();

            this.InvalidateMeasure();
            this.Measure(new Size(double.MaxValue, double.MaxValue));
            this.UpdateLayout();

            this.Layout();

            this.InvalidateArrange();
            this.UpdateLayout();
        }

        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register("Graph", typeof(VizGraph), typeof(GraphView), new UIPropertyMetadata() { PropertyChangedCallback = new PropertyChangedCallback(OnGraphChanged), CoerceValueCallback = new CoerceValueCallback(OnCoerceGraph) });
        public VizGraph Graph
        {
            get { return (VizGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        #region OnGraphChanged
        private static void OnGraphChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView control = o as GraphView;
            if (control != null)
                control.OnGraphChanged((VizGraph)e.OldValue, (VizGraph)e.NewValue);
        }

        protected virtual void OnGraphChanged(VizGraph oldValue, VizGraph newValue)
        {
            if (newValue != null)
                this.Relayout();
        }
        #endregion

        #region OnCoerceGraph
        private static object OnCoerceGraph(DependencyObject o, object value)
        {
            GraphView control = o as GraphView;
            if (control != null)
                return control.OnCoerceGraph((VizGraph)value);
            else
                return value;
        }
        #endregion

        protected virtual VizGraph OnCoerceGraph(VizGraph value)
        {
            return value;
        }

        public void WriteGraph()
        {
            if (Graph != null)
            {
                using (var file = File.Create("source.dot"))
                {
                    var w = new StreamWriter(file);
                    Graph.Serialize(w);
                    w.Close();
                    file.Close();
                }
                MessageBox.Show("source.dot created");
            }
        }

        private void Layout()
        {
            if (Graph != null)
            {

                foreach (var node in nodes.Values)
                {
                    node.Node.Size = node.DesiredSize;
                }

                var eng = new GraphEngine();
                eng.DotLayout(Graph);

                Graph.ConvertToLeftUpperCorner();

                foreach (var edge in edges)
                {
                    edge.MakeGeometry();
                }
            }
        }

        private List<EdgePresenter> edges = new List<EdgePresenter>();

        private void CreateNodes()
        {
            nodes.Clear();
            edges.Clear();

            foreach (var node in Graph.Nodes)
            {
                var nodePresenter = GetNode(node);
            }
            foreach (var edge in Graph.Edges)
            {
                var edgePresenter = CreateEdge(edge);
            }
        }

        private EdgePresenter CreateEdge(VizEdge o)
        {
            var edge = new EdgePresenter();
            edge.Edge = o;

            //edge.Edge.SourceAttributes["dir"] = "none";

            edges.Add(edge);
            this.Visuals.Insert(0, edge);
            return edge;
        }

        Dictionary<VizNode, NodePresenter> nodes = new Dictionary<VizNode, NodePresenter>();

        private NodePresenter GetNode(VizNode o)
        {
            NodePresenter node;
            nodes.TryGetValue(o, out node);
            if (node == null)
            {
                node = new NodePresenter();
                node.Node = o;
                node.Node.SourceAttributes["fixedsize"] = "true";
                nodes[o] = node;

                this.Visuals.Add(node);
            }
            return node;
        }
    }
}
