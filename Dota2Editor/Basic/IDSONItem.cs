namespace Dota2Editor.Basic
{
    public abstract class IDSONItem(IDSONItem? parent)
    {
        private bool _modified = false;
        private readonly IDSONItem? _parent = parent;

        public bool Modified 
        { 
            get => _modified; 
            set 
            { 
                _modified = value; 
                if (value && _parent != null) _parent.Modified = true;
            } 
        }

        public abstract string Text { get; set; }
    }
}
