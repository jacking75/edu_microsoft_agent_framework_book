# Shared - 공통 헬퍼

이 디렉토리는 모든 챕터에서 공통으로 사용하는 헬퍼 클래스를 포함한다.

## AIAgentBuilder

`AIAgentBuilder`는 LLM 제공자(OpenRouter, Poe 등)를 추상화하여,  
**LLM이 바뀌어도 Build() 호출부 코드를 변경할 필요가 없도록** 설계되었다.

### 사용법

```csharp
// 환경 변수에서 자동 설정 (권장)
var agent = AIAgentBuilder
    .FromEnvironment()
    .Build("MyAgent", "당신은 도움이 되는 어시스턴트다.");

// OpenRouter 직접 지정
var agent = AIAgentBuilder
    .WithOpenRouter(apiKey, "anthropic/claude-sonnet-4-5")
    .Build("MyAgent", "...");

// Poe 직접 지정
var agent = AIAgentBuilder
    .WithPoe(apiKey, "claude-sonnet-4-20250514")
    .Build("MyAgent", "...");

// Tools와 함께 사용
string GetWeather(string city) => $"{city}: 맑음, 22도";

var agent = AIAgentBuilder
    .FromEnvironment()
    .Build("WeatherAgent", "날씨를 알려주는 어시스턴트다.",
        new Delegate[] { GetWeather });

// 실행
var result = await agent.RunAsync("서울 날씨 알려줘");
Console.WriteLine(result.Text);
```

### 환경 변수

| 변수명 | 설명 | 기본값 |
|--------|------|--------|
| `LLM_API_KEY` | API 키 (필수) | - |
| `LLM_BASE_URL` | 엔드포인트 URL | `https://openrouter.ai/api/v1` |
| `LLM_MODEL` | 모델 이름 | `anthropic/claude-sonnet-4-5` |

### .env 파일 예시

```
# OpenRouter
LLM_API_KEY=sk-or-xxxxxxxxxxxxxxxx
LLM_BASE_URL=https://openrouter.ai/api/v1
LLM_MODEL=anthropic/claude-sonnet-4-5

# 또는 Poe
# LLM_API_KEY=your-poe-key
# LLM_BASE_URL=https://api.poe.com/llm/v1
# LLM_MODEL=claude-sonnet-4-20250514
```
