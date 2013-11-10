using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpEmulator
{
    public class PreDefinedFixedBody
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> HttpHeaders { get; set; }
    }
}
