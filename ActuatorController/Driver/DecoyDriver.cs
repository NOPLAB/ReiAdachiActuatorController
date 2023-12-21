namespace ActuatorController.Driver;

public class DecoyDriver: IDriver
{
    public void Drive(Actuator actuator)
    {
        // Do nothing
        Console.WriteLine($"[DRIVE] {actuator.ControllerId} {actuator.Position}, {actuator.Speed}");
    }
}