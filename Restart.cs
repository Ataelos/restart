using System;
using System.IO;
using GeniePlugin.Interfaces;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;

namespace RestartPlugin
{

    public class Restart : IPlugin
    {
        private bool _enabled = true;
        private IHost _host;
        private string _filePath;

        //## Value class for holding Restart Command
        public class RestartCommand
        {
            public string profile;
            public string command;
        }

        static void Main(string[] args)
        {
        }

        public void Initialize(IHost host)
        {
            this._host = host;

            this._filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Genie Client 3\Plugins\";

            RestartCommand restartCommand = this.readFile();
            if (restartCommand != null)
            {
                Thread t = new Thread(
                    () =>
                    {
                        Thread.Sleep(2000);
                        this._host.SendText("#connect " + restartCommand.profile);
                    }
                );
                t.Start();                
            }
        }

        public void Show()
        {
            
        }

        public void VariableChanged(string variable)
        {
        }

        public string ParseText(string text, string window)
        {
            if (text.Contains("Connected to dr.simutronics.net."))
            {
                this._host.SendText("#send 5 look");
            }
            if (text.StartsWith("All Rights Reserved"))
            {
                RestartCommand restartCommand = this.readFile();
                if (restartCommand != null)
                {
                    this._host.SendText("#send 10 " + restartCommand.command);
                    
                    try
                    {
                        File.Delete(this.getXmlFileName());
                    }
                    catch (System.Exception ex)
                    {
                        _host.EchoText("Error deleting Restart file: " + ex.Message);
                    }
                }
            }
            
            return text;
        }

        public string ParseInput(string text)
        {
            if (text.StartsWith("/restart"))
            {
                char[] delimeter = { ' ' };
                string[] parts = text.Split(delimeter);

                if (parts.Length >= 3)
                {
                    var commandWhenRestarted = new string[parts.Length - 2];
                    Array.Copy(parts, 2, commandWhenRestarted, 0, commandWhenRestarted.Length);

                    this._host.EchoText("Restart queued");
                    this._host.EchoText("Profile: " + parts[1]);
                    this._host.EchoText("Command: " + String.Join(" ", commandWhenRestarted));

                    this.saveFile(parts[1], String.Join(" ", commandWhenRestarted));

                    this._host.SendText("#script abort all");
                    this._host.SendText("exit");

                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                }
                else
                {
                    this._host.EchoText("Usage is /restart profileName command when restarted");
                }
                
                return "";
            }
            else
            {
                return text;
            }
        }
        
        public void ParseXML(string xml)
        {
        }

        public void ParentClosing()
        {
        }

        public string Name
        {
            get { return "Restart"; }
        }

        public string Version
        {
            get { return "1.0"; }
        }

        public string Description
        {
            get { return "Restart Genie and start script";  }
        }

        public string Author
        {
            get { return "UFTimmy @ AIM"; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private string getXmlFileName()
        {
            return _filePath + "Restart.xml";
        }

        private void saveFile(string profileName, string command)
        {
            RestartCommand restartCommand = new RestartCommand { profile = profileName, command = command };

            try
            {
                FileStream writer = new FileStream(this.getXmlFileName(), FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(RestartCommand));
                serializer.Serialize(writer, restartCommand);
                writer.Close();
            }
            catch (System.Exception ex)
            {
                _host.EchoText("Error writing Restart file: " + ex.Message);
            }
        }

        private RestartCommand readFile()
        {
            string fileName = this.getXmlFileName();
            if (File.Exists(fileName))
            {
                try
                {
                    using (Stream stream = File.Open(fileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(RestartCommand));
                        RestartCommand restartCommand = (RestartCommand)serializer.Deserialize(stream);
                        stream.Close();

                        return restartCommand;
                    }

                }
                catch (System.Exception ex)
                {
                    _host.EchoText("Error reading Restart file: " + ex.Message);
                }
            }

            return null;
        }
        
    }
}
