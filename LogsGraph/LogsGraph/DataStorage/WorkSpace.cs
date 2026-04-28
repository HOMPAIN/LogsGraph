using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Text.Json.Serialization;

namespace LogsGraph.DataStorage
{
    //настройки отоборажения отдельного графика
    public class WorkSpaceGraph
    {
        public Color Color { get; set; } = Colors.Blue;//цвет графика
        public int Style { get; set; } = 0;//0 - лини, 1 - пунктир, 3 - точки, 4 - линия с точкой, 5 - пунктир с точкой
        
        [JsonIgnore]
        public GraphData Graph;//точки графика, это ссылка, сами графики хранятся в WorkSpace, нужно потом переделать на id или имя
        public WorkSpaceGraph()
        {

        }
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
        public string Name { get; set; }="Plot";
        //позиция легенды на графике
        public int LegendPosition { get; set; } = 0;// 0 - снизу, 1 сверху,...дописать варианты
        //высота окна
        public double Height { get; set; } = 300;
        //список графиков в этой области
        public List<WorkSpaceGraph> Graphycs { get; set; } = new List<WorkSpaceGraph>();

        public WorkSpacePlot()
        {

        }

        public WorkSpacePlot(string _Name)
        {
            Name = _Name;
        }
    }
    //текущий проект графиков
    public class WorkSpace: ConfigBase<WorkSpace>
    {
        //все графики в текущем проекте
        public List<GraphData> Graphs { get; set; } = new List<GraphData>();
        //список областей отображения
        public List<WorkSpacePlot> Plots { get; set; } = new List<WorkSpacePlot>();

        //событие изменения списка графиков
        public event Action? GraphsListUpdated;

        public WorkSpace()
        {

        }

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
