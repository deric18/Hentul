namespace HentulWinforms
{
    using Common;
    using Hentul;
    using Hentul.Hippocampal_Entorinal_complex;

    public partial class Form2 : Form
    {
        private Hentul.Orchestrator? _orchestrator;
        private string _lastError = string.Empty;

        // Direct data pushed from Form1 after Explore completes (label, x, y, w, h)
        private List<(string label, int x, int y, int w, int h)> _directObjects = new();

        // One colour per object label, assigned on first encounter
        private readonly Dictionary<string, Color> _objectColours = new();

        private static readonly Color[] _palette = new[]
        {
            Color.FromArgb(255, 80,  80),
            Color.FromArgb(80,  200, 80),
            Color.FromArgb(80,  80,  255),
            Color.FromArgb(255, 200, 0),
            Color.FromArgb(0,   200, 220),
            Color.FromArgb(220, 80,  220),
            Color.FromArgb(255, 140, 0),
            Color.FromArgb(160, 255, 80),
            Color.FromArgb(80,  160, 255),
            Color.FromArgb(255, 100, 180),
        };

        public Form2()
        {
            InitializeComponent();
            try { _orchestrator = Hentul.Orchestrator.GetInstance(); } catch { }
        }

        // ──────────────────────────────────────────────────────────────────
        // Public API called by Form1
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by Form1 immediately after Explore completes.
        /// Passes detected regions directly so the display works even if the
        /// HC→Graph pipeline has not stored the bounds yet.
        /// </summary>
        public void ShowObjects(List<(string label, int x, int y, int w, int h)> objects)
        {
            _directObjects = objects ?? new List<(string, int, int, int, int)>();
            _objectColours.Clear();
            DrawGraph();

            if (!Visible) Show();
            BringToFront();
        }

        // ──────────────────────────────────────────────────────────────────
        // Form events
        // ──────────────────────────────────────────────────────────────────

        private void Form2_Load(object sender, EventArgs e) => DrawGraph();

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _objectColours.Clear();
            DrawGraph();
        }

        private void PictureBoxGraph_Resize(object sender, EventArgs e) => DrawGraph();

        private void PictureBoxGraph_MouseMove(object sender, MouseEventArgs e)
        {
            var env = GetEnvironmentBounds();
            if (env == null || pictureBoxGraph.Width == 0 || pictureBoxGraph.Height == 0) return;
            int envX = (int)((double)e.X / pictureBoxGraph.Width  * env.ScreenWidth);
            int envY = (int)((double)e.Y / pictureBoxGraph.Height * env.ScreenHeight);
            lblCoords.Text = $"Env: ({envX}, {envY})   PBox: ({e.X}, {e.Y})";
        }

        // ──────────────────────────────────────────────────────────────────
        // Core drawing
        // ──────────────────────────────────────────────────────────────────

        private void DrawGraph()
        {
            if (pictureBoxGraph.Width <= 0 || pictureBoxGraph.Height <= 0) return;

            var bmp = new Bitmap(pictureBoxGraph.Width, pictureBoxGraph.Height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(18, 22, 32));   // dark navy — distinguishable from pure black

            var env     = GetEnvironmentBounds();
            var objects = GetDisplayObjects();

            // Pre-compute the global coordinate range across ALL objects so every
            // object is scaled relative to the same space. This ensures objects
            // detected on a secondary screen (X > primary-screen-width) are still
            // rendered correctly within the PictureBox, not clipped off-screen.
            int minX = 0, minY = 0, rangeX = 1, rangeY = 1;
            if (objects.Count > 0)
            {
                minX   = objects.Min(o => o.x);
                minY   = objects.Min(o => o.y);
                int maxXval = objects.Max(o => o.x + o.w);
                int maxYval = objects.Max(o => o.y + o.h);
                rangeX = Math.Max(1, maxXval - minX);
                rangeY = Math.Max(1, maxYval - minY);
            }

            DrawGrid(g, env);

            if (!string.IsNullOrEmpty(_lastError))
            {
                using var errFont = new Font("Segoe UI", 9);
                g.DrawString($"Pipeline: {_lastError}", errFont, Brushes.OrangeRed,
                    new RectangleF(6, 6, bmp.Width - 12, 50));
            }

            if (objects.Count == 0)
            {
                DrawNoDataMessage(g, bmp.Width, bmp.Height);
            }
            else
            {
                foreach (var obj in objects)
                    DrawObject(g, obj.label, obj.x, obj.y, obj.w, obj.h, env,
                               minX, minY, rangeX, rangeY);
            }

            UpdateInfoLabels(objects.Count, env);

            var old = pictureBoxGraph.Image;
            pictureBoxGraph.Image = bmp;
            old?.Dispose();
        }

        private void DrawNoDataMessage(Graphics g, int w, int h)
        {
            // Gather diagnostics
            int hcCount    = -1;
            int graphCache = -1;
            int directCount = _directObjects.Count;
            try
            {
                hcCount    = _orchestrator?.HCAccessor?.Objects?.Count ?? -1;
                graphCache = Graph._graph?.GetAllObjectsInEnvironment()?.Count ?? -1;
            }
            catch { }

            string msg =
                "No objects to display.\n\n" +
                $"  Direct objects (from Explore):  {directCount}\n" +
                $"  HC.Objects dict:                {hcCount}\n" +
                $"  Graph._objectBoundsCache:       {graphCache}\n\n" +
                "Run Explore first, then click  Refresh  (or it auto-updates when you run Explore with this window open).";

            using var font  = new Font("Segoe UI", 11, FontStyle.Regular);
            using var brush = new SolidBrush(Color.FromArgb(230, 235, 255));  // near-white with slight blue tint
            g.DrawString(msg, font, brush, new RectangleF(20, 30, w - 40, h - 60));
        }

        private void DrawGrid(Graphics g, EnvironmentBounds? env)
        {
            int w = pictureBoxGraph.Width;
            int h = pictureBoxGraph.Height;
            using var gridPen  = new Pen(Color.FromArgb(55, 65, 90), 1);    // blue-tinted grid
            using var axisPen  = new Pen(Color.FromArgb(100, 115, 150), 1); // brighter axis lines
            using var font     = new Font("Segoe UI", 7);
            using var axisLblBrush = new SolidBrush(Color.FromArgb(180, 190, 210)); // visible axis labels

            int cols = 20, rows = 15;
            float cs = (float)w / cols, rs = (float)h / rows;

            for (int c = 0; c <= cols; c++)
            {
                float x = c * cs;
                g.DrawLine(c == 0 ? axisPen : gridPen, x, 0, x, h);
                if (env != null && c > 0 && c % 4 == 0)
                    g.DrawString(((int)((double)c / cols * env.ScreenWidth)).ToString(),
                        font, axisLblBrush, x + 2, 2);
            }
            for (int r = 0; r <= rows; r++)
            {
                float y = r * rs;
                g.DrawLine(r == 0 ? axisPen : gridPen, 0, y, w, y);
                if (env != null && r > 0 && r % 3 == 0)
                    g.DrawString(((int)((double)r / rows * env.ScreenHeight)).ToString(),
                        font, axisLblBrush, 2, y + 2);
            }
        }

        private void DrawObject(Graphics g, string label, int objX, int objY, int objW, int objH,
                                 EnvironmentBounds? env,
                                 int minX, int minY, int rangeX, int rangeY)
        {
            var color = GetColourForLabel(label);
            var rect  = ToScreenRect(objX, objY, objW, objH, minX, minY, rangeX, rangeY);
            if (rect.Width  < 20) rect.Width  = 20;
            if (rect.Height < 20) rect.Height = 20;

            // Semi-transparent fill — use higher alpha so it's visible on dark background
            using var fill   = new SolidBrush(Color.FromArgb(100, color));
            using var border = new Pen(color, 3);
            g.FillRectangle(fill, rect);
            g.DrawRectangle(border, rect);

            // Corner ticks
            int m = 10;
            using var cp = new Pen(color, 3);
            g.DrawLine(cp, rect.Left,  rect.Top,    rect.Left  + m, rect.Top);
            g.DrawLine(cp, rect.Left,  rect.Top,    rect.Left,      rect.Top    + m);
            g.DrawLine(cp, rect.Right, rect.Top,    rect.Right - m, rect.Top);
            g.DrawLine(cp, rect.Right, rect.Top,    rect.Right,     rect.Top    + m);
            g.DrawLine(cp, rect.Left,  rect.Bottom, rect.Left  + m, rect.Bottom);
            g.DrawLine(cp, rect.Left,  rect.Bottom, rect.Left,      rect.Bottom - m);
            g.DrawLine(cp, rect.Right, rect.Bottom, rect.Right - m, rect.Bottom);
            g.DrawLine(cp, rect.Right, rect.Bottom, rect.Right,     rect.Bottom - m);

            // Label with colour-tinted pill background so it's readable on any dark backdrop
            using var labelFont = new Font("Segoe UI", 9, FontStyle.Bold);
            var ts = g.MeasureString(label, labelFont);
            float tx = rect.Left + (rect.Width  - ts.Width)  / 2f;
            float ty = rect.Top  + 4f;

            using var pillBrush = new SolidBrush(Color.FromArgb(160, color.R / 5, color.G / 5, color.B / 5 + 20));
            g.FillRectangle(pillBrush, tx - 4, ty - 1, ts.Width + 8, ts.Height + 2);
            using var textBrush = new SolidBrush(color);
            g.DrawString(label, labelFont, textBrush, tx, ty);

            // Coordinates — white text, tinted background pill
            using var coordFont = new Font("Segoe UI", 7);
            string coord = $"({objX},{objY})  {objW}×{objH}";
            var cs2 = g.MeasureString(coord, coordFont);
            float cx = rect.Left + (rect.Width - cs2.Width) / 2f;
            float cy = rect.Bottom - cs2.Height - 4;
            if (cy > ty + ts.Height + 4)
            {
                using var cb = new SolidBrush(Color.FromArgb(150, color.R / 5, color.G / 5, color.B / 5 + 20));
                g.FillRectangle(cb, cx - 3, cy - 1, cs2.Width + 6, cs2.Height + 2);
                using var coordBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                g.DrawString(coord, coordFont, coordBrush, cx, cy);
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Data retrieval — direct > HC Graph fallback
        // ──────────────────────────────────────────────────────────────────

        private List<(string label, int x, int y, int w, int h)> GetDisplayObjects()
        {
            _lastError = string.Empty;

            // ── Primary: direct data pushed from Form1 after Explore ──────
            if (_directObjects.Count > 0)
                return new List<(string, int, int, int, int)>(_directObjects);

            // ── Fallback: HC→Graph bounds cache ──────────────────────────
            try
            {
                var bounds = _orchestrator?.HCAccessor?.GetAllObjectsInEnvironment()
                          ?? Graph._graph?.GetAllObjectsInEnvironment();

                if (bounds != null && bounds.Count > 0)
                {
                    return bounds
                        .Select(b => (b.Label, b.MinX, b.MinY, b.Width, b.Height))
                        .ToList();
                }
            }
            catch (Exception ex) { _lastError = ex.Message; }

            return new List<(string, int, int, int, int)>();
        }

        private EnvironmentBounds? GetEnvironmentBounds()
        {
            try { return _orchestrator?.HCAccessor?.GetEnvironmentBounds(); }
            catch (Exception ex) { _lastError = ex.Message; return null; }
        }

        // ──────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps object coordinates to PictureBox pixels using relative scaling so
        /// that all objects always fit within the visible area, regardless of which
        /// screen they were detected on (primary vs secondary).
        /// </summary>
        private Rectangle ToScreenRect(int objX, int objY, int objW, int objH,
                                        int minX, int minY, int rangeX, int rangeY)
        {
            int pbW = pictureBoxGraph.Width;
            int pbH = pictureBoxGraph.Height;
            const int margin = 50;   // px padding on all sides

            int drawW = pbW - margin * 2;
            int drawH = pbH - margin * 2;

            int sx = margin + (int)((double)(objX - minX) / rangeX * drawW);
            int sy = margin + (int)((double)(objY - minY) / rangeY * drawH);
            int sw = Math.Max(20, (int)((double)Math.Max(1, objW) / rangeX * drawW));
            int sh = Math.Max(20, (int)((double)Math.Max(1, objH) / rangeY * drawH));

            return new Rectangle(sx, sy, sw, sh);
        }

        private Color GetColourForLabel(string label)
        {
            if (_objectColours.TryGetValue(label, out var c)) return c;
            var colour = _palette[_objectColours.Count % _palette.Length];
            _objectColours[label] = colour;
            return colour;
        }

        private void UpdateInfoLabels(int count, EnvironmentBounds? env)
        {
            lblObjectCount.Text = $"Objects: {count}";
            lblEnvSize.Text = env != null
                ? $"Environment: {env.ScreenWidth} × {env.ScreenHeight}"
                : "Environment: not initialised";
        }
    }
}
