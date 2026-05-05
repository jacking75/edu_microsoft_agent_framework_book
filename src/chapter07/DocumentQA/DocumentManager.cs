using UglyToad.PdfPig;

namespace AgentFrameworkBook.Chapter07;

/// <summary>
/// 문서를 로드하고 청크로 분할하여 관련 청크를 검색하는 RAG 매니저다.
/// TXT와 PDF 파일을 지원한다.
/// </summary>
public class DocumentManager
{
    private readonly List<string> _chunks = [];
    private readonly int _chunkSize;
    private readonly int _overlap;

    public DocumentManager(int chunkSize = 500, int overlap = 50)
    {
        _chunkSize = chunkSize;
        _overlap   = overlap;
    }

    // ── 문서 로드 ──────────────────────────────────────────────────

    /// <summary>텍스트 파일을 로드하여 청크로 분할한다.</summary>
    public void LoadTextFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"파일을 찾을 수 없다: {filePath}");

        var text = File.ReadAllText(filePath);
        AddChunks(text, Path.GetFileName(filePath));
        Console.WriteLine($"  [📄 Document] {Path.GetFileName(filePath)} 로드 완료 ({_chunks.Count}개 청크)");
    }

    /// <summary>PDF 파일을 로드하여 청크로 분할한다.</summary>
    public void LoadPdfFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"파일을 찾을 수 없다: {filePath}");

        using var pdf = PdfDocument.Open(filePath);
        var sb = new System.Text.StringBuilder();

        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);

        AddChunks(sb.ToString(), Path.GetFileName(filePath));
        Console.WriteLine($"  [📋 Document] {Path.GetFileName(filePath)} (PDF) 로드 완료 ({_chunks.Count}개 청크)");
    }

    /// <summary>디렉터리 내 모든 TXT/PDF 파일을 로드한다.</summary>
    public void LoadDirectory(string dirPath)
    {
        foreach (var file in Directory.GetFiles(dirPath, "*.txt"))
            LoadTextFile(file);
        foreach (var file in Directory.GetFiles(dirPath, "*.pdf"))
            LoadPdfFile(file);
    }

    // ── 검색 ───────────────────────────────────────────────────────

    /// <summary>질문과 관련된 청크를 키워드 기반으로 검색한다.</summary>
    /// <param name="query">검색 질문</param>
    /// <param name="topK">반환할 최대 청크 수</param>
    public List<string> Search(string query, int topK = 3)
    {
        if (_chunks.Count == 0) return [];

        var keywords = query
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1)
            .ToHashSet();

        return _chunks
            .Select(chunk => (chunk, score: ScoreChunk(chunk.ToLower(), keywords)))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => x.chunk)
            .ToList();
    }

    // ── 내부 메서드 ────────────────────────────────────────────────

    private void AddChunks(string text, string source)
    {
        text = text.Replace("\r\n", "\n").Trim();

        for (int i = 0; i < text.Length; i += _chunkSize - _overlap)
        {
            var len   = Math.Min(_chunkSize, text.Length - i);
            var chunk = $"[출처: {source}]\n{text.Substring(i, len)}";
            _chunks.Add(chunk);

            if (i + len >= text.Length) break;
        }
    }

    private static int ScoreChunk(string chunk, HashSet<string> keywords)
        => keywords.Count(kw => chunk.Contains(kw));

    public int ChunkCount => _chunks.Count;
}
