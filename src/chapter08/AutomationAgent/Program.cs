// Chapter 08 - 자동화 Agent: 파일/데이터/시스템 자동화
// 실행: dotnet run

using DotNetEnv;
using AgentFrameworkBook.Shared;
using AgentFrameworkBook.Chapter08;

Console.WriteLine("=== Chapter 08: 자동화 Agent ===\n");

Env.Load();

// ── 테스트 데이터 준비 ─────────────────────────────────────────────
var testDir = "TestData";
Directory.CreateDirectory(testDir);

// 테스트 CSV 파일 생성
File.WriteAllText(Path.Combine(testDir, "server_stats.csv"), """
    server,cpu_usage,memory_gb,requests_per_sec,error_rate
    server-01,45.2,8.5,1250,0.02
    server-02,72.8,14.2,2100,0.05
    server-03,31.5,6.1,980,0.01
    server-04,88.9,15.8,2350,0.12
    server-05,55.3,11.4,1580,0.03
    """);

// 테스트 텍스트 파일 생성
File.WriteAllText(Path.Combine(testDir, "notes.txt"), """
    서버 운영 노트 (2026-05-05)

    server-04가 CPU 사용률 88.9%로 임계값에 근접했다.
    오후 2시에 트래픽 스파이크가 발생했다.
    server-02의 에러율이 0.05%로 다소 높다.
    내일 오전 2시에 server-01 정기 점검 예정이다.
    """);

Console.WriteLine("✅ 테스트 데이터 준비 완료\n");

// ── Tool 인스턴스 생성 ─────────────────────────────────────────────
var fileTools = new FileTools(testDir);
var dataTools = new DataTools(testDir);
var sysTools  = new SystemTools();

// ── Agent 생성 (모든 Tool 연결) ───────────────────────────────────
var agent = AIAgentBuilder.FromEnvironment()
    .Build("AutomationAgent",
        """
        당신은 서버 운영 자동화 어시스턴트다.
        파일 시스템, 데이터 분석, 시스템 정보 도구를 사용하여 운영 업무를 지원한다.
        데이터를 분석할 때는 실제 도구를 호출하여 정확한 정보를 제공한다.
        """,
        new Delegate[]
        {
            fileTools.ListFiles,
            fileTools.ReadFile,
            fileTools.WriteFile,
            dataTools.AnalyzeCsv,
            dataTools.ListCsvFiles,
            sysTools.GetCurrentTime,
            sysTools.GetSystemInfo,
            sysTools.GetDiskUsage,
        });

// ── 1. 파일 목록 조회 ─────────────────────────────────────────────
Console.WriteLine("── 1. 파일 목록 조회 ─────────────────────");
var r1 = await agent.RunAsync("현재 작업 디렉터리에 어떤 파일이 있어?");
Console.WriteLine($"🤖 {r1.Text}\n");

// ── 2. CSV 데이터 분석 ─────────────────────────────────────────────
Console.WriteLine("── 2. CSV 데이터 분석 ────────────────────");
var r2 = await agent.RunAsync("server_stats.csv를 분석하고 문제가 있는 서버를 식별해줘");
Console.WriteLine($"🤖 {r2.Text}\n");

// ── 3. 파일 읽기 및 요약 ──────────────────────────────────────────
Console.WriteLine("── 3. 파일 읽기 및 요약 ─────────────────");
var r3 = await agent.RunAsync("notes.txt 파일을 읽고 오늘의 주요 이슈를 요약해줘");
Console.WriteLine($"🤖 {r3.Text}\n");

// ── 4. 시스템 정보 수집 ───────────────────────────────────────────
Console.WriteLine("── 4. 시스템 정보 수집 ──────────────────");
var r4 = await agent.RunAsync("현재 시스템 상태와 디스크 사용량을 알려줘");
Console.WriteLine($"🤖 {r4.Text}\n");

// ── 5. 자동 리포트 생성 ───────────────────────────────────────────
Console.WriteLine("── 5. 자동 리포트 생성 ──────────────────");
var r5 = await agent.RunAsync(
    "server_stats.csv 분석과 notes.txt 내용을 합쳐서 오늘의 서버 운영 일일 리포트를 작성하고 daily_report.txt 파일로 저장해줘");
Console.WriteLine($"🤖 {r5.Text}\n");

// 저장된 파일 확인
var reportPath = Path.Combine(testDir, "daily_report.txt");
if (File.Exists(reportPath))
{
    Console.WriteLine("✅ 생성된 리포트:");
    Console.WriteLine(File.ReadAllText(reportPath));
}
