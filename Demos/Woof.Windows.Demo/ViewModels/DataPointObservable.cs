using Woof.Windows.Demo.Models;

namespace Woof.Windows.Demo.ViewModels;

public sealed class DataPointObservable : DataPoint, INotifyPropertyChanged, IDisposable {

    public event PropertyChangedEventHandler? PropertyChanged;

    public new double X { get => base.X; set { base.X = value; OnPropertyChanged(nameof(X)); } }

    public new double Y { get => base.Y; set { base.Y = value; OnPropertyChanged(nameof(Y)); } }

    public new double Z { get => base.Z; set { base.Z = value; OnPropertyChanged(nameof(Z)); } }

    public DataPointObservable() { }

    public DataPointObservable(DataPoint point) {
        Id = point.Id;
        base.X = point.X;
        base.Y = point.Y;
        base.Z = point.Z;
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose() => Debug.Print("Oh no!");

}

public static class DataPointObservableExtensions {
    public static IEnumerable<DataPointObservable> AsObservable(this IEnumerable<DataPoint> points)
        => points.Select(p => new DataPointObservable(p));

    public static IAsyncEnumerable<DataPointObservable> AsObservable(this IAsyncEnumerable<DataPoint> points)
        => points.Select(p => new DataPointObservable(p));

}
