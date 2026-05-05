// Chapter 06 - 고급 기능: Streaming, Extended Thinking, Middleware
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;

Console.WriteLine("=== Chapter 06: 고급 기능 ===\n");

Env.Load();

// ── 1. 기본 Streaming ─────────────────────────────────────────────
Console.WriteLine("── 1. 기본 Streaming 응답 ────────────────");

var agent = AIAgentBuilder.FromEnvironment()
    .Build("StreamAgent", "친절한 어시스턴트다.");

Console.Write("🤖 Agent: ");
await foreach (var chunk in agent.RunStreamAsync("게임 서버 개발에서 중요한 3가지 원칙을 설명해줘"))
{
    if (!string.IsNullOrEmpty(chunk.Text))
        Console.Write(chunk.Text);
}
Console.WriteLine("\n");

// ── 2. Thread와 함께 Streaming ────────────────────────────────────
Console.WriteLine("── 2. Thread + Streaming 대화 ────────────");

var thread = agent.CreateThread();
var chatMessages = new[]
{
    "나는 C# 게임 서버 개발자야.",
    "비동기 프로그래밍의 핵심은?",
    "방금 말한 내 직업과 연관해서 설명해줘.",
};

foreach (var msg in chatMessages)
{
    Console.WriteLine($"👤 {msg}");
    Console.Write("🤖 ");

    await foreach (var chunk in agent.RunStreamAsync(msg, thread))
    {
        if (!string.IsNullOrEmpty(chunk.Text))
            Console.Write(chunk.Text);
    }
    Console.WriteLine("\n");
}

// ── 3. Tool 호출 중 Streaming ─────────────────────────────────────
Console.WriteLine("── 3. Tool + Streaming ───────────────────");

string GetWeather(string city)
{
    Console.WriteLine($"\n  [🌤️ Tool] GetWeather(\"{city}\")");
    return $"{city}: 맑음, 22°C";
}

var toolAgent = AIAgentBuilder.FromEnvironment()
    .Build("WeatherStreamAgent", "날씨 정보를 스트리밍으로 제공하는 어시스턴트다.",
        new Delegate[] { GetWeather });

Console.Write("🤖 ");
await foreach (var chunk in toolAgent.RunStreamAsync("서울과 부산 날씨를 비교해줘"))
{
    if (!string.IsNullOrEmpty(chunk.Text))
        Console.Write(chunk.Text);
}
Console.WriteLine("\n");

// ── 4. Middleware: 로깅 Wrapper ────────────────────────────────────
Console.WriteLine("── 4. Logging Middleware Wrapper ─────────");

var loggingAgent = AIAgentBuilder.FromEnvironment()
    .Build("LogAgent", "어시스턴트다.");

var wrapper = new LoggingAgentWrapper(loggingAgent);

var r1 = await wrapper.RunAsync("짧게 자기소개 해줘");
Console.WriteLine($"응답: {r1}\n");

var r2 = await wrapper.RunAsync("AI의 미래는?");
Console.WriteLine($"응답: {r2[..Math.Min(50, r2.Length)]}...\n");

// ── 5. Middleware: 성능 측정 Wrapper ──────────────────────────────
Console.WriteLine("── 5. Metrics Wrapper ────────────────────");

var metricsAgent = AIAgentBuilder.FromEnvironment()
    .Build("MetricsAgent", "빠르게 답변하는 어시스턴트다.");

var metricsWrapper = new MetricsWrapper(metricsAgent);

await metricsWrapper.RunAsync("안녕하세요");
await metricsWrapper.RunAsync("게임 서버란?");
await metricsWrapper.RunAsync("C#의 장점 3가지");

metricsWrapper.PrintStats();

// ── Wrapper 클래스들 ──────────────────────────────────────────────

class LoggingAgentWrapper(Microsoft.Agents.AI.AIAgent agent)
{
    private int _callCount = 0;

    public async Task<string> RunAsync(string input,
        object? thread = null)
    {
        _callCount++;
        Log($"[#{_callCount}] 요청: {input}");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = thread != null
                ? await agent.RunAsync(input, thread)
                : await agent.RunAsync(input);
            sw.Stop();
            Log($"[#{_callCount}] 응답 ({sw.ElapsedMilliseconds}ms): {result.Text[..Math.Min(60, result.Text.Length)]}");
            return result.Text;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log($"[#{_callCount}] 오류: {ex.Message}");
            throw;
        }
    }

    private void Log(string msg)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  LOG | {msg}");
        Console.ResetColor();
    }
}

class MetricsWrapper(Microsoft.Agents.AI.AIAgent agent)
{
    private readonly List<(string query, long ms, int chars)> _metrics = [];

    public async Task<string> RunAsync(string input)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await agent.RunAsync(input);
        sw.Stop();
        _metrics.Add((input[..Math.Min(30, input.Length)], sw.ElapsedMilliseconds, result.Text.Length));
        return result.Text;
    }

    public void PrintStats()
    {
        Console.WriteLine($"\n📊 성능 통계 ({_metrics.Count}회 호출):");
        Console.WriteLine($"   평균 응답 시간: {_metrics.Average(m => m.ms):F0}ms");
        Console.WriteLine($"   최대 응답 시간: {_metrics.Max(m => m.ms)}ms");
        Console.WriteLine($"   평균 응답 길이: {_metrics.Average(m => m.chars):F0}자");
    }
}
