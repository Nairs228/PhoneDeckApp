using System.Collections.Generic;
using PhoneDeckApp.Models;

namespace PhoneDeckApp.Services;

public class SessionService
{
    public static SessionService Instance { get; } = new();
    public User? CurrentUser { get; set; }
    public bool IsLoggedIn => CurrentUser != null;

    public void Logout()
    {
        CurrentUser = null;
    }
}