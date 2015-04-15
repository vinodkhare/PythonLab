using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;

namespace PythonLab
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private ObservableCollection<string> history = new ObservableCollection<string>();
        private int offset;
        private TextDocument textDocument = new TextDocument();

        public ObservableCollection<string> History
        {
            get { return this.history; }

            set
            {
                if (value.Equals(this.history))
                {
                    return;
                }

                this.history = value;
                this.RaisePropertyChanged();
            }
        }

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

        private void Instance_ErrorReceived(object sender, string error)
        {
            Dispatcher.BeginInvoke(new Action(() => this.TextDocument.Text += error));
        }

        private void Instance_OutputReceived(object sender, string output)
        {
            Dispatcher.BeginInvoke(new Action(() => this.TextDocument.Insert(this.offset, output)));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PythonProcess.Instance.ErrorReceived -= Instance_ErrorReceived;
            PythonProcess.Instance.ErrorReceived += Instance_ErrorReceived;

            PythonProcess.Instance.OutputReceived -= Instance_OutputReceived;
            PythonProcess.Instance.OutputReceived += Instance_OutputReceived;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TextEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                var lastLine = this.TextDocument.Lines.Last();

                this.TextDocument.Remove(lastLine.Offset, lastLine.Length);

                this.TextDocument.Text += PythonProcess.Prompt + this.History.Last();

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                var command = this.TextDocument.GetText(this.TextDocument.Lines.Last()).Replace(PythonProcess.Prompt, "");

                this.history.Add(command);

                PythonProcess.Instance.Run(command);

                this.offset = this.TextDocument.TextLength + 1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}