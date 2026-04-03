using LogsGraph.DataStorage;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogsGraph
{
    /// <summary>
    /// Логика взаимодействия для DataParser.xaml
    /// </summary>
    public partial class DataParser : UserControl
    {
        public List<FileFormat> Templates = new List<FileFormat>();
        private FileFormat _currentTemplate;//выбраный шаблон из списка
        public List<GraphData>? GraphsData = null;//графики загруженные из файла
        private CancellationTokenSource CTS_FilePArse;//отмена для загрузки файла
        public WorkSpace WorkSpace;//ссылка на текущий проект

        public DataParser()
        {
            InitializeComponent();

            // ИСПРАВЛЕНИЕ 1: Привязываем ListBox к списку программно.
            // Теперь ListBox будет следить за этим списком. 
            // Примечание: List<T> не уведомляет об удалении/добавлении автоматически так хорошо, как ObservableCollection,
            // но вызов Items.Refresh() или переназначение ItemsSource решает проблему отображения новых элементов.
            SelectTemplate.ItemsSource = Templates;
            GraphsList.ItemsSource = GraphsData;
            WorkSpace = ((App)Application.Current).WorkSpace;
            ProjGraphList.ItemsSource = WorkSpace.Graphs;

            // Добавим первый элемент для демонстрации
            AddNewTemplate();
        }
        //добавить новый шаблок
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNewTemplate();
        }
        //удалить шаблон
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (SelectTemplate.SelectedItem is FileFormat selected)
            {
                Templates.Remove(selected);

                // ИСПРАВЛЕНИЕ 2: Явно обновляем список в ListBox после удаления
                SelectTemplate.Items.Refresh();

                if (Templates.Count == 0)
                {
                    ClearInputs();
                    _currentTemplate = null;
                }
                else
                {
                    // Выбираем соседний элемент
                    SelectTemplate.SelectedIndex = Math.Max(0, SelectTemplate.Items.Count - 1);
                }
            }
        }
        //добавить новый шаблок
        private void AddNewTemplate()
        {
            var newFormat = new FileFormat
            {
                Name = $"Профиль {Templates.Count + 1}",
                Separate = ";",
                IgnoreRows = 0,
                MultiX = false
            };

            Templates.Add(newFormat);

            // ИСПРАВЛЕНИЕ 3: Явно обновляем список в ListBox после добавления
            SelectTemplate.Items.Refresh();

            // Сразу выбираем новый элемент, чтобы поля заполнились
            SelectTemplate.SelectedItem = newFormat;
        }
        //выбор шаблона в списке
        private void SelectTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectTemplate.SelectedItem is FileFormat format)
            {
                _currentTemplate = format;
                FillInputsFromObject(format);
            }
            else
            {
                _currentTemplate = null;
                ClearInputs();
            }
        }
        //вывести параметры выделенного шаблона на форму
        private void FillInputsFromObject(FileFormat f)
        {
            TxtName.Text = f.Name;
            TxtSeparator.Text = f.Separate;
            TxtIgnoreRows.Text = f.IgnoreRows.ToString();

            RbXSingle.IsChecked = !f.MultiX;
            RbXMulti.IsChecked = f.MultiX;

            TxDateFormat.Text = f.DateFormat;
        }
        //сброс полей шаблона к значению по умолчанию
        private void ClearInputs()
        {
            TxtName.Text = "";
            TxtSeparator.Text = "";
            TxtIgnoreRows.Text = "";
            RbXSingle.IsChecked = false;
            RbXMulti.IsChecked = false;
            TxDateFormat.Text = "";
        }

        private void Input_Changed(object sender, TextChangedEventArgs e)
        {
            if (_currentTemplate == null) return;

            if (sender == TxtName)
            {
                _currentTemplate.Name = TxtName.Text;
                // Так как мы используем простой List, Refresh() перерисует все элементы, подтянув новые имена
                SelectTemplate.Items.Refresh();
            }
            else if (sender == TxtSeparator)
            {
                _currentTemplate.Separate = TxtSeparator.Text;
            }
            else if (sender == TxtIgnoreRows)
            {
                if (int.TryParse(TxtIgnoreRows.Text, out int rows))
                {
                    _currentTemplate.IgnoreRows = rows;
                }
            }

            LoadData();
        }

        private void Radio_X_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentTemplate == null) return;
            _currentTemplate.MultiX = (RbXMulti.IsChecked == true);

            LoadData();
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            // диалог выбора файла с данными
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Настраиваем фильтр
            openFileDialog.Filter = "Все файлы (*.*)|*.*|Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv";

            // Устанавливаем фильтр по умолчанию на "Все файлы"
            openFileDialog.FilterIndex = 1;

            // Показываем диалог
            // Возвращает true, если пользователь нажал "ОК", и false, если "Отмена"
            bool? result = openFileDialog.ShowDialog();

            // Обрабатываем результат
            if (result == true)
            {
                // Полный путь к файлу (например: C:\Data\my_file.txt)
                string fullPath = openFileDialog.FileName;

                // Только имя файла (например: my_file.txt)
                string fileName = openFileDialog.SafeFileName;

                // Только расширение (например: .txt)
                string extension = System.IO.Path.GetExtension(fullPath);

                FilePath.Text = fullPath;

                //предпросмотр нескольких строк
                if (File.Exists(FilePath.Text))
                {
                    List<string> rows = File.ReadLines(FilePath.Text).Take(_currentTemplate.IgnoreRows + 3).ToList();
                    if (rows.Count >= 3)
                    {
                        TB_FilePrev.Text = "";
                        TB_FilePrev.Text += rows[0] + "\n";
                        TB_FilePrev.Text += rows[1] + "\n";
                        TB_FilePrev.Text += rows[2] + "\n";
                    }
                    else
                        TB_FilePrev.Text = "";
                }

                LoadData();
            }
            else
            {
                // Пользователь нажал "Отмена" или закрыл окно
                // Ничего не делаем или логируем отмену
                return;
            }
        }
        //выбор графика для предпросмотра
        private void GraphsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //отобращение графика для предпросмотра
            PlotModel PlotModel = new PlotModel { };

            //убрать отступы графика
            PlotModel.PlotMargins = new OxyThickness(0, 2, 20, 2);//размер подписи и числа на оси 
            PlotModel.Padding = new OxyThickness(0, 0, 10, 0);//размер разметки оси

            // Настройка осей (опционально, но улучшает внешний вид)
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right });

            // 2. Создаём серию данных
            var lineSeries = new LineSeries
            {
                Title = "Сигнал",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
            };

            // Добавляем начальные точки (для примера)
            GraphData? data = GraphsList.SelectedItem as GraphData;
            if (data != null)
            {
                //если точек много уменьшим толщинц линии, что бы не тормозило
                if (data.Points.Count > 10000)
                    lineSeries.StrokeThickness = 1;

                for (int i=0;i<data.Points.Count;i+=1)
                    lineSeries.Points.Add(new DataPoint(data.Points[i].X, data.Points[i].Y));
            }

            PlotModel.Series.Add(lineSeries);
            PrevPlot.Model = PlotModel;

            //отлючаем стандартное управление мышкой
            PrevPlot.ActualController.UnbindMouseWheel();
            PrevPlot.ActualController.UnbindMouseDown(OxyMouseButton.Right);
        }
        //чтение и парсинг файла
        private async void LoadData()
        {
            if (_currentTemplate == null)
                return;
            //если предыдущая загрузка не закончена, отменяем её
            if (CTS_FilePArse != null)
                CTS_FilePArse.Cancel();

            // Создаем новый источник отмены
            CTS_FilePArse = new CancellationTokenSource();

            try
            {
                // Блокируем интерфейс или показываем индикатор загрузки
                LoadStatus.Content = "Загрузка...";
                LoadStatus.Foreground = new SolidColorBrush(Colors.Yellow);



                // Вызов асинхронного метода с передачей токена
                GraphsData = await GraphData.ParseFileAsync(FilePath.Text, _currentTemplate, CTS_FilePArse.Token);
                GraphsList.ItemsSource = GraphsData;

                // Работа с результатом
                LoadStatus.Content = "Данные успешно загружены\n";
                for(int i=0;i< GraphsData.Count;i++)
                    LoadStatus.Content+= ""+ GraphsData[i].Name+"("+ GraphsData[i].Points.Count+") ";
                LoadStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                LoadStatus.Content = "Ошибка загрузки";
                LoadStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                try
                {
                    // Возвращаем интерфейс в исходное состояние
                    if (CTS_FilePArse != null)
                    {
                        CTS_FilePArse.Dispose();
                        CTS_FilePArse = null;
                    }
                }
                catch { }
            }
        }
        //добавить загружженый график в проект
        private void AddGraph_Click(object sender, RoutedEventArgs e)
        {
            if(GraphsList.SelectedItem is GraphData graph)
            {
                if (graph.Points.Count > 0)
                {
                    WorkSpace.Add(graph);
                    ProjGraphList.Items.Refresh();
                }
            }
        }
        //выбор графика в проекте
        private void ProjGraphList_Selected(object sender, RoutedEventArgs e)
        {
            //отобращение графика для предпросмотра
            PlotModel PlotModel = new PlotModel { };

            //убрать отступы графика
            PlotModel.PlotMargins = new OxyThickness(0, 2, 20, 2);//размер подписи и числа на оси 
            PlotModel.Padding = new OxyThickness(0, 0, 10, 0);//размер разметки оси

            // Настройка осей (опционально, но улучшает внешний вид)
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right });

            // 2. Создаём серию данных
            var lineSeries = new LineSeries
            {
                Title = "Сигнал",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
            };

            // Добавляем начальные точки (для примера)
            GraphData? data = ProjGraphList.SelectedItem as GraphData;
            if (data != null)
            {
                //если точек много уменьшим толщинц линии, что бы не тормозило
                if (data.Points.Count > 10000)
                    lineSeries.StrokeThickness = 1;

                for (int i = 0; i < data.Points.Count; i += 1)
                    lineSeries.Points.Add(new DataPoint(data.Points[i].X, data.Points[i].Y));
            }

            PlotModel.Series.Add(lineSeries);
            Prev2Plot.Model = PlotModel;

            //отлючаем стандартное управление мышкой
            Prev2Plot.ActualController.UnbindMouseWheel();
            Prev2Plot.ActualController.UnbindMouseDown(OxyMouseButton.Right);
        }
    }
}
