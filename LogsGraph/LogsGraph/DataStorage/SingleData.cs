//данные для одного графика
using System;
using System.Collections.Generic;
using System.Text;

namespace LogsGraph
{
    public class SinglePoint
    {
        public double X;
        public double Y;
    }
    public class SingleData
    {
        public string Name;//имя для отображения
        public string Marker;//маркер для парсинга

        List<SinglePoint> Points;
    }
}
