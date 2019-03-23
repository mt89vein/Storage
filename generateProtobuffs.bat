@ECHO OFF
setlocal EnableDelayedExpansion
SET PROTO_PATH=Storage.Core/Protocols
SET OUTPUT_PATH=Storage.Core/Models

FOR %%i IN (./Storage.Core/Protocols/*.proto) DO (
    CALL SET "files=%%files%% %PROTO_PATH%/%%i"
    ECHO "%PROTO_PATH%/%%i"
) 

protoc3.7.exe --proto_path=%PROTOPATH% --csharp_out=%OUTPUT_PATH% --csharp_opt=file_extension=.designer.cs %files%