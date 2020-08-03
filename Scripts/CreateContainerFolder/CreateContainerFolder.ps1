if (-Not (Test-Path -Path .\mssql\data))
{
    New-Item .\mssql\data -ItemType "directory"
}
if (-Not (Test-Path -Path .\mssql\log))
{
    New-Item .\mssql\log -ItemType "directory"
}
if (-Not (Test-Path -Path .\bot\Logs))
{
    New-Item .\bot\Logs -ItemType "directory"
}
