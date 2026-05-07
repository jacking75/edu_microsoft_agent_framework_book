// Chapter 05 - 대화 흐름 관리: Thread, 장기 기억, 컨텍스트 관리
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;
using AgentFrameworkBook.Chapter05;

Console.WriteLine("=== Chapter 05: 대화 흐름 관리 ===\n");

Env.Load();

// ── 1. Thread를 이용한 멀티턴 대화 ───────────────────────────────
Console.WriteLine("── 1. Thread 기반 멀티턴 대화 ───────────");

var agent = AIAgentBuilder.FromEnvironment()
    .Build("ChatAgent", "친절한 대화 어시스턴트다. 이전 대화 내용을 기억하며 답변한다.");

var thread = await agent.CreateSessionAsync();

var conversations = new[]
{
    ("안녕! 나는 흥배야.", "인사"),
    ("나는 게임 서버 개발자야. 23년 경력이 있어.", "직업 소개"),
    ("방금 내가 뭐라고 소개했지?", "기억 확인"),
    ("내 이름이 뭐야?", "이름 확인"),
};

foreach (var (msg, label) in conversations)
{
    Console.WriteLine($"👤 [{label}] {msg}");
    var r = await agent.RunAsync(msg, thread);
    Console.WriteLine($"🤖 {r.Text}\n");
}

// ── 2. Thread 초기화 (새 대화 시작) ──────────────────────────────
Console.WriteLine("── 2. Thread 초기화 ──────────────────────");
Console.WriteLine("(새 Thread 생성으로 이전 대화 초기화)\n");

thread = await agent.CreateSessionAsync(); // 새 Thread

var r2 = await agent.RunAsync("방금 전에 내가 누군지 말했었지?", thread);
Console.WriteLine($"🤖 {r2.Text}\n");

// ── 3. SimpleMemoryProvider로 장기 기억 ──────────────────────────
Console.WriteLine("── 3. 장기 기억 (SimpleMemoryProvider) ──");

var memory = new SimpleMemoryProvider();

// 사용자 정보 저장
memory.Remember("이름", "흥배");
memory.Remember("직업", "게임 서버 개발자");
memory.Remember("경력", "23년");
memory.Remember("주력언어", "C#, C++, Golang");
memory.Remember("관심사", "AI Agent 개발");

Console.WriteLine();

// 기억을 시스템 프롬프트에 반영하여 Agent 재생성
var memoryAgent = AIAgentBuilder.FromEnvironment()
    .Build(
        name: "MemoryAgent",
        instructions: memory.BuildContext() + "당신은 사용자를 잘 아는 개인 어시스턴트다.");

var memThread = await memoryAgent.CreateSessionAsync();

var memQuestions = new[]
{
    "내 이름이 뭐야?",
    "나는 어떤 언어를 주로 사용해?",
    "내 경력은 얼마나 돼?",
    "내 관심사에 맞는 학습 로드맵을 간단히 제안해줘."
};

foreach (var q in memQuestions)
{
    Console.WriteLine($"❓ {q}");
    var r = await memoryAgent.RunAsync(q, memThread);
    Console.WriteLine($"🤖 {r.Text}\n");
}

memory.PrintAll();

// ── 4. 대화 요약 ──────────────────────────────────────────────────
Console.WriteLine("\n── 4. 대화 요약 기능 ─────────────────────");

var summaryAgent = AIAgentBuilder.FromEnvironment()
    .Build("SummaryAgent", "대화를 진행하고 요약하는 어시스턴트다.");

var sumThread = await summaryAgent.CreateSessionAsync();

var longConversation = new[]
{
    "AI Agent란 뭐야?",
    "Microsoft Agent Framework의 특징은?",
    "어떤 경우에 Agent를 사용하면 좋아?",
};

foreach (var q in longConversation)
{
    Console.WriteLine($"❓ {q}");
    var r = await summaryAgent.RunAsync(q, sumThread);
    Console.WriteLine($"🤖 {r.Text}\n");
}

// 대화 요약 요청
Console.WriteLine("📋 대화 요약 요청...");
var summary = await summaryAgent.RunAsync(
    "우리가 나눈 대화를 3줄로 요약해줘.", sumThread);
Console.WriteLine($"📌 요약: {summary.Text}");
