//WPF кастомный график
using LogsGraph.DataStorage;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

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
            WorkSpace.GraphsListUpdated += AllGraphs.Items.Refresh;
            CbLegendPosition.SelectedIndex = WorkSpacePlot.LegendPosition;
            LegendUpdate();

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
            WorkSpacePlot.Height = Height;
        }
        //кнопка уменьшения размера
        private void ClickSizeSub(object sender, RoutedEventArgs e)
        {
            Height *= 0.9;
            if (Height < 80) Height = 80;
            WorkSpacePlot.Height = Height;
        }
        //кнопка выбор графиков и настроек
        private void ClickGraphSettings(object sender, RoutedEventArgs e)
        {
            if (Settings.Visibility == Visibility.Visible)
            {
                Height = HideHeight;
                Settings.Visibility = Visibility.Hidden;
                UpdatePlot();
                LegendUpdate();
            }
            else
            {
                HideHeight = Height;
                Settings.Visibility = Visibility.Visible;
                if(Height<150)
                    Height = 150;
            }
        }
        //кнопка удалить график
        private void ClickDell(object sender, RoutedEventArgs e)
        {
            WorkSpace.GraphsListUpdated -= AllGraphs.Items.Refresh;
            if (EventDell != null)
                EventDell(this, EventArgs.Empty);
        }
        //кнопка добавить график снизу
        private void ClickAdd(object sender, RoutedEventArgs e)
        {
            if (EventAdd != null)
                EventAdd(this, EventArgs.Empty);
        }
        //изменение имени
        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            WorkSpacePlot.Name = TxtName.Text;
            LegendUpdate();
        }
        //добавить график для отображения
        private void BtnAddGraph_Click(object sender, RoutedEventArgs e)
        {
            if(AllGraphs.SelectedItem is GraphData)
            {
                WorkSpacePlot.Graphycs.Add(new WorkSpaceGraph((GraphData)AllGraphs.SelectedItem));
                PlotGraphycs.Items.Refresh();
            }
        }
        //удалить график с данного отображения
        private void BtnRemoveGraph_Click(object sender, RoutedEventArgs e)
        {
            if (PlotGraphycs.SelectedItem is WorkSpaceGraph)
            {
                PlotGraphycs.Items.Remove(PlotGraphycs.SelectedItem);
                PlotGraphycs.Items.Refresh();
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
                Color c = WorkSpacePlot.Graphycs[i].Color;
                // 2. Создаём серию данных
                var lineSeries = new LineSeries
                {
                    Title = "Сигнал",
                    Color = OxyColor.FromArgb(c.A,c.R,c.G,c.B),
                    StrokeThickness = 1,
                    MarkerType = MarkerType.None,
                    MarkerSize = 2,
                    MarkerStroke = OxyColors.Blue,
                    MarkerFill = OxyColors.White
                };
                switch(WorkSpacePlot.Graphycs[i].Style)
                {
                    case 0:
                        lineSeries.LineStyle = LineStyle.Solid;
                        lineSeries.MarkerType = MarkerType.None;
                        break;
                    case 1:
                        lineSeries.LineStyle = LineStyle.Dot;
                        lineSeries.MarkerType = MarkerType.None;
                        break;
                    case 2:
                        lineSeries.LineStyle = LineStyle.None;
                        lineSeries.MarkerType = MarkerType.Circle;
                        break;
                    case 3:
                        lineSeries.LineStyle = LineStyle.Solid;
                        lineSeries.MarkerType = MarkerType.Circle;
                        break;
                    case 4:
                        lineSeries.LineStyle = LineStyle.Dot;
                        lineSeries.MarkerType = MarkerType.Circle;
                        break;
                }


                for (int j = 0; j < WorkSpacePlot.Graphycs[i].Graph.Points.Count; j++)
                {
                    lineSeries.Points.Add(new DataPoint(WorkSpacePlot.Graphycs[i].Graph.Points[j].X, WorkSpacePlot.Graphycs[i].Graph.Points[j].Y));
                }

                PlotModel.Series.Add(lineSeries);
            }
            Plot.Model = PlotModel;
        }
        //выбор графика из списка графиков этого окна
        private void PlotGraphycs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGraphSettings();
        }
        //вывести настройки текущего графика в окно настроек
        private void UpdateGraphSettings()
        {
            if (PlotGraphycs.SelectedItem is WorkSpaceGraph)
            {
                WorkSpaceGraph wsg = (WorkSpaceGraph)PlotGraphycs.SelectedItem;
                TxtColor.Text = wsg.Color.ToString();
                //TxtColor.Background = new SolidColorBrush(wsg.Color);
                CbLineType.SelectedIndex = wsg.Style;
            }
        }
        //выбор цвета линии графика
        private void TxtColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlotGraphycs.SelectedItem is WorkSpaceGraph)
            {
                WorkSpaceGraph wsg = (WorkSpaceGraph)PlotGraphycs.SelectedItem;
                try
                {
                    wsg.Color = (Color)ColorConverter.ConvertFromString(TxtColor.Text);
                    //TxtColor.Background = new SolidColorBrush(wsg.Color);
                }
                catch { }
            }
        }
        //выбор типа линии графика
        private void CbLineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlotGraphycs.SelectedItem is WorkSpaceGraph)
            {
                WorkSpaceGraph wsg = (WorkSpaceGraph)PlotGraphycs.SelectedItem;
                wsg.Style = CbLineType.SelectedIndex;
            }
        }
        //выбор расположения легенды
        private void CbLegendPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WorkSpacePlot.LegendPosition = CbLegendPosition.SelectedIndex;
            LegendUpdate();
        }
        //обновление отрисоки легенды
        private void LegendUpdate()
        {
            switch(WorkSpacePlot.LegendPosition)
            {
                case 0:
                    LegendTop.Height = 0;
                    LegendBot.Height = 18;
                    break;
                case 1:
                    LegendTop.Height = 18;
                    LegendBot.Height = 0;
                    break;
            }
            BLName.Content = TLName.Content = WorkSpacePlot.Name;
            //заполнение легенды
            while (LegendTop.Children.Count > 1) LegendTop.Children.RemoveAt(LegendTop.Children.Count - 1);
            while (LegendBot.Children.Count > 1) LegendBot.Children.RemoveAt(LegendBot.Children.Count - 1);
            for (int i = 0; i < WorkSpacePlot.Graphycs.Count; i++)
            {
                LegendTop.Children.Add(new Label());
                SetLegendLable(LegendTop.Children[i + 1] as Label, WorkSpacePlot.Graphycs[i]);
                LegendBot.Children.Add(new Label());
                SetLegendLable(LegendBot.Children[i + 1] as Label, WorkSpacePlot.Graphycs[i]);
            }
        }
        //применение стиля графика к лэйблу легенды
        private static void SetLegendLable(Label? _Lable, WorkSpaceGraph _Graph)
        {
            if (_Lable == null) return;

            _Lable.Padding = new Thickness(1,0,1,0);
            _Lable.Content = _Graph.Graph.Name;
            _Lable.Foreground = new SolidColorBrush(_Graph.Color);
        }
    }
}
