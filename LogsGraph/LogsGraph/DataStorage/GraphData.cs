using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LogsGraph.DataStorage
{
    //точка графика
    public struct GraphPoint
    {
        public long X; 
        public double Y;

        public GraphPoint(long _X, double _Y)
        {
            X = _X;
            Y = _Y;
        }
    }
    //один график
    public class GraphData
    {
        public string Name;//имя графика
        public List<GraphPoint> Points= new List<GraphPoint>();//точки графика

        public override string ToString()
        {
            if (Name == null)
                return "null";
            return Name;
        }

        public GraphData()
        {
        }
        public GraphData(string _Name)
        {
            Name = _Name;
        }
        public GraphData(string _Name, int _Count)
        {
            Name = _Name;
            Points = new List<GraphPoint>(_Count);
        }
        //добавить, запасить значение точки
        public void Add(string _X, string _Y, FileFormat _Format)
        {
            // 1. Проверка на пустые значения
            if (string.IsNullOrWhiteSpace(_X) || string.IsNullOrWhiteSpace(_Y))
            {
                return; // Точки нет, просто выходим
            }

            CultureInfo culture = CultureInfo.InvariantCulture;
            long xValue=0;
            double yValue=0;
            bool xParsed = false;

            // 2. Парсинг X (Дата или Число)
            string cleanX = _X.Trim();

            if (!string.IsNullOrEmpty(_Format.DateFormat))
            {
                if (DateTime.TryParseExact(cleanX, _Format.DateFormat, culture, DateTimeStyles.AssumeLocal, out DateTime dt))
                {
                    xValue = ((DateTimeOffset)dt.ToUniversalTime()).ToUnixTimeSeconds();
                    xParsed = true;
                }
            }

            if (!xParsed)
            {
                // Пробуем как число
                string normalizedX = cleanX.Replace(_Format.PointSimbol, ".");
                if (double.TryParse(normalizedX, NumberStyles.Any, culture, out double numX))
                {
                    xValue = (long)numX;
                    xParsed = true;
                }
            }

            // Если X не распарсился, точку добавить нельзя
            if (!xParsed) return;

            // 3. Парсинг Y (Только число)
            string cleanY = _Y.Trim().Replace(_Format.PointSimbol, ".");
            if (!double.TryParse(cleanY, NumberStyles.Any, culture, out yValue))
            {
                return; // Y не распарсился, точку не добавляем
            }

            // 4. Добавление точки
            Points.Add(new GraphPoint(xValue, yValue));
        }
        /// <summary>
        /// Парсит текстовый файл и возвращает список графиков.
        /// </summary>
        /// <param name="_File">Путь к файлу</param>
        /// <param name="_Format">Настройки формата парсинга</param>
        public static List<GraphData> ParseFile(string _File, FileFormat _Format)
        {
            int graphs_count = 0;
            var resultGraphs = new List<GraphData>();

            //чтение файла
            if (!File.Exists(_File))
                throw new FileNotFoundException($"Файл не найден: {_File}");

            var tempPoints = new List<List<GraphPoint>>();
            var lines = File.ReadAllLines(_File);
            CultureInfo culture = CultureInfo.InvariantCulture;

            //удаление лишних пробелов
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].Trim();

            //чтение заголовка
            if (_Format.CustomMarkers.Count == 0)
            {
                List<string> names = new List<string>(lines[_Format.IgnoreRows].Split(_Format.Separate));
                names.Remove("");
                if (_Format.MultiX)
                {
                    graphs_count = names.Count / 2;
                    if (graphs_count * 2 != names.Count)
                        return null;//неправильное число столбцов, должно быть чётным
                    for(int i=0;i<graphs_count;i++)
                        resultGraphs.Add(new GraphData(names[i * 2 + 1], lines.Length - _Format.IgnoreRows));
                }
                else
                {
                    graphs_count = names.Count - 1;
                    for (int i = 0; i < graphs_count; i++)
                        resultGraphs.Add(new GraphData(names[i + 1], lines.Length - _Format.IgnoreRows));
                }
            }

            //чтение данных
            for (int i = (_Format.IgnoreRows+1); i < lines.Length; i++)
            {
                string[] valuse = lines[i].Split(_Format.Separate);

                if (_Format.MultiX)
                    for (int j = 0; j < graphs_count; j++)
                        resultGraphs[j].Add(valuse[j*2], valuse[j*2+1], _Format);
                else
                    for (int j = 0; j < graphs_count; j++)
                        resultGraphs[j].Add(valuse[0], valuse[j+1], _Format);
            }

            return resultGraphs;
        }
    }
}
