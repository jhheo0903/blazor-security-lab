namespace B6.DIServiceStateLab.Services;

public interface IPolicyService
{
    Task<List<string>> GetPoliciesAsync();
}

public class PolicyService : IPolicyService
{
    public async Task<List<string>> GetPoliciesAsync()
    {
        // 모의 데이터 반환 (실제로는 API 호출)
        await Task.Delay(300);
        return new List<string> { "정책 A", "정책 B", "정책 C" };
    }
}
