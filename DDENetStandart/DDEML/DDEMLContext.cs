using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DDENetStandart;


namespace DDENetStandart.DDEML
{
    public class DDEMLContext
    {
        private int idInst = 0;
        private IntPtr hConv;
        private readonly DDEML.DdeCallback _callback;

        private DDEMLClient client;
        private List<Action> ddemlActions;
        private Thread msgThread;
        private AutoResetEvent ddeevent;
        private bool isInit = false;

        private Dictionary<string, IntPtr> advisedItems;
        private Dictionary<string, IntPtr> AdvisedItems
        { get
            {
                if (advisedItems is null)
                {
                    advisedItems = new Dictionary<string, IntPtr>();
                }
                return advisedItems;
            }
        }




        public DDEMLContext(DDEMLClient client)
        {
            this.client = client;
            _callback = DDECallback;
            ddemlActions = new List<Action>(4);
            ddeevent = new AutoResetEvent(false);
            msgThread = new Thread(MainLoop);
            msgThread.SetApartmentState(ApartmentState.STA);
            msgThread.IsBackground = true;
            msgThread.Start();
        }

        public void Advise(string item, bool stop = false)
        {
            Invoke(() =>
            {
                int res = 0;
                var hszItem = DDEML.DdeCreateStringHandle(idInst, item, DDEML.CP_WINUNICODE);
                var hDdeData = DDEML.DdeClientTransaction(IntPtr.Zero, 0, hConv, hszItem, DDEML.CF_TEXT,
                    stop ? DDEML.XTYP_ADVSTOP : DDEML.XTYP_ADVSTART, DDEML.TIMEOUT_ASYNC, ref res);
                DDEML.DdeFreeStringHandle(idInst, hszItem);
                //Вытащить хэндл для проверки в клиенте?
                if (hDdeData == IntPtr.Zero)
                {
                    //TODO:Replace to logger
                    Console.WriteLine("Error during advise command execution");
                }
                else if(!stop)
                {
                    //TODO:Replace to logger
                    Console.WriteLine($"Added {item} to advise");
                    AdvisedItems.Add(item, hszItem);
                }
                DDEML.DdeFreeDataHandle(hDdeData);
            });
        }

        public void Connect(string service, string topic)
        {
            int res = -1;
            Invoke(() =>
            {
                res = DDEML.DdeInitialize(ref idInst, _callback, DDEML.APPCMD_CLIENTONLY, 0);
                ddeevent.Set();
            });

            ddeevent.WaitOne();
            if (res != DDEML.DMLERR_NO_ERROR)
            {
                throw new Exception($"Unable register with DDEML. Error: {res}");
            }

            //TODO:Replace to logger
            Console.WriteLine("Starting connection");
            Invoke(() =>
            {
                var hszService = DDEML.DdeCreateStringHandle(idInst, service, DDEML.CP_WINUNICODE);
                var hszTopic = DDEML.DdeCreateStringHandle(idInst, topic, DDEML.CP_WINUNICODE);
                hConv = DDEML.DdeConnect(idInst, hszService, hszTopic, IntPtr.Zero);
                DDEML.DdeFreeStringHandle(idInst, hszService);
                DDEML.DdeFreeStringHandle(idInst, hszService);
                ddeevent.Set();
            });

            ddeevent.WaitOne();
            if (hConv == IntPtr.Zero)
            {
                throw new Exception("Unable to establish connection with server");
            }
            //TODO:Replace to logger
            Console.WriteLine("Connection established");
        }

        public void Disconnect()
        {
            //Отписаться от всех запросов
            if(AdvisedItems.Count > 0)
            {
                foreach(var item in AdvisedItems)
                {

                }
            }
            //Закрыть соеденение
        }


        private void Invoke(Action action)
        {
            lock (ddemlActions)
            {
                ddemlActions.Add(action);
            }
        }

        private IntPtr DDECallback(
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
                    DDEML.DdeQueryString(idInst, hsz2, item, item.Capacity, DDEML.CP_WINANSI);
                    client.OnAdvise(item.ToString());
                    DDEML.DdeUnaccessData(hDdeData);
                }
                return new IntPtr(DDEML.DDE_FACK);
            }

            return IntPtr.Zero;
        }

        #region Main msg processing loop 
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

               public void MainLoop()
        {
            //Выполняем все, что накидали в очередь
            Console.WriteLine("Starting delegates executing");
            lock (ddemlActions)
            {
                if (ddemlActions.Count > 0)
                {
                    foreach (var del in ddemlActions)
                    {
                        del.Invoke();
                    }
                    ddemlActions.Clear();
                }
            }
            Console.WriteLine("Starting msg receiving");
            //Обрабатываем сообщения, которые получили из системы
            if (isInit)
            {
                MSG msg = new MSG();
                while (PeekMessage(ref msg, IntPtr.Zero, 0, 0, PM_REMOVE) == true)
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                Console.WriteLine("Main loop ended, repeating...");
            }
            


        }
        #endregion
    }
}
