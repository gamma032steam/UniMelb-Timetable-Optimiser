namespace Timetable_Optimiser
{
    partial class Timetabler
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
            Calendar.DrawTool drawTool5 = new Calendar.DrawTool();
            this.btn_Auto = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Prev = new System.Windows.Forms.Button();
            this.dayView1 = new Calendar.DayView();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.Btn_Next = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Auto
            // 
            this.btn_Auto.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_Auto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Auto.Location = new System.Drawing.Point(1159, 0);
            this.btn_Auto.Name = "btn_Auto";
            this.btn_Auto.Size = new System.Drawing.Size(429, 100);
            this.btn_Auto.TabIndex = 1;
            this.btn_Auto.Text = "Auto Step";
            this.btn_Auto.UseVisualStyleBackColor = true;
            this.btn_Auto.Click += new System.EventHandler(this.btn_Next_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.Btn_Next);
            this.panel1.Controls.Add(this.btn_Auto);
            this.panel1.Controls.Add(this.btn_Stop);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btn_Prev);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1823, 100);
            this.panel1.TabIndex = 2;
            // 
            // btn_Prev
            // 
            this.btn_Prev.Dock = System.Windows.Forms.DockStyle.Left;
            this.btn_Prev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Prev.Location = new System.Drawing.Point(0, 0);
            this.btn_Prev.Name = "btn_Prev";
            this.btn_Prev.Size = new System.Drawing.Size(443, 100);
            this.btn_Prev.TabIndex = 2;
            this.btn_Prev.Text = "Prev";
            this.btn_Prev.UseVisualStyleBackColor = true;
            this.btn_Prev.Click += new System.EventHandler(this.btn_Prev_Click);
            // 
            // dayView1
            // 
            drawTool5.DayView = this.dayView1;
            this.dayView1.ActiveTool = drawTool5;
            this.dayView1.AllowInplaceEditing = false;
            this.dayView1.AllowNew = false;
            this.dayView1.DaysToShow = 5;
            this.dayView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dayView1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.dayView1.HalfHourHeight = 36;
            this.dayView1.Location = new System.Drawing.Point(0, 0);
            this.dayView1.Name = "dayView1";
            this.dayView1.SelectionEnd = new System.DateTime(((long)(0)));
            this.dayView1.SelectionStart = new System.DateTime(((long)(0)));
            this.dayView1.Size = new System.Drawing.Size(1823, 1156);
            this.dayView1.StartDate = new System.DateTime(((long)(0)));
            this.dayView1.TabIndex = 0;
            this.dayView1.Text = "dayView1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(650, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // btn_Stop
            // 
            this.btn_Stop.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Stop.Location = new System.Drawing.Point(1588, 0);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(235, 100);
            this.btn_Stop.TabIndex = 4;
            this.btn_Stop.Text = "Stop";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // Btn_Next
            // 
            this.Btn_Next.Dock = System.Windows.Forms.DockStyle.Right;
            this.Btn_Next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Next.Location = new System.Drawing.Point(924, 0);
            this.Btn_Next.Name = "Btn_Next";
            this.Btn_Next.Size = new System.Drawing.Size(235, 100);
            this.Btn_Next.TabIndex = 5;
            this.Btn_Next.Text = "Next";
            this.Btn_Next.UseVisualStyleBackColor = true;
            this.Btn_Next.Click += new System.EventHandler(this.Btn_Next_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(650, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            // 
            // Timetabler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1823, 1156);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dayView1);
            this.Name = "Timetabler";
            this.Text = "Timetable Optimiser";
            this.Load += new System.EventHandler(this.Timetabler_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Calendar.DayView dayView1;
        private System.Windows.Forms.Button btn_Auto;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_Prev;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.Button Btn_Next;
        private System.Windows.Forms.Label label2;
    }
}

