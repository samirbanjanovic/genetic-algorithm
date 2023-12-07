using GUI.Views;
using Microsoft.Win32;
using OnTrac.Core.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using TravelingSalesPerson;
using K_GraphColoring;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
//using OnTrac.Extensions.String;
using System.Data;

namespace GUI.ViewModels
{
    public class GraphColoringViewModel
        : NotifyClassBase
    {
        private readonly string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private GraphColoringView _view;
        private string _filePath;
        private Dictionary<int, Ellipse> _graphPoints;

        private StringBuilder _csvBuilder;
        private DispatcherTimer _timer;
        private Queue<PointDetails> _drawingQueue;
        private Dictionary<int, Ellipse> _pointEllipsee;

        private K_Graph_GA_Init _k_graph_init;
        private K_Graph_GA_Settings _k_ga_settings;
        private K_Graph_GA _k_ga;
        private K_Graph_Greedy_Coloring _k_greedyColoring;

        private K_Graph_GA_Member _currentFittestMember;
        private K_Graph_GA_Member _wocSolutionMember;
        private K_Graph_GA_Member _finalFittestMember;
        private Queue<K_Graph_GA_Member> _expertRows;

        private Dictionary<K_GraphColoring.Color, System.Windows.Media.SolidColorBrush> _colorMapping;


        public GraphColoringViewModel(GraphColoringView view)
        {
            this._view = view;

            this.PopulationSize = 10;
            this.GenerationCap = 20;
            this.MutationRate = 0.5D;

            this.CrowdSize = 1;

            this.WriteDataToFile = false;
            this.Iterations = 1;
            _colorMapping = new Dictionary<K_GraphColoring.Color, SolidColorBrush>();
            foreach (K_GraphColoring.Color color in Enum.GetValues(typeof(K_GraphColoring.Color)))
            {
                var brush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color.ToString()));
                _colorMapping.Add(color, brush);
            }

        }

        #region Properties

        private int _numberOfColorsToUse;
        public int NumberOfColorsToUse
        {
            get { return this._numberOfColorsToUse; }
            private set
            {
                this._numberOfColorsToUse = value;
                this.OnPropertyChanged("NumberOfColorsToUse");
            }
        }

        private int _finalMemberGeneration;
        public int FinalMemberGeneration
        {
            get { return this._finalMemberGeneration; }
            private set
            {
                this._finalMemberGeneration = value;
                this.OnPropertyChanged("FinalMemberGeneration");
            }
        }

        private string _solutionType;
        public string SolutionType
        {
            get { return this._solutionType; }
            private set
            {
                this._solutionType = value;
                this.OnPropertyChanged("SolutionType");
            }
        }

        private double _elapsedRunTime;
        public double ElapsedRunTime
        {
            get
            {
                return this._elapsedRunTime;
            }
            private set
            {
                this._elapsedRunTime = Math.Round(value, 8);
                this.OnPropertyChanged("ElapsedRunTime");
            }
        }

        private string _isComplete;
        public string IsComplete
        {
            get { return this._isComplete; }
            private set
            {
                this._isComplete = value;
                this.OnPropertyChanged("IsComplete");
            }
        }

        public bool WriteDataToFile { get; set; }


        public IDictionary<int, Node> NodeData { get; private set; }
        public DataTable ExpertDetails { get; set; }
        public int PopulationSize { get; set; }
        public double MutationRate { get; set; }
        public int GenerationCap { get; set; }
        public int Iterations { get; set; }

        public bool ExecuteLocalGreedy { get; set; }
        public bool UseColorist { get; set; }
        public bool SelectFirstPointAtRandom { get; set; }
        public bool ShowAll { get; set; }
        public bool UseWoCResolver { get; set; }

        private int _numberOfColoringMembersReceived;
        public int NumberOfColoringMembersReceived
        {
            get { return this._numberOfColoringMembersReceived; }
            set
            {
                this._numberOfColoringMembersReceived = value;
                this.OnPropertyChanged("NumberOfColoringMembersReceived");
            }
        }

        private int _expertIndex;
        public int ExpertIndex
        {
            get { return this._expertIndex; }
            set
            {
                this._expertIndex = value;
                this.OnPropertyChanged("ExpertIndex");
            }
        }
        #endregion

        #region Commands

        private RelayCommand _loadGraphCommand;
        public ICommand LoadGraphCommand
        {
            get
            {
                if (this._loadGraphCommand == null)
                    this._loadGraphCommand = new RelayCommand(p => this.LoadGraphDataFile(), p => true);

                return this._loadGraphCommand;
            }
        }

        private RelayCommand _startColoringCommand;
        public ICommand StartColoringCommand
        {
            get
            {
                if (this._startColoringCommand == null)
                    this._startColoringCommand = new RelayCommand(p => this.StartKColoring(), p => true);

                return this._startColoringCommand;
            }
        }

        #endregion Commands

        #region Methods

        private Queue<K_Graph_GA_Member> _coloringQueue;
        private void LoadGraphDataFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this._filePath = dlg.FileName;
                this._k_graph_init = new K_Graph_GA_Init(this._filePath);

                this.NumberOfColorsToUse = 0;
                this.NodeData = this._k_graph_init.NodeData;

                if (this._timer != null)
                    this._timer.Stop();

                this._coloringQueue = new Queue<K_Graph_GA_Member>();
                this._drawingQueue = new Queue<PointDetails>();
                this._pointEllipsee = new Dictionary<int, Ellipse>();
                this.IsComplete = string.Empty;
                this._timer = new DispatcherTimer();

                this._timer.Tick += ShowNextPopulationMember;

                this.DrawNodes();
                this.DrawConnections();
            }
        }

        private List<Node> _greedyResults;
        private double _averageExecutionTime;
        private IList<int> _colorCountTracker;
        private void StartKColoring()
        {
            if (string.IsNullOrWhiteSpace(this._filePath))
                return;

            this._averageExecutionTime = 0D;
            this._colorCountTracker = new List<int>();

            double drawingBreakTime = this.UseWisdomOfCrowds ? 250 : 1;
            this._timer.Interval = TimeSpan.FromMilliseconds(drawingBreakTime);

            var sw = new Stopwatch();
            Task findAnswer = new Task(
                () =>
                {
                    string fileName = string.Format("{0}_Node_Coloring_{1}.csv", this._k_graph_init.NodeData.Count.ToString(), DateTime.Now.ToString("yyyyMMddHHmmss"));

                    this._csvBuilder = new StringBuilder();

                    if (ExecuteLocalGreedy)
                        this._csvBuilder.AppendLine("Node Count, Execution Time (ms), IsComplete, Color Count");
                    else if (!this.UseWisdomOfCrowds)
                        this._csvBuilder.AppendLine("Generation, Total Members Evaluated, Generation, Gen. Iteration, Number Of Colors Used, Fittes Member Range, Execution Time (ms)");
                    else
                        this._csvBuilder.AppendLine("Expert Count, Best Expert, WoC Solution");

                    for (int i = 0; i < this.Iterations; i++)
                    {
                        this.ExpertDetails = new DataTable();
                        if (this.UseWisdomOfCrowds)
                        {
                            this.WisdomOfCrowds = new K_Graph_WisdomOfCrowds(this._k_graph_init.NodeData.Count, this._k_graph_init.NodeData);
                            this._expertRows = new Queue<K_Graph_GA_Member>();
                            this.ExpertDetails.Columns.Add("Generation", typeof(int));
                            this.ExpertDetails.Columns.Add("Color Count", typeof(int));
                            this.ExpertDetails.Columns.Add("Is Complete", typeof(string));
                        }

                        this.OnPropertyChanged("ExpertDetails");

                        this.NumberOfColoringMembersReceived = 0;
                        this.ExpertIndex = 0;
                        this.NumberOfColorsToUse = 0;

                        sw.Start();
                        if (this.ExecuteLocalGreedy)
                            this.LocalGreedyColoring();
                        else
                            this.GA_Algo();

                        sw.Stop();
                        this.ElapsedRunTime = sw.Elapsed.TotalMilliseconds;
                        this._averageExecutionTime += this.ElapsedRunTime;

                        if (this.WriteDataToFile)
                            this.AppendResultsToCSV();

                        sw.Reset();
                        //this.ResetNodes();
                    }

                    if (this.WriteDataToFile)
                    {

                        this.AppendSettingsToCSV();
                        var doc = this._csvBuilder.ToString();

                        System.IO.File.WriteAllText(_desktopPath + "\\" + fileName, doc);
                    }

                });
            this._timer.Start();
            findAnswer.Start();
        }

        private void ResetNodes()
        {
            // reset viaul nodes
            //foreach (var node in this._pointEllipsee)
            //{
            //    node.Value.Fill = Brushes.White;
            //}

            // reset data nodes
            foreach (var node in this.NodeData.Values)
            {
                node.Color = -1;
                node.IsExplored = false;
                node.IsFinalNode = false;
                node.IsInitialNode = false;
                node.IsExpanded = false;
            }
        }

        private void ColorNodesFromGreedySearch(object sender, IList<Node> results)
        {
            var nodeColors = new Dictionary<int, K_GraphColoring.Color>();

            foreach (var node in results)
            {
                nodeColors.Add(node.Id, (K_GraphColoring.Color)node.Color);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsComplete = _k_greedyColoring.IsComplete.ToString();
                ColorNodes(nodeColors, 0);
            });
        }

        private void LocalGreedyColoring()
        {
            _k_greedyColoring = new K_Graph_Greedy_Coloring(this._k_graph_init, this.SelectFirstPointAtRandom);
            _k_greedyColoring.GreedyColoringComplete += ColorNodesFromGreedySearch;
            _k_greedyColoring.BeginColoring();
        }

        private void GA_Algo()
        {
            Parallel.For(0, this.CrowdSize, cs =>
            {
                this._k_ga_settings = new K_Graph_GA_Settings();

                this._k_ga_settings.PopulationSize = this.PopulationSize;
                this._k_ga_settings.MutationRate = this.MutationRate / 100;
                this._k_ga_settings.GenerationCap = this.GenerationCap;
                this._k_ga_settings.SelectFirstPointAtRandom = this.SelectFirstPointAtRandom;
                this._k_ga_settings.UseColorist = this.UseColorist;

                this._k_ga = new K_Graph_GA(this._k_graph_init, this._k_ga_settings);

                //this._k_ga.NewPopulationMember += New_KGAPopulationMember;
                //this._k_ga.NewPopulationMember += ShowNextPopulationMember;
                this._k_ga.LifeComplete += GA_LifeComplete;

                if (this.ShowAll)
                {
                    this._k_ga.NewFittestMember += (s, e) =>
                    {
                        if (!this.UseWisdomOfCrowds)
                        {
                            this._currentFittestMember = e;
                            this._coloringQueue.Enqueue(e);
                        }
                    };

                    this._k_ga.NewPopulationMember += (s, e) =>
                    {
                        if (!this.UseWisdomOfCrowds)
                            this._coloringQueue.Enqueue(e);
                    };
                }

                this._k_ga.BeginEvolution();
                this._k_graph_init.RestColorAvailabilityCount();
            });
            //(int cs = 0; cs < this.CrowdSize; cs++)


            if (this.UseWisdomOfCrowds)
            {
                this._wocSolutionMember = this.WisdomOfCrowds.GetWoCSolution(this.UseWoCResolver);
                this._coloringQueue.Enqueue(this._wocSolutionMember);
            }
        }

        private void GA_LifeComplete(object sender, EventArgs e)
        {
            var ga = sender as K_Graph_GA;
            this._finalFittestMember = ga.FittestMember;

            if (this.UseWisdomOfCrowds)
            {
                this.WisdomOfCrowds.AddExpert(this._finalFittestMember);
                this._expertRows.Enqueue(this._finalFittestMember);
            }


            this._coloringQueue.Enqueue(this._finalFittestMember);
        }

        private void ShowNextPopulationMember(object sender, EventArgs e)
        {
            if (this._coloringQueue.Count > 0)
            {
                var member = this._coloringQueue.Dequeue();

                if (this.UseWisdomOfCrowds)
                    this.ExpertIndex++;

                if (member.Generation == -1)
                    this.SolutionType = "Wisdom of Crowds Solution";
                else
                    this.SolutionType = "Genetic Algorithm Solution";


                this.ColorNodes(member.NodeColors, member.Generation);
                this.IsComplete = member.IsComplete.ToString();
                this.NumberOfColoringMembersReceived++;
            }

            if (this.UseWisdomOfCrowds && (this._expertRows != null && this._expertRows.Count > 0))
            {
                var mem = this._expertRows.Dequeue();
                this.ExpertDetails.Rows.Add(mem.Generation, mem.NumberOfColorsUsed, mem.IsComplete.ToString());
                this.OnPropertyChanged("ExpertDetails");
            }
        }

        private void AppendResultsToCSV()
        {
            if (ExecuteLocalGreedy)
            {
                var graphSize = this._k_graph_init.NodeData.Count;
                var time = this.ElapsedRunTime;
                this._csvBuilder.Append(graphSize + ", ")
                                .Append(time.ToString() + ", ")
                                .Append(_k_greedyColoring.IsComplete.ToString() + ", ")
                                .AppendLine(_k_greedyColoring.ColorsUsed.ToString());
            }
            else
            {
                this._colorCountTracker.Add(this._k_ga.FittestMember.NumberOfColorsUsed);

                if (!this.UseWisdomOfCrowds)
                {
                    int fitMemId = this._k_ga.FittestMember.MemberId;
                    int gen = this._k_ga.FittestMember.Generation;
                    int totMem = this._k_ga.TotalHistoryMemeberCount;
                    int fitMemRange = this._k_ga.FittestMemberGenerationalRange;
                    int fitColUsd = this._k_ga.FittestMember.NumberOfColorsUsed;
                    double time = this.ElapsedRunTime;

                    this._csvBuilder.Append(fitMemId.ToString() + ", ")
                                    .Append(totMem.ToString() + ", ")
                                    .Append(gen.ToString() + ", ")
                                    .Append((gen / this._k_ga_settings.GenerationCap) + ", ")
                                    .Append(fitColUsd.ToString() + ", ")
                                    .Append(fitMemRange.ToString() + ", ")
                                    .AppendLine(time.ToString());

                }
                else
                {
                    //foreach (var ex in this.WisdomOfCrowds.ExpertList)
                    //{

                    //    int fitMemId = ex.MemberId;
                    //    int gen = ex.Generation;
                    //    int fitColUsd = ex.NumberOfColorsUsed;

                    //    this._csvBuilder.Append(fitMemId.ToString() + ", ")
                    //                    .Append(gen.ToString() + ", ")
                    //                    .AppendLine(fitColUsd.ToString() + ", ");


                    //}

                    // append WOC data
                    this._csvBuilder.Append(this.CrowdSize + ", ")
                                    .Append(this.WisdomOfCrowds.ExpertList.Min(x => x.NumberOfColorsUsed).ToString() + ", ")
                                    .AppendLine(this._wocSolutionMember.NumberOfColorsUsed.ToString());
                }
            }

        }

        private void AppendSettingsToCSV()
        {
            var cc = this._colorCountTracker.Distinct();

            this._csvBuilder.AppendLine();

            foreach (var c in cc)
            {
                var csol = this._colorCountTracker.Count(x => x == c);

                this._csvBuilder.AppendLine(c.ToString() + " Color Solution, " + csol);
            }

            this._csvBuilder.AppendLine()
                .AppendLine("Select First point at Random," + this.SelectFirstPointAtRandom)
                .AppendLine("Average Execution Time," + Math.Round(this._averageExecutionTime / this.Iterations, 5));

            if (!this.ExecuteLocalGreedy)
            {
                this._csvBuilder
                    .AppendLine("Population Size," + this._k_ga_settings.PopulationSize)
                    .AppendLine("Mutation Rate," + this._k_ga_settings.MutationRate)                    
                    .AppendLine("Generation Cap," + this._k_ga_settings.GenerationCap)
                    .AppendLine("Intelligent Coloring," + this._k_ga_settings.UseColorist);
            }
        }

        private void ColorNodes(IDictionary<int, K_GraphColoring.Color> nodeColors, int generation)
        {

            var numberOfColorsUsed = nodeColors.Values.Distinct().Count(x => x != K_GraphColoring.Color.White);

            if (this.NumberOfColorsToUse != numberOfColorsUsed)
                this.NumberOfColorsToUse = numberOfColorsUsed;

            this.FinalMemberGeneration = generation;

            foreach (var xc in nodeColors)
            {

                var color = xc.Value;
                /*
                 *          Red = 0,
                            Orange,
                            Yellow,
                 *          Green,
                            Blue,
                            Indigo,
                            Violet
                 */
                SolidColorBrush fillColor = _colorMapping[color];
                this._pointEllipsee[xc.Key].Fill = fillColor;
            }
        }

        private void DrawNodes()
        {
            this._graphPoints = new Dictionary<int, Ellipse>();
            this._view.cnvsPath
                      .Dispatcher
                      .Invoke
                      (
                        () =>
                        {
                            this._view.cnvsPath.Children.Clear();

                            foreach (var n in this.NodeData.Values)
                            {
                                var labelAndEllipse = TSPUIElements.GetLabelAndEllipse(n, null, false, 20);

                                Canvas.SetZIndex(labelAndEllipse.Item1, 1);
                                Canvas.SetZIndex(labelAndEllipse.Item2, 1);

                                this._pointEllipsee.Add(n.Id, labelAndEllipse.Item2);
                                this._view.cnvsPath.Children.Add(labelAndEllipse.Item1);
                                this._view.cnvsPath.Children.Add(labelAndEllipse.Item2);
                            }
                        },
                        DispatcherPriority.Render);
        }


        private void DrawConnections()
        {
            foreach (var p in this.NodeData.Values)
            {
                var ie = this._pointEllipsee[p.Id];

                if (p.Connections != null)
                {
                    foreach (var next in p.Connections)
                    {
                        // if the edge hasn't been drawn draw it
                        if (!next.IsExpanded)
                        {
                            var de = this._pointEllipsee[next.Id];
                            var conn = new ConnectionDetails(p, next);
                            var pEdge = new PointDetails(ie, de, conn);
                            SolidColorBrush color = System.Windows.Media.Brushes.RoyalBlue;

                            var line = K_GraphDrawer.GetEdge(pEdge, false, color);
                            this._view.cnvsPath.Children.Add(line);

                            // mark that we've drawn the edge
                            p.IsExpanded = true;
                        }
                    }
                }
            }
        }


        #endregion Methods

        #region Wisdom of Crowds

        private bool _useWisdomOfCrowds;
        public bool UseWisdomOfCrowds
        {
            get { return this._useWisdomOfCrowds; }
            set
            {
                this._useWisdomOfCrowds = value;
                this.CrowdSize = 1;
                this.OnPropertyChanged(nameof(UseWisdomOfCrowds));
            }
        }

        private uint _crowdSize;
        public uint CrowdSize
        {
            get { return this._crowdSize; }
            set
            {
                this._crowdSize = value;
                this.OnPropertyChanged(nameof(CrowdSize));
            }
        }

        public K_Graph_WisdomOfCrowds WisdomOfCrowds { get; private set; }

        #endregion Wisdom of Crowds

        #region local greedy

        #endregion

    }
}
