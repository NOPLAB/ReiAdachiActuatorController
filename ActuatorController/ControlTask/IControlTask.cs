namespace ActuatorController.ControlTask;

public interface IControlTask: IDisposable
{
    public void Start();
    public ActuatorsStatus Update();
}