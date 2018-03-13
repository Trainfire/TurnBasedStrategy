using UnityEngine;
using System;
using System.Collections.Generic;

namespace Framework
{
    public enum CycleType
    {
        Forward,
        Backward,
        ToStart,
        ToEnd,
    }

    public class CyclicalListEvent<T> : EventArgs
    {
        public CycleType CycleType { get; private set; }
        public T Data { get; private set; }

        public CyclicalListEvent(CycleType cycleType, T data)
        {
            CycleType = cycleType;
            Data = data;
        }
    }

    public abstract class CyclicalList
    {
        public abstract void MoveNext();
        public abstract void MovePrev();
        public abstract void MoveToStart();
        public abstract void MoveToEnd();
    }

    /// <summary>
    /// Allows a list to be traversed via MoveNext and MovePrev.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CyclicalList<T> : CyclicalList
    {
        public event EventHandler<CyclicalListEvent<T>> Moved;

        /// <summary>
        /// If true, the list will automatically return to first item when moving forward on the last item,
        /// and will automatically return to the last item when moving backwards from the first item.
        /// </summary>
        public bool Wrapped { get; set; }
        public int Index { get; private set; }

        public T Current
        {
            get { return list.Count != 0 ? list[Index] : default(T); }
        }

        private List<T> list;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list">The reference list to cycle through.</param>
        public CyclicalList()
        {
            list = new List<T>();
        }

        public void Add(T data)
        {
            if (!list.Contains(data))
            {
                list.Add(data);
            }
        }

        public void Remove(T data)
        {
            if (list.Contains(data))
            {
                list.Remove(data);
            }
        }

        /// <summary>
        /// Updates the internal index to match the index of the specified value.
        /// </summary>
        public void Set(T data)
        {
            if (list.Contains(data))
            {
                Index = list.IndexOf(data);
            }
        }

        public override void MoveNext()
        {
            if (Index < list.Count - 1)
            {
                Index++;
                OnMove(CycleType.Forward);
            }
            else if (Wrapped)
            {
                MoveToStart();
            }
        }

        public override void MovePrev()
        {
            if (Index > 0)
            {
                Index--;
                OnMove(CycleType.Backward);
            }
            else if (Wrapped)
            {
                MoveToEnd();
            }
        }

        public override void MoveToStart()
        {
            Index = 0;
            OnMove(CycleType.ToStart);
        }

        public override void MoveToEnd()
        {
            Index = Mathf.Clamp(list.Count - 1, 0, list.Count - 1);
            OnMove(CycleType.ToEnd);
        }

        private void OnMove(CycleType moveType)
        {
            if (Moved != null)
                Moved(this, new CyclicalListEvent<T>(moveType, list[Index]));
        }
    }
}
