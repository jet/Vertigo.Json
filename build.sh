#!/usr/bin/env bash
# to properly set Travis permissions: https://stackoverflow.com/questions/33820638/travis-yml-gradlew-permission-denied
# git update-index --chmod=+x fake.sh
# git commit -m "permission access for travis"

set -eu
set -o pipefail

fake_tool_path=packages/tools
bin_dir="bin"

dotnet restore build.proj

#Check if the fake tool is installed
if [ ! -f $fake_tool_path/fake ]; then
   dotnet tool install fake-cli --tool-path $fake_tool_path
fi

if [ ! -d $bin_dir ]; then
    mkdir $bin_dir
fi

if [ ! -f build.fsx ]; then
    $fake_tool_path/fake run init.fsx
fi

$fake_tool_path/fake build $@
