using clipr.Triggers;
using System;

namespace clipr
{
    public class TriggerException : Exception
    {
        public TriggerException(ITerminatingTrigger trigger)
        {
            Trigger = trigger;
        }

        public ITerminatingTrigger Trigger { get; private set; }
    }
}
