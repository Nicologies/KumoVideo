function IncreaseVerNum([string]$manifest)
{
    $regex = new-object System.Text.RegularExpressions.Regex("versionCode=`"([0-9]+)`"", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);
    $matches = $regex.Matches($manifest);
    $ver = $matches[0].Groups[1].ToString() -as [int];
    $ver = $ver+1;
    $ret = $regex.Replace($manifest, [string]::Format("versionCode=`"{0}`"", $ver));    
    return $ret;
}

function IncreaseVerName([string]$manifest)
{
    $regex = new-object System.Text.RegularExpressions.Regex("versionName=`"(\d+)\.(\d+)\.(\d+)\.(\d+)`"", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);
    $matches = $regex.Matches($manifest);
    $ver = $matches[0].Groups[4].ToString() -as [int];
    $ver = $ver+1;
    $ret = $regex.Replace($manifest, [string]::Format("versionName=`"{0}.{1}.{2}.{3}`"",
         $matches[0].Groups[1],
         $matches[0].Groups[2],
         $matches[0].Groups[3],
         $ver));
    
    return $ret;
}