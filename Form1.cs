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
        string PATH = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        int count = 0;
        const uint MOD_ALT = 0x0001; // Alt key
        const uint VK_S = 0x53;      // S key
        const uint VK_INSERT = 0x2D; // Insert key

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

            //PREVENT MULTIPLE INSTANCES
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

            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, MOD_ALT, VK_INSERT);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID + 1, MOD_ALT, VK_S);
        }

        // Handle the hotkeys
        protected override void WndProc(ref Message m)
        {
            
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == MYACTION_HOTKEY_ID) // alt + insert
                {

                    TakeScreenshot();

                    //change pictureBox dimensions
                    (int width, int height) = GetResizedDimensions(Path.Combine(PATH, count - 1 + ".png"), 300, 300);
                    pictureBox1.Size = new Size(width, height);
                    //resize form
                    this.Size = new Size(width, height);

                    pictureBox1.Image = Image.FromFile(Path.Combine(PATH, count - 1 + ".png"));
                    shortcut.Hide();
                    vishok.Hide();
                    status1.Hide();
                    status2.Hide();

                    //show form
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.Show();
                    this.Activate();
                    this.BringToFront();

                    Task.Delay(500).Wait();
                    
                    //release and dispose image
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;

                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    this.Hide();

                    RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, MOD_ALT, VK_INSERT);
                    RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID + 1, MOD_ALT, VK_S);
                } 

                else if (id  == MYACTION_HOTKEY_ID + 1)
                {
                    if (count == 0)
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

                    string pdfPath = CompilePDF();
                    if (pdfPath != null)
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

                        shortcut.Text = "Thank you for using Shoot&Scoot!";

                        //show
                        this.WindowState = FormWindowState.Normal;
                        this.ShowInTaskbar = true;
                        this.Show();
                        this.Activate();
                        this.BringToFront();

                        Task.Delay(1500).Wait();

                        Directory.Delete(PATH, true);

                        Application.Exit();
                    }
                }
            }
            base.WndProc(ref m);
        }

        // Unregister the hotkeys when closing the form
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, MYACTION_HOTKEY_ID);
            UnregisterHotKey(this.Handle, MYACTION_HOTKEY_ID + 1);
        }

        private void TakeScreenshot()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                string filename = Path.Combine(PATH, count + ".png");
                try
                {
                    bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    count++;
                } catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

        }

        private string CompilePDF()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files|*.pdf";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (PdfDocument doc = new PdfDocument())
                {
                    foreach (var file in Directory.GetFiles(PATH, "*.png"))
                    {
                        PdfPage page = doc.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        XImage image = XImage.FromFile(file);

                        //aspect ratio of the image
                        double imageAspectRatio = (double)image.PixelWidth / image.PixelHeight;
                        double pageAspectRatio = (double)page.Width.Point / page.Height.Point; // Use .Point

                        double newWidth, newHeight;

                        if (imageAspectRatio > pageAspectRatio)
                        {
                            newWidth = page.Width.Point; // Use .Point
                            newHeight = newWidth / imageAspectRatio;
                        }
                        else
                        {
                            newHeight = page.Height.Point; // Use .Point
                            newWidth = newHeight * imageAspectRatio;
                        }

                        //to center the image on the page
                        double xOffset = (page.Width.Point - newWidth) / 2; // Use .Point
                        double yOffset = (page.Height.Point - newHeight) / 2; // Use .Point

                        gfx.DrawImage(image, xOffset, yOffset, newWidth, newHeight);
                    }
                    doc.Save(saveFileDialog.FileName);
                    return saveFileDialog.FileName;
                }
            }
            return null;
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
