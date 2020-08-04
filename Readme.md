# MicrosoftGraphBot

Microsoft Graph API 是一個 RESTful 的 Web API，可讓您存取 Microsoft Cloud 服務資源。 註冊應用程式並取得使用者或服務的驗證權杖之後，您就可以對 Microsoft Graph API 提出要求。 

本專案基於教育與開發目的建立自動化呼叫 Microsoft Graph API 機器人伺服器，該機器人會根據組態檔的設定排成呼叫 Microsoft Graph API。

應用: Microsoft365 😘

||Ubuntu|Windows|MacOS|
|----|----|----|----|
|master|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=master)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=master)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=master)|
|develop|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=develop)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=develop)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=develop)|
|hotfix|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=hotfix)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=hotfix)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=hotfix)|
|document|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=document)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=document)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=document)|
|release|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=release)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=release)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=release)|

||Master|develop|hotfix|document|release|
|----|----|----|----|----|----|
|Code coverage|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/8/master)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=master)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/8/develop)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=develop)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/8/hotfix)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=hotfix)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/8/document)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=document)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/8/release)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=8&branchName=release)

## 架設方法:
至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 appsettings.json(應用程式配置文件)

### Docker(推薦):
1. 至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 **docker.tar.gz**，並解壓縮文件
2. 安裝 [Docker](https://docs.docker.com/engine/install/#supported-platforms)

- Windows
    1. 開啟 Powershell，並切換至 **docker.tar.gz** 解壓縮後的目錄
    1. 第一次執行需要建立必要資料夾
        `PS C:\docker> .\CreateContainerFolder.ps1`
    1. 將 appsettings.json 移動至 docker 資料夾下的 bot 資料夾內
    1. 編輯 appsettings.json 文件的內容
    1. 建立並啟動容器
        `PS C:\docker> docker-compose up -d`
        

- Linux & MacOS
    1. 切換至 **docker.tar.gz** 解壓縮後的目錄
    1. 第一次執行需要建立必要資料夾
        `root@docker_server:~/docker$ .\CreateContainerFolder.sh`
    1. 將 appsettings.json 移動至 docker 資料夾下的 bot 資料夾內
    1. 編輯 appsettings.json 文件的內容
    1. 建立並啟動容器
        `root@docker_server:~/docker$ docker-compose up -d`

### Windows(x64):
待編輯

### Linux(x64):
待編輯

### MacOS(x64):
待編輯

## 技術:
- C#
- Dependency injection
- Entity Framework Core
- Microsoft Graph API
- Telegram API
- Hangfire (排程)
