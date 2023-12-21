// TODO Rcb3とRcb4をRcbControllerで抽象化して扱えるようにしたい。
// TODO IDriverインターフェースを作成してモーターのドライブを抽象化したい

using ActuatorController.KondoRcbLib;

namespace ActuatorController.Driver;

public class Rcb3Rcb4Driver: IDriver
{
    private readonly Rcb3Controller _rcb3;
    private readonly Rcb4Controller _rcb4;

    private Rcb3Rcb4Driver(string portName3, string portName4)
    {
        _rcb3 = new Rcb3Controller(portName3, Console.WriteLine);
        _rcb4 = new Rcb4Controller(portName4, Console.WriteLine);
    }

    public static Rcb3Rcb4Driver Connect3And4(string portName3, string portName4)
    {
        var driveActuator = new Rcb3Rcb4Driver(portName3, portName4);
        return driveActuator;
    }

    public void Drive(Actuator actuator)
    {
        switch (actuator.ControllerBoard)
        {
            case ControllerBoard.Rcb3:
                var rcb3Position = ConvertVirtualPosition2Rcb3Position(actuator.Position);
                _rcb3.MoveSingleServo(
                    actuator.ControllerId,
                    actuator.Speed,
                    rcb3Position
                );
                break;
            case ControllerBoard.Rcb4:
                var rcb4Position = ConvertVirtualPosition2Rcb4Position(actuator.Position);
                _rcb4.MoveSingleServo(actuator.ControllerId, actuator.Speed, rcb4Position);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static ushort ConvertVirtualPosition2Rcb3Position(int position)
    {
        throw new NotImplementedException();
        return (ushort)position;
    }

    private static ushort ConvertVirtualPosition2Rcb4Position(int position)
    {
        throw new NotImplementedException();
        return (ushort)position;
    }
}