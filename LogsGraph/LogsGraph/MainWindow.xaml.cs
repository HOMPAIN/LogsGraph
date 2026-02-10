using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
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

namespace LogsGraph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Храним все созданные PlotView для управления
        private readonly List<PlotView> _plotViews = new();
        private int _plotCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Добавляем 2 графика при старте для демонстрации
            //AddPlot();
            //AddPlot();
            Graphycs.AddPlot();
            Graphycs.AddPlot();
        }

        // === ДОБАВЛЕНИЕ ГРАФИКА ===
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //AddPlot();
            Graphycs.AddPlot();
        }

    
        // === ДОПОЛНИТЕЛЬНО: метод для динамического обновления данных ===
        /// <summary>
        /// Добавляет новую точку в график по индексу. Удаляет старые точки если их > 100.
        /// </summary>
        public void AddDataPoint(int plotIndex, double x, double y)
        {
            if (plotIndex < 0 || plotIndex >= _plotViews.Count) return;

            var plotModel = _plotViews[plotIndex].Model;
            if (plotModel.Series.Count == 0) return;

            var series = (LineSeries)plotModel.Series[0];
            series.Points.Add(new DataPoint(x, y));

            // Ограничиваем 100 точками — удаляем самые старые
            while (series.Points.Count > 100)
                series.Points.RemoveAt(0);

            // Принудительно перерисовываем график
            plotModel.InvalidatePlot(true);
        }
    }
}