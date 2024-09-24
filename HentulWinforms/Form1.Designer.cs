
using System.Configuration;

namespace HentulWinforms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartButton = new Button();
            CurrentImage = new PictureBox();
            labelX = new Label();
            labelY = new Label();
            recache = new Button();
            EdgedImage = new PictureBox();
            readyLabel = new Label();
            ObjectLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)CurrentImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)EdgedImage).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(380, 355);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(75, 23);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Cycle";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CurrentImage
            // 
            CurrentImage.Location = new Point(723, 210);
            CurrentImage.Name = "CurrentImage";
            CurrentImage.Size = new Size(250, 168);
            CurrentImage.TabIndex = 1;
            CurrentImage.TabStop = false;
            CurrentImage.Click += CurrentImage_Click;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(617, 521);
            labelX.Name = "labelX";
            labelX.Size = new Size(39, 15);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(768, 522);
            labelY.Name = "labelY";
            labelY.Size = new Size(39, 15);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            // 
            // recache
            // 
            recache.Location = new Point(380, 247);
            recache.Name = "recache";
            recache.Size = new Size(75, 23);
            recache.TabIndex = 4;
            recache.Text = "Recache Cursor image";
            recache.UseVisualStyleBackColor = true;
            recache.Click += button1_Click;
            // 
            // EdgedImage
            // 
            EdgedImage.Location = new Point(1059, 210);
            EdgedImage.Name = "EdgedImage";
            EdgedImage.Size = new Size(250, 168);
            EdgedImage.TabIndex = 5;
            EdgedImage.TabStop = false;
            // 
            // readyLabel
            // 
            readyLabel.AutoSize = true;
            readyLabel.Location = new Point(551, 247);
            readyLabel.Name = "readyLabel";
            readyLabel.Size = new Size(38, 15);
            readyLabel.TabIndex = 6;
            readyLabel.Text = "label1";
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(1105, 521);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(70, 15);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1705, 903);
            Controls.Add(ObjectLabel);
            Controls.Add(readyLabel);
            Controls.Add(EdgedImage);
            Controls.Add(recache);
            Controls.Add(labelY);
            Controls.Add(labelX);
            Controls.Add(CurrentImage);
            Controls.Add(StartButton);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)CurrentImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)EdgedImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void Init()
        {
            throw new NotImplementedException();
        }

        #endregion

        private Button StartButton;
        private PictureBox CurrentImage;
        private Label labelX;
        private Label labelY;
        private Button recache;
        private PictureBox EdgedImage;
        private Label readyLabel;
        private Label ObjectLabel;
    }
}
