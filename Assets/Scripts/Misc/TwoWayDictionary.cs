using System;
using System.Collections.Generic;
using System.Linq;

public interface IReadOnlyTwoWayDictionary<T1, T2>
{
    IReadOnlyDictionary<T1, T2> FirstToSecond { get; }
    IReadOnlyDictionary<T2, T1> SecondToFirst { get; }
    T1 this[T2 key] { get; }
    T2 this[T1 key] { get; }
    bool Contains(T1 first);
    bool Contains(T2 second);
    bool Contains(T1 first, T2 second);
}

public class TwoWayDictionary<T1, T2> : IReadOnlyTwoWayDictionary<T1, T2>
{
    private IDictionary<T1, T2> _firstToSecond = new Dictionary<T1, T2>();
    private IDictionary<T2, T1> _secondToFirst = new Dictionary<T2, T1>();

    public IReadOnlyDictionary<T1, T2> FirstToSecond { get { return _firstToSecond as IReadOnlyDictionary<T1, T2>; } }
    public IReadOnlyDictionary<T2, T1> SecondToFirst { get { return _secondToFirst as IReadOnlyDictionary<T2, T1>; } }

    public T1 this[T2 key] { get { return _secondToFirst[key]; } }
    public T2 this[T1 key] { get { return _firstToSecond[key]; } }

    public void Add(T1 first, T2 second)
    {
        if (Contains(first, second))
            throw new Exception("The given key(s) already exist.");

        _firstToSecond.Add(first, second);
        _secondToFirst.Add(second, first);
    }

    public void Add(T2 second, T1 first)
    {
        if (Contains(first, second))
            throw new Exception("The given key(s) already exist.");

        _firstToSecond.Add(first, second);
        _secondToFirst.Add(second, first);
    }

    public void Remove(T1 first, T2 second)
    {
        if (!Contains(first, second))
            throw new Exception("Does not contain key for first and/or second.");

        _firstToSecond.Remove(first);
        _secondToFirst.Remove(second);
    }

    public void Remove(T2 second, T1 first)
    {
        if (!Contains(first, second))
            throw new Exception("Does not contain key for first and/or second.");

        _firstToSecond.Remove(first);
        _secondToFirst.Remove(second);
    }

    public bool Contains(T1 first) { return _firstToSecond.ContainsKey(first); }
    public bool Contains(T2 second) { return _secondToFirst.ContainsKey(second); }
    public bool Contains(T1 first, T2 second) { return Contains(first) || Contains(second); }
}