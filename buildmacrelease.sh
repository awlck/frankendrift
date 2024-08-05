#!/bin/zsh
if [[ $# -eq 0 ]]; then
	echo "Version argument required!"
	exit 1
fi

SRCDIR=${0:a:h}
TEMPDIR=$(mktemp -d)
BLDVER=$1

cleanup() {
	rm -r "$SRCDIR/FrankenDrift.Adrift/bin"
	rm -r "$SRCDIR/FrankenDrift.Glue/bin"
	rm -r "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner/bin"
	rm -r "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner.Mac/bin"
}

cd "$SRCDIR/FrankenDrift.Runner/FrankenDrift.Runner.Mac"

cleanup
/usr/local/share/dotnet/dotnet publish --self-contained -c Release -f net8.0 -r osx-arm64 -p:PublishReadyToRun=true -p:TieredCompilation=true
mkdir "$TEMPDIR/arm64"
cp -r "bin/Release/net8.0/osx-arm64/publish/FrankenDrift.Runner.Mac.app" "$TEMPDIR/arm64"
codesign --deep --force -s - "$TEMPDIR/arm64/FrankenDrift.Runner.Mac.app"
hdiutil create -format UDZO -volname "FrankenDrift $BLDVER" -srcfolder "$TEMPDIR/arm64" "$SRCDIR/frankendrift-$BLDVER.macarm64.dmg"

cleanup
/usr/local/share/dotnet/x64/dotnet publish --self-contained -c Release -f net8.0 -r osx-x64 -p:PublishReadyToRun=true -p:TieredCompilation=true
mkdir "$TEMPDIR/x64"
cp -r "bin/Release/net8.0/osx-x64/publish/FrankenDrift.Runner.Mac.app" "$TEMPDIR/x64"
codesign --deep --remove-signature "$TEMPDIR/x64/FrankenDrift.Runner.Mac.app"
hdiutil create -fs 'HFS+' -format UDZO -volname "FrankenDrift $BLDVER" -srcfolder "$TEMPDIR/x64" "$SRCDIR/frankendrift-$BLDVER.macx64.dmg"

rm -r $TEMPDIR
