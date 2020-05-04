#!/bin/bash

# 新增 MSSQL->127.0.0.1 至 hosts 文件
ETC_HOSTS=/etc/hosts
IP="127.0.0.1"
HOSTNAME="mssql"
HOSTS_LINE="$IP		$HOSTNAME"
if [ -n "$(grep $HOSTNAME $ETC_HOSTS)" ]; then
    echo "Hosts: $HOSTNAME already exists"
else
    echo "$HOSTS_LINE" >> $ETC_HOSTS

    if [ -n "$(grep $HOSTNAME $ETC_HOSTS)" ]; then
        echo "Hosts: $HOSTNAME was added succesfully"
    else
        echo "Hosts: Failed to Add $HOSTNAME, Try again!"
    fi
fi

# 啟動 MSSQL 容器
if [ "$(docker ps -aq -f name=mssql)" ]; then
    if [ "$(docker ps -aq -f status=exited -f name=mssql)" ]; then
        docker start mssql
    else
        echo "mssql container has started!"
    fi
else
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=P@ssw0rd" -p 127.0.0.1:1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:latest
fi
