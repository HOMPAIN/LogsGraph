//WPF кастомный график
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    /// Логика взаимодействия для Graph.xaml
    /// </summary>
    public partial class Graph : UserControl
    {
        public event EventHandler EventAdd;
        public event EventHandler EventDell;
        PlotModel PlotModel;
        public Graph()
        {
            InitializeComponent();

            PlotModel = new PlotModel { Title = $"График #" };

            // Настройка осей (опционально, но улучшает внешний вид)
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            // 2. Создаём серию данных
            var lineSeries = new LineSeries
            {
                Title = "Сигнал",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.White
            };

            // Добавляем начальные точки (для примера)
            Random rnd = new();
            for (int i = 0; i < 30; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, rnd.NextDouble() * 100));
            }

            PlotModel.Series.Add(lineSeries);
            Plot.Model = PlotModel;
        }

        //кнопка скрытия
        private void ClickHide(object sender, RoutedEventArgs e)
        {

        }
        //кнопка увеличения размера
        private void ClickSizeAdd(object sender, RoutedEventArgs e)
        {
            Height *= 1.1;
        }
        //кнопка уменьшения размера
        private void ClickSizeSub(object sender, RoutedEventArgs e)
        {
            Height *= 0.9;
            if (Height < 80) Height = 80;
        }
        //кнопка выбор графиков
        private void ClickGraphSettings(object sender, RoutedEventArgs e)
        {

        }
        //кнопка удалить график
        private void ClickDell(object sender, RoutedEventArgs e)
        {
            if (EventDell != null)
                EventDell(this, EventArgs.Empty);
        }
        //кнопка добавить график
        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            if (EventAdd != null)
                EventAdd(this, EventArgs.Empty);
        }
    }
}
