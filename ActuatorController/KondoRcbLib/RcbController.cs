using System.IO.Ports;

namespace ActuatorController.KondoRcbLib
{
    public abstract class RcbController
    {
        protected readonly Action<string> Debug;
        protected readonly SerialPort SerialPort;

        protected RcbController(string portName, Action<string> debug)
        {
            Debug = debug;

            try
            {
                SerialPort = new SerialPort();

                SetupPort(portName);

                SerialPort.Open();
            }
            catch (Exception e)
            {
                Debug("Cannot Open " + portName + e);
                throw;
            }
        }

        ~RcbController()
        {
            if (IsConnected())
            {
                ClosePort();
            }
        }

        /// <summary>
        /// シリアルポートをセットアップする
        /// </summary>
        /// <param name="portName">ポート名</param>
        protected abstract void SetupPort(string portName);

        /// <summary>
        /// シリアルポートをクローズする
        /// </summary>
        public void ClosePort()
        {
            SerialPort.Close();
        }

        /// <summary>
        /// シリアルポートが開いているかのチェック
        /// </summary>
        /// <returns>可否</returns>
        public bool IsConnected()
        {
            return SerialPort != null && SerialPort.IsOpen;
        }

        /// <summary>
        /// メッセージを送信します
        /// </summary>
        /// <param name="msg">データ</param>
        /// <param name="msgName">デバッグに表示する名前</param>
        /// <returns>成功したか</returns>
        /// <exception cref="Exception"></exception>
        protected bool SendMsg(byte[] msg, string msgName = "None")
        {
            if (!IsConnected())
                throw new Exception("SerialPort is not Opened");

            try
            {
                // 送信
                SerialPort.Write(msg, 0, msg.Length);

                Debug($"SendMsg: {msgName}, {BitConverter.ToString(msg)} bytes");

                return true;
            }
            catch (Exception e)
            {
                Debug($"SendMsg: {msgName} Error {e}");

                return false;
            }
        }

        /// <summary>
        /// メッセージを受信します
        /// </summary>
        /// <param name="msgSize">受信サイズ</param>
        /// <param name="msgName">デバッグに表示する名前</param>
        /// <returns>データ</returns>
        /// <exception cref="Exception"></exception>
        protected byte[] ReadMsg(byte msgSize, string msgName = "null")
        {
            if (!IsConnected())
                throw new Exception("SerialPort is not Opened");

            try
            {
                var readMsg = new byte[msgSize];

                // 受信
                SerialPort.Read(readMsg, 0, msgSize);

                Debug($"ReadMsg: {msgName}, {BitConverter.ToString(readMsg)} bytes");

                return readMsg;
            }
            catch (Exception e)
            {
                Debug($"ReadMsg: {msgName} Error {e}");
                return new byte[] { };
            }
        }

        protected byte GetHighValue(ushort value)
        {
            return (byte)(value >> 8);
        }

        protected byte GetLowValue(ushort value)
        {
            return (byte)(value & 0xFF);
        }

        protected byte GetCheckSum(byte[] value)
        {
            return (byte)value.Sum(x => x);
        }

        // TODO
        public abstract void MoveSingleServo(byte num, byte speed, ushort pos);
        public abstract byte[] GetVersion();
    }
}