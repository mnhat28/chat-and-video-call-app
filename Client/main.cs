using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAnCuoiKy
{
    public partial class main : Form
    {
        private TcpClient server;
        private Thread nhanTinThread;
        public main(TcpClient client)
        {
            InitializeComponent();
            this.server = client;
            this.Load += new EventHandler(main_Load);
        }
        private void main_Load(object sender, EventArgs e)
        {
            // Khởi tạo và bắt đầu luồng nhận tin nhắn từ server
            nhanTinThread = new Thread(NhanTinNhanTuServer);
            nhanTinThread.IsBackground = true; // Đảm bảo luồng sẽ dừng khi ứng dụng đóng
            nhanTinThread.Start(); // Bắt đầu luồng
        }

        private async void btnGui_Click(object sender, EventArgs e)
        {
            string message = txtGui.Text;
            txtGui.Text = "";
            byte[] data = Encoding.UTF8.GetBytes(message);

            NetworkStream stream = server.GetStream();
            await stream.WriteAsync(data, 0, data.Length); // gửi server
            
        }
        private void NhanTinNhanTuServer()
        {
            NetworkStream stream = server.GetStream();
            byte[] buffer = new byte[1024];
            while (server.Connected)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Invoke(new Action(() =>
                            {
                                rtbMess.AppendText(response); // Hiển thị tin nhắn như nhận được từ server
                            }));
                        }
                    }
                    else
                    {
                        Thread.Sleep(100); // Tránh sử dụng CPU quá mức
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    // Xử lý ngắt kết nối nếu cần
                    break;
                }
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (nhanTinThread != null && nhanTinThread.IsAlive)
            {
                nhanTinThread.Abort(); // Dừng luồng khi form bị đóng
            }
            base.OnFormClosed(e);
        }
    }
}
