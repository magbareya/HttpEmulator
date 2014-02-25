using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace HttpEmulator
{
    public abstract class HttpListenerBase
    {
        public delegate void RequestReceived(object sender, string content, SortedList<String, String> headers);

        public delegate void ErrorOccured(Exception e);

        public event RequestReceived OnRequestReceived;
        public event ErrorOccured OnError;

        #region Properties

        public SortedList<string, string> Headers { get; internal set; }
        public int Port { get; internal set; }
        public int StatusCode { get; set; }
        public string RequestBody { get; set; }
        public int RunningPort { get; set; }
        public string Url { get; set; }

        public bool IsListening
        {
            get { return this.ListenerThread.IsAlive; }
        }

        #endregion

        #region private fields

        protected Thread ListenerThread;
        protected HttpListener Listener;

        #endregion

        #region constructors

        protected HttpListenerBase(int port)
        {
            this.Port = port;
            this.Headers = new SortedList<string, string>();
            this.StatusCode = 200;
        }

        #endregion

        #region public functions

        public void Start()
        {
            if (this.ListenerThread != null)
                throw new Exception("Service is already started");
            this.ListenerThread = new Thread(ThreadStart);
            this.RunningPort = this.Port;
            this.ListenerThread.Start();
        }

        public void Stop()
        {
            if (this.ListenerThread != null)
                this.ListenerThread.Abort();
            this.ListenerThread = null;

            if (this.Listener != null)
                this.Listener.Abort();
            this.Listener = null;

            this.RunningPort = 0;
        }

        #endregion

        #region protected functions

        protected virtual void ThreadStart()
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(String.Format("http://*:{0}/", Port));

            try
            {
                Listener.Start();
            }
            catch (Exception e)
            {
                if (this.OnError != null)
                    OnError(e);
                return;
            }

            while (true)
            {
                var ctx = Listener.GetContext();
                var reader = new StreamReader(ctx.Request.InputStream);

                this.RequestBody = reader.ReadToEnd();
                this.Url = ctx.Request.Url.ToString();

                reader.Close();
                new Thread(ProcessRequest).Start(ctx);
            }
        }

        protected virtual void ProcessRequest(object ctx)
        {
            var context= ctx as HttpListenerContext;

            try
            {
                InvokeOnRequestReceived(context);
                HandleResponse(context);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {
                    if (context.Response != null)
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                catch (Exception)
                {
                }
                Thread.CurrentThread.Abort();
            }
        }

        protected virtual void InvokeOnRequestReceived(HttpListenerContext context)
        {
            var headers = new SortedList<string, string>();
            foreach (string h in context.Request.Headers)
                headers.Add(h, context.Request.Headers[h]);

            if (this.OnRequestReceived != null)
                this.OnRequestReceived(this, this.RequestBody, headers);
        }

        protected virtual void HandleResponse(HttpListenerContext context)
        {
            foreach (var h in this.Headers)
            {
                if (h.Key == "Content-Type")
                {
                    context.Response.ContentType = h.Value;
                }
                else if (h.Key == " Keep-Alive")
                {
                    bool val;
                    if (!bool.TryParse(h.Value, out val))
                    {
                        val = true;
                    }
                    context.Response.KeepAlive = val;
                }
                else
                {
                    context.Response.Headers.Add(h.Key, h.Value);
                }
            }
            context.Response.StatusCode = this.StatusCode;
        }

        #endregion
    }
}