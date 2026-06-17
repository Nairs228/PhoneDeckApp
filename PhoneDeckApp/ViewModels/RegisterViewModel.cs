using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Services;
using System;

namespace PhoneDeckApp.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _patronymic = "";
    [ObservableProperty] private string _phoneModel = "";
    [ObservableProperty] private string _errorMessage = "";

    public event Action? OnRegisterSuccess;
    public event Action? OnGoToLogin;

    [RelayCommand]
    private void Register()
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName) ||
            string.IsNullOrWhiteSpace(Patronymic) || string.IsNullOrWhiteSpace(PhoneModel))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Пароль минимум 6 символов";
            return;
        }

        var success = _db.Register(Username, Password, LastName, FirstName, Patronymic, PhoneModel);
        if (!success)
        {
            ErrorMessage = "Пользователь с таким именем уже существует";
            return;
        }

        var user = _db.Login(Username, Password);
        SessionService.Instance.CurrentUser = user;
        OnRegisterSuccess?.Invoke();
    }

    [RelayCommand]
    private void GoToLogin() => OnGoToLogin?.Invoke();
}