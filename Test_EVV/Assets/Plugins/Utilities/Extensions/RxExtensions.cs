namespace Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UniRx;

    public interface ICollectionAddEvent<out T>
    {
        public int Index { get; }
        public T Value { get; }
    }
    
    public interface ICollectionMoveEvent<out T>
    {
        public int OldIndex { get; }
        public int NewIndex { get; }
        public T Value { get; }
    }
    
    public interface ICollectionRemoveEvent<out T>
    {
        public int Index { get; }
        public T Value { get; }
    }
    
    public interface ICollectionReplaceEvent<out T>
    {
        public int Index { get; }
        public T OldValue { get; }
        public T NewValue { get; }
    }
    
    public struct CollectionAddEvent<T> : IEquatable<CollectionAddEvent<T>>, ICollectionAddEvent<T>
    {
        public int Index { get; private set; }
        public T Value { get; private set; }

        public CollectionAddEvent(int index, T value)
            :this()
        {
            Index = index;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Index:{0} Value:{1}", Index, Value);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(CollectionAddEvent<T> other)
        {
            return Index.Equals(other.Index) && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct CollectionRemoveEvent<T> : IEquatable<CollectionRemoveEvent<T>>, ICollectionRemoveEvent<T>
    {
        public int Index { get; private set; }
        public T Value { get; private set; }

        public CollectionRemoveEvent(int index, T value)
            : this()
        {
            Index = index;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Index:{0} Value:{1}", Index, Value);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(CollectionRemoveEvent<T> other)
        {
            return Index.Equals(other.Index) && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct CollectionMoveEvent<T> : IEquatable<CollectionMoveEvent<T>>, ICollectionMoveEvent<T>
    {
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }
        public T Value { get; private set; }

        public CollectionMoveEvent(int oldIndex, int newIndex, T value)
            : this()
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("OldIndex:{0} NewIndex:{1} Value:{2}", OldIndex, NewIndex, Value);
        }

        public override int GetHashCode()
        {
            return OldIndex.GetHashCode() ^ NewIndex.GetHashCode() << 2 ^ EqualityComparer<T>.Default.GetHashCode(Value) >> 2;
        }

        public bool Equals(CollectionMoveEvent<T> other)
        {
            return OldIndex.Equals(other.OldIndex) && NewIndex.Equals(other.NewIndex) && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct CollectionReplaceEvent<T> : IEquatable<CollectionReplaceEvent<T>>, ICollectionReplaceEvent<T>
    {
        public int Index { get; private set; }
        public T OldValue { get; private set; }
        public T NewValue { get; private set; }

        public CollectionReplaceEvent(int index, T oldValue, T newValue)
            : this()
        {
            Index = index;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return string.Format("Index:{0} OldValue:{1} NewValue:{2}", Index, OldValue, NewValue);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(OldValue) << 2 ^ EqualityComparer<T>.Default.GetHashCode(NewValue) >> 2;
        }

        public bool Equals(CollectionReplaceEvent<T> other)
        {
            return Index.Equals(other.Index)
                && EqualityComparer<T>.Default.Equals(OldValue, other.OldValue)
                && EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);
        }
    }

    // IReadOnlyList<out T> is from .NET 4.5
    public interface IReadOnlyReactiveCollection<out T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int index] { get; }
        IObservable<ICollectionAddEvent<T>> ObserveAdd();
        IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false);
        IObservable<ICollectionMoveEvent<T>> ObserveMove();
        IObservable<ICollectionRemoveEvent<T>> ObserveRemove();
        IObservable<ICollectionReplaceEvent<T>> ObserveReplace();
        IObservable<Unit> ObserveReset();
    }

    public interface IReactiveCollection<T> : IList<T>, IReadOnlyReactiveCollection<T>
    {
        new int Count { get; }
        new T this[int index] { get; set; }
        void Move(int oldIndex, int newIndex);
    }

    [Serializable]
    public class ReactiveCollection<T> : Collection<T>, IReactiveCollection<T>, IDisposable
    {
        [NonSerialized]
        bool isDisposed = false;

        public ReactiveCollection()
        {

        }

        public ReactiveCollection(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public ReactiveCollection(List<T> list)
            : base(list != null ? new List<T>(list) : null)
        {
        }

        protected override void ClearItems()
        {
            var beforeCount = Count;
            base.ClearItems();

            if (collectionReset != null) collectionReset.OnNext(Unit.Default);
            if (beforeCount > 0)
            {
                if (countChanged != null) countChanged.OnNext(Count);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (collectionAdd != null) collectionAdd.OnNext(new CollectionAddEvent<T>(index, item));
            if (countChanged != null) countChanged.OnNext(Count);
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            T item = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);

            if (collectionMove != null) collectionMove.OnNext(new CollectionMoveEvent<T>(oldIndex, newIndex, item));
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            base.RemoveItem(index);

            if (collectionRemove != null) collectionRemove.OnNext(new CollectionRemoveEvent<T>(index, item));
            if (countChanged != null) countChanged.OnNext(Count);
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];
            base.SetItem(index, item);

            if (collectionReplace != null) collectionReplace.OnNext(new CollectionReplaceEvent<T>(index, oldItem, item));
        }


        [NonSerialized]
        Subject<int> countChanged = null;
        public IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false)
        {
            if (isDisposed) return Observable.Empty<int>();

            var subject = countChanged ??= new Subject<int>();
            if (notifyCurrentCount)
            {
                return subject.StartWith(() => this.Count);
            }
            else
            {
                return subject;
            }
        }

        [NonSerialized]
        Subject<Unit> collectionReset = null;
        public IObservable<Unit> ObserveReset()
        {
            if (isDisposed) return Observable.Empty<Unit>();
            return collectionReset ??= new Subject<Unit>();
        }

        [NonSerialized]
        Subject<ICollectionAddEvent<T>> collectionAdd = null;
        public IObservable<ICollectionAddEvent<T>> ObserveAdd()
        {
            if (isDisposed) return Observable.Empty<ICollectionAddEvent<T>>();
            return (collectionAdd ??= new Subject<ICollectionAddEvent<T>>());
        }

        [NonSerialized]
        Subject<ICollectionMoveEvent<T>> collectionMove = null;
        public IObservable<ICollectionMoveEvent<T>> ObserveMove()
        {
            if (isDisposed) return Observable.Empty<ICollectionMoveEvent<T>>();
            return (collectionMove ??= new Subject<ICollectionMoveEvent<T>>());
        }

        [NonSerialized]
        Subject<ICollectionRemoveEvent<T>> collectionRemove = null;
        public IObservable<ICollectionRemoveEvent<T>> ObserveRemove()
        {
            if (isDisposed) return Observable.Empty<ICollectionRemoveEvent<T>>();
            return (collectionRemove ??= new Subject<ICollectionRemoveEvent<T>>());
        }

        [NonSerialized]
        Subject<ICollectionReplaceEvent<T>> collectionReplace = null;
        public IObservable<ICollectionReplaceEvent<T>> ObserveReplace()
        {
            if (isDisposed) return Observable.Empty<ICollectionReplaceEvent<T>>();
            return (collectionReplace ??= new Subject<ICollectionReplaceEvent<T>>());
        }

        void DisposeSubject<TSubject>(ref Subject<TSubject> subject)
        {
            if (subject != null)
            {
                try
                {
                    subject.OnCompleted();
                }
                finally
                {
                    subject.Dispose();
                    subject = null;
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeSubject(ref collectionReset);
                    DisposeSubject(ref collectionAdd);
                    DisposeSubject(ref collectionMove);
                    DisposeSubject(ref collectionRemove);
                    DisposeSubject(ref collectionReplace);
                    DisposeSubject(ref countChanged);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        #endregion
    }

    public static partial class ReactiveCollectionExtensions
    {
        public static ReactiveCollection<T> ToReactiveCollection<T>(this IEnumerable<T> source)
        {
            return new ReactiveCollection<T>(source);
        }
    }
}
