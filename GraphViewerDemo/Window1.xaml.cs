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

namespace GraphViewerDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Controller Controller = new Controller();

        public Window1()
        {
            this.DataContext = this.Controller;
            this.Controller.Source = "123451abc";
            this.Controller.RefreshCommand.Execute();
            InitializeComponent();
        }
    }
}
