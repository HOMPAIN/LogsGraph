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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Добавляем 2 графика при старте для демонстрации
            AddPlot();
            AddPlot();
        }

        // === ДОБАВЛЕНИЕ ГРАФИКА ===
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddPlot();
        }

        private void AddPlot()
        {
            _plotCounter++;

            // 1. Создаём модель графика
            var plotModel = new PlotModel { Title = $"График #{_plotCounter}" };

            // Настройка осей (опционально, но улучшает внешний вид)
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            // 2. Создаём серию данных
            var lineSeries = new LineSeries
            {
                Title = "Сигнал",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Blue,
                MarkerFill = OxyColors.White
            };

            // Добавляем начальные точки (для примера)
            Random rnd = new();
            for (int i = 0; i < 30; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, rnd.NextDouble() * 100));
            }

            plotModel.Series.Add(lineSeries);

            // 3. Создаём PlotView и привязываем модель
            var plotView = new PlotView
            {
                Model = plotModel,
                Height = 220,          // Фиксированная высота
                Margin = new Thickness(0, 0, 0, 15), // Отступ снизу между графиками
                Background = Brushes.White
            };

            // 4. Оборачиваем в декоративную рамку
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8),
                Background = Brushes.WhiteSmoke,
                Child = plotView
            };

            // 5. Добавляем кнопку "Удалить этот график" поверх графика
            var deleteButton = new Button
            {
                Content = "✕ Удалить",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Padding = new Thickness(8, 3, 8, 3),
                Background = Brushes.IndianRed,
                Foreground = Brushes.White,
                //FontWeight = FontWeights.Bold,
                Tag = plotView  // Сохраняем ссылку на PlotView для удаления
            };
            deleteButton.Click += (s, _) =>
            {
                var pv = (PlotView)((Button)s).Tag;
                var parentBorder = (Border)pv.Parent;
                var grandParent = (UIElement)parentBorder.Parent;
                PlotsContainer.Children.Remove(grandParent);
                _plotViews.Remove(pv);
                UpdateCounter();
            };

            // 6. Компонуем элементы в панель
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"График #{_plotCounter}",
                //FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 10, 5),
                FontSize = 14
            });
            headerPanel.Children.Add(deleteButton);

            /*var contentPanel = new StackPanel();
            contentPanel.Children.Add(headerPanel);
            contentPanel.Children.Add(plotView);

            border.Child = contentPanel;*/

            // 7. Добавляем в контейнер и список
            PlotsContainer.Children.Add(border);
            _plotViews.Add(plotView);

            UpdateCounter();
        }

        // === УДАЛЕНИЕ ПОСЛЕДНЕГО ГРАФИКА ===
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plotViews.Count == 0)
            {
                MessageBox.Show("Нет графиков для удаления", "Инфо",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Берём последний добавленный PlotView
            var lastPlotView = _plotViews[^1];

            // Находим его "деда" (элемент в PlotsContainer)
            var border = (Border)lastPlotView.Parent;
            var wrapper = (UIElement)border.Parent; // StackPanel с заголовком

            // Удаляем из визуального дерева
            PlotsContainer.Children.Remove(wrapper);

            // Удаляем из списка
            _plotViews.RemoveAt(_plotViews.Count - 1);

            UpdateCounter();
        }

        // === ОЧИСТКА ВСЕХ ГРАФИКОВ ===
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plotViews.Count == 0) return;

            if (MessageBox.Show($"Удалить все {_plotViews.Count} графиков?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                PlotsContainer.Children.Clear();
                _plotViews.Clear();
                UpdateCounter();
            }
        }

        // === ОБНОВЛЕНИЕ СЧЁТЧИКА ===
        private void UpdateCounter()
        {
            CounterText.Text = $"Графиков: {_plotViews.Count}";
            RemoveButton.IsEnabled = _plotViews.Count > 0;
            ClearButton.IsEnabled = _plotViews.Count > 0;
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
    }
}