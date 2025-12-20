namespace AUDIO_NEW
{
    partial class FormInfor
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
            label1 = new Label();
            txtDisplayname = new TextBox();
            comboBox1 = new ComboBox();
            label2 = new Label();
            btnSave = new Button();
            groupBox1 = new GroupBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Light", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(6, 44);
            label1.Name = "label1";
            label1.Size = new Size(87, 17);
            label1.TabIndex = 0;
            label1.Text = "DisplayName :";
            // 
            // txtDisplayname
            // 
            txtDisplayname.Location = new Point(108, 41);
            txtDisplayname.Name = "txtDisplayname";
            txtDisplayname.Size = new Size(339, 27);
            txtDisplayname.TabIndex = 1;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(108, 96);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(225, 28);
            comboBox1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Light", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(6, 96);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 3;
            label2.Text = "Gender";
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LightPink;
            btnSave.Location = new Point(192, 154);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(95, 34);
            btnSave.TabIndex = 4;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ButtonHighlight;
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(btnSave);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Controls.Add(txtDisplayname);
            groupBox1.FlatStyle = FlatStyle.Popup;
            groupBox1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(58, 30);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(476, 224);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Information";
            // 
            // FormInfor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            BackgroundImage = Properties.Resources.img_11;
            ClientSize = new Size(594, 318);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormInfor";
            Text = "FormInfor";
            Load += FormInfor_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private TextBox txtDisplayname;
        private ComboBox comboBox1;
        private Label label2;
        private Button btnSave;
        private GroupBox groupBox1;
    }
}