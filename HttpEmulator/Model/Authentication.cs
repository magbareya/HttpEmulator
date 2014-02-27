using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpEmulator
{
    public class Authentication
    {
        private string password;
        private string username;

        public string Username
        {
            get { return this.username ?? string.Empty; }
            set { this.username = value; }
        }

        public string Password
        {
            get { return this.password ?? string.Empty; }
            set { this.password = value; }
        }

        public bool IsPreemptiveAuthentication { get; set; }
    }
}
