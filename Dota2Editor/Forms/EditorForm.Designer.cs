namespace Dota2Editor.Forms
{
    partial class EditorForm
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
            editorPanel1 = new EditorPanel();
            SuspendLayout();
            // 
            // editorPanel1
            // 
            editorPanel1.Dock = DockStyle.Fill;
            editorPanel1.Location = new Point(0, 0);
            editorPanel1.Name = "editorPanel1";
            editorPanel1.Size = new Size(884, 561);
            editorPanel1.TabIndex = 0;
            // 
            // EditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 561);
            Controls.Add(editorPanel1);
            MinimumSize = new Size(900, 600);
            Name = "EditorForm";
            Text = "EditorForm";
            ResumeLayout(false);
        }

        #endregion

        private EditorPanel editorPanel1;
    }
}