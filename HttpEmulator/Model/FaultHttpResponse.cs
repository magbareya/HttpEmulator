
namespace HttpEmulator
{
    class FaultHttpResponse : HttpListenerBase

    {
        public FaultHttpResponse(int port) : base(port)
        {
        }

        protected override void HandleResponse(System.Net.HttpListenerContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.OutputStream.Close();
        }
    }
}
