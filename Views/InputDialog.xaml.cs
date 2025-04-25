using System.Windows;

namespace WarehouseManagementUnia.Views
{
    public partial class InputDialog : Window
    {
        public string Prompt { get; }
        public string InputText { get; set; }

        public InputDialog(string prompt)
        {
            InitializeComponent();
            Prompt = prompt;
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Input is required.");
                return;
            }
            DialogResult = true;
        }
    }
}