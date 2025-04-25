using System.Windows;
using System.Windows.Controls;
using WarehouseManagementUnia.ViewModels;

namespace WarehouseManagementUnia.Views
{
    public partial class LoginView : UserControl
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // Handle PasswordBox changes to update view model
            PasswordBox.PasswordChanged += (s, e) => _viewModel.Password = PasswordBox.Password;
        }
    }
}