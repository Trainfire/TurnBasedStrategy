using System;
using System.Collections.Generic;

public class TwoWayDictionary<T1, T2>
{
    private IDictionary<T1, T2> _firstToSecond = new Dictionary<T1, T2>();
    private IDictionary<T2, T1> _secondToFirst = new Dictionary<T2, T1>();

    public void Add(T1 first, T2 second)
    {
        if (Contains(first, second))
            throw new Exception("The given key(s) already exist.");

        _firstToSecond.Add(first, second);
    }

    public void Add(T2 second, T1 first)
    {
        if (Contains(first, second))
            throw new Exception("The given key(s) already exist.");

        _secondToFirst.Add(second, first);
    }

    public T1 Get(T2 second)
    {
        if (!Contains(second))
            throw new Exception("Does not contain key for second.");

        return _secondToFirst[second];
    }

    public T2 Get(T1 first)
    {
        if (!Contains(first))
            throw new Exception("Does not contain key for first.");

        return _firstToSecond[first];
    }

    public void Remove(T1 first, T2 second)
    {
        if (!Contains(first, second))
            throw new Exception("Does not contain key for first and/or second.");

        _firstToSecond.Remove(first);
    }

    public void Remove(T2 second, T1 first)
    {
        if (!Contains(first, second))
            throw new Exception("Does not contain key for first and/or second.");

        _secondToFirst.Remove(second);
    }

    public bool Contains(T1 first) { return _firstToSecond.ContainsKey(first); }
    public bool Contains(T2 second) { return _secondToFirst.ContainsKey(second); }
    public bool Contains(T1 first, T2 second) { return Contains(first) || Contains(second); }
}