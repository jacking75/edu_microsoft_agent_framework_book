// Chapter 04 - Tool 호출: 날씨, 시각, 환율, 계산기 Tool을 가진 Agent
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;

Console.WriteLine("=== Chapter 04: Tool 호출 Agent ===\n");

Env.Load();

// ── Tool 함수 정의 ──────────────────────────────────────────────

/// <summary>도시의 현재 날씨를 반환한다.</summary>
/// <param name="city">날씨를 조회할 도시명 (예: 서울, 부산)</param>
string GetWeather(string city)
{
    Console.WriteLine($"  [🌤️ Tool] GetWeather(\"{city}\")");
    // 실제 프로젝트에서는 기상청 API를 호출한다
    var weathers = new Dictionary<string, string>
    {
        ["서울"]  = "맑음, 22°C",
        ["부산"]  = "흐림, 19°C",
        ["제주"]  = "비, 18°C",
        ["도쿄"]  = "맑음, 25°C",
        ["뉴욕"]  = "눈, -2°C",
    };
    return weathers.TryGetValue(city, out var w)
        ? $"{city}: {w}"
        : $"{city}: 정보 없음";
}

/// <summary>현재 날짜와 시각을 반환한다.</summary>
string GetCurrentTime()
{
    Console.WriteLine("  [🕐 Tool] GetCurrentTime()");
    return $"현재 시각: {DateTime.Now:yyyy년 MM월 dd일 HH시 mm분}";
}

/// <summary>두 통화 간의 환율을 반환한다.</summary>
/// <param name="from">기준 통화 (예: USD, EUR, JPY)</param>
/// <param name="to">변환 통화 (예: KRW)</param>
string GetExchangeRate(string from, string to)
{
    Console.WriteLine($"  [💱 Tool] GetExchangeRate({from} → {to})");
    var rates = new Dictionary<string, decimal>
    {
        ["USD_KRW"] = 1350m,
        ["EUR_KRW"] = 1480m,
        ["JPY_KRW"] = 9.2m,
        ["CNY_KRW"] = 188m,
    };
    var key = $"{from.ToUpper()}_{to.ToUpper()}";
    return rates.TryGetValue(key, out var rate)
        ? $"1 {from} = {rate} {to}"
        : $"{from}/{to} 환율 정보 없음";
}

/// <summary>기본 사칙연산을 계산한다.</summary>
/// <param name="expression">계산식 (예: 125 * 847)</param>
string Calculate(string expression)
{
    Console.WriteLine($"  [🧮 Tool] Calculate(\"{expression}\")");
    try
    {
        var dt = new System.Data.DataTable();
        var result = dt.Compute(expression, "");
        return $"{expression} = {result}";
    }
    catch
    {
        return $"계산 오류: '{expression}'은(는) 올바른 수식이 아니다.";
    }
}

// ── 1. 단일 Tool Agent ────────────────────────────────────────────
Console.WriteLine("── 단일 Tool (날씨) ─────────────────────");
var weatherAgent = AIAgentBuilder.FromEnvironment()
    .Build("WeatherAgent", "날씨 정보를 제공하는 어시스턴트다.",
        new Delegate[] { GetWeather });

var r1 = await weatherAgent.RunAsync("서울과 제주의 날씨를 비교해줘");
Console.WriteLine($"🤖 {r1.Text}\n");

// ── 2. 멀티 Tool Agent ────────────────────────────────────────────
Console.WriteLine("── 멀티 Tool Agent ──────────────────────");
var multiAgent = AIAgentBuilder.FromEnvironment()
    .Build("MultiToolAgent",
        "날씨, 시각, 환율, 계산 도구를 모두 사용할 수 있는 어시스턴트다.",
        new Delegate[] { GetWeather, GetCurrentTime, GetExchangeRate, Calculate });

var queries = new[]
{
    "지금 몇 시야?",
    "1달러는 몇 원이야?",
    "서울 날씨 어때?",
    "125 곱하기 847은?"
};

foreach (var q in queries)
{
    Console.WriteLine($"❓ {q}");
    var r = await multiAgent.RunAsync(q);
    Console.WriteLine($"🤖 {r.Text}\n");
}

// ── 3. 복합 질문 ──────────────────────────────────────────────────
Console.WriteLine("── 복합 질문 (여러 Tool 동시 활용) ──────");
var complexQ = "지금 서울 날씨와 뉴욕 날씨를 알려주고, 달러 환율도 알려줘. 그리고 오늘 날짜도.";
Console.WriteLine($"❓ {complexQ}");
var r3 = await multiAgent.RunAsync(complexQ);
Console.WriteLine($"🤖 {r3.Text}");
