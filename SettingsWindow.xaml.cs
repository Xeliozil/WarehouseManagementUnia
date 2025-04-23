using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WarehouseManagementUnia
{
    public partial class SettingsWindow : Window
    {
        private readonly string _username;

        public SettingsWindow(string username)
        {
            InitializeComponent();
            _username = username;
            LoadSettings();
        }

        private void LoadSettings()
        {
            string backgroundColor = Properties.Settings.Default[$"BackgroundColor_{_username}"].ToString() ?? "White";
            string fontColor = Properties.Settings.Default[$"FontColor_{_username}"].ToString() ?? "Black";
            double fontSize = Properties.Settings.Default[$"FontSize_{_username}"] != null ? (double)Properties.Settings.Default[$"FontSize_{_username}"] : 14;

            foreach (ComboBoxItem item in BackgroundColorComboBox.Items)
            {
                if (item.Tag.ToString() == backgroundColor)
                {
                    BackgroundColorComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in FontColorComboBox.Items)
            {
                if (item.Tag.ToString() == fontColor)
                {
                    FontColorComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in FontSizeComboBox.Items)
            {
                if (item.Tag.ToString() == fontSize.ToString())
                {
                    FontSizeComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var backgroundColorItem = (ComboBoxItem)BackgroundColorComboBox.SelectedItem;
            var fontColorItem = (ComboBoxItem)FontColorComboBox.SelectedItem;
            var fontSizeItem = (ComboBoxItem)FontSizeComboBox.SelectedItem;

            if (backgroundColorItem != null && fontColorItem != null && fontSizeItem != null)
            {
                Properties.Settings.Default[$"BackgroundColor_{_username}"] = backgroundColorItem.Tag.ToString();
                Properties.Settings.Default[$"FontColor_{_username}"] = fontColorItem.Tag.ToString();
                Properties.Settings.Default[$"FontSize_{_username}"] = double.Parse(fontSizeItem.Tag.ToString());
                Properties.Settings.Default.Save();

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ApplySettings();
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Proszę wybrać wszystkie opcje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}