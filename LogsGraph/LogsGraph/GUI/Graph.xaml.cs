//WPF кастомный график
using LogsGraph.DataStorage;
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
        double HideHeight = 0;

        WorkSpace WorkSpace;//текущий проект
        WorkSpacePlot WorkSpacePlot;//настройки текущей области графиков
        public Graph(WorkSpacePlot _WorkSpacePlot)
        {
            WorkSpace = ((App)Application.Current).WorkSpace;
            WorkSpacePlot = _WorkSpacePlot;

            InitializeComponent();

            //скрыть окно настроек графиков
            Settings.Visibility = Visibility.Hidden;

            //заполнение полей настроек
            TxtName.Text = WorkSpacePlot.Name;
            AllGraphs.ItemsSource = WorkSpace.Graphs;
            PlotGraphycs.ItemsSource = WorkSpacePlot.Graphycs;

            PlotModel = new PlotModel { };

            //убрать отступы графика
            PlotModel.PlotMargins = new OxyThickness(0,2,20,2);//размер подписи и числа на оси 
            PlotModel.Padding = new OxyThickness(0,0,10,0);//размер разметки оси

            // Настройка осей (опционально, но улучшает внешний вид)
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right });

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

            //отлючаем стандартное управление мышкой
            Plot.ActualController.UnbindMouseWheel();
            Plot.ActualController.UnbindMouseDown(OxyMouseButton.Right);

        }

        //кнопка скрытия
        private void ClickHide(object sender, RoutedEventArgs e)
        {
            HideHeight = Height;
            BHide.Visibility = Visibility.Visible;
            Height = 10;
        }
        //кнопка раскрытия
        private void ClickShow(object sender, RoutedEventArgs e)
        {
            BHide.Visibility = Visibility.Hidden;
            Height = HideHeight;
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
            if (Settings.Visibility == Visibility.Visible)
                Settings.Visibility = Visibility.Hidden;
            else
                Settings.Visibility = Visibility.Visible;
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
        //изменение имени
        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            WorkSpacePlot.Name = TxtName.Text;
        }
        //добавить график для отображения
        private void BtnAddGraph_Click(object sender, RoutedEventArgs e)
        {
            if(AllGraphs.SelectedItem is GraphData)
            {
                WorkSpacePlot.Graphycs.Add(new WorkSpaceGraph((GraphData)AllGraphs.SelectedItem));
                PlotGraphycs.Items.Refresh();
                UpdatePlot();
            }
        }
        //удалить график с данного отображения
        private void BtnRemoveGraph_Click(object sender, RoutedEventArgs e)
        {
            if (PlotGraphycs.SelectedItem is WorkSpaceGraph)
            {
                PlotGraphycs.Items.Remove(PlotGraphycs.SelectedItem);
                PlotGraphycs.Items.Refresh();
                UpdatePlot();
            }
        }
        //перестроить графики
        public void UpdatePlot()
        {
            PlotModel = new PlotModel { };

            //убрать отступы графика
            PlotModel.PlotMargins = new OxyThickness(0, 2, 20, 2);//размер подписи и числа на оси 
            PlotModel.Padding = new OxyThickness(0, 0, 10, 0);//размер разметки оси

            // Настройка осей (опционально, но улучшает внешний вид)
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right });

            // Добавляем точки
            for (int i = 0; i < WorkSpacePlot.Graphycs.Count; i++)
            {

                // 2. Создаём серию данных
                var lineSeries = new LineSeries
            {
                Title = "Сигнал",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.White
            };


                for (int j = 0; j < WorkSpacePlot.Graphycs[i].Graph.Points.Count; j++)
                {
                    lineSeries.Points.Add(new DataPoint(WorkSpacePlot.Graphycs[i].Graph.Points[j].X, WorkSpacePlot.Graphycs[i].Graph.Points[j].Y));
                }

                PlotModel.Series.Add(lineSeries);
            }
            Plot.Model = PlotModel;
        }
    }
}
