@echo off
cls

set fake_tool_path="packages\tools"
set bin_dir="bin\"

dotnet restore build.proj

IF NOT EXIST %fake_tool_path%\fake.exe (
  dotnet tool install fake-cli --tool-path %fake_tool_path%
)

IF NOT EXIST %bin_dir% (
 mkdir %bin_dir%
)

IF NOT EXIST build.fsx (
  %fake_tool_path%\fake run init.fsx
)
%fake_tool_path%\fake build %*
