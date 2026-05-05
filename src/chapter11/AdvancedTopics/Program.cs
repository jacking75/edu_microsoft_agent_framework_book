// Chapter 11 - 고급 주제: MCP 연동, 동적 모델 선택, Fallback 전략, 로컬 LLM
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;
using System.Text.Json;

Console.WriteLine("=== Chapter 11: 고급 주제 ===\n");

Env.Load();

var apiKey  = Environment.GetEnvironmentVariable("LLM_API_KEY")!;
var baseUrl = Environment.GetEnvironmentVariable("LLM_BASE_URL") ?? "https://openrouter.ai/api/v1";
var model   = Environment.GetEnvironmentVariable("LLM_MODEL")   ?? "anthropic/claude-sonnet-4-5";
var provider = baseUrl.Contains("poe") ? "poe" : "openrouter";

// ── 1. 동적 모델 선택 ─────────────────────────────────────────────
Console.WriteLine("── 1. 동적 모델 선택 ─────────────────────");

var queries = new[]
{
    ("안녕",                                               TaskComplexity.Simple),
    ("파이썬과 C#의 차이를 설명해줘",                     TaskComplexity.Medium),
    ("분산 시스템에서 CAP 이론과 데이터베이스 설계 전략", TaskComplexity.Complex),
};

foreach (var (query, expectedComplexity) in queries)
{
    var complexity = ModelSelector.Analyze(query);
    var selectedModel = ModelSelector.GetModel(complexity, provider);

    Console.WriteLine($"❓ {query}");
    Console.WriteLine($"   분류: {complexity} → 모델: {selectedModel}");

    // 복잡도에 맞는 모델로 Agent 생성
    var adaptiveAgent = AIAgentBuilder
        .WithCustomEndpoint(apiKey, baseUrl, selectedModel)
        .Build("AdaptiveAgent", "전문 어시스턴트다.");

    var r = await adaptiveAgent.RunAsync(query);
    Console.WriteLine($"   🤖 {r.Text[..Math.Min(80, r.Text.Length)]}...\n");
}

// ── 2. Fallback 전략 ──────────────────────────────────────────────
Console.WriteLine("── 2. Fallback 전략 ──────────────────────");

var fallbackRunner = new FallbackAgentBuilder(apiKey)
    // 우선 시도: 메인 모델
    .AddProvider(baseUrl, model)
    // Fallback 1: 더 작은 모델
    .AddProvider(baseUrl, provider == "poe"
        ? "claude-haiku-4-20250514"
        : "anthropic/claude-haiku-4-5")
    .Build("FallbackAgent", "신뢰할 수 있는 어시스턴트다.");

try
{
    var fallbackResult = await fallbackRunner.RunAsync("AI 에이전트의 장점 3가지를 나열해줘");
    Console.WriteLine($"✅ 결과: {fallbackResult[..Math.Min(150, fallbackResult.Length)]}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 모든 공급자 실패: {ex.Message}\n");
}

// ── 3. MCP Tool Wrapper (로컬 MCP 서버가 없을 경우 시뮬레이션) ────
Console.WriteLine("── 3. MCP Tool Wrapper (시뮬레이션) ──────");

// MCP 서버가 없으므로 동일한 인터페이스를 가진 시뮬레이션 도구 사용
string GetSystemInfoMcp()
{
    Console.WriteLine("  [🔌 MCP] get_system_info 호출");
    return $"OS: {Environment.OSVersion}, CPU: {Environment.ProcessorCount}코어, .NET: {Environment.Version}";
}

string CalculateMcp(string expression)
{
    Console.WriteLine($"  [🔌 MCP] calculate({expression}) 호출");
    try
    {
        var dt = new System.Data.DataTable();
        var result = dt.Compute(expression, "");
        return $"{expression} = {result}";
    }
    catch
    {
        return "계산 오류";
    }
}

var mcpAgent = AIAgentBuilder.FromEnvironment()
    .Build("McpAgent",
        "MCP 도구를 사용하여 시스템 정보를 조회하고 계산을 수행한다.",
        new Delegate[] { GetSystemInfoMcp, CalculateMcp });

var mcpR1 = await mcpAgent.RunAsync("현재 서버의 CPU 코어 수와 OS 정보를 알려줘");
Console.WriteLine($"🤖 {mcpR1.Text}\n");

var mcpR2 = await mcpAgent.RunAsync("125 곱하기 847은 얼마야?");
Console.WriteLine($"🤖 {mcpR2.Text}\n");

// ── 4. 환경 변수 기반 완전 추상화 확인 ───────────────────────────
Console.WriteLine("── 4. 환경 변수 추상화 확인 ─────────────");
Console.WriteLine("현재 .env 설정:");
Console.WriteLine($"  LLM_BASE_URL = {baseUrl}");
Console.WriteLine($"  LLM_MODEL    = {model}");
Console.WriteLine($"  공급자       = {provider}");
Console.WriteLine();
Console.WriteLine("""
  .env 파일 변경으로 공급자 전환 가능:

  # OpenRouter 사용
  LLM_BASE_URL=https://openrouter.ai/api/v1
  LLM_MODEL=anthropic/claude-sonnet-4-5

  # Poe 사용
  LLM_BASE_URL=https://api.poe.com/llm/v1
  LLM_MODEL=claude-sonnet-4-20250514

  # 로컬 Ollama 사용
  LLM_BASE_URL=http://localhost:11434/v1
  LLM_MODEL=llama3.2
  LLM_API_KEY=ollama

  코드 변경 없이 .env만 바꾸면 된다.
""");

// ── 5. 응답 품질 검증 패턴 ────────────────────────────────────────
Console.WriteLine("── 5. 응답 품질 검증 ────────────────────");

var validationAgent = AIAgentBuilder.FromEnvironment()
    .Build("ValidationAgent", "요청한 형식대로 정확하게 답변하는 어시스턴트다.");

// JSON 응답 검증 예제
var jsonPrompt = "서버 3대의 정보를 JSON 배열로 반환해줘. 각 서버는 name, cpu, memory 필드를 가진다. JSON만 반환하고 다른 설명은 하지 마라.";
Console.WriteLine($"❓ {jsonPrompt}");

string? validResult = null;
for (int attempt = 1; attempt <= 3; attempt++)
{
    var r = await validationAgent.RunAsync(jsonPrompt);
    var text = r.Text.Trim();

    // JSON 배열 형식 검증
    if (text.StartsWith("[") && text.EndsWith("]"))
    {
        try
        {
            JsonDocument.Parse(text);
            validResult = text;
            Console.WriteLine($"✅ {attempt}번째 시도에서 유효한 JSON 수신");
            break;
        }
        catch { }
    }

    Console.WriteLine($"  [{attempt}/3] JSON 형식 검증 실패, 재시도...");
}

if (validResult != null)
    Console.WriteLine($"📋 결과:\n{validResult}");
else
    Console.WriteLine("❌ 유효한 JSON 응답을 받지 못했다.");

Console.WriteLine("\n✅ Chapter 11 완료. 모든 챕터 학습이 끝났다!");

// ── 헬퍼 클래스들 ────────────────────────────────────────────────

enum TaskComplexity { Simple, Medium, Complex }

static class ModelSelector
{
    private static readonly Dictionary<TaskComplexity, string> OpenRouterModels = new()
    {
        [TaskComplexity.Simple]  = "anthropic/claude-haiku-4-5",
        [TaskComplexity.Medium]  = "anthropic/claude-sonnet-4-5",
        [TaskComplexity.Complex] = "anthropic/claude-opus-4",
    };

    private static readonly Dictionary<TaskComplexity, string> PoeModels = new()
    {
        [TaskComplexity.Simple]  = "claude-haiku-4-20250514",
        [TaskComplexity.Medium]  = "claude-sonnet-4-20250514",
        [TaskComplexity.Complex] = "claude-opus-4-20250514",
    };

    public static TaskComplexity Analyze(string prompt)
    {
        var words = prompt.Split(' ').Length;
        var hasComplex = prompt.Contains("분산") || prompt.Contains("아키텍처") || prompt.Contains("전략") || prompt.Contains("이론");
        var hasMedium  = prompt.Contains("비교") || prompt.Contains("설명") || prompt.Contains("차이");

        if (hasComplex || words > 10) return TaskComplexity.Complex;
        if (hasMedium  || words >  5) return TaskComplexity.Medium;
        return TaskComplexity.Simple;
    }

    public static string GetModel(TaskComplexity c, string provider)
        => provider == "poe" ? PoeModels[c] : OpenRouterModels[c];
}

class FallbackAgentBuilder
{
    private readonly string _apiKey;
    private readonly List<(string url, string model)> _providers = [];

    public FallbackAgentBuilder(string apiKey) => _apiKey = apiKey;

    public FallbackAgentBuilder AddProvider(string url, string model)
    {
        _providers.Add((url, model));
        return this;
    }

    public FallbackRunner Build(string name, string instructions)
        => new(_apiKey, _providers, name, instructions);
}

class FallbackRunner
{
    private readonly string _apiKey;
    private readonly List<(string url, string model)> _providers;
    private readonly string _name, _instructions;

    public FallbackRunner(string apiKey, List<(string url, string model)> providers,
        string name, string instructions)
    {
        _apiKey = apiKey; _providers = providers;
        _name = name; _instructions = instructions;
    }

    public async Task<string> RunAsync(string input)
    {
        Exception? last = null;
        foreach (var (url, model) in _providers)
        {
            try
            {
                Console.WriteLine($"  [Fallback] 시도: {model}");
                var agent = AIAgentBuilder
                    .WithCustomEndpoint(_apiKey, url, model)
                    .Build(_name, _instructions);
                var r = await agent.RunAsync(input);
                Console.WriteLine($"  [Fallback] 성공: {model}");
                return r.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Fallback] 실패: {model} - {ex.Message}");
                last = ex;
                await Task.Delay(500);
            }
        }
        throw new Exception($"모든 공급자 실패: {last?.Message}");
    }
}
