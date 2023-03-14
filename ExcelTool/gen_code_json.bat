set WORKSPACE=..

set GEN_CLIENT=%WORKSPACE%\ExcelTool\Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\ExcelTool\Config

%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %WORKSPACE%/Ancinet-God/Assets/HotUpdate/GenCode/JsonCode ^
 --output_data_dir ..\Ancinet-God\Assets\GenerateDatas\json ^
 --gen_types code_cs_unity_json,data_json ^
 -s all 

pause