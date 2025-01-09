using csc8208Maui.Services;

namespace csc8208Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<EventStore>();
            MainPage = new AppShell();
        }
    }
}
