using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneDeckApp.Models;
using PhoneDeckApp.Services;

namespace PhoneDeckApp.ViewModels;

public class BonusOption
{
    public int Cost { get; set; }
    public string Description { get; set; } = "";
}

public partial class BonusesViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private readonly int _userId;

    [ObservableProperty] private int _userBonuses;
    [ObservableProperty] private bool _showHistory = false;
    [ObservableProperty] private BonusOption? _selectedOption;
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private ObservableCollection<BonusHistoryItem> _history = new();

    public ObservableCollection<BonusOption> BonusOptions { get; } = new()
    {
        new() { Cost = 30,  Description = "Отмена домашней работы за один предмет на один день" },
        new() { Cost = 40,  Description = "Подсказка от учителя на контрольной/самостоятельной" },
        new() { Cost = 50,  Description = "Поменять вариант на контрольной" },
        new() { Cost = 60,  Description = "+1 балл за контрольную" },
        new() { Cost = 100, Description = "Отмена выхода к доске" },
        new() { Cost = 125, Description = "Бесплатная пицца в столовой" },
        new() { Cost = 300, Description = "Повышение оценки в четверти на 1 балл за один предмет" },
    };

    public BonusesViewModel()
    {
        _userId = SessionService.Instance.CurrentUser?.Id ?? 0;
        UserBonuses = _db.GetBonusBalance(_userId);
        History = new ObservableCollection<BonusHistoryItem>(_db.GetBonusHistory(_userId));
    }

    [RelayCommand]
    private void SpendBonuses()
    {
        if (SelectedOption == null) return;
        if (UserBonuses < SelectedOption.Cost)
        {
            Message = "Недостаточно бонусов";
            return;
        }

        UserBonuses -= SelectedOption.Cost;
        _db.SetBonusBalance(_userId, UserBonuses);
        _db.AddBonusHistory(_userId, SelectedOption.Cost, SelectedOption.Description);
        History = new ObservableCollection<BonusHistoryItem>(_db.GetBonusHistory(_userId));
        Message = $"Потрачено {SelectedOption.Cost} бонусов!";
        SelectedOption = null;
    }

    [RelayCommand] private void ShowBonuses() { ShowHistory = false; Message = ""; }
    [RelayCommand] private void ShowHistoryTab() { ShowHistory = true; Message = ""; }
}