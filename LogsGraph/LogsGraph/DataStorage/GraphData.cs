using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogsGraph.DataStorage
{
    //точка графика
    public struct GraphPoint
    {
        public long X { get; set; }=0; 
        public double Y { get; set; }=0;

        public GraphPoint()
        {
            X = 0;
            Y = 0;
        }
        public GraphPoint(long _X, double _Y)
        {
            X = _X;
            Y = _Y;
        }
    }
    //один график
    [JsonConverter(typeof(GraphDataBinaryConverter))]
    public class GraphData
    {
        public string Name { get; set; }="";//имя графика
        public List<GraphPoint> Points { get; set; } = new List<GraphPoint>();//точки графика

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
                string normalizedX = cleanX.Replace(",", ".");
                if (double.TryParse(normalizedX, NumberStyles.Any, culture, out double numX))
                {
                    xValue = (long)numX;
                    xParsed = true;
                }
            }

            // Если X не распарсился, точку добавить нельзя
            if (!xParsed) return;

            // 3. Парсинг Y (Только число)
            string cleanY = _Y.Trim().Replace(",", ".");
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
        /// <summary>
        /// Асинхронно парсит текстовый файл с возможностью отмены.
        /// </summary>
        public static async Task<List<GraphData>> ParseFileAsync(string _File, FileFormat _Format, CancellationToken cancellationToken = default)
        {
            // Проверка существования файла (синхронная, быстрая операция)
            if (!File.Exists(_File))
                throw new FileNotFoundException($"Файл не найден: {_File}");

            // Проверка отмены в самом начале
            cancellationToken.ThrowIfCancellationRequested();

            var resultGraphs = new List<GraphData>();
            int graphs_count = 0;

            // Асинхронное чтение всего файла в память
            // Для очень огромных файлов (ГБ) лучше использовать построчное чтение через StreamReader, 
            // но ReadAllLinesAsync проще для реализации логики с заголовками и пропусками.
            string[] lines = await File.ReadAllLinesAsync(_File, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            CultureInfo culture = CultureInfo.InvariantCulture;

            // Удаление лишних пробелов (можно распараллелить, но для простоты оставим цикл)
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // Чтение заголовка
            if (_Format.CustomMarkers.Count == 0 && lines.Length > _Format.IgnoreRows)
            {
                string headerLine = lines[_Format.IgnoreRows];
                if (!string.IsNullOrEmpty(headerLine))
                {
                    List<string> names = new List<string>(headerLine.Split(new string[] { _Format.Separate }, StringSplitOptions.RemoveEmptyEntries));

                    if (_Format.MultiX)
                    {
                        graphs_count = names.Count / 2;
                        if (graphs_count * 2 != names.Count)
                            throw new FormatException("Неправильное число столбцов в заголовке (должно быть чётным для MultiX).");

                        for (int i = 0; i < graphs_count; i++)
                            resultGraphs.Add(new GraphData(names[i * 2 + 1]));
                    }
                    else
                    {
                        graphs_count = names.Count - 1;
                        for (int i = 0; i < graphs_count; i++)
                            resultGraphs.Add(new GraphData(names[i + 1]));
                    }
                }
            }
            else if (_Format.CustomMarkers.Count > 0)
            {
                graphs_count = _Format.CustomMarkers.Count;
                foreach (var marker in _Format.CustomMarkers)
                {
                    resultGraphs.Add(new GraphData(marker));
                }
            }

            if (graphs_count == 0) return resultGraphs;

            // Чтение данных
            int startRowIndex = _Format.IgnoreRows + 1;

            for (int i = startRowIndex; i < lines.Length; i++)
            {
                // Периодическая проверка отмены (каждые 100 строк), чтобы не тормозить цикл
                if (i % 100 == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(new string[] { _Format.Separate }, StringSplitOptions.None);

                if (_Format.MultiX)
                {
                    for (int j = 0; j < graphs_count; j++)
                    {
                        int xIndex = j * 2;
                        int yIndex = j * 2 + 1;
                        if (yIndex < values.Length)
                        {
                            resultGraphs[j].Add(values[xIndex], values[yIndex], _Format);
                        }
                    }
                }
                else
                {
                    string commonX = (values.Length > 0) ? values[0] : "";
                    for (int j = 0; j < graphs_count; j++)
                    {
                        int yIndex = j + 1;
                        if (yIndex < values.Length)
                        {
                            resultGraphs[j].Add(commonX, values[yIndex], _Format);
                        }
                    }
                }
            }

            // Финальная проверка перед возвратом
            cancellationToken.ThrowIfCancellationRequested();

            // Удаляем пустые графики
            //resultGraphs.RemoveAll(g => g.Points.Count == 0);

            return resultGraphs;
        }
    }

    //---------------------------------------------------------------------------------
    // json Конвертер для класса GraphData
    public class GraphDataBinaryConverter : JsonConverter<GraphData>
    {
        public override GraphData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Ожидаем, что нам придет объект с полями Name и Data (Base64)
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            string name = null;
            byte[] dataBytes = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read(); // Читаем значение

                    switch (propertyName)
                    {
                        case "Name":
                            name = reader.GetString();
                            break;
                        case "PointsBase64": // Имя поля, в котором лежит Base64
                            string base64 = reader.GetString();
                            if (!string.IsNullOrEmpty(base64))
                                dataBytes = Convert.FromBase64String(base64);
                            break;
                    }
                }
            }

            var graph = new GraphData { Name = name ?? "" };

            if (dataBytes != null && dataBytes.Length > 0)
            {
                // Декодируем байты обратно в точки
                // Структура GraphPoint: long (8 байт) + double (8 байт) = 16 байт
                int pointSize = 16;
                if (dataBytes.Length % pointSize == 0)
                {
                    int count = dataBytes.Length / pointSize;
                    graph.Points = new List<GraphPoint>(count);

                    for (int i = 0; i < count; i++)
                    {
                        int offset = i * pointSize;
                        long x = BinaryPrimitives.ReadInt64LittleEndian(dataBytes.AsSpan(offset));
                        double y = BinaryPrimitives.ReadDoubleLittleEndian(dataBytes.AsSpan(offset + 8));
                        graph.Points.Add(new GraphPoint(x, y));
                    }
                }
            }
            else
            {
                graph.Points = new List<GraphPoint>();
            }

            return graph;
        }

        public override void Write(Utf8JsonWriter writer, GraphData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Сохраняем имя обычно
            writer.WriteString("Name", value.Name);

            // Конвертируем точки в бинарный вид
            if (value.Points != null && value.Points.Count > 0)
            {
                int pointSize = 16; // 8 байт long + 8 байт double
                byte[] buffer = new byte[value.Points.Count * pointSize];

                for (int i = 0; i < value.Points.Count; i++)
                {
                    int offset = i * pointSize;
                    BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(offset), value.Points[i].X);
                    BinaryPrimitives.WriteDoubleLittleEndian(buffer.AsSpan(offset + 8), value.Points[i].Y);
                }

                // Кодируем в Base64 и пишем в JSON
                string base64 = Convert.ToBase64String(buffer);
                writer.WriteString("PointsBase64", base64);
            }
            else
            {
                writer.WriteString("PointsBase64", "");
            }

            writer.WriteEndObject();
        }
    }
}
