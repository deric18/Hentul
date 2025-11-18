
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
            label4 = new Label();
            label5 = new Label();
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
            StartButton.Location = new Point(333, 104);
            StartButton.Margin = new Padding(3, 4, 3, 4);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(155, 29);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CurrentImage
            // 
            CurrentImage.Location = new Point(804, 108);
            CurrentImage.Margin = new Padding(3, 4, 3, 4);
            CurrentImage.Name = "CurrentImage";
            CurrentImage.Size = new Size(74, 63);
            CurrentImage.TabIndex = 1;
            CurrentImage.TabStop = false;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new Point(46, 961);
            labelX.Name = "labelX";
            labelX.Size = new Size(45, 19);
            labelX.TabIndex = 2;
            labelX.Text = "labelX";
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new Point(123, 961);
            labelY.Name = "labelY";
            labelY.Size = new Size(45, 19);
            labelY.TabIndex = 3;
            labelY.Text = "labelY";
            labelY.Click += labelY_Click;
            // 
            // Start
            // 
            Start.Location = new Point(333, 33);
            Start.Margin = new Padding(3, 4, 3, 4);
            Start.Name = "Start";
            Start.Size = new Size(155, 29);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // EdgedImage
            // 
            EdgedImage.Location = new Point(975, 108);
            EdgedImage.Margin = new Padding(3, 4, 3, 4);
            EdgedImage.Name = "EdgedImage";
            EdgedImage.Size = new Size(74, 63);
            EdgedImage.TabIndex = 5;
            EdgedImage.TabStop = false;
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(35, 114);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(81, 19);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(1064, 38);
            label_done.Name = "label_done";
            label_done.Size = new Size(74, 19);
            label_done.TabIndex = 8;
            label_done.Text = "Processing";
            label_done.Click += label1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(49, 1090);
            label1.Name = "label1";
            label1.Size = new Size(60, 19);
            label1.TabIndex = 9;
            label1.Text = "CYCLE #";
            // 
            // CycleLabel
            // 
            CycleLabel.AutoSize = true;
            CycleLabel.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Assertive;
            CycleLabel.Location = new Point(176, 1090);
            CycleLabel.Name = "CycleLabel";
            CycleLabel.Size = new Size(17, 19);
            CycleLabel.TabIndex = 10;
            CycleLabel.Text = "0";
            // 
            // wanderingButton
            // 
            wanderingButton.Location = new Point(1655, 109);
            wanderingButton.Margin = new Padding(3, 4, 3, 4);
            wanderingButton.Name = "wanderingButton";
            wanderingButton.Size = new Size(155, 29);
            wanderingButton.TabIndex = 12;
            wanderingButton.Text = "Test Burst Prevention Algo";
            wanderingButton.UseVisualStyleBackColor = true;
            wanderingButton.Click += WanderingButton_Click;
            // 
            // BackUp
            // 
            BackUp.Location = new Point(35, 1031);
            BackUp.Margin = new Padding(3, 4, 3, 4);
            BackUp.Name = "BackUp";
            BackUp.Size = new Size(155, 29);
            BackUp.TabIndex = 13;
            BackUp.Text = "Backup";
            BackUp.UseVisualStyleBackColor = true;
            BackUp.Click += BackUp_Click;
            // 
            // Restore
            // 
            Restore.Location = new Point(233, 1031);
            Restore.Margin = new Padding(3, 4, 3, 4);
            Restore.Name = "Restore";
            Restore.Size = new Size(155, 29);
            Restore.TabIndex = 14;
            Restore.Text = "Restore";
            Restore.UseVisualStyleBackColor = true;
            Restore.Click += button1_Click_1;
            // 
            // train_another_object
            // 
            train_another_object.Location = new Point(333, 228);
            train_another_object.Margin = new Padding(3, 4, 3, 4);
            train_another_object.Name = "train_another_object";
            train_another_object.Size = new Size(155, 29);
            train_another_object.TabIndex = 15;
            train_another_object.Text = "Train Another Object";
            train_another_object.UseVisualStyleBackColor = true;
            train_another_object.Click += train_another_object_Click;
            // 
            // objectBox
            // 
            objectBox.Location = new Point(757, 34);
            objectBox.Margin = new Padding(3, 4, 3, 4);
            objectBox.Name = "objectBox";
            objectBox.Size = new Size(114, 26);
            objectBox.TabIndex = 16;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(586, 45);
            label2.Name = "label2";
            label2.Size = new Size(117, 19);
            label2.TabIndex = 17;
            label2.Text = "Enter Fruit  Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1655, 45);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(123, 29);
            button1.TabIndex = 18;
            button1.Text = "Train Words";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_3;
            // 
            // startClassificationButton
            // 
            startClassificationButton.Location = new Point(333, 163);
            startClassificationButton.Margin = new Padding(3, 4, 3, 4);
            startClassificationButton.Name = "startClassificationButton";
            startClassificationButton.Size = new Size(155, 29);
            startClassificationButton.TabIndex = 19;
            startClassificationButton.Text = "Start Classification";
            startClassificationButton.UseVisualStyleBackColor = true;
            startClassificationButton.Click += startClassificationButton_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(123, 356);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1760, 126);
            pictureBox1.TabIndex = 20;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(499, 916);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(107, 126);
            pictureBox2.TabIndex = 21;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new Point(614, 916);
            pictureBox3.Margin = new Padding(3, 4, 3, 4);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(114, 126);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Location = new Point(735, 916);
            pictureBox4.Margin = new Padding(3, 4, 3, 4);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(121, 126);
            pictureBox4.TabIndex = 23;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Location = new Point(863, 916);
            pictureBox5.Margin = new Padding(3, 4, 3, 4);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(125, 126);
            pictureBox5.TabIndex = 24;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Location = new Point(994, 916);
            pictureBox6.Margin = new Padding(3, 4, 3, 4);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(113, 126);
            pictureBox6.TabIndex = 25;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Location = new Point(1114, 916);
            pictureBox7.Margin = new Padding(3, 4, 3, 4);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(109, 126);
            pictureBox7.TabIndex = 26;
            pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            pictureBox8.Location = new Point(1230, 916);
            pictureBox8.Margin = new Padding(3, 4, 3, 4);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(123, 126);
            pictureBox8.TabIndex = 27;
            pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.Location = new Point(1360, 916);
            pictureBox9.Margin = new Padding(3, 4, 3, 4);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(117, 126);
            pictureBox9.TabIndex = 28;
            pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            pictureBox10.Location = new Point(1483, 916);
            pictureBox10.Margin = new Padding(3, 4, 3, 4);
            pictureBox10.Name = "pictureBox10";
            pictureBox10.Size = new Size(126, 126);
            pictureBox10.TabIndex = 29;
            pictureBox10.TabStop = false;
            // 
            // pictureBox11
            // 
            pictureBox11.Location = new Point(1616, 916);
            pictureBox11.Margin = new Padding(3, 4, 3, 4);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new Size(126, 126);
            pictureBox11.TabIndex = 30;
            pictureBox11.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(-1, 398);
            label3.Name = "label3";
            label3.Size = new Size(98, 19);
            label3.TabIndex = 31;
            label3.Text = "V1 SOM Layer";
            // 
            // pictureBoxV2Whitescale
            // 
            pictureBoxV2Whitescale.Location = new Point(975, 182);
            pictureBoxV2Whitescale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Whitescale.Name = "pictureBoxV2Whitescale";
            pictureBoxV2Whitescale.Size = new Size(74, 63);
            pictureBoxV2Whitescale.TabIndex = 33;
            pictureBoxV2Whitescale.TabStop = false;
            // 
            // pictureBoxV2Grayscale
            // 
            pictureBoxV2Grayscale.Location = new Point(804, 182);
            pictureBoxV2Grayscale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Grayscale.Name = "pictureBoxV2Grayscale";
            pictureBoxV2Grayscale.Size = new Size(74, 63);
            pictureBoxV2Grayscale.TabIndex = 32;
            pictureBoxV2Grayscale.TabStop = false;
            // 
            // pictureBoxV3Whitescale
            // 
            pictureBoxV3Whitescale.Location = new Point(975, 253);
            pictureBoxV3Whitescale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Whitescale.Name = "pictureBoxV3Whitescale";
            pictureBoxV3Whitescale.Size = new Size(74, 63);
            pictureBoxV3Whitescale.TabIndex = 35;
            pictureBoxV3Whitescale.TabStop = false;
            // 
            // pictureBoxV3Grayscale
            // 
            pictureBoxV3Grayscale.Location = new Point(804, 253);
            pictureBoxV3Grayscale.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Grayscale.Name = "pictureBoxV3Grayscale";
            pictureBoxV3Grayscale.Size = new Size(74, 63);
            pictureBoxV3Grayscale.TabIndex = 34;
            pictureBoxV3Grayscale.TabStop = false;
            // 
            // pictureBoxV3Som
            // 
            pictureBoxV3Som.Location = new Point(123, 712);
            pictureBoxV3Som.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV3Som.Name = "pictureBoxV3Som";
            pictureBoxV3Som.Size = new Size(1760, 123);
            pictureBoxV3Som.TabIndex = 37;
            pictureBoxV3Som.TabStop = false;
            // 
            // pictureBoxV2Som
            // 
            pictureBoxV2Som.Location = new Point(123, 526);
            pictureBoxV2Som.Margin = new Padding(3, 4, 3, 4);
            pictureBoxV2Som.Name = "pictureBoxV2Som";
            pictureBoxV2Som.Size = new Size(1760, 136);
            pictureBoxV2Som.TabIndex = 36;
            pictureBoxV2Som.TabStop = false;
            pictureBoxV2Som.Click += pictureBoxV2Som_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(-1, 585);
            label4.Name = "label4";
            label4.Size = new Size(98, 19);
            label4.TabIndex = 38;
            label4.Text = "V2 SOM Layer";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(-1, 767);
            label5.Name = "label5";
            label5.Size = new Size(98, 19);
            label5.TabIndex = 39;
            label5.Text = "V3 SOM Layer";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1924, 1002);
            Controls.Add(label5);
            Controls.Add(label4);
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
        private Label label4;
        private Label label5;
    }
}
