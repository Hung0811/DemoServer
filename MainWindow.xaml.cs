using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPEndPoint IP;
        private Socket server;
        private List<Socket> list;

        public MainWindow()
        {
            InitializeComponent();
            Connect();
        }
        public void Connect()
        {
            IP = new IPEndPoint(IPAddress.Any, 9090);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            list = new List<Socket>();

            server.Bind(IP);

            Thread thread = new Thread(() => {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        list.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex);
                }


            });
            thread.IsBackground = true;
            thread.Start();
        }

        public void Close(Socket client)
        {
            client.Close();
        }

        public void Send(Socket client)
        {
            if (tbMessage.Text != string.Empty)
            {
                client.Send(Serialize(tbMessage.Text));
                AddMessage(tbMessage.Text);
            }
        }

        public void AddMessage(string msg)
        {
            this.Dispatcher.Invoke(() => { lvMessages.Items.Add(msg); });
            
        }

        public void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string msg = (string)DeSerialize(data);
                    AddMessage(msg);
                }
            }
            catch( Exception ex)
            {
                //Close();
                MessageBox.Show("Error: " + ex);
            }
        }

        public byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter format = new BinaryFormatter();

            format.Serialize(stream, obj);
            return stream.ToArray();
        }

        public object DeSerialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter format = new BinaryFormatter();

            return format.Deserialize(stream);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            foreach (Socket client in list)
            {
                Send(client);
            }
            tbMessage.Clear();

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
