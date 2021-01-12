using DoplTechnologies.Protos;
using Google.Protobuf;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DoplTechnologies.Sdk
{
    public class TeleroboticSdk
    {
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

        public static event Action<ConnectionState> OnStateChanged;

        private static ConnectionState _state = ConnectionState.Disconnected;
        public static ConnectionState State
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

        public static event Action<UInt64> OnSessionJoinedEvent;
        public static event Action<UInt64> OnSessionEndedEvent;
        public static event Func<CatheterData[]> OnGetCatheterDataEvent;
        public static event Func<RobotControllerData> OnGetRobotControllerDataEvent;
        public static event Func<ElectricalSignalData[]> OnGetElectricalSignalDataEvent;
        public static event Action<CatheterData[]> OnCatheterDataEvent;
        public static event Action<ElectricalSignalData[]> OnElectricalSignalDataEvent;

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libsdk_test();

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libsdk_initialize(GoString deviceServiceAddress, GoString sessionServiceAddress, GoString stateManagerServiceAddress, UInt64 deviceID, UInt32 devicePort, GoString ipAddress, GoSlice produces, GoSlice consumes, OnSessionCallback onSessionJoined, OnSessionCallback onSessionEnded, GetFrameCallback getFrameCallback, OnFrameCallback onFrameCallback, UInt64 defaultSessionId, int getFrameIntervalMS);

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libsdk_connect(UInt64 deviceId);

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libsdk_disconnect(UInt64 deviceID);

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 libsdk_createSession(GoString name, GoSlice deviceIDs);

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool libsdk_joinSession(UInt64 sessionID);

        [DllImport("lib/libsdk", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool libsdk_deleteSession(UInt64 sessionID);

        public static int Test()
        {
            return libsdk_test();
        }

        private static OnSessionCallback _onSessionJoined;
        private static OnSessionCallback _onSessionEnded;
        private static GetFrameCallback _getFrameCallback;
        private static OnFrameCallback _onFrameCallback;
        unsafe public static void Initialize(string deviceServiceAddress, string sessionServiceAddress, string stateManagerServiceAddress, UInt64 deviceId, UInt32 devicePort, string ipAddress, DataType[] produces, DataType[] consumes, UInt64 defaultSessionId, int getFrameIntervalMS = 30)
        {
            _onSessionJoined = new OnSessionCallback(OnSessionJoined);
            _onSessionEnded = new OnSessionCallback(OnSessionEnded);
            _getFrameCallback = new GetFrameCallback(HandleGetFrame);
            _onFrameCallback = new OnFrameCallback(HandleOnFrame);

            Console.WriteLine("Initializing platform sdk");
            libsdk_initialize(
                GoString.Create(deviceServiceAddress),
                GoString.Create(sessionServiceAddress),
                GoString.Create(stateManagerServiceAddress),
                deviceId,
                devicePort,
                GoString.Create(ipAddress),
                GoSlice.Create(produces.Select(item => (Int64)item).ToArray()),
                GoSlice.Create(consumes.Select(item => (Int64)item).ToArray()),
                _onSessionJoined,
                _onSessionEnded,
                _getFrameCallback,
                _onFrameCallback,
                defaultSessionId,
                getFrameIntervalMS
            );
        }

        unsafe public static Task Connect(UInt64 deviceId)
        {
            Console.WriteLine("Platform sdk connecting");
            State = ConnectionState.Connecting;
            return Task.Run(() =>
            {
                libsdk_connect(deviceId);
                Console.WriteLine("Connect completed");
            });
        }

        public static void Disconnect(UInt64 deviceId)
        {
            Console.WriteLine($"Disconnecting {deviceId}");
            State = ConnectionState.Disconnecting;
            libsdk_disconnect(deviceId);
        }

        public static UInt64 CreateSession(string name, UInt64[] deviceIDs)
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

        public static bool DeleteSession(UInt64 sessionID)
        {
            Console.WriteLine($"Deleting session {sessionID}");
            return libsdk_deleteSession(sessionID);
        }

        public static Task JoinSession(UInt64 sessionID)
        {
            Console.WriteLine($"Joining session {sessionID}");
            State = ConnectionState.Connecting;
            return Task.Run(() =>
            {
                libsdk_joinSession(sessionID);
                Console.WriteLine("Libsdk Join completed");
            });
        }

        private static void OnSessionJoined(UInt64 sessionId)
        {
            Console.WriteLine($"Joined session {sessionId}");
            State = ConnectionState.Connected;
            OnSessionJoinedEvent?.Invoke(sessionId);
        }

        private static void OnSessionEnded(UInt64 sessionId)
        {
            Console.WriteLine($"Session {sessionId} ended");
            State = ConnectionState.Disconnected;
            OnSessionEndedEvent?.Invoke(sessionId);
        }

        unsafe private static bool HandleGetFrame(byte* buffer, int* bufferSizePtr)
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

        unsafe private static bool HandleOnFrame(byte* buffer, int bufferSize)
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

            return true;
        }

        unsafe private static bool FillBufferWithFrame(byte* buffer, int* bufferSizePtr, Frame frame)
        {
            var franeBytes = frame.ToByteString().ToByteArray();
            Marshal.Copy(franeBytes, 0, new IntPtr(buffer), franeBytes.Length);
            *bufferSizePtr = franeBytes.Length;
            return true;
        }

        unsafe private static Frame BufferToFrame(byte* buffer, int bufferSize)
        {
            var frameBytes = new byte[bufferSize];
            Marshal.Copy(new IntPtr(buffer), frameBytes, 0, bufferSize);
            return Frame.Parser.ParseFrom(frameBytes, 0, bufferSize);
        }
    }


}