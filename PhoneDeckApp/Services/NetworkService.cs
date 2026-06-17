using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PhoneDeckApp.Services;

public static class NetworkService
{
    public static event Action? OnConnectionLost;
    private static bool _wasConnected = true;
    private static CancellationTokenSource? _cts;

    public static bool IsConnected()
    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", 2000);
            return reply.Status == IPStatus.Success;
        }
        catch { return false; }
    }

    public static void StartMonitoring()
    {
        _cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var connected = IsConnected();
                if (!connected && _wasConnected)
                {
                    _wasConnected = false;
                    OnConnectionLost?.Invoke();
                }
                else if (connected)
                {
                    _wasConnected = true;
                }
                await Task.Delay(5000, _cts.Token);
            }
        }, _cts.Token);
    }

    public static void StopMonitoring() => _cts?.Cancel();
}