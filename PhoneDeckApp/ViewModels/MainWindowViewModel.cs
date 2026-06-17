using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private ViewModelBase _currentPage;
    [ObservableProperty] private string _activePage = "dashboard";

    public bool IsDashboard => ActivePage == "dashboard";
    public bool IsSessions => ActivePage == "sessions";
    public bool IsProfile => ActivePage == "profile";
    public bool IsBonuses => ActivePage == "bonuses";
    public bool IsAdmin => ActivePage == "admin";
    public bool IsAdminUser => SessionService.Instance.CurrentUser?.IsAdmin == true;

    public event Action? OnLogout;

    public MainWindowViewModel()
    {
        var user = SessionService.Instance.CurrentUser;
        Username = user != null ? $"{user.LastName} {user.FirstName}" : "Гость";
        _currentPage = new DashboardViewModel();
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        ActivePage = page;
        CurrentPage = page switch
        {
            "dashboard" => new DashboardViewModel(),
            "sessions" => new SessionsViewModel(),
            "profile" => new ProfileViewModel(),
            "bonuses" => new BonusesViewModel(),
            "admin" => new AdminViewModel(),
            _ => new DashboardViewModel()
        };
        OnPropertyChanged(nameof(IsDashboard));
        OnPropertyChanged(nameof(IsSessions));
        OnPropertyChanged(nameof(IsProfile));
        OnPropertyChanged(nameof(IsBonuses));
        OnPropertyChanged(nameof(IsAdmin));
    }

    [RelayCommand]
    private void Logout()
    {
        SessionService.Instance.Logout();
        OnLogout?.Invoke();
    }
}