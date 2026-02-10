//WPF компонент для отображения списка графиков

using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для GraphsList.xaml
    /// </summary>
    public partial class GraphsList : UserControl
    {
        public GraphsList()
        {
            InitializeComponent();
            AddPlot();
            AddPlot();
        }

        //добавить график
        public void AddPlot()
        {
            Graph graph = new Graph();
            graph.EventDell += RemovePlot;
            graph.EventAdd += AddPlot;
            graph.Plot.MouseMove += Plot_MouseMove;
            graph.Plot.MouseWheel += Plot_MouseWheel;
            PlotsContainer.Children.Add(graph);
        }
        public void AddPlot(object? _Object, EventArgs _E)
        {
            if(_Object==null)
            {
                AddPlot();
                return;
            }
            Graph graph = new Graph();
            graph.EventDell += RemovePlot;
            graph.EventAdd += AddPlot;
            graph.Plot.MouseMove += Plot_MouseMove;
            graph.Plot.MouseWheel += Plot_MouseWheel;
            int index = PlotsContainer.Children.IndexOf((UIElement)_Object);
            PlotsContainer.Children.Insert(index + 1, graph);
        }
        //обработка колёсика мыши
        private void Plot_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            PlotView? plot = sender as PlotView;
            if (plot == null) return;

            double min = plot.Model.Axes[0].ActualMinimum;
            double max = plot.Model.Axes[0].ActualMaximum;
            double delta = max - min;
            double center = (min + max) * 0.5f;

            // Определяем коэффициент масштабирования
            double zoomFactor = e.Delta > 0 ? 0.9 : 1.1; // 0.9 = приблизить, 1.1 = отдалить
            delta *= zoomFactor*0.5;

            min = center - delta;
            max = center + delta;

            foreach (var graph in PlotsContainer.Children)
            {
                if ((graph as Graph) == null) continue;
                plot = ((Graph)graph).Plot;

                plot.Model.Axes[0].Minimum = min;
                plot.Model.Axes[0].Maximum = max;
                plot.Model.InvalidatePlot(false);
            }
        }
        //обработка движения мыши с нажатой правой кнопкой
        Point old_pos;
        private void Plot_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            if (e.RightButton==MouseButtonState.Pressed)
            {
                PlotView? plot = sender as PlotView;
                if (plot == null) return;

                double min = plot.Model.Axes[0].ActualMinimum;
                double max = plot.Model.Axes[0].ActualMaximum;

                min -= p.X - old_pos.X;
                max -= p.X - old_pos.X;

                foreach (var graph in PlotsContainer.Children)
                {
                    if ((graph as Graph) == null) continue;
                    plot = ((Graph)graph).Plot;

                    plot.Model.Axes[0].Minimum = min;
                    plot.Model.Axes[0].Maximum = max;
                    plot.Model.InvalidatePlot(false);
                }
            }
            old_pos = p;
        }

        //удалить последний график
        public void RemovePlot(object? _Object, EventArgs _E)
        {
            if (PlotsContainer.Children.Count <= 1)
                return;
            if(_Object != null )
                PlotsContainer.Children.Remove((UIElement)_Object);
        }

    }
}
