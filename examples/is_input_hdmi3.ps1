Try {
    if ((& .\get_active_source.ps1) -eq "HDMI3") {
        echo True
    } else {
        echo False
    }
} Catch {
    echo Maybe
}
