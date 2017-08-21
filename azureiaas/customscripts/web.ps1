Write-Host "Hello World!"

add-windowsfeature web-server -IncludeManagementTools

Set-ExecutionPolicy AllSigned; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 

choco install git.install -y
choco install dotnetcore -y
choco install dotnetcore-sdk --version 1.1 -y
choco install dotnetcore-windowshosting



dotnet restore ./azuredctsp.sln && dotnet publish ./azuredctsp.sln -c Release -o ./obj/Docker/publish"

git clone https://github.com/uday31in/azuredctsp.git c:\app
