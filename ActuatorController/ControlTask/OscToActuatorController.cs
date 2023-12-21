using ActuatorController.Stream;

namespace ActuatorController.ControlTask;

public class OscToActuatorController: IControlTask
{
    ActuatorsStatus _binder;
    private readonly OscHandler _oscHandler;

    public OscToActuatorController(OscHandler oscHandler, ActuatorsStatus binder)
    {
        _oscHandler = oscHandler;
        _binder = binder;
    }

    public void Start()
    {
        int loop;
        lock (_binder)
        {
             loop = _binder.ActuatorNum;
        }
        
        for (var i = 0; i < loop; i++)
        {
            // iだとアロー関数の外(for文)で値が変わってしまうので、ローカル変数に格納
            var id = i;

            // Oscメッセージを受信したときに実行する処理を登録

            // /[id]-position
            _oscHandler.AddReceiveMethod($"/{id}-position", values =>
            {
                var position = values.ReadIntElement(0);
                lock (_binder)
                {
                    // すでに追加されいるかで処理を分岐
                    if (_binder.Actuators.TryGetValue(id, out var actuator))
                    {
                        actuator.Position = position;
                    }
                }
            });

            // /[id]-speed
            _oscHandler.AddReceiveMethod($"/{id}-speed", values =>
            {
                var speed = (byte)values.ReadIntElement(0);
                lock (_binder)
                {
                    // すでに追加されいるかで処理を分岐
                    if (_binder.Actuators.TryGetValue(id, out var actuator))
                    {
                        actuator.Speed = speed;
                    }
                }
            });
        }
    }

    public ActuatorsStatus Update()
    {
        lock (_binder)
        {
            return _binder;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}