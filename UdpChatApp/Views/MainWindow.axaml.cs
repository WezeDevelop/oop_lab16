// MainWindow.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;


namespace UdpChatApp.Views
{
    public partial class MainWindow : Window
    {
        private TextBox userNameTextBox, messageTextBox, chatTextBox;
        private Button loginButton, logoutButton, sendButton, settingsButton;

        private string userName = "";
        private string serverIp = "127.0.0.1";
        private int serverPort = 12345;

        private UdpClient udpClient;
        private bool running = false;
        private Thread receiveThread;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            AttachControls();
        }

        private void AttachControls()
        {
            userNameTextBox = this.FindControl<TextBox>("UserNameTextBox");
            messageTextBox = this.FindControl<TextBox>("MessageTextBox");
            chatTextBox = this.FindControl<TextBox>("ChatTextBox");

            loginButton = this.FindControl<Button>("LoginButton");
            logoutButton = this.FindControl<Button>("LogoutButton");
            sendButton = this.FindControl<Button>("SendButton");
            settingsButton = this.FindControl<Button>("SettingsButton");

            loginButton.Click += (_, _) => Login();
            logoutButton.Click += (_, _) => Logout();
            sendButton.Click += (_, _) => SendMessage();
            settingsButton.Click += (_, _) => ConfigureSettings();

            logoutButton.IsEnabled = false;
            sendButton.IsEnabled = false;
        }

        private void Login()
        {
            userName = userNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName)) return;

            udpClient = new UdpClient();
            running = true;
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            loginButton.IsEnabled = false;
            logoutButton.IsEnabled = true;
            sendButton.IsEnabled = true;

            AppendToChat($"[{userName} приєднався до чату]");
        }

        private void Logout()
        {
            running = false;
            udpClient?.Close();
            receiveThread?.Join();

            AppendToChat($"[{userName} вийшов з чату]");

            loginButton.IsEnabled = true;
            logoutButton.IsEnabled = false;
            sendButton.IsEnabled = false;
        }

        private void SendMessage()
        {
            var message = messageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                var fullMessage = $"{userName}: {message}";
                var bytes = Encoding.UTF8.GetBytes(fullMessage);
                udpClient.Send(bytes, bytes.Length, serverIp, serverPort);
                messageTextBox.Text = "";
                LogToFile(fullMessage);
            }
        }

        private void ReceiveMessages()
        {
            var client = new UdpClient(((IPEndPoint)udpClient.Client.LocalEndPoint).Port);
            while (running)
            {
                try
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var bytes = client.Receive(ref remoteEP);
                    var message = Encoding.UTF8.GetString(bytes);
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        AppendToChat(message);
                        LogToFile(message);
                    });
                }
                catch { break; }
            }
        }

        private void AppendToChat(string message)
        {
            chatTextBox.Text += message + "\n";
        }

        private async void ConfigureSettings(object? sender, RoutedEventArgs e)
        {
            var ip = await PromptDialog.ShowDialog(this, "IP адреса", serverIp);


            if (!string.IsNullOrEmpty(ip)) serverIp = ip;

            var portStr = PromptDialog.Show("Порт", serverPort.ToString());
            if (int.TryParse(portStr, out int port)) serverPort = port;
        }

        private void LogToFile(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText("chat_log.txt", $"[{timestamp}] {message}\n");
        }
    }
}
