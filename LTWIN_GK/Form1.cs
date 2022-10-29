using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTWIN_GK
{
    public partial class Form1 : Form
    {
        #region Properties
        XuLyBanCo BanCo;

        SocketManager socket;
        #endregion
        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            BanCo = new XuLyBanCo(pnPlay, txtNamePlayer, pbImagePlayer);
            BanCo.KetThucTroChoi += BanCo_KetThucTroChoi;
            BanCo.DanhDauNguoiChoi += BanCo_DanhDauNguoiChoi;

            prbCountDown.Step = COOL_DOWN_STEP;
            prbCountDown.Maximum = COOL_DOWN_TIME;
            prbCountDown.Value = 0;

            tmCountDown.Interval = COOL_DOWN_INTERVAL;

            socket = new SocketManager();

            BanCo.VeBanCo();
        }

        void KetThucTroChoi()
        {
            tmCountDown.Stop();
            pnPlay.Enabled = false;
            lùiLạiToolStripMenuItem.Enabled = false;
        }

        void TaoMoiTroChoi()
        {
            BanCo.VeBanCo();
            prbCountDown.Value = 0;
            lùiLạiToolStripMenuItem.Enabled=true;
            tmCountDown.Stop();
        }

        void Lui()
        {
            BanCo.Lui();
            prbCountDown.Value = 0;
        }

        void ThoatTroChoi()
        {
            Application.Exit();
        }

        void BanCo_DanhDauNguoiChoi(object sender, ButtonClickEvent e)
        {
            tmCountDown.Start();
            pnPlay.Enabled = false;
            prbCountDown.Value = 0;
            
            socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));

            lùiLạiToolStripMenuItem.Enabled = false;
            Listen();
        }

        void BanCo_KetThucTroChoi(object sender, EventArgs e)
        {
            KetThucTroChoi();
            socket.Send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pnPlay_Paint(object sender, PaintEventArgs e)
        {

        }

        public static int COOL_DOWN_STEP = 100;
        public static int COOL_DOWN_TIME = 10000;
        public static int COOL_DOWN_INTERVAL = 100;
        private void tmCountDown_Tick(object sender, EventArgs e)
        {
            prbCountDown.PerformStep();
            if (prbCountDown.Value >= prbCountDown.Maximum)
            {
                KetThucTroChoi();
                socket.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));
            }
        }

        private void chơiMớiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TaoMoiTroChoi();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));
        }

        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThoatTroChoi();
        }

        private void lùiLạiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lui();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(MessageBox.Show("Bạn có thật sự muốn thoát không?", "Thông báo", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }    
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                }
                catch { }
            }    
        }

        private void btnLan_Click(object sender, EventArgs e)
        {
            socket.IP = txtbLan.Text;

            if(!socket.ConnectServer())
            {
                socket.isServer = true;
                pnPlay.Enabled = true;
                socket.CreateServer();
            }
            else
            {
                socket.isServer = false;
                pnPlay.Enabled = false;
                Listen();
            }

            
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txtbLan.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if(string.IsNullOrEmpty(txtbLan.Text))
            {
                txtbLan.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }    
        }

        void Listen()
        {
           Thread listenThread = new Thread(() =>
           {
               try
               {
                   SocketData data = (SocketData)socket.Receive();

                   ProcessData(data);
               }
               catch
               {

               }
            });
               listenThread.IsBackground = true;
               listenThread.Start();            
        }    

        private void ProcessData(SocketData data)
        {
            switch(data.Command)
            {
                case(int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        TaoMoiTroChoi();
                        pnPlay.Enabled = false;
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        prbCountDown.Value = 0;
                        pnPlay.Enabled = true;
                        tmCountDown.Start();
                        BanCo.DanhDauNguoiChoiKhac(data.Point);
                        lùiLạiToolStripMenuItem.Enabled = true;
                    }));
                    break;
                case (int)SocketCommand.UNDO:
                    prbCountDown.Value = 0;
                    break;
                case (int)SocketCommand.END_GAME:
                    MessageBox.Show("Đã kết thúc trận đấu!");
                    break;
                case (int)SocketCommand.TIME_OUT:
                    MessageBox.Show("Đã hết giờ!");
                    break;
                case (int)SocketCommand.QUIT:
                    tmCountDown.Stop();
                    MessageBox.Show("Người chơi đã thoát!");
                    break;
                default:
                    break;
            }   
            
            Listen();
        }

        private void nguồnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThongTin thongTin = new ThongTin();
            thongTin.ShowDialog();
        }

        private void hướngDẫnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HuongDan huongDan = new HuongDan();
            huongDan.ShowDialog();
        }
    }
}
