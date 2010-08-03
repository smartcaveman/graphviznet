using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GraphVizNet;
using System.Windows;

namespace GraphViewer
{
    public class NodePresenter : Control
    {
        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register("Node", typeof(VizNode), typeof(NodePresenter), new UIPropertyMetadata());
        public VizNode Node
        {
            get { return (VizNode)GetValue(NodeProperty); }
            set { SetValue(NodeProperty, value); }
        }  
    }
}
