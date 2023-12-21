using System.IO.Ports;

namespace ActuatorController.KondoRcbLib
{
    public class Rcb3Controller : RcbController
    {
        public Rcb3Controller(string portName, Action<string> debug) : base(portName, debug)
        {
        }

        protected override void SetupPort(string portName)
        {
            SerialPort.PortName = portName;
            SerialPort.BaudRate = 115200;
            SerialPort.Parity = Parity.None;
            SerialPort.StopBits = StopBits.One;
            SerialPort.DataBits = 8;
            SerialPort.ReadTimeout = 500;
        }

        /// <summary>
        /// 使用可能かチェックする(これを実行するとRCB3がコマンド待機状態になる)
        /// </summary>
        /// <returns>使用可能</returns>
        private bool SetRcbReady()
        {
            // 「送信OK?」と「OK!」のコマンド
            const byte cmd = 0x0D;

            SendMsg(new byte[] { cmd }, "Is Rcb OK?");

            var readMsg = ReadMsg(1, "Rcb3 is OK");

            Debug($"CheckRcbReady: Receive {BitConverter.ToString(readMsg)}");
            Debug(readMsg[0] == cmd ? "CheckRcbReady: Rcb3 is OK" : "CheckRcbReady: Rcb3 is False");

            return true;
        }

        // 未チェック
        public override void MoveSingleServo(byte num, byte speed, ushort pos)
        {
            // 送信コマンド(1つのポートに対して動作パラメータと動作速度を設定する リファレンスP20)
            // 0:コマンド 0xFE 1:オプション ACKあり 2:送信ポート番号 0x00-0x17 3:動作速度 0x01-0xFF 4:サーボモーターの角度データと結合する
            var msg = new byte[] { 0xFE, 0b_0000_0001, num, speed }
                .Concat(new byte[] { GetHighValue(pos), GetLowValue(pos) }).ToArray();

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            if (!SetRcbReady())
            {
                Debug("Rcb3 is not ready");
                throw new Exception("Rcb3 is not ready");
            }

            SendMsg(msgAddedCheckSum, "MoveSingleServo");

            // メッセージ受け取りを少し遅延させる。これがないと死ぬ。
            System.Threading.Thread.Sleep(100);

            var readMsg = ReadMsg(1, "MoveSingleServo");

            Debug("MoveSingleServo: OK");
        }

        // 未チェック
        public override byte[] GetVersion()
        {
            // 送信コマンド(バージョン情報を取得 リファレンスP27)
            // 0:コマンド 0xFF
            byte[] msg = { 0xFF };

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            if (!SetRcbReady())
            {
                Debug("Rcb3 is not ready");
                throw new Exception("Rcb3 is not ready");
            }

            SendMsg(msgAddedCheckSum, "GetVersion");

            // メッセージ受け取りを少し遅延させる。これがないと死ぬ。
            System.Threading.Thread.Sleep(100);

            // !TODO!
            // 受信 (FP64 + SUM 4byte)
            var readData = ReadMsg(64 + 4, "GetVersion");

            Debug("GetVersion: OK");

            return readData;
        }

        // 未チェック
        public void MoveMultipleServoSingleSpeed(List<ServoMovementData> servoMovementData, byte motionNum, byte motionPosNum, byte speed)
        {
            // ソート
            var sortedServoMovementData = servoMovementData.OrderBy(i => i.Num).ToList();

            var posParameter = new byte[24 * 2];
            foreach (var i in servoMovementData)
            {
                posParameter[i.Num * 2] = GetHighValue(i.Pos);
                posParameter[i.Num * 2 + 1] = GetLowValue(i.Pos);
            }

            // 送信コマンド(動作パラメータと動作速度を設定する リファレンスP20)
            // 0:コマンド 0xFD 1:オプション ACKあり
            var msg = new byte[] { 0xFD, 0b_0000_0001, motionNum, motionPosNum, speed }
                .Concat(posParameter).ToArray();

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            if (!SetRcbReady())
            {
                Debug("Rcb3 is not ready");
                throw new Exception("Rcb3 is not ready");
            }

            SendMsg(msgAddedCheckSum, "MoveMultipleServo");

            var readData = ReadMsg(1, "MoveMultipleServo");

            Debug("MoveMultipleServo: OK");
        }


        // 未チェック
        public int GetSingleServoPosition(byte num)
        {
            // 送信コマンド(教示機能で計測した舵角を確認する リファレンスP22)
            // 0:コマンド 0xF0
            byte[] msg = { 0xF0 };

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            if (!SetRcbReady())
            {
                Debug("Rcb3 is not ready");
                throw new Exception("Rcb3 is not ready");
            }

            SendMsg(msgAddedCheckSum, "GetServoPosition");

            // メッセージ受け取りを少し遅延させる。これがないと死ぬ。
            System.Threading.Thread.Sleep(400);

            var readData = ReadMsg(48 + 1, "GetServoPosition");

            var parameter = readData[num * 2] << 8 | readData[num * 2 + 1];

            Debug("GetSingleServoPosition: OK");

            return (parameter - 16384) / (8000 / 270);
        }

        // 未チェック
        public int[] GetAllServoPosition()
        {
            // 送信コマンド(教示機能で計測した舵角を確認する リファレンスP22)
            // 0:コマンド 0xF0
            byte[] msg = { 0xF0 };

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            if (!SetRcbReady())
            {
                Debug("Rcb3 is not ready");
                throw new Exception("Rcb3 is not ready");
            }

            SendMsg(msgAddedCheckSum, "GetServoPosition");

            // メッセージ受け取りを少し遅延させる。これがないと死ぬ。
            System.Threading.Thread.Sleep(400);

            var readData = ReadMsg(48 + 1, "GetServoPosition");

            var parameter = new int[24];
            for (int i = 0; i < 24; i++)
            {
                parameter[i] = readData[i * 2] << 8 | readData[i * 2 + 1];
                parameter[i] = (parameter[i] - 16384) / (8000 / 270);
            }

            Debug("GetAllServoPosition: OK");

            return parameter;
        }

        // 未チェック
        public void SetServoHomePosition(byte num, ushort pos)
        {
            // 送信コマンド(1つのポートに対してホームポジションを設定する リファレンスP21)
            // 0:コマンド 0xFB 1:オプション ACKあり 2:送信ポート番号 0x00-0x17 4:サーボモーターの角度データと結合する
            var msg = new byte[] { 0xFB, 0b_0000_0001, num }
                .Concat(new byte[] { GetHighValue(pos), GetLowValue(pos) }).ToArray();

            var msgAddedCheckSum = msg.Concat(new[] { GetCheckSum(msg) }).ToArray();

            SendMsg(msgAddedCheckSum, "SetServoHomePosition");

            // メッセージ受け取りを少し遅延させる。これがないと死ぬ。
            System.Threading.Thread.Sleep(100);

            var readMsg = ReadMsg(1, "SetServoHomePosition");

            Debug("SetServoHomePosition: OK");
        }
    }
}