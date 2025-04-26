using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

public class WebSocketService
{
    private ClientWebSocket _socket;
    private CancellationTokenSource _cts;

    public bool IsConnected => _socket?.State == WebSocketState.Open;

    public async Task ConnectAsync(string uri)
    {
        _socket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        await _socket.ConnectAsync(new Uri(uri), _cts.Token);
    }

    public async Task SendAsync(string message)
    {
        if (_socket?.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        }
    }

    public async Task<string> ReceiveAsync()
    {
        var buffer = new byte[4096];
        var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    public async Task DisconnectAsync()
    {
        if (_socket != null)
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", _cts.Token);
            _socket.Dispose();
            _cts.Cancel();
        }
    }
}
