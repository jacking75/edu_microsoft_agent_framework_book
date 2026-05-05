// Chapter 07 - RAG 패턴 문서 질의응답 프로젝트
// 실행: dotnet run
// 사전 준비: TestDocuments 폴더에 TXT/PDF 파일을 넣는다.

using DotNetEnv;
using AgentFrameworkBook.Shared;
using AgentFrameworkBook.Chapter07;

Console.WriteLine("=== Chapter 07: 문서 질의응답 (RAG) ===\n");

Env.Load();

// ── 1. 문서 로드 ──────────────────────────────────────────────────
Console.WriteLine("📂 문서 로드 중...");
var docs = new DocumentManager(chunkSize: 600, overlap: 60);

var testDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDocuments");
if (!Directory.Exists(testDir))
    testDir = "TestDocuments"; // 개발 환경에서 직접 실행 시

if (Directory.Exists(testDir))
{
    docs.LoadDirectory(testDir);
}
else
{
    Console.WriteLine("⚠️  TestDocuments 폴더가 없다. 인라인 문서를 사용한다.\n");
    // 인라인 테스트용 문서
    var inlineDoc = """
        [게임 서버 개요]
        게임 서버는 TCP와 UDP를 혼합 사용한다.
        C#의 async/await로 비동기 처리를 구현한다.
        Redis로 세션을 캐싱하고 MySQL로 영구 저장한다.
        """;
    File.WriteAllText("temp_doc.txt", inlineDoc);
    docs.LoadTextFile("temp_doc.txt");
}

Console.WriteLine($"✅ 총 {docs.ChunkCount}개 청크 로드 완료\n");

// ── 2. RAG Agent 생성 ────────────────────────────────────────────
var agent = AIAgentBuilder.FromEnvironment()
    .Build("DocumentQAAgent",
        "문서 기반 질의응답 어시스턴트다. 제공된 문서 내용을 바탕으로 정확하게 답변한다.");

var qaAgent = new DocumentQAAgent(agent, docs);

// ── 3. 질의응답 ────────────────────────────────────────────────
Console.WriteLine("── 문서 질의응답 ─────────────────────────");

var questions = new[]
{
    "게임 서버에서 TCP와 UDP를 어떻게 사용하나요?",
    "메모리 관리 최적화 방법은 무엇인가요?",
    "안티치트 구현 방법을 설명해주세요.",
    "게임 서버 확장성 설계에 대해 설명해주세요.",
};

foreach (var q in questions)
{
    Console.WriteLine($"\n❓ {q}");
    var answer = await qaAgent.AskAsync(q);
    Console.WriteLine($"🤖 {answer}");
    Console.WriteLine(new string('-', 60));
}

// ── 4. Streaming 질의응답 ────────────────────────────────────────
Console.WriteLine("\n── Streaming 질의응답 ─────────────────────");
var streamQ = "게임 서버 보안에서 가장 중요한 것은 무엇인가요?";
Console.WriteLine($"❓ {streamQ}");
Console.Write("🤖 ");

await foreach (var chunk in qaAgent.AskStreamAsync(streamQ))
    Console.Write(chunk);

Console.WriteLine("\n");

// ── 5. 대화형 모드 ────────────────────────────────────────────────
Console.WriteLine("── 대화형 모드 (종료: quit) ─────────────");
while (true)
{
    Console.Write("❓ 질문: ");
    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    Console.Write("🤖 ");
    await foreach (var chunk in qaAgent.AskStreamAsync(input))
        Console.Write(chunk);
    Console.WriteLine("\n");
}
