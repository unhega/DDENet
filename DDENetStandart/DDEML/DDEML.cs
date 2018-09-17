using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DDENetStandart.DDEML
{
    static class DDEML
    {
        //Функция, выполняемая DDEML библиотекой
        public delegate IntPtr DdeCallback(
            int uType, int uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hData, IntPtr dwData1,
            IntPtr dwData2);

        //Формат буфера обмена
        public const int CF_TEXT = 1;

        //Коды ошибок
        public const int DMLERR_NO_ERROR = 0x0000;
        public const int DMLERR_FIRST = 0x4000;
        public const int DMLERR_ADVACKTIMEOUT = 0x4000;
        public const int DMLERR_BUSY = 0x4001;
        public const int DMLERR_DATAACKTIMEOUT = 0x4002;
        public const int DMLERR_DLL_NOT_INITIALIZED = 0x4003;
        public const int DMLERR_DLL_USAGE = 0x4004;
        public const int DMLERR_EXECACKTIMEOUT = 0x4005;
        public const int DMLERR_INVALIDPARAMETER = 0x4006;
        public const int DMLERR_LOW_MEMORY = 0x4007;
        public const int DMLERR_MEMORY_ERROR = 0x4008;
        public const int DMLERR_NOTPROCESSED = 0x4009;
        public const int DMLERR_NO_CONV_ESTABLISHED = 0x400A;
        public const int DMLERR_POKEACKTIMEOUT = 0x400B;
        public const int DMLERR_POSTMSG_FAILED = 0x400C;
        public const int DMLERR_REENTRANCY = 0x400D;
        public const int DMLERR_SERVER_DIED = 0x400E;
        public const int DMLERR_SYS_ERROR = 0x400F;
        public const int DMLERR_UNADVACKTIMEOUT = 0x4010;
        public const int DMLERR_UNFOUND_QUEUE_ID = 0x4011;
        public const int DMLERR_LAST = 0x4011;

        //Коды ответов поля wStatus
        public const int DDE_FACK = 0x8000;
        public const int DDE_FBUSY = 0x4000;
        public const int DDE_FDEFERUPD = 0x4000;
        public const int DDE_FACKREQ = 0x8000;
        public const int DDE_FRELEASE = 0x2000;
        public const int DDE_FREQUESTED = 0x1000;
        public const int DDE_FAPPSTATUS = 0x00ff;
        public const int DDE_FNOTPROCESSED = 0x0000;

        //Флаги настройки режима опроса (ADVISE LOOP)
        public const int XTYPF_NOBLOCK = 0x0002;
        public const int XTYPF_NODATA = 0x0004; //Только оповещение
        public const int XTYPF_ACKREQ = 0x0008; //Ожидание подтверждения от клиента

        //Коды транзакций
        public const int XCLASS_MASK = 0xFC00;
        public const int XCLASS_BOOL = 0x1000;
        public const int XCLASS_DATA = 0x2000;
        public const int XCLASS_FLAGS = 0x4000;
        public const int XCLASS_NOTIFICATION = 0x8000;

        //Флаги типа транзакции
        public const int XTYP_ERROR = 0x0000 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_ADVDATA = 0x0010 | XCLASS_FLAGS;
        public const int XTYP_ADVREQ = 0x0020 | XCLASS_DATA | XTYPF_NOBLOCK;
        public const int XTYP_ADVSTART = 0x0030 | XCLASS_BOOL;
        public const int XTYP_ADVSTOP = 0x0040 | XCLASS_NOTIFICATION;
        public const int XTYP_EXECUTE = 0x0050 | XCLASS_FLAGS;
        public const int XTYP_CONNECT = 0x0060 | XCLASS_BOOL | XTYPF_NOBLOCK;
        public const int XTYP_CONNECT_CONFIRM = 0x0070 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_XACT_COMPLETE = 0x0080 | XCLASS_NOTIFICATION;
        public const int XTYP_POKE = 0x0090 | XCLASS_FLAGS;
        public const int XTYP_REGISTER = 0x00A0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_REQUEST = 0x00B0 | XCLASS_DATA;
        public const int XTYP_DISCONNECT = 0x00C0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_UNREGISTER = 0x00D0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_WILDCONNECT = 0x00E0 | XCLASS_DATA | XTYPF_NOBLOCK;
        public const int XTYP_MONITOR = 0x00F0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK;
        public const int XTYP_MASK = 0x00F0;
        public const int XTYP_SHIFT = 0x0004;

        //DDE таймаут
        public const int TIMEOUT_ASYNC = unchecked((int)0xFFFFFFFF);

        //Флаги команды (флаг инициализации)
        public const int APPCMD_CLIENTONLY = 0x00000010;

        //Флаги кодовых страниц
        public const int CP_WINANSI = 1004;
        public const int CP_WINUNICODE = 1200;

        [DllImport("user32.dll", EntryPoint = "DdeAccessData", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeAccessData(IntPtr hData, ref int pcbDataSize);

        [DllImport("user32.dll", EntryPoint = "DdeClientTransaction", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeClientTransaction(
            IntPtr pData, int cbData, IntPtr hConv, IntPtr hszItem, int wFmt, int wType, int dwTimeout,
            ref int pdwResult);

        [DllImport("user32.dll", EntryPoint = "DdeConnect", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeConnect(int idInst, IntPtr hszService, IntPtr hszTopic, IntPtr pCC);

        [DllImport("user32.dll", EntryPoint = "DdeCreateStringHandle", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeCreateStringHandle(int idInst, string psz, int iCodePage);

        [DllImport("user32.dll", EntryPoint = "DdeDisconnect", CharSet = CharSet.Ansi)]
        public static extern bool DdeDisconnect(IntPtr hConv);

        [DllImport("user32.dll", EntryPoint = "DdeFreeDataHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeFreeDataHandle(IntPtr hData);

        [DllImport("user32.dll", EntryPoint = "DdeFreeStringHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeFreeStringHandle(int idInst, IntPtr hsz);

        [DllImport("user32.dll", EntryPoint = "DdeGetLastError", CharSet = CharSet.Ansi)]
        public static extern int DdeGetLastError(int idInst);

        [DllImport("user32.dll", EntryPoint = "DdeInitialize", CharSet = CharSet.Ansi)]
        public static extern int DdeInitialize(ref int pidInst, DdeCallback pfnCallback, int afCmd, int ulRes);

        [DllImport("user32.dll", EntryPoint = "DdeQueryString", CharSet = CharSet.Ansi)]
        public static extern int DdeQueryString(int idInst, IntPtr hsz, StringBuilder psz, int cchMax,
            int iCodePage);

        [DllImport("user32.dll", EntryPoint = "DdeUnaccessData", CharSet = CharSet.Ansi)]
        public static extern bool DdeUnaccessData(IntPtr hData);

        [DllImport("user32.dll", EntryPoint = "DdeUninitialize", CharSet = CharSet.Ansi)]
        public static extern bool DdeUninitialize(int idInst);
    }
}
