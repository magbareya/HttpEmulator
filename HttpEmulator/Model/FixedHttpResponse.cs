using System.IO;
using System.Net;

namespace HttpEmulator
{
    internal class FixedHttpResponse : HttpListenerBase
    {
        public string Content { get; internal set; }

        public FixedHttpResponse(int port)
            : base(port)
        {
        }

        protected override void HandleResponse(HttpListenerContext context)
        {
            base.HandleResponse(context);

            var writer = new StreamWriter(context.Response.OutputStream);
            writer.WriteLine(this.Content);
            writer.Flush();
            writer.Close();
            context.Response.OutputStream.Close();
        }
    }
}