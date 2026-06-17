using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Models;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private List<Session> _allSessions = new();

    [ObservableProperty] private string _totalSessions = "0";
    [ObservableProperty] private string _totalUsers = "0";
    [ObservableProperty] private ObservableCollection<Session> _recentSessions = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    private const int PageSize = 8;

    public DashboardViewModel()
    {
        _allSessions = _db.GetSessions();
        TotalSessions = _allSessions.Count.ToString();
        TotalUsers = _db.GetAllUsers().Count.ToString();
        TotalPages = (_allSessions.Count + PageSize - 1) / PageSize;
        if (TotalPages == 0) TotalPages = 1;
        LoadPage();
    }

    private void LoadPage()
    {
        var page = _allSessions
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        RecentSessions = new ObservableCollection<Session>(page);
    }

    [RelayCommand]
    private void PrevPage()
    {
        if (CurrentPage > 1) { CurrentPage--; LoadPage(); }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages) { CurrentPage++; LoadPage(); }
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        CurrentPage = page;
        LoadPage();
    }
}