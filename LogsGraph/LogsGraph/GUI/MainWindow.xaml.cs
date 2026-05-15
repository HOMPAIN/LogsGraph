using LogsGraph.DataStorage;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Храним все созданные PlotView для управления
        private readonly List<PlotView> _plotViews = new();
        private int _plotCounter = 0;
        public WorkSpace WorkSpace;//ссылка на текущий проект

        public MainWindow()
        {
            InitializeComponent();
            WorkSpace = ((App)Application.Current).WorkSpace;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Добавляем 2 графика при старте для демонстрации
            //AddPlot();
            //AddPlot();
            Graphycs.AddPlot();
            Graphycs.AddPlot();
        }

        // === ДОБАВЛЕНИЕ ГРАФИКА ===
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //AddPlot();
            Graphycs.AddPlot();
        }

    
        // === ДОПОЛНИТЕЛЬНО: метод для динамического обновления данных ===
        /// <summary>
        /// Добавляет новую точку в график по индексу. Удаляет старые точки если их > 100.
        /// </summary>
        public void AddDataPoint(int plotIndex, double x, double y)
        {
            if (plotIndex < 0 || plotIndex >= _plotViews.Count) return;

            var plotModel = _plotViews[plotIndex].Model;
            if (plotModel.Series.Count == 0) return;

            var series = (LineSeries)plotModel.Series[0];
            series.Points.Add(new DataPoint(x, y));

            // Ограничиваем 100 точками — удаляем самые старые
            while (series.Points.Count > 100)
                series.Points.RemoveAt(0);

            // Принудительно перерисовываем график
            plotModel.InvalidatePlot(true);
        }

        //кнопка загрузки проекта
        private void BtnProjLoad_Click(object sender, RoutedEventArgs e)
        {
            // диалог выбора файла с данными
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Настраиваем фильтр
            openFileDialog.Filter = "Файы проекта (*.lgp)|*.lgp";

            // Устанавливаем фильтр по умолчанию на "Все файлы"
            openFileDialog.FilterIndex = 1;

            //папка по умолчанию, берём текущую
            openFileDialog.DefaultDirectory = System.IO.Path.GetDirectoryName(WorkSpace.SavePath);

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

                WorkSpace.Load(fullPath);
            }
            else
            {
                // Пользователь нажал "Отмена" или закрыл окно
                // Ничего не делаем или логируем отмену
                return;
            }
        }
        //кнопка сохранения проекта
        private void BtnProjSave_Click(object sender, RoutedEventArgs e)
        {
            // диалог выбора файла с данными
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Настраиваем фильтр
            saveFileDialog.Filter = "Файы проекта (*.lgp)|*.lgp";

            // Устанавливаем фильтр по умолчанию на "Все файлы"
            saveFileDialog.FilterIndex = 1;

            //папка по умолчанию, берём текущую
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(WorkSpace.SavePath);

            // Показываем диалог
            // Возвращает true, если пользователь нажал "ОК", и false, если "Отмена"
            bool? result = saveFileDialog.ShowDialog();

            // Обрабатываем результат
            if (result == true)
            {
                // Полный путь к файлу (например: C:\Data\my_file.txt)
                string fullPath = saveFileDialog.FileName;

                // Только имя файла (например: my_file.txt)
                string fileName = saveFileDialog.SafeFileName;

                // Только расширение (например: .txt)
                string extension = System.IO.Path.GetExtension(fullPath);

                WorkSpace.Save(fullPath);
            }
            else
            {
                // Пользователь нажал "Отмена" или закрыл окно
                // Ничего не делаем или логируем отмену
                return;
            }
        }
    }
}