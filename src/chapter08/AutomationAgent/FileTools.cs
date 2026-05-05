namespace AgentFrameworkBook.Chapter08;

/// <summary>
/// 파일 시스템을 안전하게 접근하는 도구 모음이다.
/// 허용된 디렉터리 내에서만 작업이 가능하다.
/// </summary>
public class FileTools
{
    private readonly string _baseDir;

    public FileTools(string baseDir)
    {
        _baseDir = Path.GetFullPath(baseDir);
        Directory.CreateDirectory(_baseDir);
        Console.WriteLine($"  [FileTools] 베이스 디렉터리: {_baseDir}");
    }

    /// <summary>디렉터리 내 파일 목록을 반환한다.</summary>
    /// <param name="subDir">조회할 하위 디렉터리 (비어있으면 베이스 디렉터리)</param>
    public string ListFiles(string subDir = "")
    {
        Console.WriteLine($"  [🗂️ Tool] ListFiles(\"{subDir}\")");
        var target = string.IsNullOrEmpty(subDir)
            ? _baseDir
            : SafePath(subDir);

        if (!Directory.Exists(target))
            return $"디렉터리 없음: {subDir}";

        var files = Directory.GetFiles(target)
            .Select(f => new FileInfo(f))
            .Select(fi => $"  {fi.Name} ({fi.Length:N0} bytes, {fi.LastWriteTime:yyyy-MM-dd})")
            .ToList();

        return files.Count == 0
            ? "파일 없음"
            : string.Join("\n", files);
    }

    /// <summary>파일 내용을 읽어 반환한다.</summary>
    /// <param name="fileName">읽을 파일 이름</param>
    public string ReadFile(string fileName)
    {
        Console.WriteLine($"  [📖 Tool] ReadFile(\"{fileName}\")");
        var path = SafePath(fileName);
        return File.Exists(path)
            ? File.ReadAllText(path)
            : $"파일 없음: {fileName}";
    }

    /// <summary>파일에 내용을 저장한다.</summary>
    /// <param name="fileName">저장할 파일 이름</param>
    /// <param name="content">저장할 내용</param>
    public string WriteFile(string fileName, string content)
    {
        Console.WriteLine($"  [💾 Tool] WriteFile(\"{fileName}\", {content.Length}자)");
        var path = SafePath(fileName);
        File.WriteAllText(path, content);
        return $"저장 완료: {fileName} ({content.Length}자)";
    }

    // ─── 내부 메서드 ──────────────────────────────────────────────

    private string SafePath(string relativePath)
    {
        // 경로 조작 공격 방지: 허용된 디렉터리 내에서만 접근
        var combined = Path.GetFullPath(Path.Combine(_baseDir, relativePath));
        if (!combined.StartsWith(_baseDir))
            throw new UnauthorizedAccessException($"허용되지 않은 경로: {relativePath}");
        return combined;
    }
}
