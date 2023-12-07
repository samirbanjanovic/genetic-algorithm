using Microsoft.Win32;
using OnTrac.Core.WPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using TravelingSalesPerson.ClosestVertex;


namespace GUI.ViewModels
{
    public class TSPClosestVertexViewModel
        : NotifyClassBase
    {
        private MainWindow _view;
        private TravelByClosestVertex _gcf;

        private DispatcherTimer _timer;

        private PointDetails _currentConnection;
        private Queue<PointDetails> _travelQueue;
        private IDictionary<int, Ellipse> _citiesEllipse;

        private string _filePath;

        public TSPClosestVertexViewModel(MainWindow view)
        {
            this._view = view;
            this.History = new DataTable();
            History.Columns.Add("Generation ");
            History.Columns.Add("Distance");
            History.Columns.Add("Nodes");
            History.Columns.Add("Time");
        }

        #region Properties

        public DataTable History { get; private set; }
        public double EllapsedTime { get; private set; }
        public IList<int> InsertionOrder { get; private set; }

        private int _initialNodeId;
        public int InitialNodeId
        {
            get { return _initialNodeId; }
            set
            {
                this._initialNodeId = value;

                if (this._gcf.Nodes.Keys.Contains(InitialNodeId))
                {
                    this._gcf.InitialNodeId = this.InitialNodeId;
                }
            }
        }

        public bool CanSetNodes
        {
            get
            {
                return this._gcf != null;
            }
        }


        private double _distance;
        public double Distance
        {
            get { return Math.Round(this._distance, 5); }
            set
            {
                this._distance = value;
                this.OnPropertyChanged("Distance");
            }
        }

        #endregion Properties

        #region Commands
        private RelayCommand _findPathCommand;
        public ICommand FindPathCommand
        {
            get
            {
                if (this._findPathCommand == null)
                {
                    this._findPathCommand = new RelayCommand(p => FindPath(), p => true);
                }

                return this._findPathCommand;
            }
        }

        private RelayCommand _loadTSPCommand;
        public ICommand LoadTSPCommand
        {
            get
            {
                if (this._loadTSPCommand == null)
                {
                    this._loadTSPCommand = new RelayCommand(p => this.OpenFile(), p => true);
                }

                return this._loadTSPCommand;
            }
        }
        #endregion Commands


        #region Private Methods
        private void OpenFile()
        {                        
            OpenFileDialog dlg = new OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this._filePath = dlg.FileName;
                this._gcf = new TravelByClosestVertex(this._filePath);

                this._currentConnection = null;
                this._initialNodeId = 0;
                this.Distance = 0;
                this.EllapsedTime = 0D;
                this.InsertionOrder = null;

                if (this._timer != null)
                    this._timer.Stop();

                this.OnPropertyChanged("InsertionOrder");
                this.OnPropertyChanged("EllapsedTime");
                this.OnPropertyChanged("InitialNodeId");
                this.OnPropertyChanged("CanSetNodes");

                this._travelQueue = new Queue<PointDetails>();
                this._citiesEllipse = new Dictionary<int, Ellipse>();

                this._timer = new DispatcherTimer();
                this._timer.Interval = TimeSpan.FromMilliseconds(100);
                this._timer.Tick += DrawNextCityPath;

                this.DrawCities();
            }
        }

        // evaluate all our nodes and edges
        private void FindPath()
        {
            
            if (this._gcf==null || !this._gcf.Nodes.ContainsKey(this.InitialNodeId))
                return;

            this._gcf.FindOptimalPath();

            this.EllapsedTime = this._gcf.EllapsedSearchTime.TotalMilliseconds;
            this.OnPropertyChanged("EllapsedTime");

            this.InsertionOrder = this._gcf.InsertionOrder;
            this.OnPropertyChanged("InsertionOrder");

            foreach (var conn in this._gcf.OptimalPath)
            {
                var ie = this._citiesEllipse[conn.InitialNode.Id];
                var de = this._citiesEllipse[conn.DestinationNode.Id];
                var cd = new PointDetails(ie, de, conn);

                this._travelQueue.Enqueue(cd);
            }

            this._timer.Start();
        }

        

        // draw edges at an interval
        private void DrawNextCityPath(object sender, EventArgs e)
        {
            if (this._travelQueue.Count == 0)
            {
                this.History.Rows.Add(this.InitialNodeId, this.Distance, this._gcf.Nodes.Values.Count, this.EllapsedTime);
                this.OnPropertyChanged("History");
                this._timer.Stop();                  
            }
            else
            {
                this._currentConnection = this._travelQueue.Dequeue();
                this.Distance += this._currentConnection.Connection.Distance;
                var line = TSPUIElements.GetLabelAndEdge(this._currentConnection, false);                
                this._view.cnvsPath.Children.Add(line.Item2);                  
            }

        }

        // draw all points on cnvas
        private void DrawCities()
        {
            this._view.cnvsPath
                      .Dispatcher
                      .Invoke
                      (
                        () => 
                            {
                                this._view.cnvsPath.Children.Clear();

                                foreach (var n in this._gcf.Nodes.Values)
                                {
                                    var labelAndEllipse = TSPUIElements.GetLabelAndEllipse(n);
                                    this._citiesEllipse.Add(n.Id,labelAndEllipse.Item2);
                                    this._view.cnvsPath.Children.Add(labelAndEllipse.Item1);
                                    this._view.cnvsPath.Children.Add(labelAndEllipse.Item2);
                                }
                            }, 
                            DispatcherPriority.Render);
        }
        #endregion Private Methods
    }
}
