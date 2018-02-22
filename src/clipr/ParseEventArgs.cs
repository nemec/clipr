
namespace clipr
{
    public class ParseEventArgs
    {
        public readonly object Sender;

        public readonly string ArgumentName;

        public readonly object Value;

        public ParseEventArgs(object sender, string name, object value)
        {
            Sender = sender;
            ArgumentName = name;
            Value = value;
        }
    }
}
