using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace DeskClock
{
    public partial class Form1 : PixelizedAlphaForm {
        
        delegate void CSafeSetImage(Bitmap bmp, Byte opacity);
        CSafeSetImage cssi;

        private const Int32 SIZE = 70;
        private const float HOUR_LENGTH = SIZE / 1.8f;
        private const float MIN_LENGTH = SIZE / 1.3f;
        private const float SEC_LENGTH = SIZE / 1.1f;

        private Boolean m_RenderActive = true;
        private PointF m_Axis = new PointF(SIZE, SIZE);
        private Thread m_ClockUpdator;
        private Pen m_secPen = new Pen(Color.FromArgb(192, Color.Red), 3f);
        private Pen m_Pen = new Pen(Color.FromArgb(192, Color.White), 3f);
        private Pen m_cclPen = new Pen(Color.FromArgb(224, Color.Black), 2f);

        void fnCSafeSetImage(Bitmap bmp, Byte opacity) {
            if (InvokeRequired)
                Invoke(cssi, new Object[] { bmp, opacity });
            else
                SetImage(bmp, opacity);
        }
        public Form1() {
            cssi = new CSafeSetImage(fnCSafeSetImage);
            m_ClockUpdator = new Thread(RenderClock);
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            TopMost = true;
            ShowIcon = false;
            ShowInTaskbar = false;
            ControlBox = false;
            TopLevel = true;

            ClientSize = new Size(SIZE * 2, SIZE * 2 + 60);
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - SIZE * 2 - 10, 10);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            m_secPen.SetLineCap(LineCap.Round, LineCap.ArrowAnchor, DashCap.Triangle);
            m_Pen.SetLineCap(LineCap.Round, LineCap.ArrowAnchor, DashCap.Triangle);
            m_ClockUpdator.Start();
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            base.OnFormClosing(e);
            if (e.CloseReason != CloseReason.WindowsShutDown) {
                e.Cancel = true;
                return;
            }

            m_RenderActive = false;
            m_ClockUpdator.Abort();
            m_Pen.Dispose();
            m_secPen.Dispose();
            m_cclPen.Dispose();
        }

        private void RenderClock() {
            double hourAngle;   // 시침 각도
            double minAngle;    // 분침 각도
            double secAngle;    // 초침 각도
            PointF ptHour;      // 시침 위치
            PointF ptMinute;    // 분침 위치
            PointF ptSecond;    // 초침 위치
            Bitmap renderBmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics renderGraphics = Graphics.FromImage(renderBmp);
            renderGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            renderGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            renderGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            renderGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            while (m_RenderActive) {
                // 현재 시각을 받아온다.
                DateTime now = DateTime.Now;

                // 시침 각도 계산
                hourAngle = (now.Hour % 12.0) * 30.0;
                hourAngle += now.Minute * 0.5;

                // 분침 각도 계산
                minAngle = now.Minute * 6.0;
                minAngle += now.Second * 0.1;

                // 초침 각도 계산
                secAngle = now.Second * 6.0;
                secAngle += ((now.Millisecond / 1000.0) * 6.0);


                // 시침 위치 계산
                ptHour = PointFrom(m_Axis, hourAngle, HOUR_LENGTH);

                // 분침 위치 계산
                ptMinute = PointFrom(m_Axis, minAngle, MIN_LENGTH);

                // 초침 위치 계산
                ptSecond = PointFrom(m_Axis, secAngle, SEC_LENGTH);

                // 그리기 위해서 화면 클리어!
                renderGraphics.Clear(Color.Transparent);

                // 시침, 분침, 초침 그리기
                renderGraphics.DrawLine(m_Pen, m_Axis, ptHour);
                renderGraphics.DrawLine(m_Pen, m_Axis, ptMinute);
                renderGraphics.DrawLine(m_secPen, m_Axis, ptSecond);

                // 현재 시간!
                DrawBorderedString(renderGraphics, String.Format(CultureInfo.InvariantCulture, "{0:yyyy/MM/dd HH:mm:ss}", now), Font, Brushes.DarkRed, Brushes.Orange, 0, SIZE * 2);
                DrawBorderedString(renderGraphics, String.Format(CultureInfo.InvariantCulture, "{0:yyyy/MM/dd} 00:00:00", Program.EndDate), Font, Brushes.DarkGreen, Brushes.Lime, 0, SIZE * 2 + 16);

                // 남은 기간
                TimeSpan remains = Program.EndDate - now;
                DrawBorderedString(renderGraphics, remains.Days + 1 + " 일 남았습니다", Font2, Brushes.Blue, Brushes.Cyan, 0, SIZE * 2 + 32);

                // 업데이트
                fnCSafeSetImage(renderBmp, 255);
            }
            renderGraphics.Dispose();
            renderBmp.Dispose();
        }
        private void DrawBorderedString(Graphics g, String s, Font font, Brush bb, Brush sb, Int32 x, Int32 y) {
            g.DrawString(s, font, bb, x - 1, y);
            g.DrawString(s, font, bb, x + 1, y);
            g.DrawString(s, font, bb, x, y - 1);
            g.DrawString(s, font, bb, x, y + 1);
            g.DrawString(s, font, sb, x, y);
        }
        private PointF PointFrom(PointF b, double deg, double dist) {
            PointF Base = b;
            PointF dir = Direction(deg);
            Base.X -= (Single)(dir.X * dist);
            Base.Y -= (Single)(dir.Y * dist);
            return Base;
        }
        private double DegToRad(double deg) {
            return deg * (Math.PI / 180);
        }
        private double Distance(PointF p1, PointF p2) {
            double ydf = p2.Y - p1.Y;
            double xdf = p2.X - p1.X;
            return Math.Sqrt(Math.Pow(xdf, 2) + Math.Pow(ydf, 2));
        }
        private PointF Direction(double deg) {
            return new PointF(-(Single)Math.Sin(DegToRad(deg)),
                               (Single)Math.Cos(DegToRad(deg)));
        }
       
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
        }
    }
}
