// Chapter 02 - Microsoft Agent Framework 개념 데모
// AIAgentBuilder를 사용하여 기본 Agent 생성 및 동작을 확인한다.

using DotNetEnv;
using AgentFrameworkBook.Shared;

Console.WriteLine("=== Chapter 02: Agent Framework 개념 데모 ===\n");

Env.Load();

// ── 1. AIAgentBuilder 구성 정보 출력 ──────────────────────────────
Console.WriteLine("📐 AIAgentBuilder 구성:");
var builder = AIAgentBuilder.FromEnvironment();
builder.PrintConfig();
Console.WriteLine();

// ── 2. 단일 Agent 생성 ────────────────────────────────────────────
Console.WriteLine("🤖 Agent 생성 중...");
var agent = builder.Build(
    name:         "ConceptAgent",
    instructions: "Microsoft Agent Framework를 소개하는 전문 어시스턴트다. 핵심만 간결하게 설명한다.");

Console.WriteLine("✅ Agent 생성 완료\n");

// ── 3. 단발성 질문 ────────────────────────────────────────────────
Console.WriteLine("── 단발성 질문 ──────────────────────────");
var q1 = "Microsoft Agent Framework가 무엇인지 한 문장으로 설명해줘.";
Console.WriteLine($"❓ {q1}");
var r1 = await agent.RunAsync(q1);
Console.WriteLine($"🤖 {r1.Text}\n");

// ── 4. Session(멀티턴 대화) 개념 확인 ─────────────────────────────
Console.WriteLine("── Session (멀티턴 대화) ────────────────");
var session = await agent.CreateSessionAsync();

var questions = new[]
{
    "AIAgent 객체의 핵심 역할은?",
    "Tool이란 무엇인가?",
    "Session은 왜 필요한가?"
};

foreach (var q in questions)
{
    Console.WriteLine($"❓ {q}");
    var r = await agent.RunAsync(q, session);
    Console.WriteLine($"🤖 {r.Text}\n");
}

// ── 5. 공급자별 Builder 패턴 비교 ────────────────────────────────
Console.WriteLine("── 공급자별 AIAgentBuilder 패턴 ────────");
Console.WriteLine("""
  // OpenRouter
  var agent = AIAgentBuilder
      .WithOpenRouter("sk-or-v1-...", "anthropic/claude-sonnet-4-5")
      .Build("MyAgent", "어시스턴트다.");

  // Poe
  var agent = AIAgentBuilder
      .WithPoe("poe-key-...", "claude-sonnet-4-20250514")
      .Build("MyAgent", "어시스턴트다.");

  // 환경 변수 (권장)
  var agent = AIAgentBuilder
      .FromEnvironment()
      .Build("MyAgent", "어시스턴트다.");
""");

Console.WriteLine("✅ Chapter 02 완료. 03.md에서 첫 번째 Agent를 직접 구현한다.");
