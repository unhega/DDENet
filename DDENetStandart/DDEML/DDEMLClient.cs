using System;
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

            var res = DDEML.DdeInitialize(ref _idInst, _callback, DDEML.APPCMD_CLIENTONLY, 0);
            if (res != DDEML.DMLERR_NO_ERROR)
            {
                throw new Exception($"Unable register with DDEML. Error: {res}");
            }

            var hszService = DDEML.DdeCreateStringHandle(_idInst, service, DDEML.CP_WINUNICODE);
            var hszTopic = DDEML.DdeCreateStringHandle(_idInst, topic, DDEML.CP_WINUNICODE);
            _hConv = DDEML.DdeConnect(_idInst, hszService, hszTopic, IntPtr.Zero);

            DDEML.DdeFreeStringHandle(_idInst, hszService);
            DDEML.DdeFreeStringHandle(_idInst, hszService);

            if (_hConv == IntPtr.Zero)
            {
                throw new Exception("Unable to establish connection with server");
            }
        }

        public void Dispose()
        {
            if (_hConv != IntPtr.Zero)
            {
                DDEML.DdeDisconnect(_hConv);
            }
            if (_idInst != 0)
            {
                DDEML.DdeUninitialize(_idInst);
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

        static private void MainLoop()
        {

        }
    }
}
