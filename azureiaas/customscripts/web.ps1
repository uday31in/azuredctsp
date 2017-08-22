Write-Host "Hello World!"

add-windowsfeature web-server -IncludeManagementTools

Set-ExecutionPolicy AllSigned; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 

&"C:\ProgramData\chocolatey\choco.exe" install git.install -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore-sdk -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore-windowshosting -y

&"C:\Program Files\Git\bin\git.exe" clone https://github.com/uday31in/azuredctsp.git c:\app


&"C:\Program Files\dotnet\dotnet.exe" publish "C:\app\azuredctsp\azuredctsp.csproj" -o c:\app\bin\Debug\netcoreapp2.0 -r win10-x64 --self-contained 
