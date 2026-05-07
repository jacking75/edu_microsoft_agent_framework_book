// Chapter 03 - 첫 번째 Agent: 기본 생성, 멀티턴 대화, 에러 핸들링
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;

Console.WriteLine("=== Chapter 03: 첫 번째 Agent ===\n");

Env.Load();

// ── 1. 기본 Agent 생성 ────────────────────────────────────────────
var agent = AIAgentBuilder.FromEnvironment()
    .Build(
        name:         "HelloAgent",
        instructions: "당신은 친절하고 유능한 어시스턴트다. 답변은 항상 한국어로 한다.");

Console.WriteLine("✅ Agent 생성 완료\n");

// ── 2. 단발성 질문 ────────────────────────────────────────────────
Console.WriteLine("── 단발성 질문 ──────────────────────────");
var result = await agent.RunAsync("안녕하세요! 오늘의 기분을 한 문장으로 말해줘.");
Console.WriteLine($"🤖 {result.Text}\n");

// ── 3. Session을 이용한 멀티턴 대화 ──────────────────────────────
Console.WriteLine("── 멀티턴 대화 ──────────────────────────");
var session = await agent.CreateSessionAsync();

var messages = new[]
{
    "내 이름은 흥배야.",
    "내 이름이 뭔지 기억해?",
    "나는 게임 서버 개발자야. 주로 C#과 C++을 사용해.",
    "내가 사용하는 프로그래밍 언어는 뭐라고 했지?"
};

foreach (var msg in messages)
{
    Console.WriteLine($"👤 {msg}");
    var r = await agent.RunAsync(msg, session);
    Console.WriteLine($"🤖 {r.Text}\n");
}

// ── 4. 에러 핸들링 ────────────────────────────────────────────────
Console.WriteLine("── 에러 핸들링 ──────────────────────────");

try
{
    // 빈 API 키로 Agent 생성 시도 (의도적 오류)
    var badAgent = AIAgentBuilder
        .WithOpenRouter("", "anthropic/claude-sonnet-4-5")
        .Build("BadAgent", "테스트");

    await badAgent.RunAsync("테스트");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"✅ 예상된 ArgumentException 처리: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"✅ 예외 처리: {ex.GetType().Name} - {ex.Message}");
}

// ── 5. 대화형 모드 ────────────────────────────────────────────────
Console.WriteLine("\n── 대화형 모드 (종료: quit) ─────────────");
var chatSession = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("👤 당신: ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    try
    {
        var response = await agent.RunAsync(input, chatSession);
        Console.WriteLine($"🤖 Agent: {response.Text}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 오류: {ex.Message}\n");
    }
}

Console.WriteLine("👋 대화 종료.");
