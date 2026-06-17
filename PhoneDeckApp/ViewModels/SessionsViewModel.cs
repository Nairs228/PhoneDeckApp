using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Models;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public partial class SessionsViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private List<Session> _allSessions = new();

    [ObservableProperty] private ObservableCollection<Session> _sessions = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    private const int PageSize = 12;

    public SessionsViewModel()
    {
        _allSessions = _db.GetSessions();
        TotalPages = (_allSessions.Count + PageSize - 1) / PageSize;
        if (TotalPages == 0) TotalPages = 1;
        LoadPage();
    }

    private void LoadPage()
    {
        Sessions = new ObservableCollection<Session>(
            _allSessions.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
    }

    [RelayCommand]
    private void PrevPage() { if (CurrentPage > 1) { CurrentPage--; LoadPage(); } }

    [RelayCommand]
    private void NextPage() { if (CurrentPage < TotalPages) { CurrentPage++; LoadPage(); } }
}