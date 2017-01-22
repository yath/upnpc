#!/usr/bin/powershell
param (
    [string]$source = "HDMI3",
    [bool]$wait = $false
)

$tv = "172.16.23.15"
$service = "urn:samsung.com:service:MainTVAgent2:1"
$upnpc = "..\bin\Debug\upnpc.exe"
$castnow = "..\..\castnow\castnow.cmd"

function Is-Tv-On {
    if ($IsWindows) {
        ping -n 1 -w 10 $tv *> $null
    } else {
        ping -c 1 -w 10 $tv *> $null
    }
    $LASTEXITCODE -eq 0
}

function Upnpc {
    & $upnpc /su:$service /ev:Result=OK $args
}

function Get-Active-Source {
    Upnpc /a:GetCurrentExternalSource /gv:CurrentExternalSource
}

function Get-Source {
    [xml]$s = Upnpc /a:GetSourceList /gv:SourceList
    $s.SourceList.Source | where SourceType -eq $args[0]
}

function Set-Active-Source {
    Upnpc "/a:SetMainTVSource" "/sv:Source=$($args[0].SourceType)" "/sv:ID=$($args[0].ID)"
}

if (!(Is-Tv-On)) {
    Write-Host -NoNewline "Starting TV..."
    Start-Process -NoNewWindow $castnow /dev/null
    $i = 0
    while (!(Is-Tv-On)) {
        $i += 1
        if ($i -gt 30) {
            Write-Error "Timeout"
            break
        }
        Start-Sleep -Seconds 1
        Write-Host -NoNewline "."
    }
    Write-Host
}


Write-Host -NoNewline "Determining active source..."
$curr_src = Get-Active-Source
Write-Host " $curr_src"

if ($curr_src -ne $source) {
    Write-Host -NoNewline "Determining $source's ID..."
    $o = Get-Source $source
    Write-Host " $($o.ID)"
    Write-Host -NoNewline "Switching..."
    Set-Active-Source (Get-Source $source)
    Write-Host
}

if ($wait) {
    Write-Host -NoNewline "Sleeping until switched to a different source..."
    $curr_src = $source
    do {
        Start-Sleep -Seconds 10
        $curr_src = Get-Active-Source
    } while ($curr_src -eq $source)
    Write-Host " $curr_src"
}
