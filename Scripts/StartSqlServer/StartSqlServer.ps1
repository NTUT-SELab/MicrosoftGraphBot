# 新增 MSSQL->127.0.0.1 至 hosts 文件
Install-Module -Name 'Carbon' -AllowClobber
Import-Module 'Carbon'
Set-HostsEntry -IPAddress 127.0.0.1 -HostName 'mssql'

# 啟動 MSSQL 容器
if ("$(docker ps -aq -f name=mssql)")
{
    if ("$(docker ps -aq -f status=exited -f name=mssql)")
    {
        docker start mssql
    }
    else
    {
        Write-Output "mssql container has started!"
    }
}
else
{
    # 如果出現錯誤: Error response from daemon: Ports are not available: listen tcp 127.0.0.1:1433: bind: An attempt was made to access a socket in a way forbidden by its access permissions.
    # 解決方法: powershell 使用 "netsh int ipv4 set dynamic tcp start=1434 num=16384" 並重新啟動電腦，將起始動態保留 Port 從 1024 開始，向後調整成從 1434 開始
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=P@ssw0rd" -p 127.0.0.1:1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:latest
}
