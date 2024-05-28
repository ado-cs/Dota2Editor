using Dota2Editor.Basic;

namespace Dota2Editor.Forms
{
    public partial class ChangesCheckingForm : Form
    {
        public ChangesCheckingForm(TreeNode tree)
        {
            InitializeComponent();
            foreach (TreeNode node in tree.Nodes) treeView1.Nodes.Add(node);
            treeView1.ExpandAll();
            Text = Globalization.Get("ChangesCheckingForm.Text");
            button1.Text = Globalization.Get("ChangesCheckingForm.Button.Confirm") + "(&S)";
            button2.Text = Globalization.Get("ChangesCheckingForm.Button.Cancel") + "(&C)";
            button1.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        }
    }
}
