using Serilog;
using AgentFrameworkBook.Shared;

namespace AgentFrameworkBook.Chapter10;

/// <summary>
/// Serilog를 이용한 구조화된 로깅 미들웨어다.
/// 요청 ID, 응답 시간, 토큰 추정치를 자동으로 기록한다.
/// </summary>
public class LoggingMiddleware
{
    private readonly Microsoft.Agents.AI.AIAgent _agent;
    private readonly ILogger _logger;
    private int _requestCount = 0;

    public LoggingMiddleware(Microsoft.Agents.AI.AIAgent agent, ILogger logger)
    {
        _agent  = agent;
        _logger = logger;
    }

    public async Task<string> RunAsync(string input, Microsoft.Agents.AI.AgentSession? session = null)
    {
        var requestId = Interlocked.Increment(ref _requestCount);
        var sw        = System.Diagnostics.Stopwatch.StartNew();

        _logger.Information("[{RequestId}] 요청 수신: {InputLength}자",
            requestId, input.Length);

        try
        {
            var result = session != null
                ? await _agent.RunAsync(input, session)
                : await _agent.RunAsync(input);

            sw.Stop();

            _logger.Information("[{RequestId}] 응답 완료: {ResponseLength}자, {ElapsedMs}ms",
                requestId, result.Text.Length, sw.ElapsedMilliseconds);

            return result.Text;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.Error(ex, "[{RequestId}] 오류 발생: {ErrorMessage}, {ElapsedMs}ms",
                requestId, ex.Message, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
