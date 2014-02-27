using System.IO;
using System.Net;

namespace HttpEmulator
{
    internal class EchoHttpResponse : HttpListenerBase
    {
        public EchoHttpResponse(int port) : base(port)
        {
        }

        protected override void HandleRequestInternal(HttpListenerContext context)
        {
            var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(this.RequestBody);
            writer.Flush();
            context.Response.OutputStream.Close();
        }
    }
}