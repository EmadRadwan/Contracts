<#
  Check-PBI-Latest.ps1
  Verifies whether a "Projects-29-Jul" copy contains the 2026-07-29 session edits
  (new  الملخص التنفيذي — الشامل  page, new measures, and the rewritten profit DAX),
  so you can tell the LATEST copy from an older one.

  Run in PowerShell on the machine you want to check:
      .\Check-PBI-Latest.ps1 -Root "C:\path\to\Project-29-Jul"
  -Root = the folder that contains  Projects-29-Jul.SemanticModel  and
          Projects-29-Jul.Report  (the extracted project folder). Defaults to current dir.
#>
param([string]$Root = ".")

$ErrorActionPreference = "SilentlyContinue"
Write-Host ""
Write-Host "Checking Power BI project under: $Root"
Write-Host ""

$fact = Get-ChildItem -Path $Root -Recurse -Filter "Fact_GL_Transactions.tmdl" | Select-Object -First 1
if (-not $fact) {
    Write-Host "Fact_GL_Transactions.tmdl NOT FOUND under -Root." -ForegroundColor Red
    Write-Host "Point -Root at the extracted project folder (the one holding *.SemanticModel)." -ForegroundColor Red
    exit 1
}
$tmdl = Get-Content -Raw -Encoding UTF8 $fact.FullName

# name | present-means-latest? | literal string to look for
$markers = @(
    @{ n = "New page-title measure  'Summary Title'";               want = $true;  s = "measure 'Summary Title'" },
    @{ n = "New measure  'Total Income'";                           want = $true;  s = "measure 'Total Income'" },
    @{ n = "New measure  'Total Expenses'";                         want = $true;  s = "measure 'Total Expenses'" },
    @{ n = "New measure  'Customer Advances'";                      want = $true;  s = "measure 'Customer Advances'" },
    @{ n = "Grouped sub-account 'Customer Advances & Deposits'";    want = $true;  s = "Customer Advances & Deposits" },
    @{ n = "Profit DAX rewritten (uses Total_FTP_Raw)";             want = $true;  s = "CALCULATE([Total_FTP_Raw]" },
    @{ n = "OLD descriptive filter  ""Profit and Loss""  (pre-edit)"; want = $false; s = '"Profit and Loss"' }
)

$score = 0
foreach ($m in $markers) {
    $has = $tmdl.Contains($m.s)
    $isLatest = if ($m.want) { $has } else { -not $has }
    if ($isLatest) { $score++ }
    $tag = if ($isLatest) { "[LATEST]" } else { "[older ]" }
    $col = if ($isLatest) { "Green" } else { "Yellow" }
    Write-Host ("  {0}  {1}" -f $tag, $m.n) -ForegroundColor $col
}

# report page created that session (ASCII GUID folder name)
$pageFolder = Get-ChildItem -Path $Root -Recurse -Directory -Filter "b7e4c1a9f2d80356e4b1" | Select-Object -First 1
$total = $markers.Count + 1
if ($pageFolder) {
    $score++
    Write-Host "  [LATEST]  New report page folder b7e4c1a9f2d80356e4b1 present" -ForegroundColor Green
} else {
    Write-Host "  [older ]  New report page folder b7e4c1a9f2d80356e4b1 MISSING" -ForegroundColor Yellow
}

Write-Host ""
Write-Host ("SCORE: {0}/{1} latest-version markers matched." -f $score, $total)
if ($score -eq $total) {
    Write-Host "VERDICT: LATEST  ->  this copy has ALL the 2026-07-29 edits." -ForegroundColor Green
} elseif ($score -eq 0) {
    Write-Host "VERDICT: OLDER   ->  none of the edits are here (pre-session copy)." -ForegroundColor Red
} else {
    Write-Host "VERDICT: PARTIAL ->  mixed; likely an out-of-sync copy." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Reference (this-session file: $($fact.FullName))"
