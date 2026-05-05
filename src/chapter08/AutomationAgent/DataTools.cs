using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace AgentFrameworkBook.Chapter08;

/// <summary>
/// CSV 파일을 읽고 기본 통계를 계산하는 데이터 분석 도구다.
/// </summary>
public class DataTools
{
    private readonly string _baseDir;

    public DataTools(string baseDir) => _baseDir = baseDir;

    /// <summary>CSV 파일을 분석하여 기본 통계를 반환한다.</summary>
    /// <param name="fileName">분석할 CSV 파일 이름</param>
    public string AnalyzeCsv(string fileName)
    {
        Console.WriteLine($"  [📊 Tool] AnalyzeCsv(\"{fileName}\")");
        var path = Path.Combine(_baseDir, fileName);

        if (!File.Exists(path))
            return $"파일 없음: {fileName}";

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
            };

            using var reader = new StreamReader(path);
            using var csv    = new CsvReader(reader, config);

            var records = csv.GetRecords<dynamic>().ToList();
            if (records.Count == 0) return "데이터 없음";

            var headers  = ((IDictionary<string, object>)records[0]).Keys.ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"파일: {fileName}");
            sb.AppendLine($"행 수: {records.Count}개");
            sb.AppendLine($"컬럼: {string.Join(", ", headers)}");
            sb.AppendLine();

            // 숫자 컬럼 통계
            foreach (var header in headers)
            {
                var values = records
                    .Select(r => ((IDictionary<string, object>)r)[header]?.ToString())
                    .Where(v => double.TryParse(v, out _))
                    .Select(v => double.Parse(v!))
                    .ToList();

                if (values.Count > 0)
                {
                    sb.AppendLine($"[{header}] 평균: {values.Average():F2}, 최솟값: {values.Min():F2}, 최댓값: {values.Max():F2}");
                }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"CSV 분석 오류: {ex.Message}";
        }
    }

    /// <summary>여러 CSV 파일 목록을 반환한다.</summary>
    public string ListCsvFiles()
    {
        Console.WriteLine("  [📋 Tool] ListCsvFiles()");
        var files = Directory.GetFiles(_baseDir, "*.csv");
        return files.Length == 0
            ? "CSV 파일 없음"
            : string.Join("\n", files.Select(f => Path.GetFileName(f)));
    }
}
