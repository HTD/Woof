$CWD = Get-Location
Set-Location ..
Get-ChildItem *\bin -Recurse | Remove-Item -Recurse -Force
Get-ChildItem *\obj -Recurse | Remove-Item -Recurse -Force
Set-Location $CWD