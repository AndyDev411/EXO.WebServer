namespace ServerTestApp
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
            host_btn = new Button();
            room_name_txtbox = new TextBox();
            room_key_txtbox = new TextBox();
            label1 = new Label();
            join_btn = new Button();
            label2 = new Label();
            label3 = new Label();
            name_txtbox = new TextBox();
            label4 = new Label();
            label5 = new Label();
            send_txtbox = new TextBox();
            chat_txtbox = new TextBox();
            send_btn = new Button();
            SuspendLayout();
            // 
            // host_btn
            // 
            host_btn.Location = new Point(112, 12);
            host_btn.Name = "host_btn";
            host_btn.Size = new Size(143, 29);
            host_btn.TabIndex = 1;
            host_btn.Text = "Host";
            host_btn.UseVisualStyleBackColor = true;
            host_btn.Click += host_btn_Click;
            // 
            // room_name_txtbox
            // 
            room_name_txtbox.Location = new Point(112, 47);
            room_name_txtbox.Name = "room_name_txtbox";
            room_name_txtbox.Size = new Size(294, 27);
            room_name_txtbox.TabIndex = 2;
            room_name_txtbox.Text = "Example Room";
            // 
            // room_key_txtbox
            // 
            room_key_txtbox.Location = new Point(112, 80);
            room_key_txtbox.Name = "room_key_txtbox";
            room_key_txtbox.Size = new Size(294, 27);
            room_key_txtbox.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 47);
            label1.Name = "label1";
            label1.Size = new Size(93, 20);
            label1.TabIndex = 4;
            label1.Text = "Room Name";
            // 
            // join_btn
            // 
            join_btn.Location = new Point(261, 12);
            join_btn.Name = "join_btn";
            join_btn.Size = new Size(145, 29);
            join_btn.TabIndex = 5;
            join_btn.Text = "Join";
            join_btn.UseVisualStyleBackColor = true;
            join_btn.Click += join_btn_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 83);
            label2.Name = "label2";
            label2.Size = new Size(77, 20);
            label2.TabIndex = 6;
            label2.Text = "Room Key";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(570, 50);
            label3.Name = "label3";
            label3.Size = new Size(49, 20);
            label3.TabIndex = 8;
            label3.Text = "Name";
            // 
            // name_txtbox
            // 
            name_txtbox.Location = new Point(625, 47);
            name_txtbox.Name = "name_txtbox";
            name_txtbox.Size = new Size(163, 27);
            name_txtbox.TabIndex = 7;
            name_txtbox.Text = "Billy";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(139, 291);
            label4.Name = "label4";
            label4.Size = new Size(67, 20);
            label4.TabIndex = 12;
            label4.Text = "Message";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(177, 212);
            label5.Name = "label5";
            label5.Size = new Size(39, 20);
            label5.TabIndex = 11;
            label5.Text = "Chat";
            // 
            // send_txtbox
            // 
            send_txtbox.Location = new Point(222, 288);
            send_txtbox.Name = "send_txtbox";
            send_txtbox.Size = new Size(294, 27);
            send_txtbox.TabIndex = 10;
            // 
            // chat_txtbox
            // 
            chat_txtbox.Location = new Point(222, 139);
            chat_txtbox.Multiline = true;
            chat_txtbox.Name = "chat_txtbox";
            chat_txtbox.Size = new Size(294, 143);
            chat_txtbox.TabIndex = 9;
            // 
            // send_btn
            // 
            send_btn.Location = new Point(525, 286);
            send_btn.Name = "send_btn";
            send_btn.Size = new Size(94, 29);
            send_btn.TabIndex = 13;
            send_btn.Text = "Send";
            send_btn.UseVisualStyleBackColor = true;
            send_btn.Click += send_btn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(send_btn);
            Controls.Add(label4);
            Controls.Add(label5);
            Controls.Add(send_txtbox);
            Controls.Add(chat_txtbox);
            Controls.Add(label3);
            Controls.Add(name_txtbox);
            Controls.Add(label2);
            Controls.Add(join_btn);
            Controls.Add(label1);
            Controls.Add(room_key_txtbox);
            Controls.Add(room_name_txtbox);
            Controls.Add(host_btn);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button host_btn;
        private TextBox room_name_txtbox;
        private TextBox room_key_txtbox;
        private Label label1;
        private Button join_btn;
        private Label label2;
        private Label label3;
        private TextBox name_txtbox;
        private Label label4;
        private Label label5;
        private TextBox send_txtbox;
        private TextBox chat_txtbox;
        private Button send_btn;
    }
}
