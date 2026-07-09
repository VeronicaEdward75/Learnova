$files = @("Views\Shared\_Layout.cshtml", "Services\RefundService.cs")
foreach ($file in $files) {
    if (Test-Path $file) {
        $text = [IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
        $regex = [regex]::new("[ий][\x80-\xFF]+")
        $matchEvaluator = [System.Text.RegularExpressions.MatchEvaluator] {
            param($match)
            $bytes = [System.Text.Encoding]::GetEncoding(1252).GetBytes($match.Value)
            return [System.Text.Encoding]::UTF8.GetString($bytes)
        }
        $result = $regex.Replace($text, $matchEvaluator)
        [IO.File]::WriteAllText($file, $result, [System.Text.Encoding]::UTF8)
        Write-Host "Fixed $file"
    }
}
