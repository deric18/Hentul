
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
            wanderingButton = new Button();
            BackUp = new Button();
            Restore = new Button();
            train_another_object = new Button();
            objectBox = new TextBox();
            label2 = new Label();
            button1 = new Button();
            startClassificationButton = new Button();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBox4 = new PictureBox();
            pictureBox5 = new PictureBox();
            pictureBox6 = new PictureBox();
            pictureBox7 = new PictureBox();
            pictureBox8 = new PictureBox();
            pictureBox9 = new PictureBox();
            pictureBox10 = new PictureBox();
            pictureBox11 = new PictureBox();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)CurrentImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)EdgedImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox10).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(291, 82);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(136, 23);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CurrentImage
            // 
            CurrentImage.Location = new Point(513, 90);
            CurrentImage.Name = "CurrentImage";
            CurrentImage.Size = new Size(250, 168);
            CurrentImage.TabIndex = 1;
            CurrentImage.TabStop = false;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(40, 759);
            labelX.Name = "labelX";
            labelX.Size = new Size(39, 15);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(108, 759);
            labelY.Name = "labelY";
            labelY.Size = new Size(39, 15);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            labelY.Click += labelY_Click;
            // 
            // Start
            // 
            Start.Location = new Point(291, 26);
            Start.Name = "Start";
            Start.Size = new Size(136, 23);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // EdgedImage
            // 
            EdgedImage.Location = new Point(915, 90);
            EdgedImage.Name = "EdgedImage";
            EdgedImage.Size = new Size(250, 168);
            EdgedImage.TabIndex = 5;
            EdgedImage.TabStop = false;
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(31, 90);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(70, 15);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(931, 30);
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
            // 
            // wanderingButton
            // 
            wanderingButton.Location = new Point(1448, 86);
            wanderingButton.Name = "wanderingButton";
            wanderingButton.Size = new Size(136, 23);
            wanderingButton.TabIndex = 12;
            wanderingButton.Text = "Test Burst Prevention Algo";
            wanderingButton.UseVisualStyleBackColor = true;
            wanderingButton.Click += WanderingButton_Click;
            // 
            // BackUp
            // 
            BackUp.Location = new Point(31, 814);
            BackUp.Name = "BackUp";
            BackUp.Size = new Size(136, 23);
            BackUp.TabIndex = 13;
            BackUp.Text = "Backup";
            BackUp.UseVisualStyleBackColor = true;
            BackUp.Click += BackUp_Click;
            // 
            // Restore
            // 
            Restore.Location = new Point(204, 814);
            Restore.Name = "Restore";
            Restore.Size = new Size(136, 23);
            Restore.TabIndex = 14;
            Restore.Text = "Restore";
            Restore.UseVisualStyleBackColor = true;
            Restore.Click += button1_Click_1;
            // 
            // train_another_object
            // 
            train_another_object.Location = new Point(291, 180);
            train_another_object.Name = "train_another_object";
            train_another_object.Size = new Size(136, 23);
            train_another_object.TabIndex = 15;
            train_another_object.Text = "Train Another Object";
            train_another_object.UseVisualStyleBackColor = true;
            train_another_object.Click += train_another_object_Click;
            // 
            // objectBox
            // 
            objectBox.Location = new Point(662, 27);
            objectBox.Name = "objectBox";
            objectBox.Size = new Size(100, 23);
            objectBox.TabIndex = 16;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(513, 35);
            label2.Name = "label2";
            label2.Size = new Size(99, 15);
            label2.TabIndex = 17;
            label2.Text = "Enter Fruit  Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1448, 35);
            button1.Name = "button1";
            button1.Size = new Size(108, 23);
            button1.TabIndex = 18;
            button1.Text = "Train Words";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_3;
            // 
            // startClassificationButton
            // 
            startClassificationButton.Location = new Point(291, 129);
            startClassificationButton.Name = "startClassificationButton";
            startClassificationButton.Size = new Size(136, 23);
            startClassificationButton.TabIndex = 19;
            startClassificationButton.Text = "Start Classification";
            startClassificationButton.UseVisualStyleBackColor = true;
            startClassificationButton.Click += startClassificationButton_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(108, 281);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1540, 100);
            pictureBox1.TabIndex = 20;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(437, 723);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(94, 100);
            pictureBox2.TabIndex = 21;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new Point(537, 723);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(100, 100);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Location = new Point(643, 723);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(106, 100);
            pictureBox4.TabIndex = 23;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Location = new Point(755, 723);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(109, 100);
            pictureBox5.TabIndex = 24;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Location = new Point(870, 723);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(99, 100);
            pictureBox6.TabIndex = 25;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Location = new Point(975, 723);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(95, 100);
            pictureBox7.TabIndex = 26;
            pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            pictureBox8.Location = new Point(1076, 723);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(108, 100);
            pictureBox8.TabIndex = 27;
            pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.Location = new Point(1190, 723);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(102, 100);
            pictureBox9.TabIndex = 28;
            pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            pictureBox10.Location = new Point(1298, 723);
            pictureBox10.Name = "pictureBox10";
            pictureBox10.Size = new Size(110, 100);
            pictureBox10.TabIndex = 29;
            pictureBox10.TabStop = false;
            // 
            // pictureBox11
            // 
            pictureBox11.Location = new Point(1414, 723);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new Size(110, 100);
            pictureBox11.TabIndex = 30;
            pictureBox11.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(-1, 314);
            label3.Name = "label3";
            label3.Size = new Size(80, 15);
            label3.TabIndex = 31;
            label3.Text = "V1 SOM Layer";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1705, 903);
            Controls.Add(label3);
            Controls.Add(pictureBox11);
            Controls.Add(pictureBox10);
            Controls.Add(pictureBox9);
            Controls.Add(pictureBox8);
            Controls.Add(pictureBox7);
            Controls.Add(pictureBox6);
            Controls.Add(pictureBox5);
            Controls.Add(pictureBox4);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(startClassificationButton);
            Controls.Add(button1);
            Controls.Add(label2);
            Controls.Add(objectBox);
            Controls.Add(train_another_object);
            Controls.Add(Restore);
            Controls.Add(BackUp);
            Controls.Add(wanderingButton);
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
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox10).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void labelY_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
        private Button wanderingButton;
        private Button BackUp;
        private Button Restore;
        private Button train_another_object;
        private TextBox objectBox;
        private Label label2;
        private Button button1;
        private Button startClassificationButton;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private PictureBox pictureBox8;
        private PictureBox pictureBox9;
        private PictureBox pictureBox10;
        private PictureBox pictureBox11;
        private Label label3;
    }
}
