using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RiskServerConnection
{
    public interface IGameWebSocketService : IDisposable
    {
        Task ConnectAsync(Uri uri);
        Task SendAsync(string message);
        Task<string> ReceiveAsync();
        WebSocketState State { get; }
        Task DisconnectAsync();
    }

    public class GameWebSocketService : IGameWebSocketService
    {
        private ClientWebSocket _socket;

        public WebSocketState State => _socket?.State ?? WebSocketState.None;

        public async Task ConnectAsync(Uri uri)
        {
            _socket = new ClientWebSocket();
            await _socket.ConnectAsync(uri, CancellationToken.None);
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<string> ReceiveAsync()
        {
            var buffer = new byte[4 * 1024];
            var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public async Task DisconnectAsync()
        {
            if (_socket?.State == WebSocketState.Open)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
        }

        public void Dispose() => _socket?.Dispose();
    }
}
