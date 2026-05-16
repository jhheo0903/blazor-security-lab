namespace B6.DIServiceStateLab.Services;

public interface IGlobalStateService
{
    string Message { get; set; }
    int GlobalCounter { get; }
    void IncrementGlobal();
}

public class GlobalStateService : IGlobalStateService
{
    public string Message { get; set; } = "앱 전역 상태 (Singleton)";
    private int _globalCounter = 0;

    public int GlobalCounter => _globalCounter;

    public void IncrementGlobal() => _globalCounter++;
}
