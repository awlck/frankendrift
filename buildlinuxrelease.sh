#!/bin/zsh
if [[ $# -eq 0 ]]; then
	echo "Version argument required!"
	exit 1
fi

SRCDIR=${0:a:h}
BLDVER=$1

cd "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner.Gtk"
rm -rf bin/Release/
dotnet publish --no-self-contained -f net8.0 -c Release -p:Version=$BLDVER -p:IncludeSourceRevisionInInformationalVersion=false
cd bin/Release/net8.0/publish
tar czvf "$SRCDIR/frankendrift-v$BLDVER-gtk.any.tar.gz" *

cd "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner.Gtk"
rm -rf bin/Release/
dotnet publish --self-contained -f net8.0 -c Release -r linux-x64 -p:VERSION=$BLDVER -p:IncludeSourceRevisionInInformationalVersion=false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishReadyToRunComposite=true -p:PublishTrimmed=true -p:TrimMode=partial -p:EnableUnsafeBinaryFormatterSerialization=false -p:EnableUnsafeUTF7Encoding=false -p:HttpActivityPropagationSupport=false -p:MetadataUpdaterSupport=false
cd bin/Release/net8.0/linux-x64/publish
tar czvf "$SRCDIR/frankendrift-v$BLDVER-linux.x64.tar.gz" *

cd "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner.Gtk"
rm -rf bin/Release/
dotnet publish --self-contained -f net8.0 -c Release -r linux-arm64 -p:VERSION=$BLDVER -p:IncludeSourceRevisionInInformationalVersion=false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishReadyToRunComposite=true -p:PublishTrimmed=true -p:TrimMode=partial -p:EnableUnsafeBinaryFormatterSerialization=false -p:EnableUnsafeUTF7Encoding=false -p:HttpActivityPropagationSupport=false -p:MetadataUpdaterSupport=false
cd bin/Release/net8.0/linux-arm64/publish
tar czvf "$SRCDIR/frankendrift-v$BLDVER-linux.arm64.tar.gz" *
