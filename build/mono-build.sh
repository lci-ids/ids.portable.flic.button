#!/bin/bash
#$1 Solution File
#$2 Configuration
#$3 Target
#$4 Arguments

MSBUILD2019=/Applications/Visual\ Studio.app/Contents/Resources/lib/monodevelop/bin/MSBuild/Current/bin/MSBuild.dll
MSBUILD2022=/Applications/Visual\ Studio.app/Contents/MonoBundle/MSBuild/Current/bin/MSBuild.dll
MSBUILDPATH=

if test -f "$MSBUILD2019"; then
    MSBUILDPATH=$MSBUILD2019
else
    MSBUILDPATH=$MSBUILD2022
fi

mono "$MSBUILDPATH" /t:"$3" "$1" /p:Configuration="$2" "$4"