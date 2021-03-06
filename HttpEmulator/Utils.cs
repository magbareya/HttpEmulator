﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace HttpEmulator
{
    public static class Utils
    {
        public static string TryFormatText(string str)
        {
            try
            {
                if (TryFormatStringAsXml(ref str))
                    return str;

                TryFormatStringAsJson(ref str);
                return str;
            }
            catch (Exception)
            {
                return str;
            }
        }

        public static bool TryFormatStringAsXml(ref string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var sb = new StringBuilder();
                using (var tr = new StringWriter(sb))
                {
                    var xwsettings = new XmlWriterSettings
                        {
                            Indent = true,
                            OmitXmlDeclaration = true,
                            Encoding = Encoding.UTF8,
                            NewLineChars = Environment.NewLine,
                            NewLineHandling = NewLineHandling.Replace
                        };
                    using (var wr = XmlWriter.Create(tr, xwsettings))
                    {
                        doc.Save(wr);
                        xml = sb.ToString();
                    }
                }
                sb.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool TryFormatStringAsJson(ref string json)
        {
            try
            {
                var obj = JObject.Parse(json);
                json = obj.ToString();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Dictionary<string, PreDefinedFixedBody> GetFixedBodiesFromConfig()
        {
            var responses = new Dictionary<string, PreDefinedFixedBody>();
            try
            {
                var doc = new XmlDocument();
                var filePath = ConfigurationManager.AppSettings["FixedBodiesFilePath"];

                if (!File.Exists(filePath))
                    return responses;

                doc.Load(filePath);
                var fixedBodyNodes = doc.SelectNodes(@"FixedBodies/FixedBody");
                if (fixedBodyNodes == null)
                    return responses;

                foreach (XmlNode fixedBodyNode in fixedBodyNodes)
                {
                    var fixedBody = new PreDefinedFixedBody();
                    fixedBody.Name = fixedBodyNode.Attributes["name"].Value;
                    fixedBody.Body = fixedBodyNode.InnerXml.TrimStart().TrimEnd();
                    if (fixedBodyNode.Attributes["statusCode"] != null)
                    {
                        fixedBody.StatusCode = int.Parse(fixedBodyNode.Attributes["statusCode"].Value);
                    }
                    else fixedBody.StatusCode = 200;
                    //TODO: headers
                    responses.Add(fixedBody.Name, fixedBody);
                }
            }
            catch (Exception)
            {
            }
            return responses;
        }

        public static Authentication CreateAuthentication(AdvancedFormViewModel advanced)
        {
            if (advanced == null || !advanced.UseAuthentication || string.IsNullOrEmpty(advanced.Username))
                return null;

            return new Authentication
                {
                    Username = advanced.Username,
                    Password = advanced.Password,
                    IsPreemptiveAuthentication = advanced.IsPreemptiveAuthentication
                };
        }

        public static ICommand CreateCommand(Action executeAction)
        {
            return new GeneralCommand(executeAction);
        }

        private class GeneralCommand : ICommand
        {
            private readonly Action _executeDelegate;

            public GeneralCommand(Action executeDelegate)
            {
                this._executeDelegate = executeDelegate;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                this._executeDelegate();
            }
        }

        public static int GetDelayTimeInMilliseconds(AdvancedFormViewModel advancedFormViewModel)
        {
            double d = 0;
            if (advancedFormViewModel == null || double.TryParse(advancedFormViewModel.DelayTimeString, out d))
            {
                return (int) d;
            }
            else throw new Exception("The Delay string wasn't a valid number");
        }
    }
}