using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DDENetStandart.DDEML;

namespace DDENetStandart.DDEML
{
    class DDEMLClient:IDisposable
    {
        int _idInst = 0;
        IntPtr _hConv;
        object _hConvLock = new object();
        private readonly DDEML.DdeCallback _callback;
        private List<Action> ddemlDelegats;
        private Thread msgThread;
        
        public DDEMLClient(string service, string topic)
        {
            _callback = Callback;
            ddemlDelegats = new List<Action>(4);
            msgThread = new Thread(MainLoop);
            msgThread.SetApartmentState(ApartmentState.STA);
            msgThread.IsBackground = true;
            msgThread.Start();

            AddToMainLoop(() =>
            {
                var res = DDEML.DdeInitialize(ref _idInst, _callback, DDEML.APPCMD_CLIENTONLY, 0);
                if (res != DDEML.DMLERR_NO_ERROR)
                {
                    throw new Exception($"Unable register with DDEML. Error: {res}");
                }

                Console.WriteLine("Starting connection");

                var hszService = DDEML.DdeCreateStringHandle(_idInst, service, DDEML.CP_WINUNICODE);
                var hszTopic = DDEML.DdeCreateStringHandle(_idInst, topic, DDEML.CP_WINUNICODE);
                lock(_hConvLock)
                    _hConv = DDEML.DdeConnect(_idInst, hszService, hszTopic, IntPtr.Zero);

                DDEML.DdeFreeStringHandle(_idInst, hszService);
                DDEML.DdeFreeStringHandle(_idInst, hszService);
            });
            

            if (_hConv == IntPtr.Zero)
            {
                throw new Exception("Unable to establish connection with server");
            }
        }

        public void Dispose()
        {
            if (_hConv != IntPtr.Zero)
            {
                AddToMainLoop(() => { DDEML.DdeDisconnect(_hConv); });
            }
            if (_idInst != 0)
            {
                AddToMainLoop(() => { DDEML.DdeUninitialize(_idInst); });
            }
        }

        private IntPtr Callback(
            int uType, int uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hDdeData, IntPtr dwData1,
            IntPtr dwData2)
        {
            if (uType == DDEML.XTYP_ADVDATA)
            {
                int dwSize = 0;
                var pData = DDEML.DdeAccessData(hDdeData, ref dwSize);
                if (pData != IntPtr.Zero)
                {
                    var item = new StringBuilder(255);
                    DDEML.DdeQueryString(_idInst, hsz2, item, item.Capacity, DDEML.CP_WINANSI);
                    ProcessData(item.ToString());
                    DDEML.DdeUnaccessData(hDdeData);
                }
                return new IntPtr(DDEML.DDE_FACK);
            }

            return IntPtr.Zero;
        }

        private void ProcessData(string data)
        {
            Console.WriteLine(data);
        }

        #region MainLoop
        struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        struct POINT
        {
            public int X;
            public int Y;

            public POINT(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        [DllImport("user32.dll", EntryPoint = "PeekMessageA", CharSet = CharSet.Ansi)]
        private static extern bool PeekMessage(ref MSG lpmsg, IntPtr hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG msg);
        [DllImport("user32.dll")]
        private static extern IntPtr TranslateMessage(ref MSG msg);

        private const int PM_REMOVE = 0x0001;

        private void AddToMainLoop(Action action)
        {
            lock (ddemlDelegats)
            {
                ddemlDelegats.Add(action);
            }
        }

        private void MainLoop()
        {
            //Выполняем все, что накидали в очередь
            lock (ddemlDelegats)
            {
                if (ddemlDelegats.Count > 0)
                {
                    foreach (var del in ddemlDelegats)
                    {
                        del.Invoke();
                    }
                    ddemlDelegats.Clear();
                }
            }
            //Обрабатываем сообщения, которые получили из системы
            MSG msg = new MSG();
            while(PeekMessage(ref msg, IntPtr.Zero, 0, 0, PM_REMOVE) == true)
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
            Console.WriteLine("Main loop ended, repeating...");


        }
        #endregion


    }
}
