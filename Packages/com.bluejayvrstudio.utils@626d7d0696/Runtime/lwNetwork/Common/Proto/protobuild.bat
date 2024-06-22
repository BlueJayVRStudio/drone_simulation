@echo off
set /p "id=Enter proto file name: "
echo %id%
protoc -I ./ --csharp_out=./ ./%id%.proto

pause