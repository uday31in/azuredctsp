Write-Host "Hello World!"

Set-ItemProperty HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU -Name NoAutoUpdate -Value 1
add-windowsfeature web-server -IncludeManagementTools

Set-ExecutionPolicy AllSigned; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 

&"C:\ProgramData\chocolatey\choco.exe" install putty -y
&"C:\ProgramData\chocolatey\choco.exe" install git.install -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore-sdk -y
&"C:\ProgramData\chocolatey\choco.exe" install dotnetcore-windowshosting -y


&"C:\Program Files\Git\bin\git.exe" clone https://github.com/uday31in/azuredctsp.git c:\app


&"C:\Program Files\dotnet\dotnet.exe" publish "C:\app\azuredctsp\azuredctsp.csproj" -o c -r win10-x64 --self-contained 

Set-ItemProperty 'IIS:\Sites\Default Web Site\' -Name physicalPath -Value C:\app\bin\
set-ItemProperty 'IIS:\AppPools\DefaultAppPool' -Name managedRuntimeVersion -Value ""

net stop was /y
net start w3svc

&"C:\ProgramData\chocolatey\choco.exe" install googlechrome -y

