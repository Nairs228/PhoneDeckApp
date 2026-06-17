using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();

    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _patronymic = "";
    [ObservableProperty] private string _phoneModel = "";
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _currentPassword = "";
    [ObservableProperty] private string _newPassword = "";
    [ObservableProperty] private string _confirmNewPassword = "";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isSuccess = false;
    [ObservableProperty] private bool _IsError = false;

    public ProfileViewModel()
    {
        var user = SessionService.Instance.CurrentUser;
        if (user == null) return;
        LastName = user.LastName;
        FirstName = user.FirstName;
        Patronymic = user.Patronymic;
        PhoneModel = user.PhoneModel;
        Username = user.Username;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        IsSuccess = false;
        IsError = false;
        Message = "";

        var user = SessionService.Instance.CurrentUser;
        if (user == null) return;

        if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName) ||
            string.IsNullOrWhiteSpace(Patronymic) || string.IsNullOrWhiteSpace(PhoneModel) ||
            string.IsNullOrWhiteSpace(Username))
        {
            Message = "Заполните все поля профиля";
            IsError = true;
            return;
        }

        var ok = _db.UpdateProfile(user.Id, LastName, FirstName, Patronymic, PhoneModel, Username);
        if (ok)
        {
            user.LastName = LastName;
            user.FirstName = FirstName;
            user.Patronymic = Patronymic;
            user.PhoneModel = PhoneModel;
            user.Username = Username;
            IsSuccess = true;

            // Скрываем уведомление через 3 секунды
            await Task.Delay(3000);
            IsSuccess = false;
        }
        else
        {
            Message = "Пользователь с таким логином уже существует";
            IsError = true;
        }
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        IsSuccess = false;
        IsError = false;
        Message = "";

        var user = SessionService.Instance.CurrentUser;
        if (user == null) return;

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Message = "Заполните поля пароля";
            IsError = true;
            return;
        }
        if (NewPassword.Length < 6)
        {
            Message = "Новый пароль минимум 6 символов";
            IsError = true;
            return;
        }
        if (NewPassword != ConfirmNewPassword)
        {
            Message = "Пароли не совпадают";
            IsError = true;
            return;
        }

        var ok = _db.ChangePassword(user.Id, CurrentPassword, NewPassword);
        if (ok)
        {
            CurrentPassword = "";
            NewPassword = "";
            ConfirmNewPassword = "";
            IsSuccess = true;

            await Task.Delay(3000);
            IsSuccess = false;
        }
        else
        {
            Message = "Неверный текущий пароль";
            IsError = true;
        }
    }
}