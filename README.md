# Microsoft Agent Framework 학습 가이드 (C# & VSCode)

> **저자:** 최흥배, Claude AI  
> **개발 환경:** Visual Studio Code + .NET 9.0+  
> **LLM:** OpenRouter / Poe (OpenAI 호환 API)  
> **최종 업데이트:** 2026-05

```
   __  __ _                           __ _
  |  \/  (_) ___ _ __ ___  ___  ___  / _| |_
  | |\/| | |/ __| '__/ _ \/ __|/ _ \| |_| __|
  | |  | | | (__| | | (_) \__ \ (_) |  _| |_
  |_|  |_|_|\___|_|  \___/|___/\___/|_|  \__|

    Agent Framework + OpenRouter / Poe
```

---

## 특징

- **특정 LLM 미사용**: Poe 또는 OpenRouter의 OpenAI 호환 API만 사용
- **AIAgent 추상화**: LLM이 바뀌어도 최소한의 코드 변경으로 대응 가능한 `AIAgentBuilder` 래퍼 제공
- **최신 버전 기준**: Microsoft Agent Framework 정식 출시 버전 기준으로 작성
- **실전 예제**: 각 챕터마다 바로 실행 가능한 C# 예제 코드 포함

---

## 목차

| 챕터 | 제목 | 내용 |
|------|------|------|
| [01](./01.md) | **시작하기 전에 - 환경 준비** | .NET 설치, VSCode 설정, OpenRouter/Poe API 키, 프로젝트 초기 설정 |
| [02](./02.md) | **Agent Framework 핵심 개념** | AI Agent란, Microsoft Agent Framework 아키텍처, LLM 모델 비교 |
| [03](./03.md) | **첫 번째 Agent 만들기** | AIAgentBuilder 패턴, 기본 Agent 생성, 환경 변수 관리, 대화형 프로그램 |
| [04](./04.md) | **Agent에 기능 추가하기 - Tools** | Function Tools 개념, Tool 구현, 여러 Tools 사용, 외부 API 연동 |
| [05](./05.md) | **대화 흐름 관리** | Multi-turn Conversation, Thread, Context Provider 메모리, 세션 관리 |
| [06](./06.md) | **고급 기능 활용** | Streaming 응답, Extended Thinking, Middleware로 Agent 확장 |
| [07](./07.md) | **실전 프로젝트 1 - 문서 질의응답 Agent** | RAG 패턴, 문서 로딩, PDF 파싱, 질의응답 시스템 구현 |
| [08](./08.md) | **실전 프로젝트 2 - 업무 자동화 Agent** | 파일 관리, 데이터 처리, 보고서 생성, 시스템 자동화 |
| [09](./09.md) | **Multi-Agent Workflow** | Workflow 개념, 멀티 에이전트 구성, Orchestration 패턴 |
| [10](./10.md) | **프로덕션 준비** | 로깅, 에러 처리, 보안, 성능 최적화, 배포 전략 |
| [11](./11.md) | **고급 주제** | MCP 서버 통합, OpenRouter 고급 활용, 모델 전환 전략 |

---

## 예제 코드 구조

```
src/
├── shared/                     # 공통 AIAgentBuilder (모든 챕터에서 사용)
│   ├── AIAgentBuilder.cs
│   └── README.md
├── chapter01/
│   └── EnvCheck/               # 환경 확인 프로그램
│       ├── Program.cs
│       └── EnvCheck.csproj
├── chapter02/
│   └── ConceptDemo/            # 개념 데모
│       ├── Program.cs
│       └── ConceptDemo.csproj
├── chapter03/
│   └── FirstAgent/             # 첫 번째 Agent
│       ├── Program.cs
│       ├── FirstAgent.csproj
│       └── .env.example
├── chapter04/
│   └── AgentWithTools/         # Tools 사용 Agent
│       ├── Program.cs
│       └── AgentWithTools.csproj
├── chapter05/
│   └── AgentWithThread/        # Thread 대화 관리
│       ├── Program.cs
│       ├── SimpleMemoryProvider.cs
│       └── AgentWithThread.csproj
├── chapter06/
│   └── StreamingAgent/         # 스트리밍 + 고급 기능
│       ├── Program.cs
│       └── StreamingAgent.csproj
├── chapter07/
│   └── DocumentQA/             # 문서 질의응답 시스템
│       ├── Program.cs
│       ├── DocumentManager.cs
│       ├── DocumentQAAgent.cs
│       ├── DocumentQA.csproj
│       └── TestDocuments/
│           └── sample.txt
├── chapter08/
│   └── AutomationAgent/        # 업무 자동화 Agent
│       ├── Program.cs
│       ├── FileTools.cs
│       ├── DataTools.cs
│       ├── SystemTools.cs
│       └── AutomationAgent.csproj
├── chapter09/
│   └── MultiAgent/             # 멀티 에이전트 워크플로우
│       ├── Program.cs
│       └── MultiAgent.csproj
├── chapter10/
│   └── ProductionAgent/        # 프로덕션 준비
│       ├── Program.cs
│       ├── LoggingMiddleware.cs
│       └── ProductionAgent.csproj
└── chapter11/
    └── AdvancedTopics/         # 고급 주제
        ├── Program.cs
        └── AdvancedTopics.csproj
```

---

## 빠른 시작

### 1. 환경 변수 설정

```bash
# OpenRouter 사용 (권장)
export LLM_API_KEY="your-openrouter-api-key"
export LLM_BASE_URL="https://openrouter.ai/api/v1"
export LLM_MODEL="anthropic/claude-sonnet-4-5"

# 또는 Poe 사용
export LLM_API_KEY="your-poe-api-key"
export LLM_BASE_URL="https://api.poe.com/llm/v1"
export LLM_MODEL="claude-sonnet-4-20250514"
```

### 2. 첫 번째 에이전트 실행

```bash
cd src/chapter03/FirstAgent
dotnet run
```

---

## 핵심: AIAgentBuilder 사용법

모든 예제에서 공통으로 사용하는 `AIAgentBuilder`는 LLM 제공자를 추상화한다:

```csharp
// OpenRouter로 AIAgent 생성
var agent = AIAgentBuilder
    .WithOpenRouter(apiKey, "anthropic/claude-sonnet-4-5")
    .Build("MyAgent", "당신은 도움이 되는 어시스턴트다.");

// Poe로 AIAgent 생성 (동일한 코드, 제공자만 변경)
var agent = AIAgentBuilder
    .WithPoe(apiKey, "claude-sonnet-4-20250514")
    .Build("MyAgent", "당신은 도움이 되는 어시스턴트다.");

// 환경 변수로 자동 설정 (권장)
var agent = AIAgentBuilder
    .FromEnvironment()
    .Build("MyAgent", "당신은 도움이 되는 어시스턴트다.");

// Agent 실행
var result = await agent.RunAsync("안녕하세요!");
Console.WriteLine(result.Text);
```

---

## 지원되는 LLM 모델

### OpenRouter (https://openrouter.ai)
| 모델 ID | 설명 |
|---------|------|
| `anthropic/claude-sonnet-4-5` | Claude Sonnet - 균형잡힌 성능 (권장) |
| `anthropic/claude-haiku-4-5` | Claude Haiku - 빠르고 경제적 |
| `anthropic/claude-opus-4-5` | Claude Opus - 최고 성능 |
| `openai/gpt-4o` | GPT-4o |
| `openai/gpt-4o-mini` | GPT-4o Mini |
| `google/gemini-2.0-flash` | Gemini 2.0 Flash |

### Poe (https://poe.com)
| 모델 ID | 설명 |
|---------|------|
| `claude-sonnet-4-20250514` | Claude Sonnet 4 |
| `claude-haiku-4-5` | Claude Haiku 4.5 |
| `gpt-4o` | GPT-4o |

---

## 필요 패키지

```xml
<!-- 핵심 패키지 -->
<PackageReference Include="Microsoft.Agents.AI" Version="*" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="*" />
<PackageReference Include="DotNetEnv" Version="3.1.1" />
```

---

## 참고 자료

- [Microsoft Agent Framework GitHub](https://github.com/microsoft/agent-framework)
- [Agent Framework Samples](https://github.com/microsoft/Agent-Framework-Samples)
- [OpenRouter 문서](https://openrouter.ai/docs)
- [Poe API 문서](https://poe.com/api_key)
- [Microsoft Extensions AI](https://learn.microsoft.com/en-us/dotnet/ai/)
