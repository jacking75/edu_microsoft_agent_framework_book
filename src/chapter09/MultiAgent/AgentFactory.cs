using AgentFrameworkBook.Shared;
using Microsoft.Agents.AI;

namespace AgentFrameworkBook.Chapter09;

/// <summary>
/// 워크플로우에서 사용하는 전문화된 Agent들을 생성하는 팩토리다.
/// 모든 Agent는 동일한 LLM 설정(환경 변수)을 공유한다.
/// </summary>
public static class AgentFactory
{
    /// <summary>최신 트렌드와 기술을 조사하는 리서치 Agent를 생성한다.</summary>
    public static AIAgent CreateResearchAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("ResearchAgent",
                """
                당신은 전문 리서치 분석가다.
                주어진 주제에 대해 최신 트렌드, 핵심 기술, 시장 동향을 간결하게 조사한다.
                조사 결과는 명확한 포인트로 정리한다.
                """);

    /// <summary>기술적 관점에서 분석하는 Tech Analyst Agent를 생성한다.</summary>
    public static AIAgent CreateTechAnalystAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("TechAnalystAgent",
                """
                당신은 기술 전문 분석가다.
                주어진 주제의 기술적 복잡도, 구현 난이도, 기술 스택을 분석한다.
                실용적인 기술 통찰을 제공한다.
                """);

    /// <summary>비즈니스 관점에서 분석하는 Business Analyst Agent를 생성한다.</summary>
    public static AIAgent CreateBusinessAnalystAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("BusinessAnalystAgent",
                """
                당신은 비즈니스 전략 분석가다.
                주어진 주제의 비즈니스 가치, 시장 기회, ROI를 분석한다.
                실행 가능한 비즈니스 인사이트를 제공한다.
                """);

    /// <summary>리스크를 평가하는 Risk Analyst Agent를 생성한다.</summary>
    public static AIAgent CreateRiskAnalystAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("RiskAnalystAgent",
                """
                당신은 리스크 평가 전문가다.
                주어진 주제의 기술적 리스크, 보안 취약점, 운영 리스크를 평가한다.
                리스크 완화 방안을 함께 제시한다.
                """);

    /// <summary>여러 분석을 종합하여 최종 보고서를 작성하는 Writer Agent를 생성한다.</summary>
    public static AIAgent CreateWriterAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("WriterAgent",
                """
                당신은 전문 기술 문서 작성자다.
                여러 분석 결과를 통합하여 명확하고 구조화된 보고서를 작성한다.
                독자가 핵심을 빠르게 파악할 수 있도록 구성한다.
                """);

    /// <summary>최종 결과물을 검토하는 Reviewer Agent를 생성한다.</summary>
    public static AIAgent CreateReviewerAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("ReviewerAgent",
                """
                당신은 품질 검토 전문가다.
                작성된 보고서의 완성도, 정확성, 논리적 일관성을 검토한다.
                개선점을 구체적으로 제시하고 최종 승인 여부를 결정한다.
                """);

    /// <summary>간단한 질문을 분류하는 Classifier Agent를 생성한다.</summary>
    public static AIAgent CreateClassifierAgent()
        => AIAgentBuilder.FromEnvironment()
            .Build("ClassifierAgent",
                """
                당신은 요청 분류 전문가다.
                입력 요청을 분석하여 다음 중 하나로 분류한다:
                - TECHNICAL: 기술적 질문이나 구현 요청
                - BUSINESS: 비즈니스나 전략 관련 질문
                - GENERAL: 그 외 일반적인 질문

                반드시 TECHNICAL, BUSINESS, GENERAL 중 하나만 대문자로 답한다.
                """);
}
