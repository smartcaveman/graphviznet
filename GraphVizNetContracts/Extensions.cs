using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using System.IO;

namespace GraphVizNet
{
    public static class Extensions
    {
        /// <summary>
        /// Convert graphviz layout engine data to to WPF independent units
        /// and WPF coordinate system with (0,0) in upper left corner
        /// </summary>
        /// <param name="graph"></param>
        public static void ConvertToLeftUpperCorner(this VizGraph graph)
        {
            foreach (var node in graph.Nodes)
            {
                node.Pos = graph.ConvertPointToUpperLeftCorver(node.Pos);
            }
            foreach (var edge in graph.Edges)
            {
                for (int i = 0; i < edge.Points.Count; i++)
                {
                    edge.Points[i] = graph.ConvertPointToUpperLeftCorver(edge.Points[i]);
                }
                if (edge.EndPoint.HasValue)
                {
                    edge.EndPoint = graph.ConvertPointToUpperLeftCorver(edge.EndPoint.Value);
                }
                if (edge.StartPoint.HasValue)
                {
                    edge.StartPoint = graph.ConvertPointToUpperLeftCorver(edge.StartPoint.Value);
                }
            }
        }

        public static Point ConvertPointToUpperLeftCorver(this VizGraph graph, Point p)
        {
            return new Point(p.X, graph.BoundingBox.Height - p.Y);
        }

        public static Point Scale(this Point p)
        {
            if (p != null)
            {
                p.X *= VizGraph.PointToIndUnit;
                p.Y *= VizGraph.PointToIndUnit;
            }
            return p;
        }

        public static Rect Scale(this Rect p)
        {
            if (p != null)
            {
                p.Scale(VizGraph.PointToIndUnit, VizGraph.PointToIndUnit);
            }
            return p;
        }

        /// <summary>
        /// Convert WPF independent unit to inches
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double IndUnitToInches(this double n)
        {
            return n * VizGraph.IndUnitToInches;
        }

        public static string Format(this double n)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:0.####}", n);
        }
    }
}
