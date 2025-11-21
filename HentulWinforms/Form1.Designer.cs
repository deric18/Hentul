
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
            pictureBoxV2Whitescale = new PictureBox();
            pictureBoxV2Grayscale = new PictureBox();
            pictureBoxV3Whitescale = new PictureBox();
            pictureBoxV3Grayscale = new PictureBox();
            pictureBoxV3Som = new PictureBox();
            pictureBoxV2Som = new PictureBox();
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
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Whitescale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Grayscale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Whitescale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Grayscale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Som).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Som).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(333, 109);
            StartButton.Margin = new Padding(3, 4, 3, 4);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(155, 31);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CurrentImage
            // 
            CurrentImage.Location = new Point(586, 94);
            CurrentImage.Margin = new Padding(3, 4, 3, 4);
            CurrentImage.Name = "CurrentImage";
            CurrentImage.Size = new Size(74, 66);
            CurrentImage.TabIndex = 1;
            CurrentImage.TabStop = false;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(46, 1012);
            labelX.Name = "labelX";
            labelX.Size = new Size(51, 20);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(123, 1012);
            labelY.Name = "labelY";
            labelY.Size = new Size(50, 20);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            labelY.Click += labelY_Click;
            // 
            // Start
            // 
            Start.Location = new Point(333, 35);
            Start.Margin = new Padding(3, 4, 3, 4);
            Start.Name = "Start";
            Start.Size = new Size(155, 31);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // EdgedImage
            // 
            EdgedImage.Location = new Point(757, 94);
            EdgedImage.Margin = new Padding(3, 4, 3, 4);
            EdgedImage.Name = "EdgedImage";
            EdgedImage.Size = new Size(74, 66);
            EdgedImage.TabIndex = 5;
            EdgedImage.TabStop = false;
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(35, 120);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(89, 20);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(1064, 40);
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
            
            // 
            // BackUp
            // 
            BackUp.Location = new Point(35, 1085);
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
            Restore.Location = new Point(233, 1085);
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
            train_another_object.Location = new Point(333, 240);
            train_another_object.Margin = new Padding(3, 4, 3, 4);
            train_another_object.Name = "train_another_object";
            train_another_object.Size = new Size(155, 31);
            train_another_object.TabIndex = 15;
            train_another_object.Text = "Train Another Object";
            train_another_object.UseVisualStyleBackColor = true;
            train_another_object.Click += train_another_object_Click;
            // 
            // objectBox
            // 
            objectBox.Location = new Point(757, 36);
            objectBox.Margin = new Padding(3, 4, 3, 4);
            objectBox.Name = "objectBox";
            objectBox.Size = new Size(114, 27);
            objectBox.TabIndex = 16;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(586, 47);
            label2.Name = "label2";
            label2.Size = new Size(124, 20);
            label2.TabIndex = 17;
            label2.Text = "Enter Fruit  Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1655, 47);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(123, 31);
            button1.TabIndex = 18;
            button1.Text = "Train Words";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_3;
            // 
            // startClassificationButton
            // 
            startClassificationButton.Location = new Point(333, 172);
            startClassificationButton.Margin = new Padding(3, 4, 3, 4);
            startClassificationButton.Name = "startClassificationButton";
            startClassificationButton.Size = new Size(155, 31);
            startClassificationButton.TabIndex = 19;
            startClassificationButton.Text = "Start Classification";
            startClassificationButton.UseVisualStyleBackColor = true;
            startClassificationButton.Click += startClassificationButton_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(123, 375);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1760, 133);
            pictureBox1.TabIndex = 20;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(499, 964);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(107, 133);
            pictureBox2.TabIndex = 21;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new Point(614, 964);
            pictureBox3.Margin = new Padding(3, 4, 3, 4);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(114, 133);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Location = new Point(735, 964);
            pictureBox4.Margin = new Padding(3, 4, 3, 4);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(121, 133);
            pictureBox4.TabIndex = 23;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Location = new Point(863, 964);
            pictureBox5.Margin = new Padding(3, 4, 3, 4);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(125, 133);
            pictureBox5.TabIndex = 24;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Location = new Point(994, 964);
            pictureBox6.Margin = new Padding(3, 4, 3, 4);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(113, 133);
            pictureBox6.TabIndex = 25;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Location = new Point(1114, 964);
            pictureBox7.Margin = new Padding(3, 4, 3, 4);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(109, 133);
            pictureBox7.TabIndex = 26;
            pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            pictureBox8.Location = new Point(1230, 964);
            pictureBox8.Margin = new Padding(3, 4, 3, 4);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(123, 133);
            pictureBox8.TabIndex = 27;
            pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.Location = new Point(1360, 964);
            pictureBox9.Margin = new Padding(3, 4, 3, 4);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(117, 133);
            pictureBox9.TabIndex = 28;
            pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            pictureBox10.Location = new Point(1483, 964);
            pictureBox10.Margin = new Padding(3, 4, 3, 4);
            pictureBox10.Name = "pictureBox10";
            pictureBox10.Size = new Size(126, 133);
            pictureBox10.TabIndex = 29;
            pictureBox10.TabStop = false;
            // 
            // pictureBox11
            // 
            pictureBox11.Location = new Point(1616, 964);
            pictureBox11.Margin = new Padding(3, 4, 3, 4);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new Size(126, 133);
            pictureBox11.TabIndex = 30;
            pictureBox11.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(-1, 419);
            label3.Name = "label3";
            label3.Size = new Size(101, 20);
            label3.TabIndex = 31;
            label3.Text = "V1 SOM Layer";
            // 
            // pictureBoxV2Whitescale
            // 
            pictureBoxV2Whitescale.Location = new Point(757, 172);
            pictureBoxV2Whitescale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Whitescale.Name = "pictureBoxV2Whitescale";
            pictureBoxV2Whitescale.Size = new Size(74, 66);
            pictureBoxV2Whitescale.TabIndex = 33;
            pictureBoxV2Whitescale.TabStop = false;
            // 
            // pictureBoxV2Grayscale
            // 
            pictureBoxV2Grayscale.Location = new Point(586, 172);
            pictureBoxV2Grayscale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Grayscale.Name = "pictureBoxV2Grayscale";
            pictureBoxV2Grayscale.Size = new Size(74, 66);
            pictureBoxV2Grayscale.TabIndex = 32;
            pictureBoxV2Grayscale.TabStop = false;
            // 
            // pictureBoxV3Whitescale
            // 
            pictureBoxV3Whitescale.Location = new Point(757, 246);
            pictureBoxV3Whitescale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Whitescale.Name = "pictureBoxV3Whitescale";
            pictureBoxV3Whitescale.Size = new Size(74, 66);
            pictureBoxV3Whitescale.TabIndex = 35;
            pictureBoxV3Whitescale.TabStop = false;
            // 
            // pictureBoxV3Grayscale
            // 
            pictureBoxV3Grayscale.Location = new Point(586, 246);
            pictureBoxV3Grayscale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Grayscale.Name = "pictureBoxV3Grayscale";
            pictureBoxV3Grayscale.Size = new Size(74, 66);
            pictureBoxV3Grayscale.TabIndex = 34;
            pictureBoxV3Grayscale.TabStop = false;
            // 
            // pictureBoxV3Som
            // 
            pictureBoxV3Som.Location = new Point(1092, 94);
            pictureBoxV3Som.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Som.Name = "pictureBoxV3Som";
            pictureBoxV3Som.Size = new Size(74, 66);
            pictureBoxV3Som.TabIndex = 37;
            pictureBoxV3Som.TabStop = false;
            // 
            // pictureBoxV2Som
            // 
            pictureBoxV2Som.Location = new Point(921, 94);
            pictureBoxV2Som.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Som.Name = "pictureBoxV2Som";
            pictureBoxV2Som.Size = new Size(74, 66);
            pictureBoxV2Som.TabIndex = 36;
            pictureBoxV2Som.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1924, 1055);
            Controls.Add(pictureBoxV3Som);
            Controls.Add(pictureBoxV2Som);
            Controls.Add(pictureBoxV3Whitescale);
            Controls.Add(pictureBoxV3Grayscale);
            Controls.Add(pictureBoxV2Whitescale);
            Controls.Add(pictureBoxV2Grayscale);
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
            Margin = new Padding(3, 4, 3, 4);
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
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Whitescale).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Grayscale).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Whitescale).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Grayscale).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV3Som).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxV2Som).EndInit();
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
        private PictureBox pictureBoxV2Whitescale;
        private PictureBox pictureBoxV2Grayscale;
        private PictureBox pictureBoxV3Whitescale;
        private PictureBox pictureBoxV3Grayscale;
        private PictureBox pictureBoxV3Som;
        private PictureBox pictureBoxV2Som;
    }
}
