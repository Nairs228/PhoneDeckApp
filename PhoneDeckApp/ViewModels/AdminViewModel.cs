using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Models;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();

    [ObservableProperty] private ObservableCollection<User> _users = new();
    [ObservableProperty] private int _userDbCount = 0;
    [ObservableProperty] private int _deviceDbCount = 0;
    [ObservableProperty] private string _userDbPath = "";
    [ObservableProperty] private string _deviceDbPath = "";

    // Редактирование
    [ObservableProperty] private User? _editingUser = null;
    [ObservableProperty] private string _editLastName = "";
    [ObservableProperty] private string _editFirstName = "";
    [ObservableProperty] private string _editPatronymic = "";
    [ObservableProperty] private string _editPhoneModel = "";
    [ObservableProperty] private string _editUsername = "";
    [ObservableProperty] private bool _editIsAdmin = false;
    [ObservableProperty] private string _editMessage = "";
    [ObservableProperty] private bool _isEditing = false;

    public static IValueConverter RoleConverter = new FuncValueConverter<bool, string>(
        v => v ? "Админ" : "Пользователь");
    public static IValueConverter RoleColorConverter = new FuncValueConverter<bool, IBrush>(
        v => v ? Brushes.LightGreen : new SolidColorBrush(Color.Parse("#8892b0")));
    public static IValueConverter ActionConverter = new FuncValueConverter<bool, string>(
        v => v ? "Снять права" : "Сделать админом");

    public AdminViewModel() => Load();

    private void Load()
    {
        Users = new ObservableCollection<User>(_db.GetAllUsers());
        var health = _db.GetDbHealth();
        UserDbCount = health.users;
        DeviceDbCount = health.devices;
        UserDbPath = health.usersPath;
        DeviceDbPath = health.devicesPath;
    }

    [RelayCommand]
    private void ToggleAdmin(User user)
    {
        if (user.Username == "admin") return;
        _db.SetAdminRole(user.Id, !user.IsAdmin);
        Load();
    }

    [RelayCommand]
    private void EditUser(User user)
    {
        EditingUser = user;
        EditLastName = user.LastName;
        EditFirstName = user.FirstName;
        EditPatronymic = user.Patronymic;
        EditPhoneModel = user.PhoneModel;
        EditUsername = user.Username;
        EditIsAdmin = user.IsAdmin;
        EditMessage = "";
        IsEditing = true;
    }

    [RelayCommand]
    private void SaveEdit()
    {
        if (EditingUser == null) return;
        var ok = _db.UpdateUserByAdmin(EditingUser.Id, EditLastName, EditFirstName,
            EditPatronymic, EditPhoneModel, EditUsername, EditIsAdmin);
        if (ok)
        {
            EditMessage = "Сохранено";
            IsEditing = false;
            Load();
        }
        else EditMessage = "Ошибка при сохранении";
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingUser = null;
        EditMessage = "";
    }
}