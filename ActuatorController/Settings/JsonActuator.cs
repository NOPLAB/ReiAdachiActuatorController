namespace ActuatorController.Settings;

public class JsonActuator
{
    public int virtualId { get; set; }
    public string name { get; set; }
    public string controller { get; set; }
    public byte controllerId { get; set; }
    public int maxPosition { get; set; }
    public int minPosition { get; set; }
}