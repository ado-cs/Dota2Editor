namespace Dota2Editor.Basic
{
    public class DSONValue(string text, IDSONItem? parent) : IDSONItem(parent)
    {
        private string _text = text ?? string.Empty;

        public DSONValue(string text) : this(text, null) { }

        public override string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                Modified = true;
            }
        }

        public override string ToString() => _text;
    }
}
