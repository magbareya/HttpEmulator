using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using HttpEmulator.Model;
using System.Windows;

namespace HttpEmulator
{

    internal enum ListenerType
    {
        Echo,
        Fault,
        Fixed,
        Empty
    }

    public class HttpEmulatorViewModel : INotifyPropertyChanged
    {

        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region constants

        internal const string Bar = "=========================================================";
        internal const string StartListening = "A new {0} listener was started with address: http://localhost:{1} - {2}";
        internal const string StopListening = "The {0} listener on http://localhost:{1} was stopped - {2}";
        internal const int DefaultPort = 8090;

        #endregion

        #region private fields

        private HttpListenerBase _listener;
        private List<string> _statusCodes;
        private ListenerType CurrentListenerType { get; set; }
        private Dictionary<string, PreDefinedFixedBody> _preDefinedFixedBodies;
        private AdvancedFormViewModel _advancedSettings;
        private string _hostAddress;
        private string _fixedBody;
        private string _rawHeaders;
        private string _logText;
        private bool _isEchoChecked;
        private bool _isEmptyChecked;
        private bool _isFaultChecked;
        private bool _isFixedChecked;
        private bool _isWrapChecked;
        private int _selectedFixedBodyIndex;
        private int _selectedStatusCodeIndex;

        #endregion

        #region Constructor

        public HttpEmulatorViewModel()
        {
            this.RawHeaders = "key : value";
            this.HostAddress = "8090";
            this.IsEmptyChecked = true;
            this.IsWrapChecked = true;
            this.SelectedStatusCodeIndex = 2;
            this.LogText = string.Empty;
            this._preDefinedFixedBodies = Utils.GetFixedBodiesFromConfig();
        }

        #endregion

        #region View Properties

        public AdvancedFormViewModel AdvancedSettings
        {
            get { return this._advancedSettings; }
            set { this._advancedSettings = value; }
        }

        public string HostAddress
        {
            get { return this._hostAddress; }
            set
            {
                if (value != this._hostAddress)
                {
                    this._hostAddress = value;
                    this.InvokePropertyChanged("HostAddress");
                }
            }
        }

        public IList<string> StatusCodes
        {
            get
            {
                if (this._statusCodes == null)
                {
                    _statusCodes = new List<string>();
                    var codes = Enum.GetValues(typeof (HttpStatusCode));
                    foreach (var code in codes)
                        _statusCodes.Add(string.Format("{0} - {1}", (int) code, code));
                }
                return this._statusCodes;
            }
        }

        public IList<string> PreDefinedFixedBodies
        {
            get
            {
                if (this._preDefinedFixedBodies == null)
                {
                    this._preDefinedFixedBodies = Utils.GetFixedBodiesFromConfig();
                }
                return this._preDefinedFixedBodies.Keys.ToList();
            }
        }

        public int SelectedStatusCodeIndex
        {
            get { return this._selectedStatusCodeIndex; }
            set
            {
                if (this._selectedStatusCodeIndex != value)
                {
                    this._selectedStatusCodeIndex = value;
                    this.InvokePropertyChanged("SelectedStatusCodeIndex");
                }
            }
        }

        public int SelectedFixedBodyIndex
        {
            get { return this._selectedFixedBodyIndex; }
            set
            {
                if (
                    this._selectedFixedBodyIndex != value)
                {
                    this._selectedFixedBodyIndex = value;
                    this.InvokePropertyChanged("SelectedFixedBodyIndex");
                }
            }
        }


        public string FixedBody
        {
            get { return this._fixedBody; }
            set
            {
                if (value != this._fixedBody)
                {
                    this._fixedBody = value;
                    this.InvokePropertyChanged("FixedBody");
                }
            }
        }

        public string RawHeaders
        {
            get { return this._rawHeaders; }
            set
            {
                if (this._rawHeaders != value)
                {
                    this._rawHeaders = value;
                    this.InvokePropertyChanged("RawHeaders");
                    this.InvokePropertyChanged("IsStatusCodeEnabled");
                }
            }
        }

        public bool IsEchoChecked
        {
            get { return this._isEchoChecked; }
            set
            {
                if (this._isEchoChecked != value)
                {
                    this._isEchoChecked = value;
                    this.InvokePropertyChanged("IsEchoChecked");
                }
            }
        }

        public bool IsEmptyChecked
        {
            get { return this._isEmptyChecked; }
            set
            {
                if (this._isEmptyChecked != value)
                {
                    this._isEmptyChecked = value;
                    this.InvokePropertyChanged("IsEmptyChecked");
                }
            }
        }

        public bool IsFaultChecked
        {
            get { return this._isFaultChecked; }
            set
            {
                if (this._isFaultChecked != value)
                {
                    this._isFaultChecked = value;
                    this.InvokePropertyChanged("IsFaultChecked");
                }
            }
        }

        public bool IsFixedChecked
        {
            get { return this._isFixedChecked; }
            set
            {
                if (this._isFixedChecked != value)
                {
                    this._isFixedChecked = value;
                    this.InvokePropertyChanged("IsFixedChecked");
                    this.InvokePropertyChanged("IsStatusCodeEnabled");
                    this.InvokePropertyChanged("IsHeadersTextBoxEnabled");
                }
            }
        }

        public bool IsWrapChecked
        {
            get { return this._isWrapChecked; }
            set
            {
                if (this._isWrapChecked != value)
                {
                    this._isWrapChecked = value;
                    this.InvokePropertyChanged("IsWrapChecked");
                    this.InvokePropertyChanged("FixedBodyWrapping");
                }
            }
        }

        public bool IsStatusCodeEnabled
        {
            get { return this.IsEchoChecked | this.IsFixedChecked; }
        }

        public bool IsHeadersTextBoxEnabled
        {
            get { return this.IsFixedChecked; }
        }

        public TextWrapping FixedBodyWrapping
        {
            get { return this.IsWrapChecked ? TextWrapping.Wrap : TextWrapping.NoWrap; }
        }

        public string LogText
        {
            get { return this._logText; }
            set
            {
                if (this._logText != value)
                {
                    this._logText = value;
                    this.InvokePropertyChanged("LogText");
                }
            }
        }

        public bool IsListenerStarted
        {
            get { return this._listener != null; }
        }

        #endregion

        #region Button Commands handlers

        public void OnStartClick()
        {
            if (this._listener != null && this._listener.IsListening)
                OnStopListener();

            var listenerType = GetCheckedListenerType();

            StartNewListener(listenerType);
            this.InvokePropertyChanged("IsListenerStarted");
        }

        public void OnStopListener()
        {
            if (this._listener != null)
            {
                LogListenerStopped();
                _listener.Stop();
            }
            _listener = null;
            this.InvokePropertyChanged("IsListenerStarted");
        }

        internal void OnSaveToFileClick()
        {
            var dlg = new SaveFileDialog
                {
                    FileName = "LogContent",
                    DefaultExt = ".txt",
                    Filter = "Text documents (*.txt)|*.txt|All Files (*.*)|*.*"
                };

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            string filename = string.Empty;
            if (result == true)
            {
                // Save document
                filename = dlg.FileName;
            }

            if (!string.IsNullOrEmpty(filename))
            {
                File.WriteAllText(filename, this.LogText);
            }

        }

        internal void OnLoadBodyFromFileClick()
        {
            var dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            string fileName = string.Empty;
            if (result == true)
            {
                fileName = dlg.FileName;
            }

            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                this.FixedBody = File.ReadAllText(fileName);
                this.IndentFixedBody();
            }
        }

        public void OnLoadSavedResponseClick()
        {
            var fixedBody = this._preDefinedFixedBodies.ElementAt(this.SelectedFixedBodyIndex).Value;
            this.FixedBody = fixedBody.Body;
            this.StatusCode = fixedBody.StatusCode;
            this.IndentFixedBody();
        }

        private void OnAdvancedClick()
        {
            var advancedDlg = new AdvancedForm
                {
                    DataContext =
                        this.AdvancedSettings != null ? this.AdvancedSettings.Clone() : new AdvancedFormViewModel()
                };
            if (advancedDlg.ShowDialog() == true)
            {
                this.AdvancedSettings = advancedDlg.DataContext as AdvancedFormViewModel;
            }
        }

        #endregion

        #region private functions

        private ListenerType GetCheckedListenerType()
        {
            if (this.IsEchoChecked)
                return ListenerType.Echo;

            if (this.IsFaultChecked)
                return ListenerType.Fault;
            if (this.IsEmptyChecked)
                return ListenerType.Empty;
            return ListenerType.Fixed;
        }

        private int GetPort()
        {
            int port;
            return int.TryParse(this.HostAddress, out port) ? port : DefaultPort;
        }

        private void AddHttpHeadersToListener(HttpListenerBase listener)
        {
            foreach (string header in this.RawHeaders.Split(
                new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries))
            {
                var items = header.Split(':');
                listener.Headers.Add(items[0].Trim(), items[1].Trim());
            }
        }

        private void StartNewListener(ListenerType listenerType)
        {
            HttpListenerBase listener;
            switch (listenerType)
            {
                case ListenerType.Echo:
                    listener = new EchoHttpResponse(this.GetPort());
                    break;

                case ListenerType.Fault:
                    listener = new FaultHttpResponse(this.GetPort());
                    break;

                case ListenerType.Fixed:
                    {
                        listener = new FixedHttpResponse(GetPort());
                        AddHttpHeadersToListener(listener);
                        ((FixedHttpResponse) listener).Content = this.FixedBody;
                        break;
                    }

                case ListenerType.Empty:
                default:
                    listener = new EmptyResponse(this.GetPort());
                    break;
            }

            listener.StatusCode = StatusCode;
            listener.Authentication = Utils.CreateAuthentication(this.AdvancedSettings);

            if (!listener.Headers.ContainsKey("Content-Type"))
                listener.Headers.Add("Content-Type", "text/xml");

            listener.OnRequestReceived += WriteRequestToLog;

            this._listener = listener;
            this._listener.OnError += OnErrorOccured;
            this._listener.Start();
            this.CurrentListenerType = listenerType;
            LogListenerStarted();
        }

        

        private void OnErrorOccured(Exception e)
        {
            var portNum = this.GetPort();
            var portInUseMsg =
                string.Format(
                    @"Failed to listen on prefix 'http://*:{0}/' because it conflicts with an existing registration on the machine.",
                    portNum);

            var exceptionMessage = e.Message.ToLowerInvariant().Equals(portInUseMsg.ToLowerInvariant())
                                       ? string.Format("The port {0} is already in use, please choose another port",
                                                       portNum)
                                       : e.Message;
            MessageBox.Show(string.Format("Error: {0}", exceptionMessage));
            OnStopListener();
        }

        public int StatusCode
        {
            get
            {
                string selectedCode = this._statusCodes.ElementAt(this.SelectedStatusCodeIndex);
                string portNum = selectedCode.Substring(0, 3);
                return int.Parse(portNum);
            }
            set
            {
                var key = this._statusCodes.FirstOrDefault(s => s.Contains(value.ToString()));
                if (!string.IsNullOrEmpty(key) && this._statusCodes.Contains(key))
                    this.SelectedStatusCodeIndex = this._statusCodes.IndexOf(key);
            }
        }

        #endregion

        #region log functions

        private void WriteRequestToLog(object sender, string content, SortedList<String, String> headers)
        {
            var listener = sender as HttpListenerBase;
            var builder = new StringBuilder(this.LogText);
            builder.AppendLine();

            builder.AppendLine(string.Format("Request recieved at: {0}", DateTime.Now.ToShortTimeString()));

            builder.AppendLine(string.Format("Request URL: {0}", listener.Url));

            builder.AppendLine("Headers:");
            foreach (var pair in headers)
                builder.AppendLine(string.Format("   {0} : {1}", pair.Key, pair.Value));

            builder.AppendLine("Request Body: ");
            using (var stringReader = new StringReader(Utils.TryFormatText(content)))
            {
                for (string line = stringReader.ReadLine(); line != null; line = stringReader.ReadLine())
                {
                    builder.AppendLine(string.Format("   {0}", line));
                }
            }

            builder.AppendLine();
            this.LogText = builder.ToString();
            builder.Clear();
        }

        private void LogListenerStarted()
        {
            if (this._listener == null)
                return;
            var builder = new StringBuilder(this.LogText);
            builder.AppendLine();
            builder.AppendLine(string.Format(StartListening, this.CurrentListenerType.ToString(),
                                             this._listener.RunningPort,
                                             DateTime.Now.ToShortTimeString()));
            this.LogText = builder.ToString();
            builder.Clear();
        }

        private void LogListenerStopped()
        {
            var builder = new StringBuilder(this.LogText);
            builder.AppendLine();
            builder.AppendLine(string.Format(StopListening, this.CurrentListenerType.ToString(),
                                             this._listener.RunningPort,
                                             DateTime.Now.ToShortTimeString()));
            builder.AppendLine(Bar);

            this.LogText = builder.ToString();
            builder.Clear();
        }

        private void IndentFixedBody()
        {
            try
            {
                this.FixedBody = Utils.TryFormatText(this.FixedBody);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Invoke Events

        protected virtual void InvokePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        #endregion

        #region Commands

        public ICommand ClearTextBodyCommand
        {
            get { return Utils.CreateCommand(() => this.FixedBody = string.Empty); }
        }

        public ICommand ClearLogCommand
        {
            get { return Utils.CreateCommand(() => this.LogText = string.Empty); }
        }

        public ICommand OpenAdvancedCommand
        {
            get { return Utils.CreateCommand(this.OnAdvancedClick); }
        }

        public ICommand StartCommand
        {
            get { return Utils.CreateCommand(this.OnStartClick); }
        }

        public ICommand StopCommand
        {
            get { return Utils.CreateCommand(this.OnStopListener); }
        }

        public ICommand SaveToFileCommand
        {
            get { return Utils.CreateCommand(this.OnSaveToFileClick); }
        }

        public ICommand LoadBodyFromFileCommand
        {
            get { return Utils.CreateCommand(this.OnLoadBodyFromFileClick); }
        }

        public ICommand IndentCommand
        {
            get { return Utils.CreateCommand(this.IndentFixedBody); }
        }

        public ICommand LoadSavedResponseCommand
        {
            get { return Utils.CreateCommand(this.OnLoadSavedResponseClick); }
        }

        #endregion
    }
}