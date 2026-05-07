using AgentFrameworkBook.Shared;
using Microsoft.Agents.AI;

namespace AgentFrameworkBook.Chapter07;

/// <summary>
/// RAG(Retrieval Augmented Generation) 패턴으로 문서 질의응답을 수행하는 Agent Wrapper다.
/// 질문에 관련된 문서 청크를 검색한 후 LLM에 컨텍스트와 함께 전달한다.
/// </summary>
public class DocumentQAAgent
{
    private readonly AIAgent _agent;
    private readonly DocumentManager _docs;

    public DocumentQAAgent(AIAgent agent, DocumentManager docs)
    {
        _agent = agent;
        _docs  = docs;
    }

    /// <summary>
    /// 질문에 관련된 문서 청크를 검색하고, 해당 컨텍스트로 Agent에 질의한다.
    /// </summary>
    public async Task<string> AskAsync(string question, int topK = 3)
    {
        // 1. 관련 청크 검색
        var relevant = _docs.Search(question, topK);

        if (relevant.Count == 0)
        {
            Console.WriteLine("  [RAG] 관련 문서 없음 → 일반 지식으로 답변");
            var fallback = await _agent.RunAsync(question);
            return fallback.Text;
        }

        // 2. 컨텍스트 구성
        var context = string.Join("\n\n---\n\n", relevant);
        Console.WriteLine($"  [RAG] {relevant.Count}개 청크 검색됨");

        // 3. LLM에 컨텍스트 포함 질의
        var prompt = $"""
            다음 문서 내용을 바탕으로 질문에 답해줘.
            문서에 없는 내용은 "문서에 해당 정보가 없습니다"라고 답해줘.

            === 참고 문서 ===
            {context}

            === 질문 ===
            {question}
            """;

        var result = await _agent.RunAsync(prompt);
        return result.Text;
    }

    /// <summary>스트리밍 방식으로 문서 질의응답을 수행한다.</summary>
    public async IAsyncEnumerable<string> AskStreamAsync(string question, int topK = 3)
    {
        var relevant = _docs.Search(question, topK);
        var context  = relevant.Count > 0
            ? string.Join("\n\n---\n\n", relevant)
            : "관련 문서 없음";

        Console.WriteLine($"  [RAG] {relevant.Count}개 청크 검색됨");

        var prompt = $"""
            다음 문서를 참고하여 질문에 답해줘.

            === 참고 문서 ===
            {context}

            === 질문 ===
            {question}
            """;

        await foreach (var chunk in _agent.RunStreamingAsync(prompt))
        {
            if (!string.IsNullOrEmpty(chunk.Text))
                yield return chunk.Text;
        }
    }
}
