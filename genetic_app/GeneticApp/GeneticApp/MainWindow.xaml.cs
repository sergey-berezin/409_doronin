using genetic_algorithm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace GeneticApp
{
    public interface IMainWindowWithDrawableCanvas
    {
        void canvas_DrawPopulation(int n = 3);
    }
    public partial class MainWindow : Window, IMainWindowWithDrawableCanvas
    {
        const int POINT_SIZE = 20;

        public MainWindow()
        {
            InitializeComponent();
            Data data = new Data(this);
            data.CanDrag = true;
            data.N = 0;
            data.MutIndex = 30;
            data.InputControlsEnabled = true;
            data.IterButtonEnabled = false;
            data.StopButtonEnabled = false;
            data.ResetButtonEnabled = false;
            data.CrossIndex = 1;
            data.NPop = 1500;
            data.Generation = 0;
            data.BestRouteLen = -1;
            DataContext = data;
        }
        private void clearPoints()
        {
            var ellipses = canvasArea.Children.OfType<Ellipse>().ToList();
            foreach (UIElement ellipse in ellipses)
            {
                canvasArea.Children.Remove(ellipse);
            }
        }
        private void addPoint()
        {
            Ellipse newPoint = new Ellipse();
            SolidColorBrush color = new SolidColorBrush();
            color.Color = Color.FromArgb(255, 255, 255, 0);
            newPoint.Fill = color;
            newPoint.Stroke = color;
            newPoint.Width = POINT_SIZE;
            newPoint.Height = POINT_SIZE;

            int maxx = (int)canvasArea.ActualHeight;
            int maxy = (int)canvasArea.ActualWidth;
            int x = Random.Shared.Next(POINT_SIZE / 2, maxx - POINT_SIZE);
            int y = Random.Shared.Next(POINT_SIZE / 2, maxy - POINT_SIZE);
            Canvas.SetTop(newPoint, x);
            Canvas.SetLeft(newPoint, y);

            canvasArea.Children.Add(newPoint);
        }
        private float[,] createMap()
        {
            int N = ((Data)DataContext).N;

            float[,] map = new float[N,N];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    (float ix, float iy) = ((Data)DataContext).dots[i];
                    (float jx, float jy) = ((Data)DataContext).dots[j];

                    double distance = Math.Sqrt(Math.Pow(ix - jx, 2) + Math.Pow(iy - jy, 2));
                    map[i, j] = (float)distance;
                    map[j, i] = (float)distance;
                }
            }
            return map;
        }
        private void dataDots()
        {
            ((Data)DataContext).dots.Clear();
            foreach (UIElement elem in canvasArea.Children)
            {
                if (elem is not Ellipse)
                    continue;
                int x = (int)Canvas.GetTop(elem);
                int y = (int)Canvas.GetLeft(elem);
                ((Data)DataContext).dots.Add((x + POINT_SIZE / 2, y + POINT_SIZE / 2));
            }
            ((Data)DataContext).N = ((Data)DataContext).dots.Count;
        }
        public void canvas_DrawPopulation(int n = 3)
        {
            canvas_ClearLines();
            List<(int, int)> dots = ((Data)DataContext).dots;
            List<Route> routes = ((Data)DataContext).population.bestOfN(n);
            foreach (Route route in routes)
            {
                for (int i = 0; i < Route.N; i++)
                {
                    int firstindex = route.route[i] - 1;
                    int secondindex = route.route[(i + 1) % Route.N] - 1;
                    canvas_DrawLine(dots[firstindex], dots[secondindex]);
                }
            }  
        }
        private void canvas_DrawLine((int, int) i, (int, int) j)
        {
            Line line = new Line();

            Random r = new Random();
            Brush brush = new SolidColorBrush(Color.FromRgb((byte)Random.Shared.Next(1, 255),
                              (byte)Random.Shared.Next(1, 255), (byte)Random.Shared.Next(1, 233)));

            line.Stroke = brush;

            (line.Y1, line.X1) = i;
            (line.Y2, line.X2) = j;

            line.StrokeThickness = 1;

            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            canvasArea.Children.Add(line);
        }
        private void canvas_ClearLines()
        {
            var lines = canvasArea.Children.OfType<Line>().ToList();
            foreach (UIElement line in lines)
            {
                canvasArea.Children.Remove(line);
            }
        }
        private void button_AddPoint(object sender, RoutedEventArgs e)
        {
            addPoint();
        }

        private void button_AddPoints(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++) { addPoint(); }
        }

        private void button_Initiate(object sender, RoutedEventArgs e)
        {
            ((Data)DataContext).InputControlsEnabled = false;
            ((Data)DataContext).IterButtonEnabled = true;
            ((Data)DataContext).ResetButtonEnabled = true;
            dataDots();
            Route.map = createMap();
            Route.N = ((Data)DataContext).N;
            int NPop = ((Data)DataContext).NPop;
            int MutIndex = ((Data)DataContext).MutIndex;
            int CrossIndex = ((Data)DataContext).CrossIndex;
            ((Data)DataContext).population = new Population<Route>(NPop, MutIndex, CrossIndex);
            ((Data)DataContext).UpdateBestRouteLen();

            canvas_DrawPopulation();
        }
        private void button_Reset(object sender, RoutedEventArgs e)
        {
            canvas_ClearLines();
            ((Data)DataContext).Generation = 0;
            ((Data)DataContext).BestRouteLen = -1;
            ((Data)DataContext).InputControlsEnabled = true;
            ((Data)DataContext).IterButtonEnabled = false;
            ((Data)DataContext).ResetButtonEnabled = false;
        }

        private void button_Iterate(object sender, RoutedEventArgs e)
        {
            ((Data)DataContext).StartWhile();
        }

        private void button_StopIterations(object sender, RoutedEventArgs e)
        {
            ((Data)DataContext).StopWhile();
        }

        private void button_ClearPoints(object sender, RoutedEventArgs e)
        {
            clearPoints();
        }


    }


    public class Data : INotifyPropertyChanged
    {
        public Population<Route> population;
        public List<(int, int)> dots = new List<(int, int)>();
        public CancellationTokenSource cts;
        private int n;
        private bool _canDrag;
        private bool _controlsBlocked;
        private bool _inputControlsEnabled;
        private bool _iterButtonEnabled;
        private bool _stopButtonEnabled;
        private bool _resetButtonEnabled;
        private int _n_pop;
        private int _mut_index;
        private int _cross_index;
        private int _generation;
        private readonly IMainWindowWithDrawableCanvas _mainwindow;
        private double bestroutelen;
        public int NPop
        {
            get => _n_pop;
            set
            {
                _n_pop = value;
                OnPropertyChanged(nameof(NPop));
            }
        }

        public int MutIndex
        {
            get => _mut_index;
            set
            {
                _mut_index = value;
                OnPropertyChanged(nameof(MutIndex));
            }
        }

        public int CrossIndex
        {
            get => _cross_index;
            set
            {
                _cross_index = value;
                OnPropertyChanged(nameof(CrossIndex));
            }
        }
        public int Generation
        {
            get => _generation;
            set
            {
                _generation = value;
                OnPropertyChanged(nameof(Generation));
            }
        }
        public bool CanDrag
        {
            get => _canDrag;
            set
            {
                _canDrag = value;
                OnPropertyChanged(nameof(CanDrag));
            }
        }
        public bool ResetButtonEnabled
        {
            get => _resetButtonEnabled;
            set
            {
                _resetButtonEnabled = value;
                OnPropertyChanged(nameof(ResetButtonEnabled));
            }
        }
        public bool InputControlsEnabled
        {
            get => _inputControlsEnabled;
            set
            {
                _inputControlsEnabled = value;
                OnPropertyChanged(nameof(InputControlsEnabled));
            }
        }
        public bool IterButtonEnabled
        {
            get => _iterButtonEnabled;
            set
            {
                _iterButtonEnabled = value;
                OnPropertyChanged(nameof(IterButtonEnabled));
            }
        }
        public bool StopButtonEnabled
        {
            get => _stopButtonEnabled;
            set
            {
                _stopButtonEnabled = value;
                OnPropertyChanged(nameof(StopButtonEnabled));
            }
        }
        public double BestRouteLen
        {
            get => bestroutelen;
            set
            {
                bestroutelen = value;
                OnPropertyChanged(nameof(BestRouteLen));
            }
        }
        public int N
        {
            get => n;
            set
            {
                n = value;
                OnPropertyChanged(nameof(N));
            }
        }
        public Data(IMainWindowWithDrawableCanvas mainWindow)
        {
            _mainwindow = mainWindow;
        }
        public void UpdateBestRouteLen()
        {
            BestRouteLen = -1 * population.bestOfN(1)[0].Fit();
        }
        public void ClearDots()
        {
            dots.Clear();
        }
        public void InitiateFromControls()
        { 

        }
        public void StartWhile()
        {
            Task.Run(() =>
            {
                cts = new CancellationTokenSource();
                ResetButtonEnabled = false;
                IterButtonEnabled = false;
                StopButtonEnabled = true;
                while (!cts.Token.IsCancellationRequested)
                {
                    var print = System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _mainwindow.canvas_DrawPopulation();
                    });
                    population.MutateP();
                    population.CrossoverP();
                    print.Wait();
                    population.SelectBySortP();
                    UpdateBestRouteLen();
                    Generation++;
                }
                IterButtonEnabled = true;
                StopButtonEnabled = false;
                ResetButtonEnabled = true;
                
            });
        }
        public void StopWhile()
        {
            cts.Cancel();
        }
            
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
  
}
