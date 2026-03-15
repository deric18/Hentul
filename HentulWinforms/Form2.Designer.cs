namespace HentulWinforms
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnRefresh = new Button();
            lblObjectCount = new Label();
            lblEnvSize = new Label();
            pictureBoxGraph = new PictureBox();
            lblCoords = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGraph).BeginInit();
            SuspendLayout();
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(6, 5);
            btnRefresh.Margin = new Padding(3, 4, 3, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 29);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // lblObjectCount
            // 
            lblObjectCount.AutoSize = true;
            lblObjectCount.Location = new Point(130, 12);
            lblObjectCount.Name = "lblObjectCount";
            lblObjectCount.Size = new Size(70, 19);
            lblObjectCount.TabIndex = 1;
            lblObjectCount.Text = "Objects: 0";
            // 
            // lblEnvSize
            // 
            lblEnvSize.AutoSize = true;
            lblEnvSize.Location = new Point(280, 12);
            lblEnvSize.Name = "lblEnvSize";
            lblEnvSize.Size = new Size(175, 19);
            lblEnvSize.TabIndex = 2;
            lblEnvSize.Text = "Environment: not initialised";
            // 
            // pictureBoxGraph
            // 
            pictureBoxGraph.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBoxGraph.BackColor = Color.FromArgb(20, 20, 20);
            pictureBoxGraph.Location = new Point(0, 42);
            pictureBoxGraph.Name = "pictureBoxGraph";
            pictureBoxGraph.Size = new Size(1980, 961);
            pictureBoxGraph.TabIndex = 3;
            pictureBoxGraph.TabStop = false;
            pictureBoxGraph.MouseMove += PictureBoxGraph_MouseMove;
            pictureBoxGraph.Resize += PictureBoxGraph_Resize;
            // 
            // lblCoords
            // 
            lblCoords.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCoords.AutoSize = true;
            lblCoords.Location = new Point(6, 1009);
            lblCoords.Name = "lblCoords";
            lblCoords.Size = new Size(0, 19);
            lblCoords.TabIndex = 4;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1980, 1031);
            Controls.Add(lblCoords);
            Controls.Add(pictureBoxGraph);
            Controls.Add(lblEnvSize);
            Controls.Add(lblObjectCount);
            Controls.Add(btnRefresh);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(600, 400);
            Name = "Form2";
            Text = "Graph Network Visualiser";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxGraph).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnRefresh;
        private Label lblObjectCount;
        private Label lblEnvSize;
        private PictureBox pictureBoxGraph;
        private Label lblCoords;
    }
}
