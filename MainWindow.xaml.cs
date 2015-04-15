using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace PythonLab
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string command;
        private string input = "";
        private int offset;
        private Process process;
        private BackgroundWorker pythonErrorReader;
        private BackgroundWorker pythonOutputReader;
        private TextDocument textDocument = new TextDocument();

        public TextDocument TextDocument
        {
            get { return this.textDocument; }

            set
            {
                if (value.Equals(this.textDocument))
                {
                    return;
                }

                this.textDocument = value;
                this.RaisePropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.process = new Process
            {
                StartInfo = new ProcessStartInfo(@"C:\Python34\python.exe", "-i")
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            this.process.Start();

            this.pythonErrorReader = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            this.pythonErrorReader.DoWork += this.pythonErrorReader_DoWork;

            this.pythonErrorReader.RunWorkerAsync();

            this.pythonOutputReader = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            this.pythonOutputReader.DoWork += this.pythonOutputReader_DoWork;

            this.pythonOutputReader.RunWorkerAsync();

            this.TextEditor.TextArea.ReadOnlySectionProvider = new TextSegmentReadOnlySectionProvider<TextSegment>(this.TextDocument);

            this.TextDocument.Changed -= TextDocument_Changed;
            this.TextDocument.Changed += TextDocument_Changed;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TextDocument_Changed(object sender, DocumentChangeEventArgs e)
        {
            var nLines = this.TextDocument.Lines.Count;

            var lastLineText = this.TextDocument.GetText(this.TextDocument.Lines[nLines - 1]);
            var lastButOneLineText = this.TextDocument.GetText(this.TextDocument.Lines[nLines - 2]);

            if (lastLineText == "" && lastButOneLineText.Contains(">>> "))
            {
                this.process.StandardInput.WriteLine(lastButOneLineText.Replace(">>> ", ""));
                this.offset = this.TextDocument.TextLength;
            }
        }

        private void TextEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
            }
        }

        private void pythonErrorReader_DoWork(object sender, DoWorkEventArgs e)
        {
            var output = "";

            while (true)
            {
                output += (char) this.process.StandardError.Read();

                if (output.EndsWith(">>> "))
                {
                    var output1 = output;
                    Dispatcher.BeginInvoke(new Action(() => this.TextDocument.Text += output1));
                    output = "";
                }
            }
        }

        private void pythonOutputReader_DoWork(object sender, DoWorkEventArgs e)
        {
            var output = "";

            while (true)
            {
                output += (char) this.process.StandardOutput.Read();

                if (output.Trim(" ".ToCharArray()).EndsWith("\r\n"))
                {
                    var output1 = output;
                    Dispatcher.BeginInvoke(new Action(() => this.TextDocument.Insert(this.offset, output1)));
                    output = "";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}