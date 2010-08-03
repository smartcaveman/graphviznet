using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace GraphVizNet
{
    public class VizGraph : VizBaseEntity
    {
        public override void Serialize(StreamWriter w)
        {
            w.Write("strict digraph ");
            if (!String.IsNullOrEmpty(this.Name))
            {
                w.Write(this.Name);
            }
            else
            {
                w.Write("g");
            }
            w.Write(" {"); w.Write(Environment.NewLine);
            foreach (var n in Nodes)
            {
                n.Serialize(w);
                w.Write(Environment.NewLine);
            }
            foreach (var e in Edges)
            {
                e.Serialize(w);
                w.Write(Environment.NewLine);
            }
            w.Write("}");
        }

        // from point (1/72 inch) to device independent unit (1/96 inch)
        public static readonly double PointToIndUnit = ((double)1 / (double)72) * (double)96;
        public static readonly double IndUnitToPoint = 1/PointToIndUnit;
        public static readonly double IndUnitToInches = (double)1/(double)96;

        public Rect BoundingBox
        {
            get;
            set;
        }

        protected override void OnSetAttribute(string name, string value)
        {
            switch (name)
            {
                case "bb":
                    SetBoundingBox(value);
                    break;
            }
            base.OnSetAttribute(name, value);
        }

        public void SetBoundingBox(string bb)
        {
            if (!String.IsNullOrEmpty(bb))
            {
                BoundingBox = Rect.Parse(bb).Scale();
            }
            else
            {
                BoundingBox = Rect.Empty;
            }
        }

        private List<VizNode> _nodes = new List<VizNode>();
        public IList<VizNode> Nodes
        {
            get
            {
                return _nodes;
            }
        }

        private List<VizEdge> _edges = new List<VizEdge>();
        public IList<VizEdge> Edges
        {
            get
            {
                return _edges;
            }
        }

        public VizGraphType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }

    public enum VizGraphType
    {
        Graph, // Ненаправленный граф
        DiGraph, // Направленный граф
        StrictGraph, // Граф где допустима только одна связь между узлами
        StrictDiGraph // Направленный граф с одной связью между узлами
    }
}
