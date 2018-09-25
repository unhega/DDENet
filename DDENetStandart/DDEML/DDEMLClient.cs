using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DDENetStandart.DDEML;

namespace DDENetStandart.DDEML
{
    public class DDEMLClient:IDisposable
    {
        
        
        private DDEMLContext context;
        
        public DDEMLClient(string service, string topic)
        {
            context = new DDEMLContext(this);
            context.Connect(service, topic);
        }

        public void Dispose()
        {
            context.Close();
        }

        public void OnAdvise(string data)
        {
            Console.WriteLine(data);
        }

        


    }
}
