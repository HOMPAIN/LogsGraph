using LogsGraph.DataStorage;
using System;
using System.Collections.Generic;
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
        private FileFormat _currentTemplate;

        public DataParser()
        {
            InitializeComponent();

            // ИСПРАВЛЕНИЕ 1: Привязываем ListBox к списку программно.
            // Теперь ListBox будет следить за этим списком. 
            // Примечание: List<T> не уведомляет об удалении/добавлении автоматически так хорошо, как ObservableCollection,
            // но вызов Items.Refresh() или переназначение ItemsSource решает проблему отображения новых элементов.
            SelectTemplate.ItemsSource = Templates;

            // Добавим первый элемент для демонстрации
            AddNewTemplate();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNewTemplate();
        }

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

        private void AddNewTemplate()
        {
            var newFormat = new FileFormat
            {
                Name = $"Профиль {Templates.Count + 1}",
                Separate = ";",
                PointSimbol = ".",
                IgnoreRows = 0,
                MultiX = false
            };

            Templates.Add(newFormat);

            // ИСПРАВЛЕНИЕ 3: Явно обновляем список в ListBox после добавления
            SelectTemplate.Items.Refresh();

            // Сразу выбираем новый элемент, чтобы поля заполнились
            SelectTemplate.SelectedItem = newFormat;
        }

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

        private void FillInputsFromObject(FileFormat f)
        {
            TxtName.Text = f.Name;
            TxtSeparator.Text = f.Separate;
            TxtIgnoreRows.Text = f.IgnoreRows.ToString();

            RbPointComma.IsChecked = (f.PointSimbol == ",");
            RbPointDot.IsChecked = (f.PointSimbol == ".");

            RbXSingle.IsChecked = !f.MultiX;
            RbXMulti.IsChecked = f.MultiX;
        }

        private void ClearInputs()
        {
            TxtName.Text = "";
            TxtSeparator.Text = "";
            TxtIgnoreRows.Text = "";
            RbPointComma.IsChecked = false;
            RbPointDot.IsChecked = false;
            RbXSingle.IsChecked = false;
            RbXMulti.IsChecked = false;
        }

        private void Input_Changed(object sender, TextChangedEventArgs e)
        {
            if (_currentTemplate == null) return;

            if (sender == TxtName)
            {
                _currentTemplate.Name = TxtName.Text;
                // ИСПРАВЛЕНИЕ 4: Обновляем элемент в списке, чтобы изменилось имя в ListBox
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
        }

        private void Radio_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentTemplate == null) return;
            _currentTemplate.PointSimbol = (RbPointComma.IsChecked == true) ? "," : ".";
        }

        private void Radio_X_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentTemplate == null) return;
            _currentTemplate.MultiX = (RbXMulti.IsChecked == true);
        }
    }
}
