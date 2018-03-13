using System;

namespace Framework
{
    public abstract class EventBase<T>
    {
        public T Sender { get; private set; }

        public EventBase(T sender)
        {
            Sender = sender;
        }
    }
}
