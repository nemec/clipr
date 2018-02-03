
using clipr.Triggers;
using System;

namespace clipr
{
    public class ParseEventArgs : EventArgs
    {
        public readonly string ArgumentName;

        public readonly object Value;

        /// <summary>
        /// While handling the event, set this to an instance of ITerminatingTrigger
        /// to stop parsing immediately.
        /// </summary>
        public ITerminatingTrigger StopParsing { get; set; }

        public ParseEventArgs(string name, object value)
        {
            ArgumentName = name;
            Value = value;
        }
    }
}
