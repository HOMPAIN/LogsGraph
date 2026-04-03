using System;
using System.Collections.Generic;
using System.Text;

namespace LogsGraph.DataStorage
{
    //текущий проект графиков
    public class WorkSpace
    {
        //графики в текущем проекте
        public List<GraphData> Graphs=new List<GraphData>();

        public void Add(GraphData _Graph)
        {
            Graphs.Add(_Graph);
        }
    }
}
