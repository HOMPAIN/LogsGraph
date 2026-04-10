using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace LogsGraph.DataStorage
{
    //настройки отоборажения отдельного графика
    public class WorkSpaceGraph
    {
        public Color Color = Colors.Blue;//цвет графика
        public int Style=0;//0 - лини, 1 - пунктир, 3 - точки, 4 - линия с точкой, 5 - пунктир с точкой
        //точки графика
        public GraphData Graph;
        public WorkSpaceGraph(GraphData _Graph)
        {
            Graph = _Graph;
        }
        public override string ToString()
        {
            return Graph.ToString();
        }
    }
    //настройки одной области вывода графиков проекта
    public class WorkSpacePlot
    {
        //имя для области вывода
        public string Name;
        //позиция легенды на графике
        public int LegendPosition = 0;// 0 - снизу, 1 сверху,...дописать варианты
        //высота окна
        public double Height = 300;
        //список графиков в этой области
        public List<WorkSpaceGraph> Graphycs=new List<WorkSpaceGraph>();

        public WorkSpacePlot(string _Name)
        {
            Name = _Name;
        }
    }
    //текущий проект графиков
    public class WorkSpace
    {
        //все графики в текущем проекте
        public List<GraphData> Graphs=new List<GraphData>();
        //список областей отображения
        public List<WorkSpacePlot> Plots =new List<WorkSpacePlot>();

        public event Action? GraphsListUpdated;

        public void Add(GraphData _Graph)
        {
            Graphs.Add(_Graph);
            GraphsListUpdated?.Invoke();
        }
        //добавить окно графиков
        public WorkSpacePlot AddPlot(int _Index=-1)
        {
            WorkSpacePlot plot = new WorkSpacePlot("Plot"+ Plots.Count);
            if(_Index<0 || _Index> Plots.Count)
                Plots.Add(plot);
            else
                Plots.Insert(_Index, plot);

            return plot;
        }
    }
}
