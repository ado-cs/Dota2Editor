using Dota2Editor.Basic;

namespace Dota2Editor.Forms
{
    public partial class InputForm : Form
    {
        public string Result { get; private set; }

        public InputForm(string caption, string text, string value)
        {
            InitializeComponent();
            Text = caption;
            label1.Text = text;
            textBox1.Text = Result = value;
            button1.Text = Globalization.Get("InputForm.Button.Confirm") + "(&S)";
            button2.Text = Globalization.Get("InputForm.Button.Cancel") + "(&C)";
            button1.Click += (_, _) => { Result = textBox1.Text; DialogResult = DialogResult.OK; Close(); };
            textBox1.SelectAll();
            textBox1.Focus();
        }
    }
}
