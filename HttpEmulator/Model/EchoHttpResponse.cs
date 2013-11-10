using System.IO;
using System.Net;

namespace HttpEmulator
{
    class EchoHttpResponse : HttpListenerBase
    {
        public EchoHttpResponse(int port) : base(port)
        {
        }

        protected override void HandleResponse(HttpListenerContext context)
        {
            base.HandleResponse(context);

            var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(this.RequestBody);
            writer.Flush();
            context.Response.OutputStream.Close();
        }
    }
}
