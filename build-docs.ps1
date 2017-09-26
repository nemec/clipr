param
(
    [ValidateSet("Build","Watch")]
    [string]$Action="Build"
)


if($Action -eq "Build")
{
    & wyam build -c "src-docs/config.wyam" -i "src-docs/Input" -o "docs/"
}
else
{
    & wyam build -c "src-docs/config.wyam" -p --watch --virtual-dir "/clipr" -i "src-docs/Input" -o "docs/"
}