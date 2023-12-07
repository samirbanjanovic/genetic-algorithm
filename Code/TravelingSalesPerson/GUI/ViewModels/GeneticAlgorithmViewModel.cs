using OnTrac.Core.WPF;
using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using System.Data;
using TravelingSalesPerson.GeneticAlgorithm;
using TravelingSalesPerson.WisdomOfCrowds;


namespace GUI.ViewModels
{
    public class GeneticAlgorithmViewModel
        : NotifyClassBase
    {
        private GeneticAlgorithmView _view;
        private GAInit _gaInit;
        

        private DispatcherTimer _timer;
        private PointDetails _currentConnection;
        private Queue<PointDetails> _travelQueue;
        private Dictionary<int, Ellipse> _citiesEllipse;

        private BackgroundWorker _backgroundGAWorker;

        private string _filePath;

        private readonly string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public GeneticAlgorithmViewModel(GeneticAlgorithmView view)
        {
            this._view = view;

            this.StablePopulationSize = 150;
            this.MutationRate = 10D;
            this.MutationPercentageSize = 2D;
            this.GenerationCap = 200;
            this.DominanceCap = 50;
            this.GeneticInheitancePercentage = 25;
            this.CrowdSize = 1;
            
            this.GeneticAlg = new GAInstance(null);
            this.CrossoverSelection = new List<string>() {"Random Splices Technique",  "Closest Edge Grouping Inheritance" , "Basic Sequential Inheritance", "Fill The Blanks"};
            this.SelectedCrossoverItem = this.CrossoverSelection[0];
            this.CreateTable();
            this.Status = "Import file";
        }

        private void CreateTable()
        {
            this.History = new DataTable();
            History.Columns.Add("Generation");
            History.Columns.Add("Fittest");
            History.Columns.Add("Average");
        }

        #region Properties
        public GAInstance GeneticAlg { get; private set; }
        public DataTable History { get; private set; }
        public double EllapsedTime { get; private set; }
        public IList<int> InsertionOrder { get; private set; }

        private string _status;
        public string Status 
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this.OnPropertyChanged("Status");
            }
        }

        private string _selectedCrossOverItem;
        public string SelectedCrossoverItem 
        {
            get { return this._selectedCrossOverItem; }
            set
            {
                this._selectedCrossOverItem = value;
            }
        }

        public List<string> CrossoverSelection { get; private set; }

        private bool _complete;
        public bool Complete
        {
            get { return this._complete; }
            set
            {
                this._complete = value;
                this.OnPropertyChanged("Complete");
            }
        }

        private int _stablePopulationSize;
        public int StablePopulationSize
        {
            get
            {
                return this._stablePopulationSize;
            }
            set
            {
                this._stablePopulationSize = value;
                this.OnPropertyChanged("StablePopulationSize");
            }
        }

        private double _mutationRate;
        public double MutationRate 
        { 
            get
            {
                return this._mutationRate;
            }
            set
            {
                this._mutationRate = value;
                this.OnPropertyChanged("MutationRate");
            }
        }

        private double _mutationPercentageSize;
        public double MutationPercentageSize
        {
            get
            {
                return this._mutationPercentageSize;
            }
            set
            {
                this._mutationPercentageSize = value;
                this.OnPropertyChanged("MutationPercentageSize");
            }
        }

        private double _geneticInheritancePercentage;
        public double GeneticInheitancePercentage
        {
            get { return this._geneticInheritancePercentage; }
            set
            {
                this._geneticInheritancePercentage = value;
                this.OnPropertyChanged("GeneticInheitancePercentage");
            }
        }


        private int _generationCap;
        public int GenerationCap
        {
            get
            {
                return this._generationCap;
            }
            set
            {
                this._generationCap = value;
                this.OnPropertyChanged("GenerationCap");

                if (this._generationCap == -1)
                {
                    this.EnableSearchForFittestAfterNGenerations = true;
                    this.OnPropertyChanged("EnableSearchForFittestAfterNGenerations");
                }
                else
                {
                    this.EnableSearchForFittestAfterNGenerations = false;
                    this.OnPropertyChanged("EnableSearchForFittestAfterNGenerations");
                }
                    
            }
        }

        private double _currentFittest;
        public double CurrentFittest
        {
            get { return Math.Round(this._currentFittest, 5); }
            set
            {
                this._currentFittest = value;
                this.OnPropertyChanged("CurrentFittest");
            }
        }

        private int _dominanceCap;
        public int DominanceCap
        {
            get { return this._dominanceCap; }
            set
            {
                this._dominanceCap = value;
                this.OnPropertyChanged("DominanceCap");
            }
        }
        
        public bool EnableSearchForFittestAfterNGenerations { get; private set; }

        public int GenerationIndex { get; set; }

        #endregion Properties

        #region Commands
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

        private RelayCommand _stopSearchCommand;
        public ICommand StopSearchCommand
        {
            get
            {
                if (this._stopSearchCommand == null)
                {
                    this._stopSearchCommand = new RelayCommand(p => { this._backgroundGAWorker.CancelAsync(); }, p => true);
                }

                return this._stopSearchCommand;
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
                this._gaInit = new GAInit(this._filePath);                

                if (this._timer != null)
                    this._timer.Stop();

                this._travelQueue = new Queue<PointDetails>();
                this._citiesEllipse = new Dictionary<int, Ellipse>();
                this._fittestMemberToDraw = new Queue<PopulationMember>();

                this.CurrentFittest = 0D;

                this._timer = new DispatcherTimer();
                this._timer.Interval = TimeSpan.FromMilliseconds(100);
                this._timer.Tick += DrawNewFittestMember;

                this.DrawCities();
                this.Status = "Idle";
            }
        }


        private void FindPath()
        {
            if (this._gaInit == null)
                return;


            this.Status = "Idle";
            var mp = this.MutationRate / 100;
            var mps = this.MutationPercentageSize / 100;
            var gi = this.GeneticInheitancePercentage / 100;
            var gci = this.GenerationCap;
            var ugci = !this.EnableSearchForFittestAfterNGenerations;

            int type = this.CrossoverSelection.IndexOf(this.SelectedCrossoverItem);
            this._historyQueue = new Queue<int>();
            this._fittestMemberToDraw = new Queue<PopulationMember>();

            // write history to a file
            this.CreateTable();

            this.CurrentCrowdSize = 0;

            //this.GeneticAlg = new GAInstance(this._gaInit.Cities, mp, mps, gi, this.StablePopulationSize, ugci, gci, this.DominanceCap, type);

            //this.GeneticAlg.NewFittestMember += EnqueueNewFittestMemeber;
            //this.GeneticAlg.NewGenerationCreated += GeneticAlg_GenerationIncreased;

            //if (this.UseWisdomOfCrowds)            
            //    this.InitWisdomOfCrowdsAlg();
            
                
            this.Status = "Working";

            this._backgroundGAWorker = new BackgroundWorker();
            this._backgroundGAWorker.WorkerReportsProgress = true;
            this._backgroundGAWorker.DoWork +=
                (s, e) =>
                {

                    if (this.UseWisdomOfCrowds)
                        this.InitWisdomOfCrowdsAlg();
                    
                    for (int n = 0; n < this.CrowdSize; n++)
                    {
                        //this.DrawCities();

                        this.GeneticAlg = new GAInstance(this._gaInit.Cities, mp, mps, gi, this.StablePopulationSize, ugci, gci, this.DominanceCap, type);

                        this.GeneticAlg.NewFittestMember += EnqueueNewFittestMemeber;

                        if (!this.UseWisdomOfCrowds)
                            this.GeneticAlg.NewGenerationCreated += GeneticAlg_GenerationIncreased;
                                                
                        this.GeneticAlg.BeginLife();

                        this.CurrentCrowdSize = n + 1;
                        this.OnPropertyChanged("CurrentCrowdSize");

                        if (this.UseWisdomOfCrowds)
                        {
                            this.ExpertReceived(this.GeneticAlg.FittestMember);
                        }                         
                    }
                    
                    if(this.UseWisdomOfCrowds)
                    {
                        this._woc_wisdomOfCrowds.ProcessExpertSolutionsByChosenDestinationV2();
                        //this._woc_wisdomOfCrowds.ProcessExpertSolutionsByDescendingChoiceOption();                       
                        this._fittestMemberToDraw.Enqueue(this._woc_wisdomOfCrowds.CombinedExpertsSolution);
                    }                    
                };

            this._backgroundGAWorker.RunWorkerCompleted += bw_RunWorkerCompleted;
            this._backgroundGAWorker.RunWorkerAsync();

            this._timer.Start();
        }

        private void ExpertReceived(PopulationMember expert)
        {
            if (this._woc_generatedExperts == null)
                throw new Exception("Improper initialization of Wisdom Of Crowds algorithm");

            this._woc_wisdomOfCrowds.AddExpertSolution(expert);
            this._woc_generatedExperts.Add(expert);            
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Complete = true;
            this.Status = "Complete";

            List<string> data = new List<string>();
            string header = null;

            if(this.UseWisdomOfCrowds)
            {
                header = "City Count, Id, Expert Fitness";
                data.Add(header);
                int cityCount = this._woc_wisdomOfCrowds.NodeCount;
                for(int i = 0; i < this._woc_generatedExperts.Count; i++)
                {
                    string cc = cityCount.ToString();
                    var fitness = Math.Round(this._woc_generatedExperts[i].Distance, 8).ToString();

                    string item = string.Join(", ", cc, (i+1).ToString(), fitness);
                    data.Add(item);
                }

                string ag = "Aggregate: ," + Math.Round(this._woc_wisdomOfCrowds.CombinedExpertsSolution.Distance, 8);

                data.Add(ag);
            }
            else
            {                
                header = "Generation, Average, Fittest";
                data.Add(header);

                foreach (var g in this.GeneticAlg.GenerationalData)
                {
                    var gen = g.Key.ToString();
                    var avg = Math.Round(g.Value.Item3, 8).ToString();
                    var fit = Math.Round(g.Value.Item2.Distance, 8).ToString();

                    string d = string.Join(", ", gen, avg, fit);
                    data.Add(d);
                }
            
            }

            string fileName = null;
            int nodeCount = this._gaInit.Cities.Count;
            if (this.UseWisdomOfCrowds)
                fileName = "WOC_" + nodeCount.ToString() + "_" + this.SelectedCrossoverItem + "_" + DateTime.Now.ToString("HHmmss") + ".csv";
            else
                fileName = nodeCount.ToString() + "_" + this.SelectedCrossoverItem + "_" + DateTime.Now.ToString("HHmmss") + ".csv";

            System.IO.File.WriteAllLines(_desktopPath + @"\" + fileName, data);

        }

        private Queue<int> _historyQueue = new Queue<int>();
        private void GeneticAlg_GenerationIncreased(object sender, int e)
        {            
            this._historyQueue.Enqueue(this.GeneticAlg.GenerationIndex);            
        }

        #region Drawing Methods
        private Queue<PopulationMember> _fittestMemberToDraw;
        void EnqueueNewFittestMemeber(object sender, EventArgs e)
        {
            this.CurrentFittest = this.GeneticAlg.FittestMember.Distance;
            this._fittestMemberToDraw.Enqueue(this.GeneticAlg.FittestMember);
        }

        private void DrawNewFittestMember(object sender, EventArgs e)
        {
            if (this._fittestMemberToDraw.Count > 0)
            {
                var nf = this._fittestMemberToDraw.Dequeue();
                this.DrawCities();

                this.CurrentFittest = nf.Distance;
                foreach (var conn in nf.Connections)
                {
                    var ie = this._citiesEllipse[conn.InitialNode.Id];
                    var de = this._citiesEllipse[conn.DestinationNode.Id];
                    this._currentConnection = new PointDetails(ie, de, conn);

                    SolidColorBrush color = System.Windows.Media.Brushes.Black;

                    if (nf.IsAggregateResult)
                        color = System.Windows.Media.Brushes.Red;                   

                    var line = TSPUIElements.GetLabelAndEdge(this._currentConnection, false, color);
                    this._view.cnvsPath.Children.Add(line.Item2);
                }
            }

            if (this._historyQueue.Count > 0)
            {
                var g = _historyQueue.Dequeue();
                this.GenerationIndex = g;
                this.OnPropertyChanged("GenerationIndex");
                var d = this.GeneticAlg.GenerationalData[g];
                var f = Math.Round(d.Item2.Distance, 4);
                var avg = Math.Round(d.Item3, 4);

                this.History.Rows.Add(g, f, avg);
                this.OnPropertyChanged("History");
            }
        }

        // draw all points on canvas
        private void DrawCities()
        {
            this._citiesEllipse = new Dictionary<int, Ellipse>();

            this._view.cnvsPath
                      .Dispatcher
                      .Invoke
                      (
                        () => 
                            {
                                this._view.cnvsPath.Children.Clear();

                                foreach (var n in this._gaInit.Cities)
                                {
                                    var labelAndEllipse = TSPUIElements.GetLabelAndEllipse(n);
                                    this._citiesEllipse.Add(n.Id,labelAndEllipse.Item2);
                                    this._view.cnvsPath.Children.Add(labelAndEllipse.Item1);
                                    this._view.cnvsPath.Children.Add(labelAndEllipse.Item2);
                                }
                            }, 
                            DispatcherPriority.Render);


        }
        #endregion Drawing Methods

        #endregion Private Methods

        #region Wisdom of Crowds

        private List<PopulationMember> _woc_generatedExperts;
        private WisdomOfCrowds _woc_wisdomOfCrowds;

        private uint _woc_currentCrowdSize;

        private void InitWisdomOfCrowdsAlg()
        {
            this._woc_wisdomOfCrowds = new WisdomOfCrowds(this._gaInit.Cities);
            this._woc_generatedExperts = new List<PopulationMember>();
            this._woc_currentCrowdSize = 0;
        }


        private bool _useWisdomOfCrowds;
        public bool UseWisdomOfCrowds
        {
            get { return this._useWisdomOfCrowds; }
            set
            {
                this._useWisdomOfCrowds = value;
                this.CrowdSize = 1;
                this.OnPropertyChanged("UseWisdomOfCrowds");
            }
        }

        private uint _crowdSize;
        public uint CrowdSize
        {
            get { return this._crowdSize; }
            set
            {
                this._crowdSize = value;
                this.OnPropertyChanged("CrowdSize");
            }
        }

        public int CurrentCrowdSize { get; set; }

        #endregion Wisdom of Crowds
    }
}
