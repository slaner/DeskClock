using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
namespace DeskClock {
    public class PixelizedAlphaForm : Form {
        #region Definition
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BLEND_FUNCTION {
            public Byte BlendOption;
            public Byte BlendFlags;
            public Byte SourceConstantAlpha;
            public Byte AlphaFormat;
        }

        [DllImport("user32")]
        static extern Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);
        [DllImport("user32")]
        static extern Boolean UpdateLayeredWindow(IntPtr hWnd, IntPtr hDestDC, ref Point ptDest, ref Size size, IntPtr hSrcDC, ref Point ptSrc, Int32 nColorKey, ref BLEND_FUNCTION bf, Int32 dwFlags);
        [DllImport("user32")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32")]
        static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32")]
        static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32")]
        static extern Boolean DeleteDC(IntPtr hDC);
        [DllImport("gdi32")]
        static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32")]
        static extern Boolean DeleteObject(IntPtr hObject);

        const Int32 ULW_ALPHA = 0x2;
        const Byte AC_SRC_OVER = 0x0;
        const Byte AC_SRC_ALPHA = 0x1;
        #endregion

        public PixelizedAlphaForm() {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        protected void SetImage(Bitmap bmp, Byte opacity) {

            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            IntPtr sDC = GetDC(IntPtr.Zero);
            IntPtr mDC = CreateCompatibleDC(sDC);
            IntPtr hOldBmp = IntPtr.Zero;
            IntPtr hBmp = IntPtr.Zero;

            try {
                hBmp = bmp.GetHbitmap(Color.FromArgb(0));
                hOldBmp = SelectObject(mDC, hBmp);

                Size sz = bmp.Size;
                Point ptSrc = Point.Empty;
                Point ptTop = new Point(Left, Top);
                BLEND_FUNCTION bf = default(BLEND_FUNCTION);

                bf.BlendOption = AC_SRC_OVER;
                bf.SourceConstantAlpha = opacity;
                bf.AlphaFormat = AC_SRC_ALPHA;

                UpdateLayeredWindow(Handle, sDC, ref ptTop, ref sz, mDC, ref ptSrc, 0, ref bf, ULW_ALPHA);
            } finally {
                ReleaseDC(IntPtr.Zero, sDC);
                if (hBmp != IntPtr.Zero) {
                    SelectObject(mDC, hOldBmp);
                    DeleteObject(hOldBmp);
                }
                DeleteObject(hBmp);
                DeleteDC(mDC);
            }
        }
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80020;
                return cp;
            }
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PixelizedAlphaForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "PixelizedAlphaForm";
            this.ResumeLayout(false);

        }
    }
}
