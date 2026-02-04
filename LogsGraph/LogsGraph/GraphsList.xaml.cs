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
            Graph graph=new Graph();
            PlotsContainer.Children.Add(graph);
        }
        //удалить последний график
        public void RemovePlot()
        {

        }
    }
}
