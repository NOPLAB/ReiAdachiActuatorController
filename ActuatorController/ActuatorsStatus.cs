using ActuatorController.Settings;

namespace ActuatorController;

public class ActuatorsStatus
{
    /// <summary>
    /// Key:VirtualId Value:PhysicalActuator
    /// </summary>
    public Dictionary<int, Actuator> Actuators { get; } = new ();
    
    public int ActuatorNum => Actuators.Count;

    public ActuatorsStatus(SettingsWrapper settings)
    {
        foreach (var actuatorSettings in settings.Json.actuators)
        {
            Actuators.Add(actuatorSettings.virtualId, Actuator.FromSettings(actuatorSettings));
        }
    }
}