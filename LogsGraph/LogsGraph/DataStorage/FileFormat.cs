//описание формата для парсинга файла
using System;
using System.Collections.Generic;
using System.Text;

namespace LogsGraph.DataStorage
{
    public class FileFormat
    {
        public string Name;//имя для данного формата
        public int IgnoreRows = 0;//сколько первых строк в файле игнорировать
        public bool MultiX = false; //для каждого графика свой столбец для X
        public string Separate = ";"; //разделитель значений
        public string PointSimbol=".";//разделите для числа с плавающей точкой

        public List<string> CustomMarkers = new List<string>(); //маркеры столбцов на случай если они не описаны в файле

        public override string ToString()
        {
            return Name ?? "Без имени";
        }
    }
}
