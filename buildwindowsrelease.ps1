param(
    [string]$version
)

dotnet publish --self-contained -c Release -f net8.0-windows -r win-x64 "$PSScriptRoot\FrankenDrift.Runner\FrankenDrift.Runner.Win" -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishReadyToRunComposite=true -p:Version=$version -p:IncludeSourceRevisionInInformationalVersion=false -p:PublishTrimmed=true -p:TrimMode=partial -p:BuiltInComInteropSupport=true -p:EnableUnsafeBinaryFormatterSerialization=false -p:EnableUnsafeUTF7Encoding=false -p:HttpActivityPropagationSupport=false -p:MetadataUpdaterSupport=false
Compress-Archive -Path "$PSScriptRoot\FrankenDrift.Runner\FrankenDrift.Runner.Win\bin\Release\net8.0-windows\win-x64\publish\*" -DestinationPath "$PSScriptRoot\frankendrift-v$version.winx64.zip"