using System.ComponentModel;
using System.Windows.Media;

namespace WarehouseManagementUnia
{
    public class SettingsHelper : INotifyPropertyChanged
    {
        private static SettingsHelper _instance;
        private string _username;
        private SolidColorBrush _fontColorBrush;
        private double _fontSize;

        public static SettingsHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsHelper();
                return _instance;
            }
        }

        private SettingsHelper()
        {
            // Nie inicjalizujemy ustawień w konstruktorze, bo _username może być jeszcze null
            _fontColorBrush = new SolidColorBrush(Colors.Black); // Domyślny kolor
            _fontSize = 14; // Domyślny rozmiar
        }

        public void SetUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return; // Nie aktualizuj, jeśli username jest pusty

            _username = username;
            UpdateSettings();
        }

        public void UpdateSettings()
        {
            if (string.IsNullOrEmpty(_username))
            {
                FontColorBrush = new SolidColorBrush(Colors.Black);
                FontSize = 14;
                return;
            }

            try
            {
                string fontColorKey = $"FontColor_{_username}";
                string fontColor = Properties.Settings.Default[fontColorKey]?.ToString() ?? "Black";
                FontColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor));

                string fontSizeKey = $"FontSize_{_username}";
                FontSize = Properties.Settings.Default[fontSizeKey] != null ? (double)Properties.Settings.Default[fontSizeKey] : 14;
            }
            catch (System.Configuration.SettingsPropertyNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ustawień: {ex.Message}");
                FontColorBrush = new SolidColorBrush(Colors.Black);
                FontSize = 14;
            }
        }

        public SolidColorBrush FontColorBrush
        {
            get => _fontColorBrush;
            set
            {
                _fontColorBrush = value;
                NotifyPropertyChanged(nameof(FontColorBrush));
            }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                NotifyPropertyChanged(nameof(FontSize));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}