/* TODO
 未チェックな関数のテスト
 角度取得関数の実装
*/

using System.IO.Ports;

namespace ActuatorController.KondoRcbLib
{
    public class Rcb4Controller : RcbController
    {
        public Rcb4Controller(string portName, Action<string> debug) : base(portName, debug)
        {
            byte[] msg = { 0x09, 0x00, 0b0000_0010, 0x00, 0x00, 0x00, 0b0001_0011, 0b1000_0000 };
            byte[] msgAddedCheckSum = msg.Concat(new byte[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "Rcb4Controller");

            System.Threading.Thread.Sleep(100);

            byte[] readMsg = ReadMsg(4, "Rcb4Controller");

            if (readMsg[1] == 0x00 && readMsg[2] == 0x06)
            {
                Debug("Rcb4Controller: OK");
            }

            if (readMsg[1] == 0x00 && readMsg[2] == 0x15)
            {
                Debug("Rcb4Controller: CheckSumError or ICS is not defined");
            }
        }

        protected override void SetupPort(string portName)
        {
            SerialPort.PortName = portName;
            SerialPort.BaudRate = 115200;
            SerialPort.Parity = Parity.Even; //ここがRCB-3HVと違う
            SerialPort.StopBits = StopBits.One;
            SerialPort.DataBits = 8;
            SerialPort.ReadTimeout = 500;
        }

        /// <summary>
        /// サーボを駆動する (RCB4-HVコマンドリファレンス)
        /// </summary>
        /// <param name="num">ICS番号 0~35</param>
        /// <param name="speed">動作速度 1~0xFF</param>
        /// <param name="pos">フリー 0x8000 ホールド 0x7fff</param>
        public override void MoveSingleServo(byte num, byte speed, ushort pos)
        {
            byte[] msg = { 0x07, 0x0F, num, speed, GetLowValue(pos), GetHighValue(pos) };
            byte[] msgAddedCheckSum = msg.Concat(new byte[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "MoveSingleServo");

            System.Threading.Thread.Sleep(100);

            byte[] readMsg = ReadMsg(4, "MoveSingleServo");

            if (readMsg[1] == 0x0F && readMsg[2] == 0x06)
            {
                Debug("MoveSingleServo: OK");
            }

            if (readMsg[1] == 0x0F && readMsg[2] == 0x15)
            {
                Debug("MoveSingleServo: CheckSumError or ICS is not defined");
            }
        }

        /// <summary>
        /// バージョンを取得する
        /// </summary>
        /// <returns>バージョン情報</returns>
        /// <exception cref="Exception"></exception>
        public override byte[] GetVersion()
        {
            byte[] msg = { 0x03, 0xFD };
            byte[] msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "GetVersion");

            System.Threading.Thread.Sleep(400);

            byte[] readMsg = ReadMsg(35, "GetVersion");

            return readMsg.Skip(2).Take(31).ToArray();
        }

        // 未チェック
        public void MoveMultipleServoSingleSpeed(List<ServoMovementData> servoMovementData, byte speed)
        {
            var msgSize = 8 + servoMovementData.Count * 2 + 1;

            var sortedServoMovementData = servoMovementData.OrderBy(i => i.Num).ToList();

            // 動かすサーボのビットを立てる
            var moveBitParameter = new byte[5];

            foreach (var i in sortedServoMovementData)
            {
                switch (i.Num)
                {
                    case <= 7:
                        moveBitParameter[0] |= (byte)(1 << i.Num);
                        break;
                    case >= 8 and <= 15:
                        moveBitParameter[1] |= (byte)(1 << i.Num - 8);
                        break;
                    case >= 16 and <= 23:
                        moveBitParameter[2] |= (byte)(1 << i.Num - 16);
                        break;
                    case >= 24 and <= 31:
                        moveBitParameter[3] |= (byte)(1 << i.Num - 24);
                        break;
                    case >= 32 and <= 35:
                        moveBitParameter[4] |= (byte)(1 << i.Num - 32);
                        break;
                }
            }

            // ポジションを二つに分割する
            var posParameter = new byte[sortedServoMovementData.Count * 3];
            foreach (var i in sortedServoMovementData.Select((servoData, index) => new { servoData, index }))
            {
                posParameter[i.index * 2] = GetLowValue(i.servoData.Pos);
                posParameter[i.index * 2 + 1] = GetHighValue(i.servoData.Pos);
            }

            var msg = new byte[] { (byte)msgSize, 0x10 }
                .Concat(moveBitParameter)
                .Concat(new[] { speed })
                .Concat(posParameter).ToArray();

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "MoveMultipleServoSingleSpeed");

            System.Threading.Thread.Sleep(400);

            byte[] readMsg = ReadMsg(4, "MoveMultipleServoSingleSpeed");

            if (readMsg[1] == 0x10 && readMsg[2] == 0x06)
            {
                Debug("MoveMultipleServoSingleSpeed: OK");
            }

            if (readMsg[1] == 0x10 && readMsg[2] == 0x15)
            {
                Debug("MoveMultipleServoSingleSpeed: CheckSumError");
            }
        }

        public byte[] GetServoData(byte num, ushort offset, byte receiveDataSize)
        {
            const byte cmd = 0x00;
            const byte msgSize = 10;

            // Servo Address + Servo data size * Servo ID + Address offset
            ushort ramAddress = (ushort)(0x90 + 20 * num + offset);

            // Message size, Move cmd, RAM to COM, 0x00, 0x00, 0x00, Ram L Address, Ram H Address, RAM to COM message size
            var msg = new byte[]
            {
                msgSize, cmd, 0b00_10_00_00, 0, 0, 0, (byte)ramAddress, (byte)(ramAddress >> 8), receiveDataSize
            };
            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "GetServoData");

            System.Threading.Thread.Sleep(100);

            byte[] readMsg = ReadMsg((byte)(receiveDataSize + 3), "");

            // 本来は最初に0x04が帰ってくるはずだが、0x05が帰ってくるので仕方がなく先頭を外している
            // if (readMsg[0] != 0x04 || readMsg[1] != cmd)
            // {
            //     throw new Exception("Different message received.");
            // }

            if (readMsg[1] != cmd)
            {
                throw new Exception("Different message received.");
            }

            byte[] result = readMsg.Skip(2).Take(receiveDataSize).ToArray();

            return result;
        }

        public ushort GetServoPos(byte num)
        {
            var pos = GetServoData(num, 4, 2);
            return (ushort)(pos[0] | (pos[1] << 8));
        }
    }
}