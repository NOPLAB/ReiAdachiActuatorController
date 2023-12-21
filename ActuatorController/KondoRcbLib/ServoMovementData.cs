namespace ActuatorController.KondoRcbLib
{
    public record ServoMovementData
    {
        public byte Num { get; set; }
        public ushort Pos { get; set; }
    }
}