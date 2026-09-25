using csc8208Maui.Services;
using csc8208Maui.Views;

namespace csc8208Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<EventStore>();
            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
