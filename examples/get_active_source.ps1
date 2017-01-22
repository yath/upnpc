$tv = "172.16.23.15"
$service = "urn:samsung.com:service:MainTVAgent2:1"
$upnpc = "..\bin\Debug\upnpc.exe"

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


if (!(Is-Tv-On)) {
    echo OFF
} else {
    Get-Active-Source
}       
