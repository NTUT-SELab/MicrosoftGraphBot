# MicrosoftGraphBot

Microsoft Graph API 是一個 RESTful 的 Web API，可讓您存取 Microsoft Cloud 服務資源。 註冊應用程式並取得使用者或服務的驗證權杖之後，您就可以對 Microsoft Graph API 提出要求。 

藉由本專案建立自動化觸發 Microsoft Graph API 機器人伺服器，滿足微軟 Office365 開發者訂閱續約條件並自動續約，該機器人會根據組態檔的設定排成觸發 Microsoft Graph API。

應用: [Office365 開發者訂閱](https://developer.microsoft.com/en-us/microsoft-365/dev-program) 😘

||Ubuntu|Windows|MacOS|
|----|----|----|----|
|master|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=master)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=master)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=master&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=master)|
|develop|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=develop)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=develop)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=develop&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=develop)|
|hotfix|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=hotfix)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=hotfix)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=hotfix&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=hotfix)|
|document|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=document)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=document)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=document&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=document)|
|release|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20Ubuntu%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20Windows%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&jobname=Build%20and%20test%20project%20on%20MacOS%20platform)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|

||master|develop|hotfix|document|release|
|----|----|----|----|----|----|
|CodeQL|![CodeQL](https://github.com/NTUT-SELab/MicrosoftGraphBot/workflows/CodeQL/badge.svg?branch=master)|![CodeQL](https://github.com/NTUT-SELab/MicrosoftGraphBot/workflows/CodeQL/badge.svg?branch=develop)|![CodeQL](https://github.com/NTUT-SELab/MicrosoftGraphBot/workflows/CodeQL/badge.svg?branch=hotfix)|![CodeQL](https://github.com/NTUT-SELab/MicrosoftGraphBot/workflows/CodeQL/badge.svg?branch=document)|![CodeQL](https://github.com/NTUT-SELab/MicrosoftGraphBot/workflows/CodeQL/badge.svg?branch=release)|
|Coverage|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/9/master)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=master)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/9/develop)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=develop)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/9/hotfix)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=hotfix)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/9/document)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=document)|[![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/KennethTang/github/9/release)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|

||Github|Docker|
|----|----|----|
|Publish|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&stagename=Publish%20the%20current%20version%20to%20Github)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|[![Build Status](https://dev.azure.com/KennethTang/Github/_apis/build/status/NTUT-SELab.MicrosoftGraphBot?branchName=release&stagename=Publish%20the%20current%20version%20to%20Dockerhub)](https://dev.azure.com/KennethTang/Github/_build/latest?definitionId=9&branchName=release)|

## 教學:
  - [綁定 Office 365 授權教學](./Docs/Bind.md)
  - [Azure 應用程式註冊教學](./Docs/AppRegistrations.md)

## 組態檔介紹:

範例文件: [appsettings.json.example](appsettings.json.example)
```
{
  "JoinBotMessage": "歡迎使用 Microsoft Graph Bot",
  "Cron": "0 */4 * * *",
  "AdminPassword": "P@ssw0rd",
  "Telegram": {
    "Token": "1119104861:AAH4D1-ZdtwvFPeQARLJAdhBYPA1xK7px08"
  },
  "MSSQL": {
    "Host": "mssql",
    "Port": 1433,
    "User": "sa",
    "Password": "P@ssw0rd",
    "DataBase": "MicrosoftGraphBot"
  },
  "API": {
    "NumberOfServiceCall": 0,
    "NumberOfMethodCall": 0
  }
}
```

||配置項|說明|
|----|----|----|
||JoinBotMessage|使用者第一次與 Bot 建立聯繫時，Bot的問候語|
||Cron|Api 排程觸發的頻率，請參考 Crontab 格式|
||CheckVerCron|檢查 **Bot** 是否有新版本的頻率，請參考 Crontab 格式|
||AdminPassword|管理者密碼，與 Bot 溝通後，輸入此密碼可取得管理者權限|
|Telegram|Token|Telegram bot token，請與 [Telegram bot father](https://core.telegram.org/bots) 聊天建立 Telegram bot 並取得 Token|
|MSSQL|Host|SQL server 主機位置 (備註:使用 Docker compose 在 MSSQL 項不需要做任何更改)|
||Port|SQL server port|
||User|SQL server 使用者|
||Password|SQL server 使用者的密碼|
||DataBase|資料庫名稱|
|API|NumberOfServiceCall|每次呼叫 API 時，呼叫 API 種類數量的上限 例如: Outlook API (備註: 0 為所有 API 種類)|
||NumberOfMethodCall|某個 API 種類，呼叫 API 數量的上限 例如: List messages (備註: 0 為某個 API 種類中，所有的 API)|

## 架設方法:
至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 appsettings.json(應用程式配置文件)，編輯其內容。 備註: [Telegram:Token] 請改成自己 **Bot** 的 Token。

### Docker(推薦):
1. 至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 **docker.tar.gz**，並解壓縮文件
1. 安裝 [Docker](https://docs.docker.com/engine/install/#supported-platforms)

- Windows
    1. 開啟 Powershell，並切換至 **docker.tar.gz** 解壓縮後的目錄
    1. 第一次執行需要建立必要資料夾
        ```
        PS C:\docker> .\CreateContainerFolder.ps1
        ```
    1. 將 appsettings.json 移動至 docker 資料夾下的 bot 資料夾內
    1. 編輯 appsettings.json 文件的內容
    1. 建立並啟動容器
        ```
        PS C:\docker> docker-compose up -d
        ```    

- Linux & MacOS
    1. 切換至 **docker.tar.gz** 解壓縮後的目錄
    1. 第一次執行需要建立必要資料夾
        ```
        root@docker_server:~/docker$ ./CreateContainerFolder.sh
        ```
    1. 將 appsettings.json 移動至 docker 資料夾下的 bot 資料夾內
    1. 編輯 appsettings.json 文件的內容
    1. 建立並啟動容器
        ```
        root@docker_server:~/docker$ docker-compose up -d
        ```

- 更新版本方法
  ```
  docker-compose down
  docker-compose pull
  docker-compose up -d
  ```

### 其它
1. 自行安裝 [SQL server](https://www.microsoft.com/zh-tw/sql-server/sql-server-downloads)
1. 編輯 **appsettings.json** 內 MSSQL 配置項的配置

- Windows(x64):
  1. 至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 **win-x64.zip**，並解壓縮文件
  1. 執行 **MicrosoftGraphAPIBot.exe**

- Linux(x64):
  1. 至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 **linux-x64.zip**，並解壓縮文件
  1. 執行 **MicrosoftGraphAPIBot**
      ```
      root@server:~/MicrosoftGraphAPIBot$ ./MicrosoftGraphAPIBot
      ```

- MacOS(x64):
  1. 至 [Github Release](https://github.com/NTUT-SELab/MicrosoftGraphBot/releases) 下載最新的 **osx-x64.zip**，並解壓縮文件
  1. 執行 **MicrosoftGraphAPIBot**
      ```
      root@server:~/MicrosoftGraphAPIBot$ ./MicrosoftGraphAPIBot
      ```

## 支援 API:
**未來還會持續更新新的Api**

### [Outlook API](https://github.com/NTUT-SELab/MicrosoftGraphBot/issues/3):
- List messages
- Create Message
- Get message
- Update message
- Delete message
- message: send

### [OneDrive API](https://github.com/NTUT-SELab/MicrosoftGraphBot/issues/9)
- List children of a driveItem
- Create a new folder in a drive
- Update DriveItem properties
- Delete a DriveItem
- Move a DriveItem to a new folder

### [Permissions API](https://github.com/NTUT-SELab/MicrosoftGraphBot/issues/11)
- Create a sharing link for a DriveItem
- Accessing shared DriveItems
- List sharing permissions on a driveItem
- Get sharing permission for a file or folder
- Update sharing permission
- Delete a sharing permission from a file or folder

### [Calendar API](https://github.com/NTUT-SELab/MicrosoftGraphBot/issues/12)
- List calendars
- Create Calendar
- Update calendar
- Delete calendar

## 版本:
請至 [ReleaseNotes](./ReleaseNotes) 資料夾查看變更紀錄。


## 貢獻:
我們將貢獻分為3類:
1. 提出功能需求，但無撰寫程式能力
    - 開啟新的 issue，並使用 **功能要求** 模板

1. 提出錯誤報告，但無撰寫程式能力
    - 開啟新的 issue，並使用 **錯誤報告** 模板

1. 有撰寫程式能力者
    - 請參考 [Contributing.md](./Docs/Contributing.md)

## 技術:
- C#
- Dependency injection
- Entity Framework Core
- Microsoft Graph API
- Telegram API
- Hangfire (排程)
