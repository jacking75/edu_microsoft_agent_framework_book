// Chapter 10 - 프로덕션 배포: 로깅, 재시도, 타임아웃, Rate Limiting, 캐싱
// 실행: dotnet run

using DotNetEnv;
using Serilog;
using Polly;
using AgentFrameworkBook.Shared;
using AgentFrameworkBook.Chapter10;

Console.WriteLine("=== Chapter 10: 프로덕션 Agent ===\n");

Env.Load();

// ── 1. Serilog 설정 ───────────────────────────────────────────────
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}")
    .WriteTo.File("Logs/agent_.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

logger.Information("=== 프로덕션 Agent 시작 ===");

// ── 2. Polly 재시도 정책 ──────────────────────────────────────────
Console.WriteLine("\n── 2. Polly 재시도 정책 ─────────────────");

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            logger.Warning("재시도 {Attempt}회 - {Delay}초 대기", attempt, delay.TotalSeconds);
            return delay;
        });

// ── 3. 기본 Agent 생성 ────────────────────────────────────────────
var baseAgent = AIAgentBuilder.FromEnvironment()
    .Build("ProductionAgent",
        "프로덕션 환경에서 운영되는 안정적인 어시스턴트다. 간결하고 정확하게 답변한다.");

// ── 4. 로깅 미들웨어 래핑 ─────────────────────────────────────────
var loggedAgent = new LoggingMiddleware(baseAgent, logger);

Console.WriteLine("로깅 미들웨어로 요청 처리:");
var r1 = await loggedAgent.RunAsync("프로덕션 서버 배포 체크리스트 3가지");
Console.WriteLine($"응답: {r1[..Math.Min(100, r1.Length)]}...\n");

// ── 5. Rate Limiter ───────────────────────────────────────────────
Console.WriteLine("── 5. Rate Limiter ───────────────────────");

var rateLimiter = new RateLimiter(maxPerMinute: 10);

for (int i = 1; i <= 3; i++)
{
    await rateLimiter.WaitIfNeededAsync();
    var r = await baseAgent.RunAsync($"질문 {i}: 간단한 답변");
    Console.WriteLine($"  [{i}] 처리 완료: {r.Text[..Math.Min(40, r.Text.Length)]}");
}
Console.WriteLine();

// ── 6. 응답 캐싱 ─────────────────────────────────────────────────
Console.WriteLine("── 6. 응답 캐싱 ─────────────────────────");

var cachedAgent = new CachedAgent(baseAgent, ttlMinutes: 5);

var sameQuestion = "AI Agent의 주요 장점은?";
Console.WriteLine($"질문: {sameQuestion}");

var firstCall = await cachedAgent.RunAsync(sameQuestion);
Console.WriteLine($"첫 번째 호출: {firstCall[..Math.Min(60, firstCall.Length)]}...");

var secondCall = await cachedAgent.RunAsync(sameQuestion);
Console.WriteLine($"두 번째 호출 (캐시): {secondCall[..Math.Min(60, secondCall.Length)]}...");

cachedAgent.PrintStats();

// ── 7. 타임아웃 처리 ──────────────────────────────────────────────
Console.WriteLine("\n── 7. 타임아웃 처리 ─────────────────────");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    // 타임아웃 내에서 실행
    var timeoutTask = baseAgent.RunAsync("짧게 답변: 비동기 프로그래밍이란?");
    var completedTask = await Task.WhenAny(timeoutTask, Task.Delay(30_000, cts.Token));

    if (completedTask == timeoutTask)
    {
        var result = await timeoutTask;
        Console.WriteLine($"✅ 타임아웃 내 완료: {result.Text[..Math.Min(80, result.Text.Length)]}");
    }
    else
    {
        Console.WriteLine("⏰ 타임아웃 발생");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("⏰ 작업 취소됨");
}

// ── 8. 배치 처리 ─────────────────────────────────────────────────
Console.WriteLine("\n── 8. 배치 처리 ──────────────────────────");

var inputs = Enumerable.Range(1, 6)
    .Select(i => $"질문 {i}: C# 팁 하나 알려줘")
    .ToList();

Console.WriteLine($"총 {inputs.Count}개 질문을 배치 처리 (2개씩)");
var batchSw = System.Diagnostics.Stopwatch.StartNew();

foreach (var batch in inputs.Chunk(2))
{
    var batchTasks = batch.Select(q => baseAgent.RunAsync(q));
    var results    = await Task.WhenAll(batchTasks);

    foreach (var (q, r) in batch.Zip(results))
        Console.WriteLine($"  ✅ {q[..20]}... → {r.Text[..Math.Min(40, r.Text.Length)]}");
}

batchSw.Stop();
Console.WriteLine($"배치 완료: {batchSw.ElapsedMilliseconds}ms\n");

logger.Information("=== 프로덕션 Agent 종료 ===");
Log.CloseAndFlush();

// ── 헬퍼 클래스들 ────────────────────────────────────────────────

class RateLimiter
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly int _maxPerMinute;
    private readonly Queue<DateTime> _callTimes = new();

    public RateLimiter(int maxPerMinute) => _maxPerMinute = maxPerMinute;

    public async Task WaitIfNeededAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            while (_callTimes.Count > 0 && (now - _callTimes.Peek()).TotalMinutes >= 1)
                _callTimes.Dequeue();

            if (_callTimes.Count >= _maxPerMinute)
            {
                var wait = TimeSpan.FromMinutes(1) - (now - _callTimes.Peek());
                Console.WriteLine($"  [RateLimit] {wait.TotalSeconds:F1}초 대기");
                await Task.Delay(wait);
            }

            _callTimes.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

class CachedAgent
{
    private readonly Microsoft.Agents.AI.AIAgent _agent;
    private readonly Dictionary<string, (string text, DateTime expiry)> _cache = new();
    private readonly TimeSpan _ttl;
    private int _hits = 0, _misses = 0;

    public CachedAgent(Microsoft.Agents.AI.AIAgent agent, int ttlMinutes = 5)
    {
        _agent = agent;
        _ttl   = TimeSpan.FromMinutes(ttlMinutes);
    }

    public async Task<string> RunAsync(string input)
    {
        var key = ComputeKey(input);

        if (_cache.TryGetValue(key, out var cached) && cached.expiry > DateTime.UtcNow)
        {
            _hits++;
            Console.WriteLine("  [Cache] HIT");
            return cached.text;
        }

        _misses++;
        Console.WriteLine("  [Cache] MISS");
        var result = await _agent.RunAsync(input);
        _cache[key] = (result.Text, DateTime.UtcNow + _ttl);
        return result.Text;
    }

    public void PrintStats()
    {
        var total   = _hits + _misses;
        var hitRate = total > 0 ? (double)_hits / total * 100 : 0;
        Console.WriteLine($"\n📊 캐시 통계: {_hits}히트 / {_misses}미스 (히트율 {hitRate:F1}%)");
    }

    private static string ComputeKey(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }
}
