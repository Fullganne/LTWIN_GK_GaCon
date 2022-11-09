using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTWIN_GK
{

    public class XuLyBanCo
    {
        #region Properties
        private Panel BanCo;
        public Panel BanCo1 { get => BanCo; set => BanCo = value; }

        private List<Player> player; 
        internal List<Player> Player { get => player; set => player = value; }

        private int currentPlayer;
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }
        
        private TextBox playerName;
        public TextBox PlayerName { get => playerName; set => playerName = value; }
        
        private PictureBox playerMark;
        public PictureBox PlayerMark { get => playerMark; set => playerMark = value; }
        
        private List<List<Button>> matran;
        public List<List<Button>> Matran { get => matran; set => matran = value; }

        private event EventHandler<ButtonClickEvent> danhdauNguoiChoi;
        public event EventHandler<ButtonClickEvent> DanhDauNguoiChoi
        {
            add
            {
                danhdauNguoiChoi += value;
            }
            remove
            {
                danhdauNguoiChoi -= value;
            }
        }

        private event EventHandler ketthucTroChoi;
        public event EventHandler KetThucTroChoi
        {
            add
            {
                ketthucTroChoi += value;
            }
            remove
            {
                ketthucTroChoi -= value;
            }
        }

        private Stack<ThongTinTroChoi> lichsuBuocDi;
        public Stack<ThongTinTroChoi> LichsuBuocDi { get => lichsuBuocDi; set => lichsuBuocDi = value; }
        #endregion

        #region Initialize
        public XuLyBanCo(Panel BanCo, TextBox playerName, PictureBox mark)
        { 
            this.BanCo1 = BanCo; 
            this.playerName = playerName;
            this.playerMark = mark;
            this.player = new List<Player>() 
            {
                new Player("Player1", Image.FromFile(Application.StartupPath + "\\Resource\\P1.png")),
                new Player("Player2", Image.FromFile(Application.StartupPath + "\\Resource\\P2.png"))
            };
            LichsuBuocDi = new Stack<ThongTinTroChoi>();
        }
        #endregion

        #region Methods
        public static int Chieudai_O = 30;
        public static int Chieurong_O = 30;
        public static int Chieudai_Banco = 30;
        public static int Chieurong_Banco = 30;

        public void VeBanCo()
        {
            BanCo.Enabled = true;
            BanCo.Controls.Clear();

            lichsuBuocDi = new Stack<ThongTinTroChoi>();

            CurrentPlayer = 0;
            DoiNguoiChoi();

            Matran = new List<List<Button>>();
            Button oldbtn = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Chieurong_Banco; i++)
            {
                Matran.Add(new List<Button>());

                for (int j = 0; j < Chieudai_Banco; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Chieurong_O,
                        Height = Chieudai_O,
                        Location = new Point(oldbtn.Location.X + oldbtn.Width, oldbtn.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackColor = Color.LightGray,
                        Tag = i.ToString() 
                    };
                    
                    btn.Click += Btn_Click;

                    BanCo1.Controls.Add(btn);

                    Matran[i].Add(btn);

                    oldbtn = btn;
                }
                oldbtn.Location = new Point(0, oldbtn.Location.Y + Chieudai_O);
                oldbtn.Width = 0;
                oldbtn.Height = 0;
            }
        }
        private void playSoundOfClick()
        {
            SoundPlayer clickedSound = new SoundPlayer(Application.StartupPath + "\\SoundGame\\click.wav");
            clickedSound.Play();
        }
        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if(btn.BackgroundImage != null)
                return;
            Danhdau(btn);

            LichsuBuocDi.Push(new ThongTinTroChoi(GetViTriCo(btn),CurrentPlayer));


            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            DoiNguoiChoi();

            if(danhdauNguoiChoi != null)
            {
                danhdauNguoiChoi(this, new ButtonClickEvent(GetViTriCo(btn)));
            }    

            if(ktKetThucGame(btn))
            {
                KetThucGame();
            }
        }

        public void DanhDauNguoiChoiKhac(Point point)
        {
            Button btn = Matran[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            BanCo.Enabled = true;

            Danhdau(btn);

            LichsuBuocDi.Push(new ThongTinTroChoi(GetViTriCo(btn), CurrentPlayer));

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            DoiNguoiChoi();

            if (ktKetThucGame(btn))
            {
                KetThucGame();
            }
        }

        public void KetThucGame()
        {
            if(ketthucTroChoi != null)
                ketthucTroChoi(this, new EventArgs());
        }

        public Stack<ThongTinTroChoi> GetLichsuBuocDi()
        {
            return LichsuBuocDi;
        }

        public bool Lui()
        {
            if(LichsuBuocDi.Count <=  0)
                return false;

            bool Luil1 = LuiMotBuoc();
            bool Luil2 = LuiMotBuoc();

            ThongTinTroChoi oldPoint = LichsuBuocDi.Peek();
            CurrentPlayer = oldPoint.CurrentPlayer == 1 ? 0 : 1;

            return Luil1 && Luil2;
        }

        public bool LuiMotBuoc()
        {
            if(lichsuBuocDi.Count <= 0)
            {
                return false;
            }    
            ThongTinTroChoi oldPoint = lichsuBuocDi.Pop();
            Button btn = Matran[oldPoint.Point.Y][oldPoint.Point.X];

            btn.BackgroundImage = null;

            oldPoint = lichsuBuocDi.Peek();

            if(oldPoint == null)
            {
                CurrentPlayer = 0;
            }    
            else
            {
                oldPoint = lichsuBuocDi.Peek();

            }    

            DoiNguoiChoi();

            return true;
        }

        private bool ktKetThucGame(Button btn)
        {
            return ktKetThucNgang(btn)||ktKetThucDoc(btn)||ktKetThucCheoChinh(btn)||ktKetThucCheoPhu(btn);
        }
        private Point GetViTriCo(Button btn)
        {
            int ngang = Convert.ToInt32(btn.Tag);
            int doc = Matran[ngang].IndexOf(btn);

            Point vitri = new Point(doc, ngang);

            return vitri;
        }
        private bool ktKetThucNgang(Button btn)
        {
            Point vitri = GetViTriCo(btn);

            int demTrai = 0;
            for (int i = vitri.X; i >= 0; i--)
            {
                if (Matran[vitri.Y][i].BackgroundImage == btn.BackgroundImage)
                    demTrai++;
                else
                    break;
            }

            int demPhai = 0;
            for (int i = vitri.X + 1; i < Chieurong_Banco; i++)
            {
                if (Matran[vitri.Y][i].BackgroundImage == btn.BackgroundImage)
                    demPhai++;
                else
                    break;
            }

            return demPhai + demTrai == 5;
        }
        private bool ktKetThucDoc(Button btn)
        {
            Point vitri = GetViTriCo(btn);
            
            int demTren = 0;
            for (int i = vitri.Y; i >= 0; i--)
            {
                if (Matran[i][vitri.X].BackgroundImage == btn.BackgroundImage)
                    demTren++;
                else
                    break;
            }    

            int demDuoi = 0;
            for (int i = vitri.Y + 1; i < Chieudai_Banco; i++)
            {
                if (Matran[i][vitri.X].BackgroundImage == btn.BackgroundImage)
                    demDuoi++;
                else
                    break;
            }

            return demTren + demDuoi == 5;
        }
        private bool ktKetThucCheoChinh(Button btn)
        {
            Point vitri = GetViTriCo(btn);

            int demTren = 0;
            for (int i = 0; i <= vitri.X; i++)
            {
                if (vitri.X - i < 0 || vitri.Y - i < 0)
                    break;
                if (Matran[vitri.Y - i][vitri.X - i].BackgroundImage == btn.BackgroundImage)
                    demTren++;
                else
                    break;
            }

            int demDuoi = 0;
            for (int i = 1; i <= Chieurong_Banco - vitri.X; i++)
            {
                if (vitri.X + i >= Chieurong_Banco || vitri.Y + i >= Chieudai_Banco)
                    break;
                if (Matran[vitri.Y + i][vitri.X + i].BackgroundImage == btn.BackgroundImage)
                    demDuoi++;
                else
                    break;
            }

            return demTren + demDuoi == 5;
        }
        private bool ktKetThucCheoPhu(Button btn)
        {
            Point vitri = GetViTriCo(btn);

            int demTren = 0;
            for (int i = 0; i <= vitri.X; i++)
            {
                if (vitri.X + i >= Chieurong_Banco || vitri.Y - i < 0)
                    break;
                if (Matran[vitri.Y - i][vitri.X + i].BackgroundImage == btn.BackgroundImage)
                    demTren++;
                else
                    break;
            }

            int demDuoi = 0;
            for (int i = 1; i <= Chieurong_Banco - vitri.X; i++)
            {
                if (vitri.X - i < 0 || vitri.Y + i >= Chieudai_Banco)
                    break;
                if (Matran[vitri.Y + i][vitri.X - i].BackgroundImage == btn.BackgroundImage)
                    demDuoi++;
                else
                    break;
            }

            return demTren + demDuoi == 5;
        }
        private void Danhdau(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;
        }
        private void DoiNguoiChoi()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;
            PlayerMark.Image = Player[CurrentPlayer].Mark;
        }
        #endregion
    }

    public class ButtonClickEvent : EventArgs
    {
        private Point clickedPoint;

        public Point ClickedPoint { get => clickedPoint; set => clickedPoint = value; }
        
        public ButtonClickEvent(Point point)
        {
            this.ClickedPoint = point;
        }
    }
    
}
