//описание формата для парсинга файла
using System;
using System.Collections.Generic;
using System.Text;

namespace LogsGraph.DataStorage
{
    //формат текстового файла для парсинга
    public class FileFormat : ConfigBase<FileFormat>
    {
        public string Name { get; set; }//имя для данного формата
        public int IgnoreRows { get; set; } = 0;//сколько первых строк в файле игнорировать
        public int[] IgnoreColumns { get; set; } = new int[]{ };//номера колонок, которые игнорировать
        public bool MultiX { get; set; } = false; //для каждого графика свой столбец для X
        public int XColumn { get; set; } = 0;//номер столбца X если он один общий для всех
        public string Separate { get; set; } = ";"; //разделитель значений
        public string DateFormat { get; set; } = "dd.MM.yyyy HH:mm:ss.fff";//если страка указанна (например "dd.MM.yyyy HH:mm"), то по Х парсится дата в нужном формате
        public List<string> CustomMarkers { get; set; } = new List<string>(); //маркеры столбцов на случай если они не описаны в файле

        public override string ToString()
        {
            return Name ?? "Без имени";
        }
    }
}
