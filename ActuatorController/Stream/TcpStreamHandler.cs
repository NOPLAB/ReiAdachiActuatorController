using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ActuatorController.Stream;

public class TcpStreamHandler : IDisposable
{
    private readonly Socket _server;
    private readonly IPEndPoint _localEndPoint;

    public TcpStreamHandler(int port)
    {
        _localEndPoint = new IPEndPoint(IPAddress.Any, port);
        _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public Task Listen()
    {
        _server.Bind(_localEndPoint);
        _server.Listen(10);

        return Task.Run(() =>
        {
            while (true)
            {
                // 非同期ソケットを開始して、接続をリッスンする
                Console.WriteLine("Waiting for a connection...");
                var handler = _server.Accept();
                var bytes = new byte[1024];
                var bytesRec = handler.Receive(bytes);
                var data1 = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                Console.WriteLine(data1);
            }
        });
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}