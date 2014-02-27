using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
        public Authentication Authentication { get; set; }

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
            this.Listener = new HttpListener();
            this.Listener.Prefixes.Add(String.Format("http://*:{0}/", Port));
            this.HandleListenerAuthentication();
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
                var ctx = this.Listener.GetContext();
                var reader = new StreamReader(ctx.Request.InputStream);

                this.RequestBody = reader.ReadToEnd();
                this.Url = ctx.Request.Url.ToString();

                reader.Close();
                new Thread(ProcessRequest).Start(ctx);
            }
        }

        private void HandleListenerAuthentication()
        {
            if (this.Authentication == null || string.IsNullOrEmpty(this.Authentication.Username) ||
                this.Authentication.IsPreemptiveAuthentication)
            {
                return;
            }

            this.Listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
        }

        protected virtual void ProcessRequest(object ctx)
        {
            var context= ctx as HttpListenerContext;

            try
            {
                InvokeOnRequestReceived(context);
                HandleRequest(context);
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

        protected virtual void HandleRequest(HttpListenerContext context)
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

            if (this.HandleRequestAuthentication(context))
            {
                context.Response.StatusCode = this.StatusCode;
                this.HandleRequestInternal(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.OutputStream.Close();
            }
        }
        
        protected abstract void HandleRequestInternal(HttpListenerContext context);

        protected virtual bool HandleRequestAuthentication(HttpListenerContext context)
        {
            if (this.Authentication == null || string.IsNullOrEmpty(this.Authentication.Username))
            {
                return true;
            }

            string username = null, password = null;

            if (this.Authentication.IsPreemptiveAuthentication)
            {
                var authorization = context.Request.Headers.Get("Authorization");
                if(authorization == null)
                    return false;

                var hashsedValue = authorization.Split(' ')[1];
                var usernamePasswordArray = Encoding.UTF8.GetString(Convert.FromBase64String(hashsedValue)).Split(':');
                username = usernamePasswordArray[0];
                password = usernamePasswordArray[1];
            }

            else
            {
                if (context.User == null || !context.User.Identity.IsAuthenticated)
                {
                    return false;
                }

                var identity = (HttpListenerBasicIdentity) context.User.Identity;
                username = identity.Name;
                password = identity.Password;
            }

            return this.Authentication.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) &&
                   this.Authentication.Password.Equals(password, StringComparison.InvariantCulture);
        }

        #endregion
    }
}