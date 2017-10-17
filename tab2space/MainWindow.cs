using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//30,30,30    Gainsboro

namespace tab2space {

    public partial class MainWindow : Form {

        private bool InputFinished = false;
        private bool IgnoreChangingText = false;
        private string OriginalText = "";
        private int DragCornerSize = 4;
        private Bitmap DragCornerPicture;

        TransparentPictureBox DragCornerPictureBox;
        TransparentLabel TrackBarValueLabel;

        Graphics graphics;

        public MainWindow()
        {
            InitializeComponent();

        }


        private void Initialize()
        {
            graphics = CreateGraphics();
            Text = Text + "  " + Program.Version;

            button1.Font = button2.Font = button3.Font = button4.Font = SystemFonts.MessageBoxFont;
            Font font = new Font("Consolas", 11.25F, GraphicsUnit.Point);
            if (font.Name != "Consolas") {
                font = new Font("Courier New", 11.25F, GraphicsUnit.Point);
            }
            textBox1.Font = font;
            

            AddControlsByProgram();
            InitializeByProgramData();
            AdjustControlsPositions();
        }


        private void InitializeByProgramData()
        {
            try {
                if (Program.ProgramData.MainWindowWidth != 0)
                    Width = Program.ProgramData.MainWindowWidth;
                if (Program.ProgramData.MainWindowHeight != 0)
                    Height = Program.ProgramData.MainWindowHeight;
                if (Program.ProgramData.DefaultTabWidth != 0) {
                    trackBar1.Value = Program.ProgramData.DefaultTabWidth;
                    TrackBarValueLabel.Text = trackBar1.Value.ToString();
                }
                if (Program.ProgramData.MainWindowMaximized) {
                    WindowState = FormWindowState.Maximized;
                }
                if (Program.ProgramData.Wordwrap) {
                    textBox1.WordWrap = wordwrapToolStripMenuItem.Checked = true;
                }
                if (Program.ProgramData.FontFamilyName != null && Program.ProgramData.FontSize != 0)
                    fontDialog1.Font = textBox1.Font = new Font(Program.ProgramData.FontFamilyName, Program.ProgramData.FontSize, GraphicsUnit.Point);
                if (Program.ProgramData.ForeColorSpecified)
                    colorDialog1.Color = textBox1.ForeColor = Color.FromArgb(Program.ProgramData.ForeColor);
                if (Program.ProgramData.BackColorSpecified)
                    colorDialog2.Color = textBox1.BackColor = Color.FromArgb(Program.ProgramData.BackColor);
            }
            catch (Exception ex) {

            }
        }

        private void AddControlsByProgram()
        {
            DragCornerPictureBox = new TransparentPictureBox();
            Controls.Add(DragCornerPictureBox);
            DragCornerPicture = GetDragCornerPicture(DragCornerSize);
            DragCornerPictureBox.Width = DragCornerPicture.Width;
            DragCornerPictureBox.Height = DragCornerPicture.Height;
            DragCornerPictureBox.Image = DragCornerPicture;


            TrackBarValueLabel = new TransparentLabel();
            Controls.Add(TrackBarValueLabel);
            TrackBarValueLabel.Font = new Font("Consolas", 11.25F, GraphicsUnit.Point);
        }

        private void AdjustControlsPositions()
        {
            DragCornerPictureBox.Top = ClientSize.Height - DragCornerPictureBox.Height - 3;
            DragCornerPictureBox.Left = ClientSize.Width - DragCornerPictureBox.Width - 3;

            TrackBarValueLabel.Top = ClientSize.Height - 35;
            TrackBarValueLabel.Left = ClientSize.Width - 30;

        }

        private void ConvertTabToSpace()
        {
            string lineending = Environment.NewLine;
            int tabwidth = trackBar1.Value;
            float unitwidth = GetStringDisplayWidth("aa", textBox1.Font) - GetStringDisplayWidth("a", textBox1.Font);
            StringBuilder sb = new StringBuilder(OriginalText.Length << 1);

            string[] lines = OriginalText.Split(new string[] { lineending }, StringSplitOptions.None);
            int start = 0;
            int tabposition = 0;
            string substring = "";
            int substringwidth = 0;    // in units
            int paddingwidth = 0;
            bool firstline = true;
            foreach (string line in lines) {
                start = 0;
                tabposition = 0;
                if (firstline) {
                    firstline = false;
                }
                else {
                    sb.Append(lineending);
                }
                while ((tabposition = line.IndexOf('\t', start)) != -1) {
                    substring = line.Substring(start, tabposition - start);
                    float substringpixelwidth = GetStringDisplayWidth(substring, textBox1.Font);
                    substringwidth = substringpixelwidth != 0 ? (int)(Math.Round(substringpixelwidth / unitwidth)) - 1 : 0;
                    paddingwidth = tabwidth - (substringwidth % tabwidth);
                    sb.Append(substring);
                    sb.Append(' ', paddingwidth);
                    start = tabposition + 1;
                }
                sb.Append(line.Substring(start, line.Length - start));
            }
            

            IgnoreChangingText = true;
            textBox1.Text = sb.ToString();
            IgnoreChangingText = false;

        }

        private float GetStringDisplayWidth(string str,Font font)
        {

            //return graphics.MeasureString(str, font).Width;
            return TextRenderer.MeasureText(graphics, str, font).Width;
        }

        private void FinishInput()
        {
            InputFinished = true;
            trackBar1.Enabled = true;
        }

        private void StartInput()
        {
            if (!IgnoreChangingText) {
                InputFinished = false;
                trackBar1.Enabled = false;
            }
        }

        private void StartConvert()
        {
            OriginalText = textBox1.Text;
            FinishInput();
            ConvertTabToSpace();
        }

        private void ContinueConvert()
        {
            TrackBarValueLabel.Text = trackBar1.Value.ToString();
            Program.ProgramData.DefaultTabWidth = trackBar1.Value;
            if (InputFinished) {
                ConvertTabToSpace();
            }
        }

        private string GetClipboardText()
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text)) {
                return (String)iData.GetData(DataFormats.Text);
            }
            else {
                return null;
            }

        }

        private void SetClipboardText(string str)
        {
            Clipboard.SetData("Text", str);
        }

        private Bitmap GetDragCornerPicture(int size)
        {
            const int DotSize = 2;
            const int SpaceWidth = 1;
            Color SpaceColor = Color.FromKnownColor(KnownColor.Control);
            Color DotCoor = Color.FromKnownColor(KnownColor.ControlDark);
            int PicWidth = DotSize * size + SpaceWidth * (size - 1);
            int PicHeight = PicWidth;

            Bitmap bmp = new Bitmap(PicWidth, PicHeight, PixelFormat.Format24bppRgb);
            //Bitmap bmp = new Bitmap(@"C:\Users\liuqx\Desktop\New folder\Untitled.bmp");

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat
                );

            // Get the address of the first line.
            IntPtr bmpDataPtr = bmpData.Scan0;
            // Declare an array to hold the bytes of the bitmap.
            // The array consists of a series of strides. A stride contains the data of one line.
            //Stride may be larger than bmp.Width*3 , because stride must be a multiple of four.
            int rgbValuesSizeInBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[rgbValuesSizeInBytes];

            Func<int, int, int> GetIndex = (int x, int y) =>
            {
                if (x < 0) x = 0;
                if (x >= bmp.Width) y = bmp.Height - 1;
                if (y < 0) y = 0;
                if (y >= bmp.Width) y = bmp.Height - 1;
                return y * Math.Abs(bmpData.Stride) + 3 * x;
            };

            Action<int, int> DrawDot = (int x, int y) =>
            {
                for(int i = 0; i < DotSize; i++) {
                    for(int j = 0; j < DotSize; j++) {
                        rgbValues[GetIndex(x + j, y + i)] = DotCoor.B;
                        rgbValues[GetIndex(x + j, y + i) + 1] = DotCoor.G;
                        rgbValues[GetIndex(x + j, y + i) + 2] = DotCoor.R;
                    }
                }
            };

            //// Copy the RGB values into the array.
            //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //Draw background
            for (int i = 0; i < rgbValues.Length - 3; i += 3) {
                rgbValues[i] = SpaceColor.B;
                rgbValues[i + 1] = SpaceColor.G;
                rgbValues[i + 2] = SpaceColor.R;
            }

            //Draw dots
            for(int i = 0; i < size; i++) {
                for(int j = 0; j <= i; j++) {
                    DrawDot((DotSize + SpaceWidth) * (size - i - 1 + j), (DotSize + SpaceWidth) * i);
                }
            }


            // Copy the RGB values to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpDataPtr, rgbValuesSizeInBytes);

            bmp.UnlockBits(bmpData);

            return bmp;

        }


        private bool IsMouseInDragCorner(Point pos)
        {
            //CreateGraphics().DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(pos.X, pos.Y, 1, 1));
            if (pos.X >= DragCornerPictureBox.Left - 5 && pos.Y >= DragCornerPictureBox.Top - 5)
                return true;
            else
                return false;
        }

        private void ClearText()
        {
            textBox1.Clear();
        }

        private void RestoreText()
        {
            textBox1.Text = OriginalText;
        }

        private void ChangeWordwrapSetting()
        {
            if (wordwrapToolStripMenuItem.Checked) {
                textBox1.WordWrap = Program.ProgramData.Wordwrap = wordwrapToolStripMenuItem.Checked = false;
            }
            else {
                textBox1.WordWrap = Program.ProgramData.Wordwrap = wordwrapToolStripMenuItem.Checked = true;
            }
        }

        private void ChangeForeColorSetting()
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                textBox1.ForeColor = colorDialog1.Color;
                Program.ProgramData.ForeColor = colorDialog1.Color.ToArgb();
                Program.ProgramData.ForeColorSpecified = true;
            }
        }

        private void ChangeBackColorSetting()
        {
            DialogResult result = colorDialog2.ShowDialog();
            if (result == DialogResult.OK) {
                textBox1.BackColor = colorDialog2.Color;
                Program.ProgramData.BackColor = colorDialog2.Color.ToArgb();
                Program.ProgramData.BackColorSpecified = true;
            }
        }

        private void ChangeFontSetting()
        {
            DialogResult result = fontDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                textBox1.Font = fontDialog1.Font;
                Program.ProgramData.FontFamilyName = fontDialog1.Font.FontFamily.Name;
                Program.ProgramData.FontSize = fontDialog1.Font.SizeInPoints;
            }
        }

        private void SaveMainWindowSize()
        {
            Program.ProgramData.MainWindowWidth = Width;
            Program.ProgramData.MainWindowHeight = Height;
        }

        private void PasteAndConvert()
        {
            string ClipboardText = GetClipboardText();
            if (ClipboardText != null) {
                IgnoreChangingText = true;
                ClearText();
                textBox1.Text = ClipboardText;
                IgnoreChangingText = false;

                StartConvert();
            }
        }

        private void CopyTextToClipboard()
        {
            SetClipboardText(textBox1.Text);
        }

        private void ShowAboutDialog()
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

















        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                // Maximize     0xF030
                // Restore      0xF120
                // Minimize     0XF020
                if ((m.WParam.ToInt32() & 0xFFF0)==0xF030) {
                    // THe window is being maximized
                    Program.ProgramData.MainWindowMaximized = true;
                }
                if ((m.WParam.ToInt32() & 0xFFF0) == 0xF120) {
                    // THe window is being restored
                    Program.ProgramData.MainWindowMaximized = false;

                }

            }

            if (m.Msg == 0x84) {  // WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (IsMouseInDragCorner(pos)) {
                    m.Result = (IntPtr)17;            // HTBOTTOMRIGHT
                    return;
                }

            }

            base.WndProc(ref m);
        }


        //You return true to indicate that you handled the keystroke and don't want it to be passed on to other controls
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A)) {
                textBox1.SelectAll();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.V)) {
                PasteAndConvert();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.C)) {
                CopyTextToClipboard();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            ContinueConvert();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartConvert();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CopyTextToClipboard();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            StartInput();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RestoreText();
        }

        private void MainWindow_ResizeEnd(object sender, EventArgs e)
        {
            SaveMainWindowSize();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        private void foreColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeForeColorSetting();
        }

        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeBackColorSetting();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PasteAndConvert();
        }

        private void FontsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeFontSetting();
        }

        private void wordwrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeWordwrapSetting();
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            AdjustControlsPositions();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearText();
        }
    }






    /// <summary>
    /// transparent for mouse event
    /// </summary>
    public class TransparentPictureBox : PictureBox {

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST) {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else {
                base.WndProc(ref m);
            }
        }

    }

    public class TransparentLabel : Label {

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST) {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else {
                base.WndProc(ref m);
            }
        }

    }

}
