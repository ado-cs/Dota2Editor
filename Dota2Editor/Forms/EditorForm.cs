using Dota2Editor.Basic;

namespace Dota2Editor.Forms
{
    public partial class EditorForm : Form, IEditor
    {
        public EditorForm(int index)
        {
            InitializeComponent();
            Text = Common.GetViewName(index);
            FormClosed += (_, _) => Panel.StashChanges();
            editorPanel1.ResetView(index);
        }

        public EditorPanel Panel => editorPanel1;
    }
}
