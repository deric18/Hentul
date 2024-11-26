
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
            Start = new Button();
            EdgedImage = new PictureBox();
            ObjectLabel = new Label();
            label_done = new Label();
            label1 = new Label();
            CycleLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)CurrentImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)EdgedImage).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(380, 290);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(136, 23);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
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
            labelX.Location = new Point(398, 508);
            labelX.Name = "labelX";
            labelX.Size = new Size(39, 15);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(499, 508);
            labelY.Name = "labelY";
            labelY.Size = new Size(39, 15);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            labelY.Click += labelY_Click;
            // 
            // Start
            // 
            Start.Location = new Point(380, 247);
            Start.Name = "Start";
            Start.Size = new Size(136, 23);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // EdgedImage
            // 
            EdgedImage.Location = new Point(1059, 210);
            EdgedImage.Name = "EdgedImage";
            EdgedImage.Size = new Size(250, 168);
            EdgedImage.TabIndex = 5;
            EdgedImage.TabStop = false;
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
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(31, 26);
            label_done.Name = "label_done";
            label_done.Size = new Size(64, 15);
            label_done.TabIndex = 8;
            label_done.Text = "Processing";
            label_done.Click += label1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(43, 860);
            label1.Name = "label1";
            label1.Size = new Size(52, 15);
            label1.TabIndex = 9;
            label1.Text = "CYCLE #";
            label1.Click += label1_Click_1;
            // 
            // CycleLabel
            // 
            CycleLabel.AutoSize = true;
            CycleLabel.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            CycleLabel.Location = new Point(154, 860);
            CycleLabel.Name = "CycleLabel";
            CycleLabel.Size = new Size(13, 15);
            CycleLabel.TabIndex = 10;
            CycleLabel.Text = "0";
            CycleLabel.Click += CycleLabel_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1705, 903);
            Controls.Add(CycleLabel);
            Controls.Add(label1);
            Controls.Add(label_done);
            Controls.Add(ObjectLabel);
            Controls.Add(EdgedImage);
            Controls.Add(Start);
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
        private Button Start;
        private PictureBox EdgedImage;
        private Label ObjectLabel;
        private Label label_done;
        private Label label1;
        private Label CycleLabel;
    }
}
