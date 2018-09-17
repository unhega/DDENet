using System;
using System.Threading;
using System.Collections.Generic;
using DDENetStandart.DDEML;

namespace DDENetStandart
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new DDEMLClient("TESTDDE", "mytopic");
        }
    }
}
