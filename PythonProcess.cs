using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PythonLab
{
    public class PythonProcess
    {
        public const string Prompt = ">>> ";

        public static readonly PythonProcess Instance = new PythonProcess();
        
        private readonly Process process = new Process();

        private PythonProcess()
        {
            this.process.StartInfo = new ProcessStartInfo(@"C:\Python34\python.exe", "-i")
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            this.process.Start();

            this.StartReading(this.process.StandardError, ">>> ", this.RaiseErrorReceived);
            this.StartReading(this.process.StandardOutput, "\r\n", this.RaiseOutputReceived);

            Console.WriteLine("Python process started.");
        }

        public void Run(string command)
        {
            this.process.StandardInput.WriteLine(command);
        }

        private void RaiseErrorReceived(string str)
        {
            if (this.ErrorReceived != null)
            {
                this.ErrorReceived(this, str);
            }
        }

        private void RaiseOutputReceived(string str)
        {
            if (this.OutputReceived != null)
            {
                this.OutputReceived(this, str);
            }
        }

        private void StartReading(StreamReader streamReader, string prompt, Action<string> action)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (o, e) =>
            {
                var output = "";

                while (!this.process.HasExited)
                {
                    output += (char) streamReader.Read();

                    if (output.EndsWith(prompt))
                    {
                        action(output);
                        output = "";
                    }
                }
            };

            backgroundWorker.RunWorkerAsync();
        }

        public event EventHandler<string> ErrorReceived;

        public event EventHandler<string> OutputReceived;
    }
}