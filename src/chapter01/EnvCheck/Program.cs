// Chapter 01 - 환경 확인 및 기본 연결 테스트
// 실행: dotnet run
// 사전 준비: .env.example을 복사하여 .env 파일을 만들고 API 키를 설정한다.

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;

Console.WriteLine("=== Chapter 01: 환경 확인 ===\n");

// 1. .env 파일 로드
Env.Load();

var apiKey  = Environment.GetEnvironmentVariable("LLM_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("LLM_BASE_URL") ?? "https://openrouter.ai/api/v1";
var model   = Environment.GetEnvironmentVariable("LLM_MODEL")   ?? "anthropic/claude-sonnet-4-5";

// 2. 환경 변수 확인
Console.WriteLine("📋 환경 변수 확인:");
Console.WriteLine($"   LLM_BASE_URL : {baseUrl}");
Console.WriteLine($"   LLM_MODEL    : {model}");
Console.WriteLine($"   LLM_API_KEY  : {(string.IsNullOrEmpty(apiKey) ? "❌ 미설정" : "✅ 설정됨")}");
Console.WriteLine();

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("❌ LLM_API_KEY가 설정되지 않았다.");
    Console.WriteLine("   .env.example을 .env로 복사하고 API 키를 입력한 후 다시 실행해라.");
    return;
}

// 3. 공급자 판별
var provider = baseUrl.Contains("poe.com") ? "Poe"
             : baseUrl.Contains("openrouter") ? "OpenRouter"
             : baseUrl.Contains("localhost") ? "로컬 (Ollama)"
             : "커스텀";

Console.WriteLine($"🌐 LLM 공급자: {provider}");
Console.WriteLine();

// 4. 간단한 연결 테스트
Console.WriteLine("🔗 연결 테스트 중...");
Console.WriteLine();

try
{
    var client = new OpenAIAgentClient(new Uri(baseUrl), apiKey);
    var agent  = client.CreateAIAgent(
        model:        model,
        name:         "TestAgent",
        instructions: "간결하게 답변하는 테스트 어시스턴트다.");

    var result = await agent.RunAsync("한 문장으로 자기소개를 해줘.");

    Console.WriteLine("✅ 연결 성공!");
    Console.WriteLine($"🤖 Agent 응답: {result.Text}");
    Console.WriteLine();
    Console.WriteLine("환경 설정이 완료되었다. 02.md부터 학습을 시작할 수 있다.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 연결 실패: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("확인 사항:");
    Console.WriteLine("  1. API 키가 올바른지 확인한다.");
    Console.WriteLine("  2. LLM_BASE_URL 형식이 올바른지 확인한다.");
    Console.WriteLine("  3. 네트워크 연결 상태를 확인한다.");
}
