### First, Install dotnet script tool:
dotnet tool install -g dotnet-script

### Run next commands in DirCrypt folder, which contains *.csproj

### Generate dummy files:
dotnet script GenerateFiles.csx source 10 100

### Encrypt files
dotnet run -e 10 source encrypted key

### Decrypt files
dotnet run -e 10 encrypted decrypted key

### Compare files:
dotnet script CompareDirectories.csx source decrypted


### All together:

dotnet script GenerateFiles.csx source 10 100

dotnet run -e 10 source encrypted key

dotnet run -d 10 encrypted decrypted key

dotnet script CompareDirectories.csx source decrypted

