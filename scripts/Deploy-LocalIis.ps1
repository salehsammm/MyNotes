$ErrorActionPreference = 'Stop'

$projectPath = Split-Path -Parent $PSScriptRoot
$stagePath = Join-Path $projectPath 'bin\Release\net9.0\publish'
$sitePath = 'F:\My.Prog\BookmarkCounter\Publish'
$backupPath = "$sitePath-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss-fff')"
$offlinePath = Join-Path $sitePath 'app_offline.htm'

if (!(Test-Path -LiteralPath $sitePath -PathType Container)) {
    throw "IIS site directory is missing: $sitePath"
}
if (Test-Path -LiteralPath $offlinePath) {
    throw 'The site is already offline. Resolve the existing deployment before continuing.'
}

dotnet publish (Join-Path $projectPath 'MyNotes.csproj') -c Release -o $stagePath
if ($LASTEXITCODE -ne 0) { throw 'Release publish failed.' }

$configHashes = @{}
foreach ($file in (Get-ChildItem -LiteralPath $sitePath -File | Where-Object Name -Like 'appsettings*.json')) {
    $configHashes[$file.Name] = (Get-FileHash -LiteralPath $file.FullName).Hash
}
if (!$configHashes.ContainsKey('appsettings.Production.json')) {
    throw 'Expected machine-local appsettings.Production.json is missing.'
}

robocopy $sitePath $backupPath /E /R:2 /W:1 /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -ge 8) { throw 'Deployment backup failed.' }
Write-Output "Backup: $backupPath"

Set-Content -LiteralPath $offlinePath -Value '<html><body>Updating MyNotes. Please refresh shortly.</body></html>'
Start-Sleep -Seconds 3
robocopy $stagePath $sitePath /E /R:3 /W:2 /XF 'appsettings*.json' 'app_offline.htm' /NFL /NDL /NJH /NJS /NP
if ($LASTEXITCODE -ge 8) {
    throw "Deployment copy failed. The site remains offline; restore from $backupPath before removing app_offline.htm."
}

foreach ($name in $configHashes.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $sitePath $name)).Hash -ne $configHashes[$name]) {
        throw "Configuration verification failed for $name. The site remains offline. Backup: $backupPath"
    }
}
if ((Get-FileHash -LiteralPath (Join-Path $stagePath 'MyNotes.dll')).Hash -ne
    (Get-FileHash -LiteralPath (Join-Path $sitePath 'MyNotes.dll')).Hash) {
    throw "Deployed assembly does not match staging. The site remains offline. Backup: $backupPath"
}
Remove-Item -LiteralPath $offlinePath -Force

foreach ($route in @('/', '/journal', '/bookmarks/api/questions', '/bookmarks/js/dashboard.js?v=11')) {
    $response = $null
    for ($attempt = 0; $attempt -lt 10; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5050$route" -UseBasicParsing -TimeoutSec 15
            break
        } catch {
            if ($attempt -eq 9) { throw }
            Start-Sleep -Seconds 2
        }
    }
    if ($response.StatusCode -ne 200) { throw "Route verification failed: $route" }
    if ($route -eq '/journal' -and $response.Content -notmatch 'Needs adding') {
        throw 'The journal response does not contain the Needs adding filter.'
    }
    if ($route -like '/bookmarks/js/dashboard.js*' -and
        !$response.Content.Contains('for (const q of [...open, ...answered].slice(0, 4))')) {
        throw 'The dashboard script does not contain the unanswered-question priority change.'
    }
    Write-Output "Verified: http://localhost:5050$route"
}
Write-Output 'Deployment verified. Machine-local configuration was preserved.'
