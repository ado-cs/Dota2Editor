using Dota2Editor.Basic;

namespace Dota2Editor.Forms
{
    public partial class BatchModificationForm : Form
    {
        private readonly Func<string, string, double, Operator, int> _handler;

        public BatchModificationForm(Func<string, string, double, Operator, int> handler)
        {
            InitializeComponent();
            _handler = handler;
            Text = Globalization.Get("BatchModificationForm.Text");
            label1.Text = Globalization.Get("BatchModificationForm.Label.Key");
            label2.Text = Globalization.Get("BatchModificationForm.Label.Value");
            button1.Text = Globalization.Get("BatchModificationForm.Button.Apply") + "(&A)";
            button2.Text = Globalization.Get("BatchModificationForm.Button.Close") + "(&C)";
            comboBox1.Items.Add(Globalization.Get("BatchModificationForm.ComboBox.Equals"));
            comboBox1.Items.Add(Globalization.Get("BatchModificationForm.ComboBox.Increase"));
            comboBox1.Items.Add(Globalization.Get("BatchModificationForm.ComboBox.Multiply"));
            button1.Click += (_, _) => Apply();
            comboBox1.SelectedIndex = 0;
            textBox1.Focus();
        }

        public void Apply()
        {
            if (textBox1.TextLength == 0) { textBox1.Focus(); return; }
            if (textBox2.TextLength == 0) { textBox2.Focus(); return; }
            if (comboBox1.SelectedIndex == -1) { comboBox1.Focus(); return; }
            int num;
            if (comboBox1.SelectedIndex == 0) num = _handler(textBox1.Text, textBox2.Text, 0, Operator.Equals);
            else if (double.TryParse(textBox2.Text, out var d)) 
                num = _handler(textBox1.Text, textBox2.Text, d, comboBox1.SelectedIndex == 1 ? Operator.Increase : Operator.Multiply);
            else { MessageBox.Show(Globalization.Get("BatchModificationForm.FailedInParsingNumber")); return; }
            MessageBox.Show(Globalization.Get("BatchModificationForm.SuccessInModification", num));
            textBox2.SelectAll();
            textBox2.Focus();
        }

        public enum Operator
        {
            Equals,
            Increase,
            Multiply
        }
    }
}
