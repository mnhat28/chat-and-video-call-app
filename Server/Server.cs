using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        private TcpListener listener;
        //  private int port = 5555;
        private List<TcpClient> clients = new List<TcpClient>(); // Danh sách client kết nối
        private const int maxClients = 4; // Số lượng kết nối tối đa
        private List<Taikhoan> taikhoans; // Danh sách tài khoản
        private bool isRunning = false;  // Biến cờ để kiểm soát luồng
        private Thread acceptClientThread; // Luồng lắng nghe kết nối client
        private Thread handleMessageThread; // Luồng lắng nghe tin nhắn
    //    private List<string> loggedInUsers = new List<string>();
        public Server()
        {
            InitializeComponent();
            KhoiTaoTaiKhoan();
        }

        private void KhoiTaoTaiKhoan()
        {
            // Khởi tạo danh sách tài khoản với 4 tài khoản mẫu
            taikhoans = new List<Taikhoan>
            {
                new Taikhoan("user1", "pass1"),
                new Taikhoan("user2", "pass2"),
                new Taikhoan("user3", "pass3"),
                new Taikhoan("user4", "pass4")
            };
        }
        public void StartServer()
        {
            // mở cổng 5555 lắng nghe trên tất cả IP mà server đang có
            listener = new TcpListener(IPAddress.Any, 5555);
            listener.Start();
            // khởi tạo luồng kết nối cho client
            isRunning = true;
            Thread acceptClient = new Thread(AcceptClient);
            acceptClient.Start();

        }
        public void AcceptClient()
        {
            while (isRunning)
            {
                try
                {
                    if (clients.Count < maxClients)
                    {
                        // Chấp nhận kết nối từ client
                        TcpClient client = listener.AcceptTcpClient();
                        Thread loginThread = new Thread(() => HandleLogin(client));
                        loginThread.Start(); // Chạy luồng xử lý đăng nhập riêng
                    }
                    else
                    {
                        // Từ chối kết nối nếu đã đạt tối đa

                        TcpClient rejectedClient = listener.AcceptTcpClient();
                        NetworkStream stream = rejectedClient.GetStream();

                        byte[] message = Encoding.UTF8.GetBytes("rejected");
                        stream.Write(message, 0, message.Length); // Gửi thông báo từ chối

                        rejectedClient.Close(); // Sau đó đóng kết nối
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public void NhanMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            try
            {
                while (isRunning)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length); // nhan message
                    IPEndPoint remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string guiClient = remoteEndPoint.Address + ":" + remoteEndPoint.Port + ": " + message + Environment.NewLine;
                    Invoke(new Action(() =>
                    {
                        rtbMessage.AppendText(guiClient);
                    }));
                    byte[] data = Encoding.UTF8.GetBytes(guiClient); // Chuyển đổi tin nhắn thành mảng byte
                    foreach (var cl in clients)
                    {
                        NetworkStream str = cl.GetStream();
                        str.Write(data, 0, data.Length);
                    } // Gọi hàm gửi tin nhắn cho tất cả client
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //Invoke(new Action(() =>
                //{
                //    rtbMessage.AppendText($"Lỗi: {ex.Message}\n");
                //}));
            }
            finally
            {
                clients.Remove(client);
                client.Close();
            }
        }


        


        // Hàm xử lý đăng nhập cho mỗi client
        private void HandleLogin(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] credentials = message.Split(':');

                if (credentials.Length == 2)
                {
                    string username = credentials[0];
                    string password = credentials[1];
                    
                    // Kiểm tra thông tin đăng nhập
                    bool loginSuccess = taikhoans.Any(t => t.Username == username && t.Password == password);

                    if (loginSuccess)
                    {
                        lock (clients)
                        {
                            clients.Add(client);
                        }

                        byte[] successMessage = Encoding.UTF8.GetBytes("1");
                        stream.Write(successMessage, 0, successMessage.Length);

                        IPEndPoint remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        Invoke(new Action(() =>
                        {
                            rtbClientConnect.AppendText($"Client kết nối: {remoteEndPoint.Address}:{remoteEndPoint.Port}\n");
                        }));
                        Thread receiveMessageThread = new Thread(() => NhanMessage(client));
                        receiveMessageThread.Start();
                    }
                    else
                    {
                        byte[] failMessage = Encoding.UTF8.GetBytes("0");
                        stream.Write(failMessage, 0, failMessage.Length);
                        client.Close(); // Đóng kết nối nếu đăng nhập thất bại
                    }
                }
            }
        }

        private void rtbClientConnect_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbMessage_TextChanged(object sender, EventArgs e)
        {

        }
        public class Taikhoan
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public Taikhoan(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }
        public void StopServer()
        {
            isRunning = false;  // Dừng server
            // Dừng lắng nghe kết nối mới
            if (listener != null)
            {
                listener.Stop();
            }
            

            // Đóng tất cả các kết nối client
            foreach (var client in clients)
            {
                client.Close();
            }

            // Xóa danh sách client
            clients.Clear();
        }
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            StartServer();
        }
    }
}