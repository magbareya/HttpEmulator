using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpEmulator.Model
{
    internal class EmptyResponse : HttpListenerBase
    {
        public EmptyResponse(int port) : base(port)
        {
        }

        protected override void HandleRequestInternal(System.Net.HttpListenerContext context)
        {
            context.Response.OutputStream.Close();
        }
    }
}