namespace PhoneDeckApp.Models;

public class Session
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Charge { get; set; } = "";
    public string ConnectionTime { get; set; } = "";
    public string DisconnectionTime { get; set; } = "";
}