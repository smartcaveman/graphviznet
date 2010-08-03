using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GraphVizNet;
using System.Windows;
using System.Windows.Media;

namespace GraphViewer
{
    public class EdgePresenter : Control
    {
        public static readonly DependencyProperty EdgeProperty = DependencyProperty.Register("Edge", typeof(VizEdge), typeof(EdgePresenter), new UIPropertyMetadata());
        public VizEdge Edge
        {
            get { return (VizEdge)GetValue(EdgeProperty); }
            set { SetValue(EdgeProperty, value); }
        }

        public static readonly DependencyProperty EdgeGeometryProperty = DependencyProperty.Register("EdgeGeometry", typeof(StreamGeometry), typeof(EdgePresenter), new UIPropertyMetadata());
        public StreamGeometry EdgeGeometry
        {
            get { return (StreamGeometry)GetValue(EdgeGeometryProperty); }
            set { SetValue(EdgeGeometryProperty, value); }
        }  

        private void DrawArrow(StreamGeometryContext c, Point from, Point to, double width)
        {
            Vector v = from - to;
            v.Normalize();

            c.BeginFigure(from, true, true);

            var m = new Matrix();
            m.Rotate(90);
            c.LineTo(from + m.Transform(v * width), true, true);
            c.LineTo(to, true, true);

            m.Rotate(180);

            c.LineTo(from + m.Transform(v * width), true, true);
        }

        internal void MakeGeometry()
        {
            if (Edge.Points.Count > 0)
            {
                var points = Edge.Points;
                StreamGeometry g = new StreamGeometry();
                StreamGeometryContext c = g.Open();

                c.BeginFigure(points[0], false, false);
                var r = Edge.Points.Where(p => p != Edge.Points[0]);
                c.PolyBezierTo(r.ToList(), true, false);

                if (Edge.EndPoint.HasValue)
                {
                    Point from = Edge.Points.Last();
                    Point to = Edge.EndPoint.Value;

                    DrawArrow(c, from, to, 4);
                }

                if (Edge.StartPoint.HasValue)
                {
                    Point from = Edge.Points.First();
                    Point to = Edge.StartPoint.Value;

                    DrawArrow(c, from, to, 4);
                }

                c.Close();
                g.Freeze();

                EdgeGeometry = g;
            }
        }
    }
}
