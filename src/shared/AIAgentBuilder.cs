using System;
using System.Collections.Generic;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;

namespace AgentFrameworkBook.Shared;

/// <summary>
/// LLM 제공자를 추상화하는 AIAgent 빌더.
/// LLM이 바뀌어도 Build() 호출 코드는 변경할 필요가 없다.
/// </summary>
public class AIAgentBuilder
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;

    // ─────────────────────────────────────────────────────────
    // 정적 팩토리: 제공자별 프리셋
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// OpenRouter (https://openrouter.ai) 를 사용하는 빌더를 생성한다.
    /// </summary>
    /// <param name="apiKey">OpenRouter API 키</param>
    /// <param name="model">사용할 모델 (기본값: anthropic/claude-sonnet-4-5)</param>
    public static AIAgentBuilder WithOpenRouter(
        string apiKey,
        string model = "anthropic/claude-sonnet-4-5")
        => new(apiKey, "https://openrouter.ai/api/v1", model);

    /// <summary>
    /// Poe (https://poe.com) API를 사용하는 빌더를 생성한다.
    /// </summary>
    /// <param name="apiKey">Poe API 키</param>
    /// <param name="model">사용할 모델 (기본값: claude-sonnet-4-20250514)</param>
    public static AIAgentBuilder WithPoe(
        string apiKey,
        string model = "claude-sonnet-4-20250514")
        => new(apiKey, "https://api.poe.com/llm/v1", model);

    /// <summary>
    /// 커스텀 OpenAI-호환 엔드포인트를 사용하는 빌더를 생성한다.
    /// </summary>
    public static AIAgentBuilder WithCustomEndpoint(
        string apiKey,
        string baseUrl,
        string model)
        => new(apiKey, baseUrl, model);

    /// <summary>
    /// 환경 변수에서 설정을 자동으로 읽어 빌더를 생성한다 (권장).
    /// 필요한 환경 변수:
    ///   LLM_API_KEY  - API 키
    ///   LLM_BASE_URL - 엔드포인트 (기본값: https://openrouter.ai/api/v1)
    ///   LLM_MODEL    - 모델 이름 (기본값: anthropic/claude-sonnet-4-5)
    /// </summary>
    public static AIAgentBuilder FromEnvironment()
    {
        var apiKey  = Environment.GetEnvironmentVariable("LLM_API_KEY")
                      ?? throw new InvalidOperationException(
                             "LLM_API_KEY 환경 변수가 설정되지 않았다.");
        var baseUrl = Environment.GetEnvironmentVariable("LLM_BASE_URL")
                      ?? "https://openrouter.ai/api/v1";
        var model   = Environment.GetEnvironmentVariable("LLM_MODEL")
                      ?? "anthropic/claude-sonnet-4-5";

        return new(apiKey, baseUrl, model);
    }

    // ─────────────────────────────────────────────────────────
    // 생성자 (private)
    // ─────────────────────────────────────────────────────────

    private AIAgentBuilder(string apiKey, string baseUrl, string model)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API 키가 비어있다.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL이 비어있다.", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("모델 이름이 비어있다.", nameof(model));

        _apiKey  = apiKey;
        _baseUrl = baseUrl;
        _model   = model;
    }

    // ─────────────────────────────────────────────────────────
    // 빌드 메서드
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// AIAgent 인스턴스를 생성한다.
    /// </summary>
    /// <param name="name">에이전트 이름 (로깅·디버깅용)</param>
    /// <param name="instructions">시스템 프롬프트 (에이전트의 역할·행동 방침)</param>
    /// <param name="tools">에이전트가 사용할 수 있는 AIFunction 목록</param>
    public AIAgent Build(
        string name,
        string instructions,
        params AIFunction[] tools)
    {
        var client = new OpenAIAgentClient(new Uri(_baseUrl), _apiKey);

        return tools.Length == 0
            ? client.CreateAIAgent(
                model:        _model,
                name:         name,
                instructions: instructions)
            : client.CreateAIAgent(
                model:        _model,
                name:         name,
                instructions: instructions,
                tools:        tools);
    }

    /// <summary>
    /// Delegate 배열로 Tool을 전달하는 편의 오버로드.
    /// AIFunctionFactory.Create()를 내부에서 자동 처리한다.
    /// </summary>
    public AIAgent Build(
        string name,
        string instructions,
        IEnumerable<Delegate> tools)
    {
        var aiFunctions = new List<AIFunction>();
        foreach (var tool in tools)
            aiFunctions.Add(AIFunctionFactory.Create(tool));

        return Build(name, instructions, aiFunctions.ToArray());
    }

    // ─────────────────────────────────────────────────────────
    // 진단 정보
    // ─────────────────────────────────────────────────────────

    /// <summary>현재 설정 정보를 출력한다 (API 키는 마스킹).</summary>
    public void PrintConfig()
    {
        var maskedKey = _apiKey.Length > 8
            ? _apiKey[..8] + "..."
            : "****";

        Console.WriteLine($"🔧 AIAgentBuilder 설정:");
        Console.WriteLine($"   Base URL : {_baseUrl}");
        Console.WriteLine($"   Model    : {_model}");
        Console.WriteLine($"   API Key  : {maskedKey}");
    }

    public string Model   => _model;
    public string BaseUrl => _baseUrl;
}
