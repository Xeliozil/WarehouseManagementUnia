using System.Windows;
using WarehouseManagementUnia.Views;

namespace WarehouseManagementUnia
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
        }
    }
}