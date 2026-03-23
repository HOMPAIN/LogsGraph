using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LogsGraph.DataStorage
{
    //точка графика
    struct GraphPoint
    {
        public double X; // Изменил long на double, так как координаты часто бывают дробными
        public double Y;

        public GraphPoint(double _X, double _Y)
        {
            X = _X;
            Y = _Y;
        }
    }
    //один график
    class GraphData
    {
        public string Name;//имя графика
        List<GraphPoint> Points= new List<GraphPoint>();//точки графика

        /// <summary>
        /// Парсит текстовый файл и возвращает список графиков.
        /// </summary>
        /// <param name="_File">Путь к файлу</param>
        /// <param name="_Format">Настройки формата парсинга</param>
        public static List<GraphData> ParseFile(string _File, FileFormat _Format)
        {
            var resultGraphs = new List<GraphData>();

            if (!File.Exists(_File))
                throw new FileNotFoundException($"Файл не найден: {_File}");

            // Временное хранилище для сырых данных перед распределением по графикам
            // Структура: [Индекс графика] -> Список точек
            var tempPoints = new List<List<GraphPoint>>();

            // Читаем все строки
            var lines = File.ReadAllLines(_File);

            for (int i = _Format.IgnoreRows; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Разбиваем строку на части
                string[] parts = line.Split(new string[] { _Format.Separate }, StringSplitOptions.RemoveEmptyEntries);

                // Пропускаем строки, где недостаточно данных хотя бы для одной точки
                if (parts.Length < 2) continue;

                // Подготавливаем массив чисел из строки с учетом замены разделителя дроби
                double[] values = new double[parts.Length];
                try
                {
                    for (int k = 0; k < parts.Length; k++)
                    {
                        // Нормализация числа: заменяем пользовательский разделитель точки на точку (стандарт invariant)
                        // Или используем конкретную культуру. Здесь простой заменой.
                        string cleanVal = parts[k].Trim().Replace(_Format.PointSimbol, ".");

                        if (double.TryParse(cleanVal, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                        {
                            values[k] = val;
                        }
                        else
                        {
                            // Если число не распарсилось, пропускаем всю строку (или можно обработать ошибку)
                            goto NextLine;
                        }
                    }
                }
                catch
                {
                    goto NextLine;
                }

                // Распределение данных по графикам в зависимости от режима MultiX
                if (!_Format.MultiX)
                {
                    // РЕЖIM 1: Общий X для всех
                    // Столбец 0 - это X. Столбцы 1..N - это Y для графиков 0..N-1

                    double commonX = values[0];

                    // Убеждаемся, что у нас достаточно списков для всех Y
                    int graphCount = values.Length - 1; // Количество графиков = кол-во столбцов минус 1 (X)
                    while (tempPoints.Count < graphCount)
                    {
                        tempPoints.Add(new List<GraphPoint>());
                    }

                    // Добавляем точки в каждый график
                    for (int g = 0; g < graphCount; g++)
                    {
                        // Проверяем, есть ли значение Y для этого графика в данной строке
                        if (g + 1 < values.Length)
                        {
                            tempPoints[g].Add(new GraphPoint(commonX, values[g + 1]));
                        }
                    }
                }
                else
                {
                    // РЕЖИМ 2: Индивидуальный X для каждого (Пары X,Y)
                    // Столбцы: [X1, Y1, X2, Y2, X3, Y3...]

                    int pairsCount = values.Length / 2; // Целочисленное деление, лишние столбцы игнорируются

                    while (tempPoints.Count < pairsCount)
                    {
                        tempPoints.Add(new List<GraphPoint>());
                    }

                    for (int g = 0; g < pairsCount; g++)
                    {
                        double xVal = values[g * 2];     // Четный индекс (0, 2, 4...)
                        double yVal = values[g * 2 + 1]; // Нечетный индекс (1, 3, 5...)

                        tempPoints[g].Add(new GraphPoint(xVal, yVal));
                    }
                }

            NextLine:;
            }

            // Преобразуем временные списки в объекты GraphData и присваиваем имена
            for (int i = 0; i < tempPoints.Count; i++)
            {
                if (tempPoints[i].Count == 0) continue;

                var graph = new GraphData
                {
                    Points = tempPoints[i],
                    // Берем имя из CustomMarkers, если есть, иначе генерируем
                    Name = (_Format.CustomMarkers != null && i < _Format.CustomMarkers.Count && !string.IsNullOrEmpty(_Format.CustomMarkers[i]))
                           ? _Format.CustomMarkers[i]
                           : $"График {i + 1}"
                };

                resultGraphs.Add(graph);
            }

            return resultGraphs;
        }
    }
}
