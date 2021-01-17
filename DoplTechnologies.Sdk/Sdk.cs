using DoplTechnologies.Protos;
using Google.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DoplTechnologies.Sdk
{
    public class TeleroboticSdk
    {
        public string DefaultConfigFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            ".dopltech.yml"
        );
        public struct GoSlice
        {
            public IntPtr data;
            public long len, cap;
            public GoSlice(IntPtr data, long len, long cap)
            {
                this.data = data;
                this.len = len;
                this.cap = cap;
            }

            public static GoSlice Create(Int64[] arr)
            {
                IntPtr data_ptr = Marshal.AllocHGlobal(Buffer.ByteLength(arr));
                Marshal.Copy(arr, 0, data_ptr, arr.Length);
                return new GoSlice(data_ptr, arr.Length, arr.Length);
            }

            public static GoSlice Create(UInt64[] arr)
            {
                byte[] arrBytes = new byte[arr.Length * sizeof(UInt64)];
                Buffer.BlockCopy(arr, 0, arrBytes, 0, arrBytes.Length);

                IntPtr data_ptr = Marshal.AllocHGlobal(arrBytes.Length);
                Marshal.Copy(arrBytes, 0, data_ptr, arrBytes.Length);
                return new GoSlice(data_ptr, arr.Length, arr.Length);
            }
        }
        public struct GoString
        {
            public string msg;
            public long len;
            public GoString(string msg, long len)
            {
                this.msg = msg;
                this.len = len;
            }

            public static GoString Create(string str)
            {
                return new GoString(str, str.Length);
            }
        }

        public enum ConnectionState
        {
            Connected,
            Connecting,
            Disconnecting,
            Disconnected,
        }

        public event Action<ConnectionState> OnStateChanged;

        private ConnectionState _state = ConnectionState.Disconnected;
        public ConnectionState State
        {
            get { return _state; }
            set
            {
                if (value != _state)
                {
                    _state = value;
                    OnStateChanged?.Invoke(_state);
                }
            }
        }

        public delegate Frame GetFrame();
        public delegate bool OnFrame(Frame frame);

        public delegate void OnSessionCallback(UInt64 sessionId);
        unsafe private delegate bool GetFrameCallback(byte* data, int* bufferSizePtr);
        unsafe private delegate bool OnFrameCallback(byte* data, int bufferSize);

        public event Action<UInt64> OnSessionJoinedEvent;
        public event Action<UInt64> OnSessionEndedEvent;
        public event Func<CatheterData[]> OnGetCatheterDataEvent;
        public event Func<RobotControllerData> OnGetRobotControllerDataEvent;
        public event Func<ElectricalSignalData[]> OnGetElectricalSignalDataEvent;
        public event Action<CatheterData[]> OnCatheterDataEvent;
        public event Action<ElectricalSignalData[]> OnElectricalSignalDataEvent;
        public event Action<RobotControllerData> OnRobotControllerDataEvent;

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libsdk_test();

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool libsdk_initialize(GoString configFilePath, OnSessionCallback onSessionJoined, OnSessionCallback onSessionEnded, GetFrameCallback getFrameCallback, OnFrameCallback onFrameCallback);

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libsdk_connect();

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libsdk_disconnect();

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 libsdk_createSession(GoString name, GoSlice deviceIDs);

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool libsdk_joinSession(UInt64 sessionID);

        [DllImport("libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool libsdk_deleteSession(UInt64 sessionID);

        public static int Test()
        {
            return libsdk_test();
        }

        private OnSessionCallback _onSessionJoined;
        private OnSessionCallback _onSessionEnded;
        private GetFrameCallback _getFrameCallback;
        private OnFrameCallback _onFrameCallback;

        public TeleroboticSdk(string configFilePath = null) {
            if (!Initialize(configFilePath))
                throw new ArgumentException("Invalid config");
        }

        unsafe private bool Initialize(string configFilePath = null)
        {
            if (configFilePath == null) {
                configFilePath = DefaultConfigFilePath;
            }

            _onSessionJoined = new OnSessionCallback(OnSessionJoined);
            _onSessionEnded = new OnSessionCallback(OnSessionEnded);
            _getFrameCallback = new GetFrameCallback(HandleGetFrame);
            _onFrameCallback = new OnFrameCallback(HandleOnFrame);

            Console.WriteLine("Initializing platform sdk");
            return libsdk_initialize(
                GoString.Create(configFilePath),
                _onSessionJoined,
                _onSessionEnded,
                _getFrameCallback,
                _onFrameCallback
            );
        }

        unsafe public Task Connect()
        {
            Console.WriteLine("Platform sdk connecting");
            State = ConnectionState.Connecting;
            return Task.Run(() =>
            {
                libsdk_connect();
                Console.WriteLine("Connect completed");
            });
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnecting from platform");
            State = ConnectionState.Disconnecting;
            libsdk_disconnect();
        }

        public UInt64 CreateSession(string name, UInt64[] deviceIDs)
        {
            Console.WriteLine($"Creating session");
            var sessionID = libsdk_createSession(GoString.Create(name), GoSlice.Create(deviceIDs));

            if (sessionID > 0)
            {
                Console.WriteLine($"Session created with id: {sessionID}");
                return sessionID;
            }

            return 0;
        }

        public bool DeleteSession(UInt64 sessionID)
        {
            Console.WriteLine($"Deleting session {sessionID}");
            return libsdk_deleteSession(sessionID);
        }

        public Task JoinSession(UInt64 sessionID)
        {
            Console.WriteLine($"Joining session {sessionID}");
            State = ConnectionState.Connecting;
            return Task.Run(() =>
            {
                libsdk_joinSession(sessionID);
                Console.WriteLine("Libsdk Join completed");
            });
        }

        private void OnSessionJoined(UInt64 sessionId)
        {
            Console.WriteLine($"Joined session {sessionId}");
            State = ConnectionState.Connected;
            OnSessionJoinedEvent?.Invoke(sessionId);
        }

        private void OnSessionEnded(UInt64 sessionId)
        {
            Console.WriteLine($"Session {sessionId} ended");
            State = ConnectionState.Disconnected;
            OnSessionEndedEvent?.Invoke(sessionId);
        }

        unsafe private bool HandleGetFrame(byte* buffer, int* bufferSizePtr)
        {
            CatheterData[] catheterData = OnGetCatheterDataEvent?.Invoke();
            var robotControllerData = OnGetRobotControllerDataEvent?.Invoke();
            var electricalSignalData = OnGetElectricalSignalDataEvent?.Invoke();

            Frame frame = new Frame();
            if (catheterData != null)
            {
                frame.CatheterData.Add(catheterData);
            }

            if (robotControllerData != null)
            {
                frame.RobotControllerData = robotControllerData;
            }

            if (electricalSignalData != null)
            {
                frame.ElectricalSignals.Add(electricalSignalData);
            }

            return FillBufferWithFrame(buffer, bufferSizePtr, frame);
        }

        unsafe private bool HandleOnFrame(byte* buffer, int bufferSize)
        {
            var frame = BufferToFrame(buffer, bufferSize);
            if (frame == null)
            {
                return false;
            }

            if (frame.CatheterData != null)
            {
                OnCatheterDataEvent?.Invoke(frame.CatheterData.ToArray());
            }

            if (frame.ElectricalSignals != null)
            {
                OnElectricalSignalDataEvent?.Invoke(frame.ElectricalSignals.ToArray());
            }

            if (frame.RobotControllerData != null)
            {
                OnRobotControllerDataEvent?.Invoke(frame.RobotControllerData);
            }

            return true;
        }

        unsafe private bool FillBufferWithFrame(byte* buffer, int* bufferSizePtr, Frame frame)
        {
            var franeBytes = frame.ToByteString().ToByteArray();
            Marshal.Copy(franeBytes, 0, new IntPtr(buffer), franeBytes.Length);
            *bufferSizePtr = franeBytes.Length;
            return true;
        }

        unsafe private Frame BufferToFrame(byte* buffer, int bufferSize)
        {
            var frameBytes = new byte[bufferSize];
            Marshal.Copy(new IntPtr(buffer), frameBytes, 0, bufferSize);
            return Frame.Parser.ParseFrom(frameBytes, 0, bufferSize);
        }
    }


}