using System;

public class CachedValue<T>
{
    public event Action<CachedValue<T>> Changed;

    public T Current
    {
        get { return _current; }
        set
        {
            _previous = _current;
            _current = value;
            Changed?.Invoke(this);
        }
    }

    public T Previous { get { return _previous; } }

    private T _current;
    private T _previous;

    public CachedValue() { }
    
    public CachedValue(T value)
    {
        Current = Current;
    }
}