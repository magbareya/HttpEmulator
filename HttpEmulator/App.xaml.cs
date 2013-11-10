using System.Windows;
using System.Windows.Input;

namespace HttpEmulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var window = new MainWindow();
            var vm = new HttpEmulatorViewModel();
            try
            {
                base.OnStartup(e);
                window.Closing += (sender, args) => vm.OnStopListener();

                window.DataContext = vm;
                window.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(string.Format("unexpected error happened: {0}", ex.Message));
                window.Close();
            }
            finally
            {
                vm.OnStopListener();
            }
        }
    }
}