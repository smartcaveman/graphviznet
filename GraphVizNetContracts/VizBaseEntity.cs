using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace GraphVizNet
{
    public class VizBaseEntity
    {
        public virtual void Serialize(StreamWriter w)
        {
        }

        public void SerializeAttributes(StreamWriter w)
        {
            w.Write("[");

            var r = from k in attributes.Keys.Cast<string>()
                    select k + "=\"" + SourceAttributes[k] + "\"";
            w.Write(String.Join(",", r.ToArray()));

            w.Write("]");
        }

        private StringDictionary attributes = new StringDictionary();

        /// <summary>
        /// Attributes from graphviz layout engine
        /// </summary>
        protected StringDictionary Attributes
        {
            get
            {
                return attributes;
            }
        }

        private StringDictionary sourceAttributes = new StringDictionary();

        /// <summary>
        /// Source attributes for graphviz layout engine
        /// </summary>
        public StringDictionary SourceAttributes
        {
            get
            {
                return sourceAttributes;
            }
        }

        /// <summary>
        /// For internal use, use SourceAttributes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetAttribute(string name, string value)
        {
            attributes[name] = value;
            OnSetAttribute(name, value);
        }

        /// <summary>
        /// Override this to get attributes from graphviz layout engine
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void OnSetAttribute(string name, string value)
        {
        }
    }
}
