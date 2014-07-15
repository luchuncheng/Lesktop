using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Client
{
    public partial class GradScreenForm : Form
    {
        private Rectangle rectSelected = Rectangle.Empty;
        private Bitmap screen, origScreen;
        private Bitmap resultBmp = null;
		private Point _prePos, _adjust_pre_loc;
		Rectangle _adjust_pre_rect;
		private int _adjust_pos = 0;
		private string _step = "none";

		public Rectangle[] GetRects()
		{
			Rectangle[] rects = new Rectangle[9];
			int[] xs = new int[3] { rectSelected.Left, rectSelected.Left + rectSelected.Width / 2, rectSelected.Right };
			int[] ys = new int[3] { rectSelected.Top, rectSelected.Top + rectSelected.Height / 2, rectSelected.Bottom };
			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < 3; x++)
				{
					if (y * 3 + x != 4)
					{
						rects[y * 3 + x] = new Rectangle(xs[x] - 3, ys[y] - 3, 6, 6);
					}
					else
					{
						rects[y * 3 + x] = new Rectangle(xs[0] + 3, ys[0] + 3, rectSelected.Width - 6, rectSelected.Height - 6);
					}
				}
			}
			return rects;
		}

		Cursor[] posCursors = new Cursor[10]{
			Cursors.SizeNWSE,
			Cursors.SizeNS,
			Cursors.SizeNESW,
			Cursors.SizeWE,
			Cursors.SizeAll,
			Cursors.SizeWE,
			Cursors.SizeNESW,
			Cursors.SizeNS,
			Cursors.SizeNWSE,
			Cursors.Default
		};

		public int GetPosition(Point p)
		{
			Rectangle[] rs = GetRects();
			for (int i = 0; i < 9; i++)
			{
				if (rs[i].Contains(p)) return i;
			}
			return 9;
		}

        public GradScreenForm(Bitmap screen)
        {

            InitializeComponent();

            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            Bitmap bitmap = new Bitmap(width, height);
            Graphics graph = Graphics.FromImage(bitmap);
            graph.DrawImage(screen, 0, 0);
            graph.FillRectangle(new SolidBrush(Color.FromArgb(80, 0, 0, 0)), new Rectangle(0, 0, width, height));

            this.screen = bitmap;
            this.origScreen = screen;
            this.Bounds = new Rectangle(0, 0, width, height);
            this.DoubleBuffered = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
				if (_step == "none")
				{
					_step = "capture";
					rectSelected = Rectangle.Empty;
					rectSelected.Location = e.Location;
					_prePos = e.Location;
					this.Invalidate();
				}
				else if (_step == "selected")
				{
					int pos = GetPosition(e.Location);
					if (pos == 9)
					{
						//_step = "capture";
						//rectSelected = Rectangle.Empty;
						//rectSelected.Location = e.Location;
						//_prePos = e.Location;
						//this.Invalidate();
					}
					else
					{
						_step = "adjust";
						_adjust_pre_rect = new Rectangle(rectSelected.X, rectSelected.Y, rectSelected.Width, rectSelected.Height);
						_adjust_pos = pos;
						_adjust_pre_loc = e.Location;
					}
				}
            }

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
			if (e.Button == MouseButtons.Left)
			{
				if (_step == "capture")
				{
					if (e.X > _prePos.X)
					{
						rectSelected.Width = e.X - _prePos.X;
					}
					else
					{
						rectSelected.Width = _prePos.X - e.X;
						rectSelected.X = e.X;
					}
					if (e.Y > _prePos.Y)
					{
						rectSelected.Height = e.Y - _prePos.Y;
					}
					else
					{
						rectSelected.Height = _prePos.Y - e.Y;
						rectSelected.Y = e.Y;
					}
					this.Invalidate();
				}
				else if (_step == "adjust")
				{
					if (_adjust_pos == 0)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Right),
							Math.Min(e.Location.Y, _adjust_pre_rect.Bottom),
							Math.Abs(e.X - _adjust_pre_rect.Right),
							Math.Abs(e.Y - _adjust_pre_rect.Bottom)
						);
					}
					else if (_adjust_pos == 1)
					{
						rectSelected = new Rectangle(
							_adjust_pre_rect.Left,
							Math.Min(e.Location.Y, _adjust_pre_rect.Bottom),
							_adjust_pre_rect.Width,
							Math.Abs(e.Y - _adjust_pre_rect.Bottom)
						);
					}
					else if (_adjust_pos == 2)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Left),
							Math.Min(e.Location.Y, _adjust_pre_rect.Bottom),
							Math.Abs(e.X - _adjust_pre_rect.Left),
							Math.Abs(e.Y - _adjust_pre_rect.Bottom)
						);
					}
					else if (_adjust_pos == 3)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Right),
							_adjust_pre_rect.Top,
							Math.Abs(e.Location.X - _adjust_pre_rect.Right),
							_adjust_pre_rect.Height
						);
					}
					else if (_adjust_pos == 4)
					{
						rectSelected = new Rectangle(
							_adjust_pre_rect.Left + (e.Location.X - _adjust_pre_loc.X),
							_adjust_pre_rect.Top + (e.Location.Y - _adjust_pre_loc.Y),
							_adjust_pre_rect.Width,
							_adjust_pre_rect.Height
						);
					}
					else if (_adjust_pos == 5)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Left),
							_adjust_pre_rect.Top,
							Math.Abs(e.Location.X - _adjust_pre_rect.Left),
							_adjust_pre_rect.Height
						);
					}
					else if (_adjust_pos == 6)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Right),
							Math.Min(e.Location.Y, _adjust_pre_rect.Top),
							Math.Abs(e.X - _adjust_pre_rect.Right),
							Math.Abs(e.Y - _adjust_pre_rect.Top)
						);
					}
					else if (_adjust_pos == 7)
					{
						rectSelected = new Rectangle(
							_adjust_pre_rect.Left,
							Math.Min(e.Location.Y, _adjust_pre_rect.Top),
							_adjust_pre_rect.Width,
							Math.Abs(e.Y - _adjust_pre_rect.Top)
						);
					}
					else if (_adjust_pos == 8)
					{
						rectSelected = new Rectangle(
							Math.Min(e.Location.X, _adjust_pre_rect.Left),
							Math.Min(e.Location.Y, _adjust_pre_rect.Top),
							Math.Abs(e.X - _adjust_pre_rect.Left),
							Math.Abs(e.Y - _adjust_pre_rect.Top)
						);
					}
					this.Invalidate();
				}
			}
			if (_step == "selected")
			{
				Cursor = posCursors[GetPosition(e.Location)];
			}
        }

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			try
			{
				resultBmp = new Bitmap(rectSelected.Width, rectSelected.Height);
				using (Graphics g = Graphics.FromImage(resultBmp))
				{
					g.DrawImage(origScreen, new Rectangle(0, 0, rectSelected.Width, rectSelected.Height), rectSelected, GraphicsUnit.Pixel);
				}
			}
			catch
			{
			}
			this.DialogResult = resultBmp == null ? DialogResult.Cancel : DialogResult.OK;
		}

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
					if (_step == "capture")
					{
						_step = "selected";
						if (e.X > _prePos.X)
						{
							rectSelected.Width = e.X - _prePos.X;
						}
						else
						{
							rectSelected.Width = _prePos.X - e.X;
							rectSelected.X = e.X;
						}
						if (e.Y > _prePos.Y)
						{
							rectSelected.Height = e.Y - _prePos.Y;
						}
						else
						{
							rectSelected.Height = _prePos.Y - e.Y;
							rectSelected.Y = e.Y;
						}
						this.Invalidate();
					}
					else if (_step == "adjust")
					{
						_step = "selected";
					}
                }
                catch
                {
                }
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (_step == "selected")
				{
					_step = "none";
					rectSelected = Rectangle.Empty;
					this.Invalidate();
				}
				else if (_step == "none")
				{
					this.DialogResult = DialogResult.Cancel;
				}
			}

        }

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			g.DrawImage(screen, 0, 0);
			g.DrawImage(origScreen, rectSelected, rectSelected, GraphicsUnit.Pixel);
			if (rectSelected == Rectangle.Empty)
			{
				g.DrawString("双击完成截图，右键取消，Esc退出", new Font("宋体", 12), Brushes.Red, 0, 0);
			}
			else
			{
				g.DrawString("双击完成截图，右键取消，Esc退出", new Font("宋体", 12), Brushes.Red, rectSelected.X, rectSelected.Y - 20);
			}
			Rectangle[] rs = GetRects();
			for (int i = 0; i < 9; i++)
			{
				if (i != 4) g.FillRectangle(Brushes.Blue, rs[i]);
			}
			g.DrawRectangle(new Pen(Color.Blue), rectSelected);
		}

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
            }

        }

        private void PaintRectangle()
        {
            

        }

        public Bitmap ResultBitmap
        {
            get { return resultBmp; }
        }

    }
}
