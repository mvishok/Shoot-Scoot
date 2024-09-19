using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Shoot_Scoot
{
    public partial class Form1 : Form
    {
        private string PATH = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        private int count = 0;

        private const uint MOD_ALT = 0x0001; // Alt key
        private const uint VK_S = 0x53; // S key
        private const uint VK_INSERT = 0x2D; // Insert key

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int MYACTION_HOTKEY_ID = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            this.TopMost = true;

            // Prevent multiple instances
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show("Another instance of the application is already running", "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }

            if (!Directory.Exists(PATH))
            {
                Directory.CreateDirectory(PATH);
            }

            await Task.Delay(1200);
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            RegisterHotKeys();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();

                if (id == MYACTION_HOTKEY_ID) // Alt + Insert
                {
                    HandleScreenshotHotkey();
                }
                else if (id == MYACTION_HOTKEY_ID + 1) // Alt + S
                {
                    HandleSaveHotkey();
                }
            }

            base.WndProc(ref m);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKeys();
        }

        private void RegisterHotKeys()
        {
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, MOD_ALT, VK_INSERT);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID + 1, MOD_ALT, VK_S);
        }

        private void UnregisterHotKeys()
        {
            UnregisterHotKey(this.Handle, MYACTION_HOTKEY_ID);
            UnregisterHotKey(this.Handle, MYACTION_HOTKEY_ID + 1);
        }

        private void HandleScreenshotHotkey()
        {
            TakeScreenshot();

            (int width, int height) = GetResizedDimensions(Path.Combine(PATH, $"{count - 1}.png"), 300, 300);

            pictureBox1.Size = new Size(width, height);
            this.Size = new Size(width, height);
            shortcut.Hide();

            pictureBox1.Image = Image.FromFile(Path.Combine(PATH, $"{count - 1}.png"));

            ShowForm();

            Task.Delay(500).Wait();

            DisposeImage();

            MinimizeForm();

            RegisterHotKeys();

            shortcut.Show();
        }

        private void HandleSaveHotkey()
        {
            if (count == 0)
            {
                MessageBox.Show("No screenshots taken yet", "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RegisterHotKeys();
                return;
            }

            string pdfPath = CompilePDF();

            if (pdfPath != null)
            {
                InitializePictureBoxSize();
                ShowForm(true);

                Task.Delay(1500).Wait();

                Application.Exit();
            }
        }

        private void InitializePictureBoxSize()
        {
            pictureBox1.Size = new Size(300, 300);
            this.Size = new Size(300, 300);

            pictureBox1.Image = Shoot_Scoot.Properties.Resources.ss;

            shortcut.Show();
            vishok.Show();

            status1.Show();
            status2.Show();

            status2.Text = "INACTIVE";
            status2.ForeColor = Color.Red;

            shortcut.Text = "Exiting Shoot&Scoot";

            Task.Delay(800).Wait();
            
            Directory.Delete(PATH, true);
            Application.Exit();
        }

        private void ShowForm(bool tq = false)
        {
            if (tq)
            {
                shortcut.Text = "Thank you for using Shoot&Scoot!";
                ShowForm();
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Show();
                this.Activate();
                this.BringToFront();
            }
            this.Activate();
            this.BringToFront();
        }

        private void MinimizeForm()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void DisposeImage()
        {
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
        }

        private void TakeScreenshot()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                string filename = Path.Combine(PATH, $"{count}.png");
                try
                {
                    bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    count++;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private string CompilePDF()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                PdfDocument doc;

                if (File.Exists(filePath))
                {
                    doc = PdfSharp.Pdf.IO.PdfReader.Open(filePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify);
                }
                else
                {
                    doc = new PdfDocument();
                }

                AddScreenshotsToPDF(doc);

                doc.Save(filePath);
                return filePath;
            }

            return null;
        }

        private void AddScreenshotsToPDF(PdfDocument doc)
        {
            foreach (var file in Directory.GetFiles(PATH, "*.png"))
            {
                PdfPage page = doc.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XImage image = XImage.FromFile(file);

                double imageAspectRatio = (double)image.PixelWidth / image.PixelHeight;
                double pageAspectRatio = (double)page.Width.Point / page.Height.Point;

                double newWidth, newHeight;

                if (imageAspectRatio > pageAspectRatio)
                {
                    newWidth = page.Width.Point;
                    newHeight = newWidth / imageAspectRatio;
                }
                else
                {
                    newHeight = page.Height.Point;
                    newWidth = newHeight * imageAspectRatio;
                }

                double xOffset = (page.Width.Point - newWidth) / 2;
                double yOffset = (page.Height.Point - newHeight) / 2;

                gfx.DrawImage(image, xOffset, yOffset, newWidth, newHeight);
            }
        }

        public static (int width, int height) GetResizedDimensions(string imagePath, int maxWidth, int maxHeight)
        {
            using (Image image = Image.FromFile(imagePath))
            {
                float imageAspectRatio = (float)image.Width / image.Height;

                int width = maxWidth;
                int height = (int)(width / imageAspectRatio);

                if (height > maxHeight)
                {
                    height = maxHeight;
                    width = (int)(height * imageAspectRatio);
                }

                return (width, height);
            }
        }
    }
}