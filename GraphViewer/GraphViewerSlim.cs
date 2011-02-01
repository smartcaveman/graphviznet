using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphVizNet;
using System.Collections.ObjectModel;
using System.IO;

namespace GraphViewer
{
    public class GraphViewerSlim : FrameworkElement
    {
        public void WriteGraph()
        {
            if (this.Graph != null)
            {
                WriteGraph(this.Graph);
                MessageBox.Show("dot source written to source.dot");
            }
        }

        private void WriteGraph(VizGraph g)
        {
            using (var file = File.Create("source.dot"))
            {
                var w = new StreamWriter(file);
                g.Serialize(w);
                w.Close();
                file.Close();
            }
        }

        public static readonly DependencyProperty NodeTemplateSelectorProperty =
                    DependencyProperty.Register("NodeTemplateSelector",
                        typeof(DataTemplateSelector), typeof(GraphViewerSlim),
                        null);

        public DataTemplateSelector NodeTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(NodeTemplateSelectorProperty); }
            set { SetValue(NodeTemplateSelectorProperty, value); }
        }

        public GraphViewerSlim()
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
            //if (this.IsArrangeValid)
            //{
            //    return this.DesiredSize;
            //}
            Size availableSize = this.DesiredSize;
            foreach (var node in Nodes)
            {
                var rect = new Rect();
                rect.Size = node.DesiredSize;
                rect.X = node.Node.Pos.X - (rect.Size.Width / 2) + WidthMargin / 2;
                rect.Y = node.Node.Pos.Y - (rect.Size.Height / 2) + HeightMargin / 2;

                node.Arrange(rect);
            }

            foreach (var a in Asso)
                a.Arrange(new Rect(new Point(WidthMargin / 2, HeightMargin / 2), this.DesiredSize));

            return this.DesiredSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //if (this.IsMeasureValid)
            //{
            //    return this.DesiredSize;
            //}

            foreach (var n in Nodes)
            {
                n.Measure(availableSize);
            }

            foreach (var a in Asso)
                a.Measure(availableSize);

            if (Graph != null && Graph.BoundingBox != null)
            {
                var res = new Size(Graph.BoundingBox.Size.Width + WidthMargin, Graph.BoundingBox.Size.Height + HeightMargin);
                return res;
            }

            return Size.Empty;
        }

        private double WidthMargin = 200;
        private double HeightMargin = 200;

        ObservableCollection<NodePresenter> Nodes = new ObservableCollection<NodePresenter>();
        ObservableCollection<EdgePresenter> Asso = new ObservableCollection<EdgePresenter>();

        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register("Graph", typeof(VizGraph), typeof(GraphViewerSlim), new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true, AffectsRender = true, AffectsArrange = true, AffectsMeasure = true, PropertyChangedCallback = new PropertyChangedCallback(OnGraphChanged) });
        public VizGraph Graph
        {
            get { return (VizGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        #region OnGraphChanged
        private static void OnGraphChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphViewerSlim control = o as GraphViewerSlim;
            if (control != null)
                control.OnGraphChanged((VizGraph)e.OldValue, (VizGraph)e.NewValue);
        }

        protected virtual void OnGraphChanged(VizGraph oldValue, VizGraph newValue)
        {
            Fill(newValue);
        }
        #endregion

        Dictionary<object, NodePresenter> objToNode = new Dictionary<object, NodePresenter>();

        private void Fill(VizGraph graph)
        {
            objToNode.Clear();

            this.ClearVisuals();
            if (graph != null)
            {

                Graph = graph;
                foreach (var node in graph.Nodes)
                {
                    var NodePresenter = new NodePresenter(node);
                    NodePresenter.Width = node.Size.Width;
                    NodePresenter.Height = node.Size.Height;
                    Nodes.Add(NodePresenter);
                    Visuals.Add(NodePresenter);
                    objToNode[node] = NodePresenter;
                }

                foreach (var edge in graph.Edges)
                {
                    var child = objToNode[edge.Head];
                    var parent = objToNode[edge.Tail];
                    var asso = new EdgePresenter(edge);
                    asso.MakeGeometry();
                    Asso.Add(asso);
                    Visuals.Add(asso);
                }
            }
        }

        public Point? GetObjectCenter(object o)
        {
            NodePresenter node;
            if (objToNode.TryGetValue(o, out node))
            {
                return new Point(node.Node.Pos.X + WidthMargin / 2, node.Node.Pos.Y + HeightMargin / 2);
            }
            return null;
        }
    }
}
