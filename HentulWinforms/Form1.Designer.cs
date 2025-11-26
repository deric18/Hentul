
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
            labelX = new Label();
            labelY = new Label();
            Start = new Button();
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
            label3 = new Label();
            pictureBox2 = new PictureBox();
            openFileDialog1 = new OpenFileDialog();
            pictureBox3 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Location = new Point(401, 0);
            StartButton.Margin = new Padding(3, 4, 3, 4);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(155, 29);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start Training";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
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
            Start.Location = new Point(207, 0);
            Start.Margin = new Padding(3, 4, 3, 4);
            Start.Name = "Start";
            Start.Size = new Size(155, 29);
            Start.TabIndex = 4;
            Start.Text = "Init ";
            Start.UseVisualStyleBackColor = true;
            Start.Click += button1_Click;
            // 
            // ObjectLabel
            // 
            ObjectLabel.AutoSize = true;
            ObjectLabel.Location = new Point(0, 89);
            ObjectLabel.Name = "ObjectLabel";
            ObjectLabel.Size = new Size(81, 19);
            ObjectLabel.TabIndex = 7;
            ObjectLabel.Text = "ObjectLabel";
            // 
            // label_done
            // 
            label_done.AutoSize = true;
            label_done.Location = new Point(7, 45);
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
            wanderingButton.Location = new Point(0, 0);
            wanderingButton.Name = "wanderingButton";
            wanderingButton.Size = new Size(75, 22);
            wanderingButton.TabIndex = 38;
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
            train_another_object.Location = new Point(789, 0);
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
            objectBox.Location = new Point(1268, 7);
            objectBox.Margin = new Padding(3, 4, 3, 4);
            objectBox.Name = "objectBox";
            objectBox.Size = new Size(114, 26);
            objectBox.TabIndex = 16;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1099, 10);
            label2.Name = "label2";
            label2.Size = new Size(117, 19);
            label2.TabIndex = 17;
            label2.Text = "Enter Fruit  Name";
            label2.Click += label2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(1486, 10);
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
            startClassificationButton.Location = new Point(593, 0);
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
            pictureBox1.Location = new Point(123, 790);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1760, 126);
            pictureBox1.TabIndex = 20;
            pictureBox1.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(0, 853);
            label3.Name = "label3";
            label3.Size = new Size(77, 19);
            label3.TabIndex = 31;
            label3.Text = "SOM Layer";
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(33, 145);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(401, 362);
            pictureBox2.TabIndex = 39;
            pictureBox2.TabStop = false;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.FileOk += openFileDialog1_FileOk;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new Point(638, 145);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(1245, 623);
            pictureBox3.TabIndex = 40;
            pictureBox3.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1924, 1002);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(label3);
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
            Controls.Add(Start);
            Controls.Add(labelY);
            Controls.Add(labelX);
            Controls.Add(StartButton);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
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
        private Label labelX;
        private Label labelY;
        private Button Start;
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
        private Label label3;
        private PictureBox pictureBox2;
        private OpenFileDialog openFileDialog1;
        private PictureBox pictureBox3;
    }
}
