namespace Woof.Windows.Mvvm;

/// <summary>
/// Observable <see cref="List{T}"/> implementing <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.<br/>
/// Disposes removed or replaced objects if they implement <see cref="IDisposable"/>.<br/>
/// Provides live filtering for controls like DataGrid via providing a <see cref="ShadowList"/> of the filtered items.<br/>
/// Use this to bind any synchronous or asynchronous data sources to any visual item sources.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public class ObservableList<T> : List<T>, IList, ICollection, INotifyCollectionChanged, INotifyPropertyChanged {

    /// <summary>
    /// Occurs when the collection elements are added, removed or replaced.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when the collection element is changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the predicate used to filter items for the enumerator.
    /// </summary>
    public Func<T, bool>? Filter { get; set; }

    //private Func<T, bool>? _Filter;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public new T this[int index] {
        get => base[index];
        set {
            base[index] = value;
            if (value is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(value, e);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }
    }

    /// <summary>
    /// Creates an empty list.
    /// </summary>
    public ObservableList() : base() { }

    /// <summary>
    /// Creates an empty list with initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public ObservableList(int capacity) : base(capacity) { } //=> Items = new(capacity);

    /// <summary>
    /// Adds an item to the end of the <see cref="ObservableList{T}"/> and triggers <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="item">The item to be added to the end of the <see cref="ObservableList{T}"/>.</param>
    public new void Add(T item) {
        base.Add(item);
        if (Filter is not null && ShadowList is null) ShadowList = new();
        if (Filter is not null && Filter(item)) ShadowList!.Add(item);
        if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        if (Filter is null) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ShadowList!.IndexOf(item)));
    }

    /// <summary>
    /// Adds an item to the end of the <see cref="ObservableList{T}"/> if it's a new item
    /// and triggers <see cref="CollectionChanged"/> event if applicable.
    /// </summary>
    /// <param name="item">The item to be added to the end of the <see cref="ObservableList{T}"/>.</param>
    public void AddDistinct(T item) {
        if (Contains(item)) return;
        base.Add(item);
        if (Filter is not null && ShadowList is null) ShadowList = new();
        if (Filter is not null && Filter(item)) ShadowList!.Add(item);
        if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        if (Filter is null) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ShadowList!.IndexOf(item)));
    }

    /// <summary>
    /// Adds all items from the collection to the end of the <see cref="ObservableList{T}"/> and
    /// triggers <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of <see cref="ObservableList{T}"/>.
    /// The collection itself cannot be null, but it can contain elements that are null,
    /// if type <typeparamref name="T"/> is a reference type.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public new void AddRange(IEnumerable<T> collection) {
        base.AddRange(collection);
        foreach (T item in this as List<T>)
            if (item is INotifyPropertyChanged observableItem)
                observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        if (Filter is null)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        else Refresh();
    }

    /// <summary>
    /// Adds all items from the asynchronous stream to the end of the <see cref="ObservableList{T}"/> and
    /// triggers <see cref="CollectionChanged"/> event on each item added.
    /// </summary>
    /// <param name="asyncStream">Asynchronous stream of <typeparamref name="T"/> items.</param>
    /// <returns><see cref="ValueTask"/> completed when all items are added.</returns>
    public async ValueTask AddRangeAsync(IAsyncEnumerable<T> asyncStream) {
        await foreach (T? item in asyncStream) Add(item);
    }

    /// <summary>
    /// Removes all elements from the <see cref="ObservableList{T}"/> and trigers <see cref="CollectionChanged"/> event.
    /// </summary>
    public new void Clear() {
        foreach (T? item in this as List<T>) {
            if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged -= (s, e) => OnPropertyChanged(observableItem, e);
            if (item is IDisposable disposableItem) disposableItem.Dispose();
        }
        base.Clear();
        if (ShadowList is not null) ShadowList.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Inserts an item to the <see cref="ObservableList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="ObservableList{T}"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the System.Collections.Generic.IList`1.</exception>
    public new void Insert(int index, T item) {
        base.Insert(index, item);
        if (ShadowList is not null && Filter is not null) ShadowList = new(this.Where(Filter));
        if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        if (Filter is null) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        else OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, ShadowList!.IndexOf(item)));
    }

    /// <summary>
    /// Refreshes the view to apply changes like filtering.
    /// </summary>
    public void Refresh() {
        ShadowList = Filter is null ? null : new List<T>(this.Where(Filter));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ObservableList{T}"/>
    /// and triggers <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>
    /// true if item is successfully removed; otherwise, false. This method also returns
    /// false" if item was not found in the <see cref="ObservableList{T}"/>.
    /// </returns>
    public new bool Remove(T item) {
        int index = IndexOf(item);
        if (index < 0) return false;
        if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged -= (s, e) => OnPropertyChanged(observableItem, e);
        if (item is IDisposable disposableItem) disposableItem.Dispose();
        base.RemoveAt(index);
        if (ShadowList is null) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        else {
            index = ShadowList.IndexOf(item);
            ShadowList.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
        return true;
    }

    /// <summary>
    /// Removes the element at the specified index of the <see cref="ObservableList{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0. -or- <paramref name="index"/> is equal to or greater than <see cref="List{T}.Count"/>.
    /// </exception>
    public new void RemoveAt(int index) => Remove(base[index]);

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to remove.</param>
    /// <exception cref="ArgumentNullException">match is null.</exception>
    public new void RemoveAll(Predicate<T> match) {
        while (this.FirstOrDefault(i => match(i)) is T item) Remove(item);
    }

    /// <summary>
    /// Removes a range of elements from the <see cref="ObservableList{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0. -or- <paramref name="count"/> is less than 0.</exception>
    /// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="ObservableList{T}"/>.</exception>
    public new void RemoveRange(int index, int count) {
        base.RemoveRange(index, count);
        for (int i = index; i < count && i < Count; i++) RemoveAt(index);
    }

    /// <summary>
    /// Replaces the first matching element in the collection.
    /// </summary>
    /// <param name="item">Element to replace.</param>
    /// <param name="replacement">Replacement.</param>
    public void Replace(T item, T replacement) {
        int index = IndexOf(item);
        if (item is INotifyPropertyChanged observableItem) observableItem.PropertyChanged -= (s, e) => OnPropertyChanged(observableItem, e);
        if (item is IDisposable disposableItem) disposableItem.Dispose();
        if (index >= 0) this[index] = replacement;
    }

    /// <summary>
    /// Reverses the order of the elements in the entire <see cref="ObservableList{T}"/>.
    /// </summary>
    public new void Reverse() {
        base.Reverse();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Reverses the order of the elements in the specified range.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to reverse.</param>
    /// <param name="count">The number of elements in the range to reverse.</param>
    public new void Reverse(int index, int count) {
        base.Reverse(index, count);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="ObservableList{T}"/> using the default comparer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The default comparer <see cref="System.Collections.Generic.Comparer{T}.Default"/> cannot find
    /// an implementation of the <see cref="IComparable{T}"/> generic interface
    /// or the <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
    /// </exception>
    public new void Sort() {
        base.Sort();
        if (ShadowList != null) ShadowList.Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="ObservableList{T}"/> using the specified comparer.
    /// </summary>
    /// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}"/> implementation to use when comparing
    /// elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="comparer"/> is null, and the default comparer
    /// <see cref="System.Collections.Generic.Comparer{T}.Default"/>
    /// cannot find implementation of the <see cref="IComparable{T}"/> generic interface or the
    /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The implementation of <paramref name="comparer"/> caused an error during the sort. For example,
    /// <paramref name="comparer"/> might not return 0 when comparing an item with itself.
    /// </exception>
    public new void Sort(IComparer<T>? comparer) {
        base.Sort(comparer);
        if (ShadowList != null) ShadowList.Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="ObservableList{T}"/> using the specified <see cref="Comparison{T}"/>.
    /// </summary>
    /// <param name="comparison">The <see cref="Comparison{T}"/> to use when comparing elements.</param>
    /// <exception cref="ArgumentNullException"><paramref name="comparison"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// The implementation of <paramref name="comparison"/> caused an error during the sort. For example,
    /// <paramref name="comparison"/> might not return 0 when comparing an item with itself.
    /// </exception>
    public new void Sort(Comparison<T> comparison) {
        base.Sort(comparison);
        if (ShadowList != null) ShadowList.Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Sorts the elements in a range of elements in <see cref="ObservableList{T}"/> using the specified comparer.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range to sort.</param>
    /// <param name="count">The length of the range to sort.</param>
    /// <param name="comparer">The <see cref="System.Collections.Generic.IComparer{T}"/> implementation to use when comparing
    /// elements, or null to use the default comparer <see cref="System.Collections.Generic.Comparer{T}.Default"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0. -or- <paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not specify a valid range in the <see cref="ObservableList{T}"/>.
    /// -or- The implementation of comparer caused an error during the sort. For example,
    /// comparer might not return 0 when comparing an item with itself.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="comparer"/> is null, and the default comparer
    /// <see cref="System.Collections.Generic.Comparer{T}.Default"/>
    /// cannot find implementation of the <see cref="IComparable{T}"/> generic interface or the
    /// <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
    /// </exception>
    public new void Sort(int index, int count, IComparer<T>? comparer) {
        base.Sort(index, count, comparer);
        if (ShadowList != null) ShadowList.Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    int ICollection.Count => ShadowList is null ? Count : ShadowList.Count;

    /// <summary>
    /// Gets the standard or filtering enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => Filter is null
            ? (IEnumerator)BaseIEnumerableGetEnumerator.Invoke(this, null)!
            : this.Where(Filter).GetEnumerator();

    /// <summary>
    /// Gets the standard or filtering enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
    public new IEnumerator<T> GetEnumerator()
        => Filter is null
            ? base.GetEnumerator()
            : this.Where(Filter).GetEnumerator();

    /// <summary>
    /// Triggers the <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="e">Information about the event.</param>
    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    /// <summary>
    /// Triggers the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="item">The source of the event.</param>
    /// <param name="e">Information about the event.</param>
    protected void OnPropertyChanged(object? item, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(item, e);

    private List<T>? ShadowList;

    #region IList implementation

    /// <summary>
    /// Invokes <see cref="System.Collections.IList.Add"/> on the base class.
    /// </summary>
    /// <param name="value">The object to add to the <see cref="IList"/>.</param>
    /// <returns>The position into which the new element was inserted, or -1 to indicate that
    /// the item was not inserted into the collection.</returns>
    private int BaseIListAdd(object? value) => (int)BaseIListAddMethodInfo.Invoke(this, new object?[] { value })!;

    /// <summary>
    /// Invokes <see cref="System.Collections.IList.Insert(int, object?)"/> on the base class.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="value">The object to insert into the <see cref="ObservableList{T}"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the System.Collections.Generic.IList`1.</exception>
    private void BaseIListInsert(int index, object? value) => BaseIListInsertMethodInfo.Invoke(this, new object?[] { index, value });

    /// <summary>
    /// Adds an item to the <see cref="IList"/> and triggers <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="value">The object to add to the <see cref="IList"/>.</param>
    /// <returns>The position into which the new element was inserted, or -1 to indicate that
    /// the item was not inserted into the collection.</returns>
    int IList.Add(object? value) {
        int index = BaseIListAdd(value);
        if (ShadowList is not null) ShadowList.Add((T)value!);
        if (index < 0) return index;
        if (value is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        return index;
    }

    /// <summary>
    /// Removes all elements from the <see cref="IList{T}"/> and trigers <see cref="CollectionChanged"/> event.
    /// </summary>
    void IList.Clear() => Clear();

    /// <summary>
    /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="value">The object to insert into the <see cref="List{T}"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the System.Collections.Generic.IList`1.</exception>
    void IList.Insert(int index, object? value) {
        BaseIListInsert(index, value);
        if (ShadowList is not null) ShadowList.Insert(index, (T)value!);
        if (value is INotifyPropertyChanged observableItem) observableItem.PropertyChanged += (s, e) => OnPropertyChanged(observableItem, e);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="IList"/> and triggers <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="value">The object to remove from the <see cref="IList"/>.</param>
    void IList.Remove(object? value) => RemoveAt((this as IList).IndexOf(value));

    /// <summary>
    /// Removes the <see cref="IList"/> item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="IList"/>.</exception>
    void IList.RemoveAt(int index) => (this as IList).Remove(base[index]);

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    object? IList.this[int index] {
        get => ShadowList is null ? base[index] : ShadowList[index];
        set {
            if (ShadowList is null) base[index] = (T)value!;
            else ShadowList[index] = (T)value!;
        }
    }

    /// <summary>
    /// Base type of the class.
    /// </summary>
    private static readonly Type BaseType = typeof(ObservableList<T>).BaseType!;

    /// <summary>
    /// <see cref="System.Collections.IList.Add(object?)"/> from the base class method info.
    /// </summary>
    private static readonly MethodInfo BaseIListAddMethodInfo = BaseType.GetMethod(
            "System.Collections.IList.Add",
            BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null
        )!;

    /// <summary>
    /// <see cref="System.Collections.IList.Insert(int, object?)"/> from the base class method info.
    /// </summary>
    private static readonly MethodInfo BaseIListInsertMethodInfo = BaseType.GetMethod(
            "System.Collections.IList.Insert",
            BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(object) }, null
        )!;

    private static readonly MethodInfo BaseIEnumerableGetEnumerator = BaseType.GetMethod(
        "System.Collections.IEnumerable.GetEnumerator", BindingFlags.NonPublic | BindingFlags.Instance)!;

    #endregion

}
