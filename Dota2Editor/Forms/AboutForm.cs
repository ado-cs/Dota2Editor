using Dota2Editor.Basic;
using System.Diagnostics;

namespace Dota2Editor.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm(string url)
        {
            InitializeComponent();
            var a = typeof(AboutForm).Assembly.GetName();
            var name = a.Name?.ToString() ?? string.Empty;
            var vs = a.Version?.ToString() ?? string.Empty;
            Text = Globalization.Get("AboutForm.Text");
            label1.Text = Globalization.Get("AboutForm.Introduction", name, vs);
            linkLabel1.Text = url;
            linkLabel1.Click += (_, _) => Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
            button1.Text = Globalization.Get("AboutForm.Button") + "(&C)";
        }
    }
}
