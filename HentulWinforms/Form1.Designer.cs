
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
            labelBox = new TextBox();
            wanderingButton = new Button();
            BackUp = new Button();
            Restore = new Button();
            train_another_object = new Button();
            textBox1 = new TextBox();
            label2 = new Label();
            button1 = new Button();
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
            labelX.Location = new Point(723, 521);
            labelX.Name = "labelX";
            labelX.Size = new Size(39, 15);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(853, 521);
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
            ObjectLabel.Location = new Point(209, 26);
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
            // labelBox
            // 
            labelBox.Location = new Point(1216, 674);
            labelBox.Name = "labelBox";
            labelBox.Size = new Size(100, 23);
            labelBox.TabIndex = 11;
            // 
            // wanderingButton
            // 
            wanderingButton.Location = new Point(380, 339);
            wanderingButton.Name = "wanderingButton";
            wanderingButton.Size = new Size(136, 23);
            wanderingButton.TabIndex = 12;
            wanderingButton.Text = "Test Burst Prevention Algo";
            wanderingButton.UseVisualStyleBackColor = true;
            wanderingButton.Click += wanderingButton_Click;
            // 
            // BackUp
            // 
            BackUp.Location = new Point(380, 654);
            BackUp.Name = "BackUp";
            BackUp.Size = new Size(136, 23);
            BackUp.TabIndex = 13;
            BackUp.Text = "Backup";
            BackUp.UseVisualStyleBackColor = true;
            BackUp.Click += BackUp_Click;
            // 
            // Restore
            // 
            Restore.Location = new Point(380, 704);
            Restore.Name = "Restore";
            Restore.Size = new Size(136, 23);
            Restore.TabIndex = 14;
            Restore.Text = "Restore";
            Restore.UseVisualStyleBackColor = true;
            Restore.Click += button1_Click_1;
            // 
            // train_another_object
            // 
            train_another_object.Location = new Point(380, 402);
            train_another_object.Name = "train_another_object";
            train_another_object.Size = new Size(136, 23);
            train_another_object.TabIndex = 15;
            train_another_object.Text = "Train Another Object";
            train_another_object.UseVisualStyleBackColor = true;
            train_another_object.Click += train_another_object_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(1475, 112);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 16;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1320, 116);
            label2.Name = "label2";
            label2.Size = new Size(95, 15);
            label2.TabIndex = 17;
            label2.Text = "Enter Icon Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1475, 177);
            button1.Name = "button1";
            button1.Size = new Size(108, 23);
            button1.TabIndex = 18;
            button1.Text = "Train Words";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1705, 903);
            Controls.Add(button1);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(train_another_object);
            Controls.Add(Restore);
            Controls.Add(BackUp);
            Controls.Add(wanderingButton);
            Controls.Add(labelBox);
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
        private TextBox labelBox;
        private Button wanderingButton;
        private Button BackUp;
        private Button Restore;
        private Button train_another_object;
        private TextBox textBox1;
        private Label label2;
        private Button button1;
    }
}
