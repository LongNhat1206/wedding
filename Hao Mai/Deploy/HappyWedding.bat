set cdir=%cd%
set cdir=%cdir:Deploy=HappyWedding% 
set cdir=%cdir: =%
set folderProject=%cdir%
RMDIR /Q /S %folderProject%\bin\Release\api
del /f %folderProject%\bin\Release\api.zip
dotnet publish -c Release --output %folderProject%\bin\Release\api %folderProject%\HappyWedding.csproj
REM del /s /q /f %folderProject%\bin\Release\api\*.pdb
REM del /s /q /f %folderProject%\bin\Release\api\*.exe
powershell Add-Type -Assembly "System.IO.Compression.FileSystem";[System.IO.Compression.ZipFile]::CreateFromDirectory('%folderProject%\bin\Release\api', '%folderProject%\bin\Release\api.zip')
RMDIR /Q /S %folderProject%\bin\Release\api
::APP1
plink -batch -P 22 -pw jD4kM8@# root@103.146.23.157 sudo rm -f -r /root/product/wedding/maihao/
pscp -P 22 -pw jD4kM8@# -r %folderProject%\bin\Release\api.zip root@103.146.23.157:/root/product/wedding/
::DELETE ZIP FILE
del /f %folderProject%\bin\Release\api.zip
::APP1
plink -batch -P 22 -pw jD4kM8@# root@103.146.23.157 mkdir /root/product/wedding/maihao
plink -batch -P 22 -pw jD4kM8@# root@103.146.23.157 unzip /root/product/wedding/api.zip -d /root/product/wedding/maihao/
plink -batch -P 22 -pw jD4kM8@# root@103.146.23.157 rm -f -r /root/product/wedding/api.zip
plink -batch -P 22 -pw jD4kM8@# root@103.146.23.157 sudo systemctl restart maihao.service

pause
exit

