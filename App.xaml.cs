using CheckingCamera.ViewModel;
using System.Windows;

namespace CheckingCamera
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            MainWindow win = new MainWindow();
            MainVM mainVM = new MainVM();
            win.DataContext = mainVM;
            win.Show();
        }
    }
}
