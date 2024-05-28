using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer = new byte[1024];

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverAddress = ServerAddressTextBox.Text;
            int port = 12345; // Укажите порт, используемый сервером

            try
            {
                _client = new TcpClient(serverAddress, port);
                _stream = _client.GetStream();
                string nickname = NickNameTextBox.Text;
                byte[] data = Encoding.ASCII.GetBytes($"CON|{nickname}");
                await _stream.WriteAsync(data, 0, data.Length);
                ServerAddressTextBox.IsEnabled = false;
                NickNameTextBox.IsEnabled = false;
                ActiveClientsBox.Items.Add("All");

                ChatTextBox.AppendText("Connected to the server.\n");

                // Начинаем асинхронное чтение данных от сервера
                await Task.Run(() => ReceiveMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || !_client.Connected)
            {
                MessageBox.Show("Not connected to the server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string message = MessageTextBox.Text;
            // TYPE|FROM|TO|MESSAGE
            byte[] data = Encoding.ASCII.GetBytes($"SMSG|{NickNameTextBox.Text}|{ActiveClientsBox.SelectedItem.ToString()}|{message}");

            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
                if(ActiveClientsBox.SelectedItem.ToString() != "All")
                {
                    ChatTextBox.AppendText($"You->{ActiveClientsBox.SelectedItem.ToString()}:{message}\n");
                }
                MessageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (_client.Connected)
                {
                    int bytesRead = _stream.Read(_buffer, 0, _buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(_buffer, 0, bytesRead);
                        string message_type = message.Split("|")[0];
                        switch(message_type)
                        {
                            case "CON":
                                Dispatcher.Invoke(() => {
                                    ChatTextBox.AppendText($"User connected {message.Split("|")[1]}\n");
                                    ActiveClientsBox.Items.Add(message.Split("|")[1]);
                                });
                                break;
                            case "RMSG":
                                Dispatcher.Invoke(() => {
                                    ChatTextBox.AppendText($"{message.Split("|")[1]}->You: {message.Split("|")[2]}\n");
                                });
                                break;
                            case "NCA":
                                string[] active_clients = message.Split("|")[1].Split(",");
                                foreach (string client in active_clients)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        ActiveClientsBox.Items.Add(client);
                                    });
                                }
                                break;
                            case "DISC":
                                Dispatcher.Invoke(() =>
                                {
                                    ChatTextBox.AppendText($"User disconnected {message.Split("|")[1]}\n");
                                    ActiveClientsBox.Items.Remove(message.Split("|")[1]);
                                });
                                break;
                            default:
                                Dispatcher.Invoke(() => {
                                    ChatTextBox.AppendText($"Unsupporeted message: {message}\n");
                                });
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _stream?.Close();
            _client?.Close();
            base.OnClosed(e);
        }
    }
}
