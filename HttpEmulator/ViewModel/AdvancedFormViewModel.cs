using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HttpEmulator
{
    public class AdvancedFormViewModel : INotifyPropertyChanged
    {
        public AdvancedFormViewModel()
        {
            this.UseAuthentication = false;
        }

        #region Fields

        private string _username;
        private string _password;
        private bool _useAuthentication;
        private bool _isPreemptiveAuthentication;

        #endregion

        #region Properties

        public string Username
        {
            get { return this._username; }
            set
            {
                if (value != this._username)
                {
                    this._username = value;
                    this.InvokePropertyChanged("Username");
                }
            }
        }

        public string Password
        {
            get { return this._password; }
            set
            {
                if (value != this._password)
                {
                    this._password = value;
                    this.InvokePropertyChanged("Password");
                }
            }
        }

        public bool UseAuthentication
        {
            get { return this._useAuthentication; }
            set
            {
                if (value != this._useAuthentication)
                {
                    this._useAuthentication = value;
                    this.InvokePropertyChanged("UseAuthentication");
                }
            }
        }

        public bool IsPreemptiveAuthentication
        {
            get { return this._isPreemptiveAuthentication; }
            set
            {
                if (value != this._isPreemptiveAuthentication)
                {
                    this._isPreemptiveAuthentication = value;
                    this.InvokePropertyChanged("IsPreemptiveAuthentication");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Methods

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void InvokePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        #endregion

        public AdvancedFormViewModel Clone()
        {
            return new AdvancedFormViewModel()
                {
                    IsPreemptiveAuthentication = this.IsPreemptiveAuthentication,
                    Password = this.Password,
                    Username = this.Username,
                    UseAuthentication = this.UseAuthentication
                };
        }
    }
}