using System;

public interface IReadOnlyCachedValue<T>
{
    event Action<IReadOnlyCachedValue<T>> Changed;
    T Current { get; }
    T Previous { get; }
}

public class CachedValue<T> : IReadOnlyCachedValue<T>
{
    public event Action<IReadOnlyCachedValue<T>> Changed;

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