namespace Dota2Editor.Basic
{
    public class DSONValue(string text) : IDSONItem
    {
        private string _text = text;

        public string Text 
        { 
            get => _text; 
            set
            {
                _text = value ?? string.Empty;
                Modified = true;
            }
        }

        public bool Modified { get; set; } = false;

        public override string ToString() => _text;
    }
}
