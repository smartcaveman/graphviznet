using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphVizNet;

namespace GraphViewer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            this.btnDump.Click += new RoutedEventHandler(btnDump_Click);
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void btnDump_Click(object sender, RoutedEventArgs e)
        {
            view.WriteGraph();
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            VizGraph g = new VizGraph();
            g.Type = VizGraphType.StrictDiGraph;

            string source = "Test string. Letters will make the graph";

            var array = source.ToCharArray();

            foreach (var c in source.ToCharArray().Distinct())
            {
                var node = new VizNode() { Name = c.ToString() };
                g.Nodes.Add(node);
            }

            for (int i = 0; i < array.Length - 1; i++)
            {
                var edge = new VizEdge();
                edge.Head = g.Nodes.First(n => n.Name == array[i+1].ToString());
                edge.Tail = g.Nodes.First(n => n.Name == array[i].ToString());
                edge.SourceAttributes["dir"] = "forward";
                g.Edges.Add(edge);
            }

            this.view.Graph = g;
        }
    }
}
