namespace MilitaryGradeAPI_Client
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
            btnSend = new Button();
            label1 = new Label();
            label2 = new Label();
            txtInput = new TextBox();
            txtOutput = new RichTextBox();
            SuspendLayout();
            // 
            // btnSend
            // 
            btnSend.Location = new Point(150, 116);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(166, 68);
            btnSend.TabIndex = 0;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 27);
            label1.Name = "label1";
            label1.Size = new Size(54, 25);
            label1.TabIndex = 1;
            label1.Text = "Input";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 235);
            label2.Name = "label2";
            label2.Size = new Size(69, 25);
            label2.TabIndex = 2;
            label2.Text = "Output";
            // 
            // txtInput
            // 
            txtInput.Location = new Point(87, 27);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(367, 31);
            txtInput.TabIndex = 3;
            // 
            // txtOutput
            // 
            txtOutput.Location = new Point(87, 235);
            txtOutput.Name = "txtOutput";
            txtOutput.Size = new Size(367, 144);
            txtOutput.TabIndex = 4;
            txtOutput.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(499, 438);
            Controls.Add(txtOutput);
            Controls.Add(txtInput);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnSend);
            Name = "Form1";
            Text = "Military grade API client";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSend;
        private Label label1;
        private Label label2;
        private TextBox txtInput;
        private RichTextBox txtOutput;
    }
}
