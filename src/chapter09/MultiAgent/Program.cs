// Chapter 09 - Multi-Agent 워크플로우: 순차, 병렬, 조건 실행
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Chapter09;

Console.WriteLine("=== Chapter 09: Multi-Agent 워크플로우 ===\n");

Env.Load();

var topic = "AI 기반 게임 서버 자동 스케일링 시스템";

// ── 1. 순차 워크플로우 ─────────────────────────────────────────────
Console.WriteLine($"── 1. 순차 워크플로우 ────────────────────");
Console.WriteLine($"주제: {topic}\n");

// Step 1: 조사
Console.WriteLine("📚 [1/4] 리서치 중...");
var researcher = AgentFactory.CreateResearchAgent();
var research   = await researcher.RunAsync($"다음 주제를 조사해줘: {topic}");
Console.WriteLine($"✅ 조사 완료 ({research.Text.Length}자)\n");

// Step 2: 분석
Console.WriteLine("🔍 [2/4] 분석 중...");
var analyst  = AgentFactory.CreateTechAnalystAgent();
var analysis = await analyst.RunAsync(
    $"다음 조사 결과를 기술적으로 분석해줘:\n\n{research.Text}");
Console.WriteLine($"✅ 분석 완료 ({analysis.Text.Length}자)\n");

// Step 3: 문서 작성
Console.WriteLine("✍️  [3/4] 문서 작성 중...");
var writer = AgentFactory.CreateWriterAgent();
var draft  = await writer.RunAsync(
    $"조사와 분석을 바탕으로 보고서를 작성해줘:\n\n조사:\n{research.Text}\n\n분석:\n{analysis.Text}");
Console.WriteLine($"✅ 초안 완료 ({draft.Text.Length}자)\n");

// Step 4: 검토
Console.WriteLine("🔎 [4/4] 검토 중...");
var reviewer = AgentFactory.CreateReviewerAgent();
var review   = await reviewer.RunAsync(
    $"다음 보고서를 검토하고 피드백을 줘:\n\n{draft.Text}");
Console.WriteLine($"✅ 검토 완료\n");

Console.WriteLine("📋 최종 검토 결과:");
Console.WriteLine(review.Text[..Math.Min(300, review.Text.Length)] + "...\n");

// ── 2. 병렬 워크플로우 ─────────────────────────────────────────────
Console.WriteLine("── 2. 병렬 워크플로우 ────────────────────");
Console.WriteLine($"주제: {topic}\n");

var sw = System.Diagnostics.Stopwatch.StartNew();

Console.WriteLine("⚡ 기술/비즈니스/리스크 분석을 병렬로 실행 중...");

var techAgent = AgentFactory.CreateTechAnalystAgent();
var bizAgent  = AgentFactory.CreateBusinessAnalystAgent();
var riskAgent = AgentFactory.CreateRiskAnalystAgent();

var parallelTasks = new Task<string>[]
{
    RunAgent(techAgent, $"기술적 관점에서 분석해줘 (3줄 이내): {topic}"),
    RunAgent(bizAgent,  $"비즈니스 관점에서 분석해줘 (3줄 이내): {topic}"),
    RunAgent(riskAgent, $"리스크 관점에서 분석해줘 (3줄 이내): {topic}"),
};

var parallelResults = await Task.WhenAll(parallelTasks);
sw.Stop();

Console.WriteLine($"✅ 병렬 분석 완료 ({sw.ElapsedMilliseconds}ms)\n");
Console.WriteLine($"🔧 기술 분석: {parallelResults[0]}\n");
Console.WriteLine($"💼 비즈니스 분석: {parallelResults[1]}\n");
Console.WriteLine($"⚠️  리스크 분석: {parallelResults[2]}\n");

// 병렬 결과 통합
var synthesizer = AgentFactory.CreateWriterAgent();
var combined = await synthesizer.RunAsync($"""
    다음 3가지 관점의 분석을 통합하여 핵심 요약을 3줄로 작성해줘.

    기술: {parallelResults[0]}
    비즈니스: {parallelResults[1]}
    리스크: {parallelResults[2]}
    """);

Console.WriteLine($"📊 통합 요약: {combined.Text}\n");

// ── 3. 조건 분기 워크플로우 ───────────────────────────────────────
Console.WriteLine("── 3. 조건 분기 워크플로우 ──────────────");

var classifier = AgentFactory.CreateClassifierAgent();

var requests = new[]
{
    "Redis와 Memcached의 성능 차이를 비교해줘",
    "AI 도입으로 인한 시장 규모 변화 예측해줘",
    "오늘 날씨가 어때?",
};

foreach (var request in requests)
{
    Console.WriteLine($"❓ {request}");

    // 분류
    var classResult = await classifier.RunAsync(request);
    var category    = classResult.Text.Trim().ToUpper();
    Console.WriteLine($"🏷️  분류: {category}");

    // 분류에 따른 라우팅
    string answer;
    if (category.Contains("TECHNICAL"))
    {
        var techExpert = AgentFactory.CreateTechAnalystAgent();
        var r = await techExpert.RunAsync(request);
        answer = r.Text;
    }
    else if (category.Contains("BUSINESS"))
    {
        var bizExpert = AgentFactory.CreateBusinessAnalystAgent();
        var r = await bizExpert.RunAsync(request);
        answer = r.Text;
    }
    else
    {
        var generalAgent = AgentFactory.CreateResearchAgent();
        var r = await generalAgent.RunAsync(request);
        answer = r.Text;
    }

    Console.WriteLine($"🤖 {answer[..Math.Min(150, answer.Length)]}...\n");
}

// ─── 헬퍼 ────────────────────────────────────────────────────────

static async Task<string> RunAgent(Microsoft.Agents.AI.AIAgent agent, string prompt)
{
    var result = await agent.RunAsync(prompt);
    return result.Text;
}
