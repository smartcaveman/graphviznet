using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphVizNet
{
    public class VizNode : VizBaseEntity
    {
        public override void Serialize(System.IO.StreamWriter w)
        {
            w.Write('"');
            w.Write(this.Name);
            w.Write("\" ");
            this.SerializeAttributes(w);
        }

        public string Name
        {
            get;
            set;
        }

        public Point Pos
        {
            get;
            set;
        }

        public void SetPos(string pos)
        {
            if (!String.IsNullOrEmpty(pos))
            {
                Pos = Point.Parse(pos).Scale();
            }
            else
            {
                Pos = new Point();
            }
        }

        protected override void OnSetAttribute(string name, string value)
        {
            switch (name)
            {
                case "pos":
                    SetPos(value);
                    break;
            }
            base.OnSetAttribute(name, value);
        }

        private Size _size;

        public Size Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                SourceAttributes["shape"] = "box";
                SourceAttributes["width"] = value.Width.IndUnitToInches().Format();
                SourceAttributes["height"] = value.Height.IndUnitToInches().Format();

                SourceAttributes["fixedsize"] = "true";
            }
        }
    }
}
