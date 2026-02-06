//WPF компонент для отображения списка графиков

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
        }

        //добавить график
        public void AddPlot()
        {
            Graph graph = new Graph();
            graph.EventDell += RemovePlot;
            graph.EventAdd += AddPlot;
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
            int index = PlotsContainer.Children.IndexOf((UIElement)_Object);
            PlotsContainer.Children.Insert(index + 1, graph);
        }
        //удалить последний график
        public void RemovePlot(object? _Object, EventArgs _E)
        {
            if(_Object != null )
                PlotsContainer.Children.Remove((UIElement)_Object);
        }

    }
}
