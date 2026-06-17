using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Services;
using System;

namespace PhoneDeckApp.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _rememberMe = false;

    public event Action? OnLoginSuccess;
    public event Action? OnGoToRegister;

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        var user = _db.Login(Username, Password);
        if (user == null)
        {
            ErrorMessage = "Неверный логин или пароль";
            return;
        }

        SessionService.Instance.CurrentUser = user;
        if (RememberMe) PersistentSessionService.Save(user);
        else PersistentSessionService.Clear();

        OnLoginSuccess?.Invoke();
    }

    [RelayCommand]
    private void GoToRegister() => OnGoToRegister?.Invoke();
}