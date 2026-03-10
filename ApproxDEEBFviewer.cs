using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using RedstoneShell.ApproxD;

public class EEBFViewer : Form
{
    private EEBF8x8Font font;
    private int selected = 0;
    private string currentPath;

    public EEBFViewer(string path)
    {
        this.currentPath = path;
        this.Text = "EEBF Font Viewer - " + Path.GetFileName(path);
        this.ClientSize = new Size(800, 600);
        this.DoubleBuffered = true;
        this.KeyPreview = true;

        try {
            font = EEBF8x8Font.Load(path);
        } catch (Exception ex) {
            MessageBox.Show("Error loading font: " + ex.Message);
            Application.Exit();
        }

        this.MouseClick += new MouseEventHandler(ViewerClick);
        this.KeyDown += new KeyEventHandler(ViewerKeyDown);
    }

    private void ViewerClick(object sender, MouseEventArgs e)
    {
        int cols = 16;
        int cell = 32;

        if (e.X < 512 && e.Y < 512) {
            int x = e.X / cell;
            int y = e.Y / cell;
            int idx = y * cols + x;
            if (idx >= 0 && idx < 256) {
                selected = idx;
                this.Invalidate();
            }
        }

        int gridX = 550;
        int gridY = 50;
        int pSize = 25;

        if (e.X >= gridX && e.X < gridX + (8 * pSize) &&
            e.Y >= gridY && e.Y < gridY + (8 * pSize)) 
        {
            int pX = (e.X - gridX) / pSize;
            int pY = (e.Y - gridY) / pSize;

            byte currentByte = font.Glyphs[selected, pY];
            font.Glyphs[selected, pY] = (byte)(currentByte ^ (1 << pX));
            this.Invalidate();
        }
    }

    private void ViewerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Right) { selected = (selected + 1) % 256; Invalidate(); }
        if (e.KeyCode == Keys.Left) { selected = (selected + 255) % 256; Invalidate(); }

        if (e.Control && e.KeyCode == Keys.S) {
            SaveFont();
        }

        if (e.Control && e.KeyCode == Keys.C) {
            CopyGlyphToClipboard();
        }

        if (e.Control && e.KeyCode == Keys.V) {
            PasteGlyphFromClipboard();
        }

        if (e.Control && e.KeyCode == Keys.F) ExportToCode();
    }

    private void CopyGlyphToClipboard()
    {
        StringBuilder sb = new StringBuilder();
        for (int r = 0; r < 8; r++)
        {
            sb.Append(font.Glyphs[selected, r].ToString("X2"));
        }
        Clipboard.SetText("EEBF:" + sb.ToString());
    }

    private void PasteGlyphFromClipboard()
    {
        string data = Clipboard.GetText();
        if (data.StartsWith("EEBF:") && data.Length == 21)
        {
            string hex = data.Substring(5);
            try {
                for (int r = 0; r < 8; r++)
                {
                    string byteHex = hex.Substring(r * 2, 2);
                    font.Glyphs[selected, r] = Convert.ToByte(byteHex, 16);
                }
                Invalidate();
            } catch {}
        }
    }

    private void ExportToCode() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("public static byte[,] MyFont = {");
        for (int i = 0; i < 256; i++) {
            sb.Append("    { ");
            for (int r = 0; r < 8; r++) {
                sb.Append("0x" + font.Glyphs[i, r].ToString("X2"));
                if (r < 7) sb.Append(", ");
            }
            char c = (i > 32 && i < 127) ? (char)i : ' ';
            sb.AppendLine(" }, // Index: " + i + " '" + c + "'");
        }

        sb.AppendLine("};");

        string result = sb.ToString();
        if (!string.IsNullOrEmpty(result)) {
            Clipboard.SetText(result);
            MessageBox.Show("Font array (256 glyphs) exported to clipboard!");
        }
    }

    private void SaveFont()
    {
        try {
            font.Save(currentPath); 
            this.Text = "EEBF Font Viewer - " + Path.GetFileName(currentPath) + " (Saved!)";
        } catch (Exception ex) {
            MessageBox.Show("Save failed: " + ex.Message);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.Clear(Color.FromArgb(30, 30, 30));

        int cell = 32;
        int cols = 16;

        for (int i = 0; i < 256; i++)
        {
            int x = (i % cols) * cell;
            int y = (i / cols) * cell;

            if (i == selected) {
                g.FillRectangle(Brushes.DarkSlateBlue, x, y, cell, cell);
            }
            g.DrawRectangle(Pens.DimGray, x, y, cell, cell);

            DrawGlyph(g, x, y, i, 2);
        }
        string testText = "ABCDEFJHIJKLMNOPQRSTUVWXYZ1234567890_+-=_()[]abcdefghijklmnopqrstuvwxyz*";
        int startX = 550;
        int startY = 320; 
        int charWidth = 10; 
        int charsInRow = 15;

        for (int i = 0; i < testText.Length; i++)
        {
            int ascii = (int)testText[i];
            int px = startX + (i % charsInRow) * charWidth;
            int py = startY + (i / charsInRow) * 12;
            DrawGlyphPlain(g, px, py, ascii);
        }

        DrawEditor(g, 550, 50);
    }

    private void DrawGlyphPlain(Graphics g, int x, int y, int index)
    {
        if (index < 0 || index >= 256) return;

        for (int row = 0; row < 8; row++)
        {
            byte bits = font.Glyphs[index, row];
            for (int col = 0; col < 8; col++)
            {
                if ((bits & (1 << col)) != 0)
                {
                    g.FillRectangle(Brushes.White, x + col, y + row, 1, 1);
                }
            }
        }
    }

    private void DrawGlyph(Graphics g, int x, int y, int index, int pixelSize)
    {
        for (int row = 0; row < 8; row++)
        {
            byte bits = font.Glyphs[index, row];
            for (int col = 0; col < 8; col++)
            {
                if ((bits & (1 << col)) != 0)
                {
                    g.FillRectangle(Brushes.White, 
                        x + (col * pixelSize) + 8, 
                        y + (row * pixelSize) + 8, 
                        pixelSize, pixelSize);
                }
            }
        }
    }

    private void DrawEditor(Graphics g, int x, int y)
    {
        int pSize = 25;
        g.DrawString("Symbol: " + selected + " (0x" + selected.ToString("X2") + ")", 
            this.Font, Brushes.Yellow, x, y - 25);
        g.DrawString("Ctrl+S - Save", this.Font, Brushes.Gray, x, y + (8 * pSize) + 10);
        g.DrawString("Ctrl+C - Copy selected column", this.Font, Brushes.Gray, x, y + (8 * pSize) + 20);
        g.DrawString("Ctrl+V - Paste copied symbol to selected column", this.Font, Brushes.Gray, x, y + (8 * pSize) + 30);
        g.DrawString("Ctrl+F - Copy all font at byte[] C# list", this.Font, Brushes.Gray, x, y + (8 * pSize) + 40);

        for (int row = 0; row < 8; row++)
        {
            byte bits = font.Glyphs[selected, row];
            for (int col = 0; col < 8; col++)
            {
                bool isSet = (bits & (1 << col)) != 0;
                Rectangle rect = new Rectangle(x + col * pSize, y + row * pSize, pSize, pSize);
                
                g.FillRectangle(isSet ? Brushes.Lime : Brushes.Black, rect);
                g.DrawRectangle(Pens.Gray, rect);
            }
        }
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        string file = (args.Length > 0) ? args[0] : null;

        if (string.IsNullOrEmpty(file) || !File.Exists(file)) {
            MessageBox.Show("File not selected!");
            return;
        }

        Application.Run(new EEBFViewer(file));
    }
}