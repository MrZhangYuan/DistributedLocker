#递归删除文件
Get-ChildItem * -Include *.nupkg -Recurse | Remove-Item
Get-ChildItem * -Include *.snupkg -Recurse | Remove-Item

$nugetkey="$($args[0])"	

#Pack
dotnet pack src\DistributedLocker\DistributedLocker.csproj --output packages --configuration Release

dotnet pack src\DistributedLocker.DataBase\DistributedLocker.DataBase.csproj --output packages --configuration Release

dotnet pack src\DistributedLocker.Oracle\DistributedLocker.Oracle.csproj --output packages --configuration Release

dotnet pack src\DistributedLocker.Postgres\DistributedLocker.Postgres.csproj --output packages --configuration Release

dotnet pack src\DistributedLocker.SqlServer\DistributedLocker.SqlServer.csproj --output packages --configuration Release

#Push
dotnet nuget push packages\*.nupkg -s https://api.nuget.org/v3/index.json -k $nugetkey