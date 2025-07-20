using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace KmaxXR
{
    public delegate void RemoteMsgHandler(byte[] bytes);
    interface IPNRemote: IDisposable
    {
        event RemoteMsgHandler OnMsg;
        void SendMsg(byte[] msg);
        /// <summary>
        /// 用于接收Unity消息的对象
        /// 设定此对象用于解耦
        /// </summary>
        GameObject Holder { get; }
    }

    internal class PNClient
    {
        internal enum CommandID
        {
            Connection, XRMode, Control
        }

        private static PNClient instance;

        private IPNRemote remote;
        int frame = 0;
        private float dataFactor = 1.0f;

        private PNClient()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            remote = new WebRemote().Connect();
#else
            remote = new RemoteService().Connect();
#endif
            remote.OnMsg += ProcessMsg;
        }

        void ProcessMsg(byte[] bytes)
        {
            frame++;
            if (frame >= int.MaxValue)
            {
                frame = 0;
            }

            ParsePNData(bytes);
        }


        void SendMsg(byte[] bytes)
        {
            remote.SendMsg(bytes);
        }

        void HoldMsg<T>(string type, T obj) where T : struct
        {
            remote.Holder?.SendMessage($"On{type}", obj);
        }

        void HoldMsg(string type)
        {
            remote.Holder?.SendMessage($"On{type}");
        }

        static void ParsePNData(byte[] bytes)
        {
            var result = Encoding.Default.GetString(bytes);
            var td = JsonUtility.FromJson<TrackerData>(result);
            FixData(ref td);
            Handlers?.Invoke(td);
        }

        [Serializable]
        struct Command<T> where T : struct
        {
            /// <summary>
            /// 客户端请求的命令类型
            /// </summary>
            public int cid;
            /// <summary>
            /// 数据结构类型名(typeof(T))
            /// </summary>
            public string type;
            /// <summary>
            /// 数据
            /// </summary>
            public T data;
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <typeparam name="T">指令的数据类型</typeparam>
        /// <param name="id">指令的类型</param>
        /// <param name="obj">指令包含的数据</param>
        internal static void SendCommand<T>(CommandID id, T obj) where T : struct
        {
            var data = JsonUtility.ToJson(new Command<T>
            {
                cid = (int)id,
                type = typeof(T).Name,
                data = obj
            });
            //Debug.Log($"SendCommand:{data}");
            switch (id)
            {
                case CommandID.Control:
                    instance.HoldMsg(typeof(T).Name, obj);
                    break;
                default:
                    break;
            }
            instance.SendMsg(Encoding.Default.GetBytes(data));
        }

        internal static void Start()
        {
            if (instance == null || instance.remote == null)
            {
                instance = new PNClient();
            }
            else
            {
                instance.HoldMsg("Start");
            }
        }

        internal static void Stop()
        {
            instance.remote?.Dispose();
            instance.remote = null;
        }

        /// <summary>
        /// 适应不同来源的数据
        /// 应用无需关心运行在什么平台，什么尺寸的设备上，不同来源的数据会在此处做归一化处理。
        /// </summary>
        /// <param name="td">原数据</param>
        private static void FixData(ref TrackerData td)
        {
            if (td.dataVersion > 0)
            {
                var size = XRRig.Screen.Size;
                var screenScale = size.x / td.screenWidth;
                td.pen.pos *= screenScale;
                td.eye.pos *= screenScale;

                instance.dataFactor = screenScale;
            }
        }

        /// <summary>
        /// trigger event from external data source
        /// </summary>
        /// <param name="extData">external data source</param>
        internal static void Trigger(TrackerData extData)
        {
            FixData(ref extData);
            Handlers?.Invoke(extData);
        }

        internal delegate void TrackerHandler(TrackerData data);

        internal static event TrackerHandler Handlers;

        /// <summary>
        /// Tracking data factor
        /// 试配不同尺寸下的追踪结果
        /// </summary>
        internal static float DataFactor => instance.dataFactor;
    }

    internal class RemoteService : IPNRemote
    {
#if UNITY_STANDALONE
        [DefaultExecutionOrder(-100)]
        class LocalBridge : MonoBehaviour
        {
            private TrackerData trackerData;
            private void Start()
            {
                DontDestroyOnLoad(this);
                OnStart();

            }

            void OnStart()
            {
                if (KmaxNative.SoftwareStereoDisplay)
                {
                    var rig = FindObjectOfType<XRRig>();
                    if (!rig)
                    {
                        Debug.LogError("Can not find XRRig!");
                        return;
                    }

                    // 渲染到纹理
                    var texs = (rig.StereoRender as VRRenderer).RenderToTexture();
                    var ret = KmaxNative.kxrCreateStereoOverlay(texs[0].GetNativeTexturePtr(), texs[1].GetNativeTexturePtr(), 0);
                    Debug.Log($"Native kxrCreateStereoOverlay code: {ret}.");
                    rig.SwitchViewMode(false);
                    KmaxNative.kSetTexture(texs[0].GetNativeTexturePtr(), texs[1].GetNativeTexturePtr());
                }
            }

            private void Update()
            {
                if (KmaxNative.kxrGetTrackData(ref trackerData) >= 0)
                    PNClient.Trigger(trackerData);
            }
            void LateUpdate()
            {
                if (KmaxNative.SoftwareStereoDisplay)
                    GL.IssuePluginEvent(KmaxNative.kGetRenderFunc(), 0);
            }

            private void OnPenShakeCommand(PenShakeCommand cmd)
            {
                KmaxNative.kxrPenShake(cmd.time, cmd.strength);
            }

            private void OnDestroy()
            {
                KmaxNative.kDestroy();
            }

        }
#endif
        private UdpClient client;
        private IPEndPoint endPoint;

        internal IPAddress Address => endPoint.Address;

        public GameObject Holder { get; private set; }

        const int PORT = 9898;
        public RemoteService(int port = PORT)
        {
#if UNITY_EDITOR
            var host = IPAddress.Any;
#else
            var host = IPAddress.Loopback;
#endif
            endPoint = new IPEndPoint(host, port);
            client = new UdpClient();
#if UNITY_STANDALONE
            Holder = new GameObject(nameof(LocalBridge), typeof(LocalBridge));
#endif
        }

        public RemoteService Connect()
        {
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(endPoint);
            client.BeginReceive(ReceiveMsg, null);
            return this;
        }

        public void Dispose()
        {
            client?.Close();
            client?.Dispose();
        }

        public void SendMsg(byte[] sendBytes)
        {
            void Sent(IAsyncResult ar)
            {
                client.EndSend(ar);
            }

            if (endPoint.Port != PORT)
            {
                client.BeginSend(sendBytes, sendBytes.Length, endPoint, Sent, null);
            }
        }

        private void ReceiveMsg(IAsyncResult ar)
        {
            if (client == null || client.Client == null) return;
            try
            {
                byte[] receiveBytes = client.EndReceive(ar, ref endPoint);
                OnMsg?.Invoke(receiveBytes);
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError(e);
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            client.BeginReceive(ReceiveMsg, null);
        }

        public event RemoteMsgHandler OnMsg;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    internal class WebRemote : IPNRemote, IWebSocketHandler
    {
        private readonly string host;

        public enum WebSocketState
        {
            Connecting,
            Open,
            Closing,
            Closed
        }

        /// <summary>
        /// WebSocket 的状态
        /// 如果有错误将抛出异常
        /// </summary>
        public static WebSocketState State
        {
            get
            {
                int state = KmaxNative.wsGetState();

                if (state < 0)
                {
                    Debug.LogError(GetErrorMessageFromCode(state));
                    return WebSocketState.Closed;
                }

                state = Math.Min(state, (int)WebSocketState.Closed);
                return (WebSocketState)state;
            }
        }

        public GameObject Holder { get; private set; }

        public WebRemote(string host = null)
        {
            this.host = string.IsNullOrEmpty(host) ? "localhost" : host;
            KmaxNative.InitializeWS();
            KmaxNative.SetWSHandler(this);
            Holder = new GameObject(nameof(WebMessageReceiver), typeof(WebMessageReceiver));
        }

        public WebRemote Connect()
        {
            KmaxNative.wsConnect(host);
            return this;
        }

        public void Close()
        {
            KmaxNative.wsClose();
        }

        void IWebSocketHandler.OnOpen()
        {
            Debug.Log(nameof(IWebSocketHandler.OnOpen));
            var ver = KmaxNative.SDKVersion;
            PNClient.SendCommand(PNClient.CommandID.Connection, new ConnectionCommand(){
                platform = (int)Application.platform,
                sdkMajor = ver.Major,
                sdkMinor = ver.Minor,
                appId = 0,
                appName = Application.productName
            });
        }
        void IWebSocketHandler.OnClose(int closeCode)
        {
            Debug.Log($"{nameof(IWebSocketHandler.OnClose)}({closeCode})");
        }
        void IWebSocketHandler.OnError(string errorMsg)
        {
            Debug.LogError($"{nameof(IWebSocketHandler.OnError)}({errorMsg})");
        }

        void IWebSocketHandler.OnMessage(byte[] data)
        {
            try
            {
                OnMsg?.Invoke(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(IWebSocketHandler.OnMessage)} Exception: {ex.Message}");
            }
        }

        public event RemoteMsgHandler OnMsg;
        void IPNRemote.SendMsg(byte[] msg)
        {
            KmaxNative.wsSendText(Encoding.Default.GetString(msg));
        }

        public void Dispose()
        {
            KmaxNative.wsClose();
        }

        public static string GetErrorMessageFromCode(int errorCode)
        {
            switch (errorCode)
            {
                case -1: return "WebSocket instance not found.";
                case -2: return "WebSocket is already connected or in connecting state.";
                case -3: return "WebSocket is not connected.";
                case -4: return "WebSocket is already closing.";
                case -5: return "WebSocket is already closed.";
                case -6: return "WebSocket is not in open state.";
                case -7: return "Cannot close WebSocket. An invalid code was specified or reason is too long.";
                default: return "Unknown error.";
            }
        }

        internal class WebMessageReceiver: MonoBehaviour
        {
            void Start()
            {
                DontDestroyOnLoad(this);
            }
            void SetDisplayMode(int i) => CommonSetXRMode(-1, 1);
            void SetTracking(int i) => CommonSetXRMode(i, -1);
            void SetXRMode(int i) => CommonSetXRMode(i, i);
            private void CommonSetXRMode(int i, int j)
            {
                if (State != WebSocketState.Open)
                    Debug.LogError($"{nameof(SetDisplayMode)}({i}) failed.");
                PNClient.SendCommand(PNClient.CommandID.XRMode, new XRModeCommand()
                {
                    tracking = i,
                    displayMode = j
                });
                var rig = FindObjectOfType<XRRig>();
                if (rig) rig.SwitchViewMode(j == 1);
            }
        }
    }
#endif
}
