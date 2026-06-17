namespace PhoneDeckApp.Models;

public class BonusHistoryItem
{
    public int Code { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = "";
    public string Date { get; set; } = "";
}