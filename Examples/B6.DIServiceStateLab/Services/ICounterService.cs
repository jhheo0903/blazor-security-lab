namespace B6.DIServiceStateLab.Services;

public interface ICounterService
{
    int GetCount();
    void Increment();
    void Reset();
    string InstanceId { get; }
}

public class CounterService : ICounterService
{
    private int _count = 0;
    private readonly string _instanceId = Guid.NewGuid().ToString()[..8];

    public string InstanceId => _instanceId;

    public int GetCount() => _count;

    public void Increment() => _count++;

    public void Reset() => _count = 0;
}
