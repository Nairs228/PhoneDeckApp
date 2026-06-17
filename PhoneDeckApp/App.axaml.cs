using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PhoneDeckApp.Services;
using PhoneDeckApp.ViewModels;
using PhoneDeckApp.Views;

namespace PhoneDeckApp;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (!NetworkService.IsConnected())
            {
                ShowNoInternet(desktop);
                base.OnFrameworkInitializationCompleted();
                return;
            }

            StartApp(desktop);

            // Мониторинг интернета
            NetworkService.OnConnectionLost += () =>
            {
                Dispatcher.UIThread.Post(() => ShowNoInternet(desktop));
            };
            NetworkService.StartMonitoring();
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void ShowNoInternet(IClassicDesktopStyleApplicationLifetime desktop)
    {
        NetworkService.StopMonitoring();
        var noInternetWindow = new NoInternetWindow();
        noInternetWindow.Closed += (_, _) =>
        {
            desktop.MainWindow?.Close();
        };
        var oldWindow = desktop.MainWindow;
        desktop.MainWindow = noInternetWindow;
        noInternetWindow.Show();
        oldWindow?.Close();
    }

    private void StartApp(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var authWindow = new AuthWindow();
        desktop.MainWindow = authWindow;
        authWindow.Show();

        var savedId = PersistentSessionService.LoadUserId();
        if (savedId.HasValue)
        {
            var db = new DatabaseService();
            var user = db.GetUserById(savedId.Value);
            if (user != null)
            {
                SessionService.Instance.CurrentUser = user;
                ShowMain(desktop);
                return;
            }
        }

        ShowLogin(desktop);
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var vm = new LoginViewModel();
        vm.OnLoginSuccess += () => ShowMain(desktop);
        vm.OnGoToRegister += () => ShowRegister(desktop);
        if (desktop.MainWindow != null)
            desktop.MainWindow.Content = new LoginView { DataContext = vm };
    }

    private void ShowRegister(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var vm = new RegisterViewModel();
        vm.OnRegisterSuccess += () => ShowMain(desktop);
        vm.OnGoToLogin += () => ShowLogin(desktop);
        if (desktop.MainWindow != null)
            desktop.MainWindow.Content = new RegisterView { DataContext = vm };
    }

    private void ShowMain(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var vm = new MainWindowViewModel();
        vm.OnLogout += () =>
        {
            PersistentSessionService.Clear();
            SessionService.Instance.Logout();

            var authWindow = new AuthWindow();
            var loginVm = new LoginViewModel();
            loginVm.OnLoginSuccess += () => ShowMain(desktop);
            loginVm.OnGoToRegister += () => ShowRegister(desktop);
            authWindow.Content = new LoginView { DataContext = loginVm };

            var oldWindow = desktop.MainWindow;
            desktop.MainWindow = authWindow;
            authWindow.Show();
            oldWindow?.Close();
        };

        var mainWindow = new MainWindow { DataContext = vm };
        var old = desktop.MainWindow;   // сохраняем старое окно
        desktop.MainWindow = mainWindow; // сначала ставим новое
        mainWindow.Show();               // показываем
        old?.Close();                    // только потом закрываем старое
    }
}