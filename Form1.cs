using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Net.Http;
using Newtonsoft.Json.Linq;

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

        private void update()
        {
            //check for updates from github releases. if yes then install silently and notify user to restart the app
            string url = "https://api.github.com/repos/mvishok/shoot-scoot/releases/latest";
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FADE/1.0");
            string response = client.GetStringAsync(url).Result;
            Version nv = new Version(response.Split(new string[] { "\"tag_name\":\"" }, StringSplitOptions.None)[1].Split('"')[0]);
            Version cv = new Version("0.1.0");
            if (nv > cv)
            {
                try
                {
                    //download the latest release
                    string downloadUrl = response.Split(new string[] { "\"browser_download_url\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
                    string downloadPath = Path.Combine(Path.GetTempPath(), "shoot-scoot-update.exe");
                    new System.Net.WebClient().DownloadFile(downloadUrl, downloadPath);

                    //install the update
                    System.Diagnostics.Process.Start(downloadPath, "/VERYSILENT /FORCECLOSEAPPLICATIONS /RESTARTAPPLICATIONS");
                } catch (Exception e)
                {
                    MessageBox.Show("Error updating the application: " + e.Message, "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void Form1_LoadAsync(object sender, EventArgs e)
        {
            this.TopMost = true;

            // Prevent multiple instances
            if (IsAnotherInstanceRunning())
            {
                MessageBox.Show("Another instance of the application is already running", "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }

            update();

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

        private bool IsAnotherInstanceRunning()
        {
            return System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1;
        }

        private void HandleScreenshotHotkey()
        {
            TakeScreenshot();

            (int width, int height) = GetResizedDimensions(Path.Combine(PATH, $"{count - 1}.png"), 300, 300);

            pictureBox1.Size = new Size(width, height);
            this.Size = new Size(width, height);

            pictureBox1.Image = Image.FromFile(Path.Combine(PATH, $"{count - 1}.png"));
            shortcut.Hide();
            ShowForm();

            Task.Delay(500).Wait();

            DisposeImage();

            MinimizeForm();

            RegisterHotKeys();
        }

        private void HandleSaveHotkey()
        {
            if (count == 0)
                return;

            string pdfPath = CompilePDF();

            if (pdfPath != null)
            {
                MessageBox.Show($"PDF saved to {pdfPath}", "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            } else
            {
                MessageBox.Show("Error saving PDF", "Shoot&Scoot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowForm()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();
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
                    doc = PdfSharp.Pdf.IO.PdfReader.Open(filePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify);
                else
                    doc = new PdfDocument();

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
