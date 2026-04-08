using System;
using System.Collections.Generic;
using System.Text;

namespace LogsGraph.DataStorage
{
    //настройки отоборажения отдельного графика
    public class WorkSpaceGraph
    {
        //точки графика
        GraphData Graph;
    }
    //настройки одной области вывода графиков проекта
    public class WorkSpacePlot
    {
        //имя для области вывода
        public string Name;
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

        public void Add(GraphData _Graph)
        {
            Graphs.Add(_Graph);
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
