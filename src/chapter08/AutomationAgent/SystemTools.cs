namespace AgentFrameworkBook.Chapter08;

/// <summary>
/// 시스템 정보를 조회하는 도구 모음이다.
/// </summary>
public class SystemTools
{
    /// <summary>현재 날짜와 시각을 반환한다.</summary>
    public string GetCurrentTime()
    {
        Console.WriteLine("  [🕐 Tool] GetCurrentTime()");
        return $"현재: {DateTime.Now:yyyy년 MM월 dd일 HH시 mm분ss초}";
    }

    /// <summary>시스템 정보(OS, CPU, 메모리)를 반환한다.</summary>
    public string GetSystemInfo()
    {
        Console.WriteLine("  [💻 Tool] GetSystemInfo()");
        return $"""
            OS: {Environment.OSVersion}
            CPU 코어: {Environment.ProcessorCount}개
            .NET 버전: {Environment.Version}
            현재 프로세스 메모리: {Environment.WorkingSet / 1024 / 1024}MB
            """;
    }

    /// <summary>지정한 드라이브의 디스크 사용량을 반환한다.</summary>
    /// <param name="driveLetter">드라이브 문자 (예: C, D). 비어있으면 시스템 드라이브를 사용한다.</param>
    public string GetDiskUsage(string driveLetter = "")
    {
        Console.WriteLine($"  [💿 Tool] GetDiskUsage(\"{driveLetter}\")");
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .ToList();

            if (!string.IsNullOrEmpty(driveLetter))
                drives = drives.Where(d => d.Name.StartsWith(driveLetter.ToUpper())).ToList();

            var sb = new System.Text.StringBuilder();
            foreach (var drive in drives)
            {
                var total = drive.TotalSize / 1024 / 1024 / 1024;
                var free  = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                var used  = total - free;
                var pct   = total > 0 ? (double)used / total * 100 : 0;
                sb.AppendLine($"  {drive.Name}: {used}GB 사용 / {total}GB 전체 ({pct:F1}% 사용)");
            }
            return sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return $"디스크 정보 오류: {ex.Message}";
        }
    }
}
