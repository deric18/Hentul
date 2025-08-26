
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
            V1Gray = new PictureBox();
            labelX = new Label();
            labelY = new Label();
            Start = new Button();
            V1White = new PictureBox();
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
            V2White = new PictureBox();
            V2Gray = new PictureBox();
            V3White = new PictureBox();
            V3Gray = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)V1Gray).BeginInit();
            ((System.ComponentModel.ISupportInitialize)V1White).BeginInit();
            ((System.ComponentModel.ISupportInitialize)V2White).BeginInit();
            ((System.ComponentModel.ISupportInitialize)V2Gray).BeginInit();
            ((System.ComponentModel.ISupportInitialize)V3White).BeginInit();
            ((System.ComponentModel.ISupportInitialize)V3Gray).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(434, 387);
            StartButton.Margin = new Padding(3, 4, 3, 4);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(155, 31);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // V1Gray
            // 
            V1Gray.Location = new Point(815, 106);
            V1Gray.Margin = new Padding(3, 4, 3, 4);
            V1Gray.Name = "V1Gray";
            V1Gray.Size = new Size(130, 43);
            V1Gray.TabIndex = 1;
            V1Gray.TabStop = false;
            V1Gray.Click += CurrentImage_Click;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(826, 695);
            labelX.Name = "labelX";
            labelX.Size = new Size(51, 20);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(975, 695);
            labelY.Name = "labelY";
            labelY.Size = new Size(50, 20);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            labelY.Click += labelY_Click;
            // 
            // Start
            // 
            Start.Location = new Point(434, 329);
            Start.Margin = new Padding(3, 4, 3, 4);
            Start.Name = "Start";
            Start.Size = new Size(155, 31);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // V1White
            // 
            V1White.Location = new Point(1083, 106);
            V1White.Margin = new Padding(3, 4, 3, 4);
            V1White.Name = "V1White";
            V1White.Size = new Size(130, 43);
            V1White.TabIndex = 5;
            V1White.TabStop = false;
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(239, 35);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(89, 20);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(35, 35);
            label_done.Name = "label_done";
            label_done.Size = new Size(79, 20);
            label_done.TabIndex = 8;
            label_done.Text = "Processing";
            label_done.Click += label1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(49, 1147);
            label1.Name = "label1";
            label1.Size = new Size(62, 20);
            label1.TabIndex = 9;
            label1.Text = "CYCLE #";
            label1.Click += label1_Click_1;
            // 
            // CycleLabel
            // 
            CycleLabel.AutoSize = true;
            CycleLabel.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            CycleLabel.Location = new Point(176, 1147);
            CycleLabel.Name = "CycleLabel";
            CycleLabel.Size = new Size(17, 20);
            CycleLabel.TabIndex = 10;
            CycleLabel.Text = "0";
            CycleLabel.Click += CycleLabel_Click;
            // 
            // labelBox
            // 
            labelBox.Location = new Point(1390, 899);
            labelBox.Margin = new Padding(3, 4, 3, 4);
            labelBox.Name = "labelBox";
            labelBox.Size = new Size(114, 27);
            labelBox.TabIndex = 11;
            // 
            // wanderingButton
            // 
            wanderingButton.Location = new Point(434, 452);
            wanderingButton.Margin = new Padding(3, 4, 3, 4);
            wanderingButton.Name = "wanderingButton";
            wanderingButton.Size = new Size(155, 31);
            wanderingButton.TabIndex = 12;
            wanderingButton.Text = "Test Burst Prevention Algo";
            wanderingButton.UseVisualStyleBackColor = true;
            wanderingButton.Click += wanderingButton_Click;
            // 
            // BackUp
            // 
            BackUp.Location = new Point(434, 872);
            BackUp.Margin = new Padding(3, 4, 3, 4);
            BackUp.Name = "BackUp";
            BackUp.Size = new Size(155, 31);
            BackUp.TabIndex = 13;
            BackUp.Text = "Backup";
            BackUp.UseVisualStyleBackColor = true;
            BackUp.Click += BackUp_Click;
            // 
            // Restore
            // 
            Restore.Location = new Point(434, 939);
            Restore.Margin = new Padding(3, 4, 3, 4);
            Restore.Name = "Restore";
            Restore.Size = new Size(155, 31);
            Restore.TabIndex = 14;
            Restore.Text = "Restore";
            Restore.UseVisualStyleBackColor = true;
            Restore.Click += button1_Click_1;
            // 
            // train_another_object
            // 
            train_another_object.Location = new Point(434, 536);
            train_another_object.Margin = new Padding(3, 4, 3, 4);
            train_another_object.Name = "train_another_object";
            train_another_object.Size = new Size(155, 31);
            train_another_object.TabIndex = 15;
            train_another_object.Text = "Train Another Object";
            train_another_object.UseVisualStyleBackColor = true;
            train_another_object.Click += train_another_object_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(1686, 149);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(114, 27);
            textBox1.TabIndex = 16;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1509, 155);
            label2.Name = "label2";
            label2.Size = new Size(119, 20);
            label2.TabIndex = 17;
            label2.Text = "Enter Icon Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1686, 236);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(123, 31);
            button1.TabIndex = 18;
            button1.Text = "Train Words";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_3;
            // 
            // V2White
            // 
            V2White.Location = new Point(1083, 169);
            V2White.Margin = new Padding(3, 4, 3, 4);
            V2White.Name = "V2White";
            V2White.Size = new Size(185, 142);
            V2White.TabIndex = 20;
            V2White.TabStop = false;
            // 
            // V2Gray
            // 
            V2Gray.Location = new Point(815, 169);
            V2Gray.Margin = new Padding(3, 4, 3, 4);
            V2Gray.Name = "V2Gray";
            V2Gray.Size = new Size(185, 142);
            V2Gray.TabIndex = 19;
            V2Gray.TabStop = false;
            // 
            // V3White
            // 
            V3White.Location = new Point(1083, 329);
            V3White.Margin = new Padding(3, 4, 3, 4);
            V3White.Name = "V3White";
            V3White.Size = new Size(240, 209);
            V3White.TabIndex = 22;
            V3White.TabStop = false;
            // 
            // V3Gray
            // 
            V3Gray.Location = new Point(815, 329);
            V3Gray.Margin = new Padding(3, 4, 3, 4);
            V3Gray.Name = "V3Gray";
            V3Gray.Size = new Size(240, 209);
            V3Gray.TabIndex = 21;
            V3Gray.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1924, 1055);
            Controls.Add(V3White);
            Controls.Add(V3Gray);
            Controls.Add(V2White);
            Controls.Add(V2Gray);
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
            Controls.Add(V1White);
            Controls.Add(Start);
            Controls.Add(labelY);
            Controls.Add(labelX);
            Controls.Add(V1Gray);
            Controls.Add(StartButton);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)V1Gray).EndInit();
            ((System.ComponentModel.ISupportInitialize)V1White).EndInit();
            ((System.ComponentModel.ISupportInitialize)V2White).EndInit();
            ((System.ComponentModel.ISupportInitialize)V2Gray).EndInit();
            ((System.ComponentModel.ISupportInitialize)V3White).EndInit();
            ((System.ComponentModel.ISupportInitialize)V3Gray).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void Init()
        {
            throw new NotImplementedException();
        }

        #endregion

        private Button StartButton;
        private PictureBox V1Gray;
        private Label labelX;
        private Label labelY;
        private Button Start;
        private PictureBox V1White;
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
        private PictureBox V2White;
        private PictureBox V2Gray;
        private PictureBox V3White;
        private PictureBox V3Gray;
    }
}
