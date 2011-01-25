using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphVizNet;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows;

namespace GraphViewerDemo
{
    public class Controller : PropertyChangedHelper
    {
        public enum StateEnum
        {
            Ready,
            Working
        }

        #region StateProperty
        public static readonly PropertyChangedEventArgs StateArgs = PropertyChangedHelper.CreateArgs<Controller>(c => c.State);
        private StateEnum _State = StateEnum.Ready;

        public StateEnum State
        {
            get
            {
                return _State;
            }
            private set
            {
                var oldValue = State;
                _State = value;
                if (oldValue != value)
                {
                    OnStateChanged(oldValue, value);
                    OnPropertyChanged(StateArgs);
                }
            }
        }

        protected virtual void OnStateChanged(StateEnum oldValue, StateEnum newValue)
        {
            RefreshCommand.RaiseCanExecuteChanged();
        }
        #endregion


        Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

        #region RefreshCommand
        public static readonly string RefreshCommandProperty = "RefreshCommand";
        private DelegateCommand _RefreshCommand;

        public DelegateCommand RefreshCommand
        {
            get
            {
                if (_RefreshCommand == null)
                {
                    _RefreshCommand = new DelegateCommand(OnRefreshCommandExecuted, OnRefreshCommandCanExecute);
                }
                return _RefreshCommand;
            }
        }

        protected virtual bool OnRefreshCommandCanExecute()
        {
            return !String.IsNullOrEmpty(this.Source) && State == StateEnum.Ready;
        }

        protected virtual void OnRefreshCommandExecuted()
        {
            Refresh(this.Source);
        }
        #endregion

        #region GraphProperty
        public static readonly PropertyChangedEventArgs GraphArgs = PropertyChangedHelper.CreateArgs<Controller>(c => c.Graph);
        private VizGraph _Graph;

        public VizGraph Graph
        {
            get
            {
                return _Graph;
            }
            set
            {
                var oldValue = Graph;
                _Graph = value;
                if (oldValue != value)
                {
                    OnGraphChanged(oldValue, value);
                    OnPropertyChanged(GraphArgs);
                }
            }
        }

        protected virtual void OnGraphChanged(VizGraph oldValue, VizGraph newValue)
        {
        }
        #endregion

        #region SourceProperty
        public static readonly PropertyChangedEventArgs SourceArgs = PropertyChangedHelper.CreateArgs<Controller>(c => c.Source);
        private string _Source;

        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                var oldValue = Source;
                _Source = value;
                if (oldValue != value)
                {
                    OnSourceChanged(oldValue, value);
                    OnPropertyChanged(SourceArgs);
                }
            }
        }

        protected virtual void OnSourceChanged(string oldValue, string newValue)
        {
        }
        #endregion

        class Data
        {
            public VizGraph Graph;
            public string Source;
        }

        void Refresh(string source)
        {
            var data = new Data();
            data.Source = source;
            var w = (Action<Data>)Worker;
            w.BeginInvoke(data, null, null);
        }

        void Worker(Data data)
        {
            try
            {
                BuildGraph(data);

                Dispatcher.BeginInvoke((Action<Data>)this.OnComplete, DispatcherPriority.Normal, data);
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke((Action<Exception>)this.OnError, DispatcherPriority.Normal, e);
            }
        }

        void BuildGraph(Data data)
        {
            data.Graph = new VizGraph();
            data.Graph.Type = VizGraphType.DiGraph;

            var g = data.Graph;
            var source = data.Source;

            var array = source.ToCharArray();

            foreach (var c in source.ToCharArray().Distinct())
            {
                var node = new LetterNode() { Name = c.ToString() };
                g.Nodes.Add(node);
            }

            for (int i = 0; i < array.Length - 1; i++)
            {
                var edge = new VizEdge();
                edge.Head = g.Nodes.First(n => n.Name == array[i + 1].ToString());
                edge.Tail = g.Nodes.First(n => n.Name == array[i].ToString());
                edge.SourceAttributes["dir"] = "forward";
                g.Edges.Add(edge);
            }

            var engine = new GraphEngine();
            engine.DotLayout(g);
            g.ConvertToLeftUpperCorner();
        }

        void OnError(Exception e)
        {
            MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            State = StateEnum.Ready;
        }

        void OnComplete(Data data)
        {
            this.Graph = data.Graph;
            State = StateEnum.Ready;
        }
    }
}
