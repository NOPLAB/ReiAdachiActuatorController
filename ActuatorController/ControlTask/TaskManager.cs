using ActuatorController.Driver;
using ActuatorController.Settings;

namespace ActuatorController.ControlTask;

public class TaskManager
{
    private readonly SettingsWrapper _settings;
    private readonly ActuatorsStatus _binder;
    private readonly IDriver _driver;

    public TaskManager(SettingsWrapper settings, IDriver driver, ActuatorsStatus binder)
    {
        _settings = settings;
        _binder = binder;
        _driver = driver;
    }

    public Task CreateControlTask(IControlTask task, CancellationToken token)
    {
        return Task.Run(() =>
        {
            task.Start();

            while (true)
            {
                var status = task.Update();
                
                foreach (var pair in status.Actuators)
                {
                    _driver.Drive(pair.Value);
                }
                
                Thread.Sleep(100);
            }
        }, token);
    }
}