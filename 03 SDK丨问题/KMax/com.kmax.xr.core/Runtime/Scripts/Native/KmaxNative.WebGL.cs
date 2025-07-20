using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

namespace KmaxXR
{
#if UNITY_WEBGL && !UNITY_EDITOR
    public interface IWebSocketHandler
    {
        void OnOpen();
        void OnClose(int closeCode);
        void OnError(string errorMsg);
        void OnMessage(byte[] data);
    }

    public static partial class KmaxNative
    {
        /* Delegates */
        public delegate void OnOpenCallback();
        public delegate void OnMessageCallback(System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(System.IntPtr errorPtr);
        public delegate void OnCloseCallback(int closeCode);

        #region WebSocket API bound to JSLIB.

        [DllImport("__Internal")]
        public static extern void wsSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void wsSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void wsSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void wsSetOnClose(OnCloseCallback callback);


        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int wsConnect(string host);

        [DllImport("__Internal")]
        public static extern int wsClose();

        [DllImport("__Internal")]
        public static extern int wsSend(byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int wsSendText(string message);

        [DllImport("__Internal")]
        public static extern int wsGetState();
        #endregion

        /* If callbacks was initialized and set */
        private static bool isWSInitialized = false;
        private static IWebSocketHandler wsHandler = null;

        /// <summary>
        /// 初始化WebSocket
        /// </summary>
        public static void InitializeWS()
        {
            if (isWSInitialized) return;
            wsSetOnOpen(WSOnOpenEvent);
            wsSetOnMessage(WSOnMessageEvent);
            wsSetOnError(WSOnErrorEvent);
            wsSetOnClose(WSOnCloseEvent);

            isWSInitialized = true;
        }

        /// <summary>
        /// 设置WebSocket事件监听对象
        /// </summary>
        /// <param name="handler">事件监听对象</param>
        public static void SetWSHandler(IWebSocketHandler handler) { wsHandler = handler; }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void WSOnOpenEvent()
        {
            wsHandler?.OnOpen();
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void WSOnMessageEvent(System.IntPtr msgPtr, int msgSize)
        {
            byte[] msg = new byte[msgSize];
            Marshal.Copy(msgPtr, msg, 0, msgSize);
            wsHandler?.OnMessage(msg);
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void WSOnErrorEvent(System.IntPtr errorPtr)
        {
            string errorMsg = Marshal.PtrToStringAuto(errorPtr);
            wsHandler?.OnError(errorMsg);
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void WSOnCloseEvent(int closeCode)
        {
            wsHandler?.OnClose(closeCode);
        }
    }
#endif
}
