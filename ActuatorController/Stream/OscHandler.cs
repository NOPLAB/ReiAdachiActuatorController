using BuildSoft.OscCore;

namespace ActuatorController.Stream;

public class OscHandler : IDisposable
{
    private readonly OscServer _oscServer;

    public OscHandler(int port)
    {
        _oscServer = OscServer.GetOrCreate(port);
    }

    public void AddReceiveMethod(string address, Action<OscMessageValues> func)
    {
        if (!_oscServer.TryAddMethod(address, func))
        {
            throw new Exception("Cannot add method!");
        }
    }

    public void Dispose()
    {
        _oscServer.Dispose();
    }
}