$ErrorActionPreference = "Stop"
$key = $env:STITCH_API_KEY
$projectId = "12227662788794145381"
$screenId  = "a2face04d8874a798863e5a8155cc139"
$payload = @{ jsonrpc="2.0"; id=1; method="tools/call"; params=@{ name="get_screen"; arguments=@{ name="projects/$projectId/screens/$screenId" } } } | ConvertTo-Json -Depth 8 -Compress

Write-Host "Calling Stitch MCP get_screen (x-goog-api-key)..."
$resp = Invoke-RestMethod -Method Post -Uri "https://stitch.googleapis.com/mcp" `
  -Headers @{ "x-goog-api-key"=$key; "Content-Type"="application/json" } `
  -Body $payload -TimeoutSec 60

$raw = $resp.result.content[0].text
$data = $raw | ConvertFrom-Json

$htmlUrl = $data.htmlCode.downloadUrl
$pngUrl  = $data.screenshot.downloadUrl
$width   = if ($data.width) { $data.width } else { 2560 }
$imgUrlSized = "$pngUrl=w$width"

Write-Host "Title: $($data.title)"
Write-Host "Width: $width"
Write-Host "HTML:  $htmlUrl"
Write-Host "IMG :  $imgUrlSized"

$outDir = $PSScriptRoot
$htmlPath = Join-Path $outDir "AuraLearn.html"
$pngPath  = Join-Path $outDir "AuraLearn.png"

Write-Host "Downloading HTML..."
Invoke-WebRequest -Uri $htmlUrl -OutFile $htmlPath -UseBasicParsing -TimeoutSec 120
Write-Host "Downloading PNG (w=$width)..."
Invoke-WebRequest -Uri $imgUrlSized -OutFile $pngPath -UseBasicParsing -TimeoutSec 120

$h = (Get-Item $htmlPath).Length
$p = (Get-Item $pngPath).Length
Write-Host "Saved: $htmlPath ($h bytes) ; $pngPath ($p bytes)"
if ($h -lt 500 -or $p -lt 5000) { throw "Files too small" }
Write-Host "DONE"
