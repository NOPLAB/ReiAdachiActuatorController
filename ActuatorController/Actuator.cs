using ActuatorController.Settings;

namespace ActuatorController;

public enum ControllerBoard
{
    Rcb3,
    Rcb4,
}

public class Actuator
{ 
    public ControllerBoard ControllerBoard { get; }
    public byte ControllerId { get; }
    public int Position { get; set; }
    public byte Speed { get; set; }

    private Actuator(ControllerBoard controllerBoard, byte controllerId)
    {
        ControllerBoard = controllerBoard;
        ControllerId = controllerId;
    }

    public static Actuator FromSettings(JsonActuator actuatorSettings)
    {
        var board = ControllerBoardFromString(actuatorSettings.controller);
        return new Actuator(board, actuatorSettings.controllerId);
    }
    
    private static ControllerBoard ControllerBoardFromString(string str)
    {
        return str switch
        {
            "3" => ControllerBoard.Rcb3,
            "rcb3" => ControllerBoard.Rcb3,
            
            "4" => ControllerBoard.Rcb4,
            "rcb4" => ControllerBoard.Rcb4,
            
            _ => throw new Exception("Controller is not found!"),
        };
    }
}