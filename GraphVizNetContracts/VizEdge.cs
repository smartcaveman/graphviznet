using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphVizNet
{
    public class VizEdge : VizBaseEntity
    {
        public override void Serialize(System.IO.StreamWriter w)
        {
            w.Write('"');
            w.Write(this.Head.Name);
            w.Write("\" -> \"");
            w.Write(this.Tail.Name);
            w.Write("\" ");
            this.SerializeAttributes(w);
        }

        public VizNode Tail
        {
            get;
            set;
        }

        public VizNode Head
        {
            get;
            set;
        }

        List<Point> points = new List<Point>();
        public IList<Point> Points
        {
            get
            {
                return points;
            }
        }

        protected override void OnSetAttribute(string name, string value)
        {
            switch (name)
            {
                case "pos":
                    SetPoints(value);
                    break;
            }
            base.OnSetAttribute(name, value);
        }

        public Point? StartPoint
        {
            get;
            set;
        }

        public Point? EndPoint
        {
            get;
            set;
        }

        private void SetPoints(string p)
        {
            StartPoint = null;
            EndPoint = null;

            if (String.IsNullOrEmpty(p))
            {
                Points.Clear();
                return;
            }

            foreach (var str in p.Split(' '))
            {
                if (str.StartsWith("s"))
                {
                    StartPoint = Point.Parse(str.Trim(new char[] { 's', 'e', ',' })).Scale();
                }
                else if (str.StartsWith("e"))
                {
                    EndPoint = Point.Parse(str.Trim(new char[] { 's', 'e', ',' })).Scale();
                }
                else
                {
                    Points.Add(Point.Parse(str).Scale());
                }
            }
        }
    }
}
