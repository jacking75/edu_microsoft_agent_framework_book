namespace AgentFrameworkBook.Chapter05;

/// <summary>
/// 간단한 인메모리 기억 저장소다.
/// 장기 기억이 필요한 정보(사용자 선호, 프로필 등)를 key-value로 관리한다.
/// </summary>
public class SimpleMemoryProvider
{
    private readonly Dictionary<string, string> _memories = new();

    /// <summary>기억을 저장한다.</summary>
    public void Remember(string key, string value)
    {
        _memories[key] = value;
        Console.WriteLine($"  [🧠 Memory] 저장: {key} = {value}");
    }

    /// <summary>기억을 조회한다.</summary>
    public string? Recall(string key)
        => _memories.TryGetValue(key, out var v) ? v : null;

    /// <summary>모든 기억을 시스템 프롬프트에 삽입할 수 있는 형식으로 반환한다.</summary>
    public string BuildContext()
    {
        if (_memories.Count == 0) return "";
        var lines = _memories.Select(kv => $"- {kv.Key}: {kv.Value}");
        return "=== 사용자 정보 ===\n" + string.Join("\n", lines) + "\n\n";
    }

    /// <summary>현재 저장된 기억 수를 반환한다.</summary>
    public int Count => _memories.Count;

    /// <summary>모든 기억을 출력한다.</summary>
    public void PrintAll()
    {
        Console.WriteLine($"\n📖 저장된 기억 ({_memories.Count}개):");
        foreach (var (k, v) in _memories)
            Console.WriteLine($"  • {k}: {v}");
    }
}
