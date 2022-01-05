# Preface

This is a sample codebase for the implementation of a command query pattern in .NET.
This uses .NET 6 and has an optional dependency on swashbuckle (you can clone/fork and remove if you do not want swagger)

# Configuration

This runs on two main configuration settings that are both present in appsettings.json. 
IsPrimaryApi determines whether this API is the command API authorised to service requests tagged as command. 
PrimaryApiUrlBase is the Url that the middleware will redirect to if it recieves a request that requires command priveleges (and it is not command)

# Running the demo

This demonstrates an implementation fo the command query pattern for an API. This pattern is intended to work across different physical locations, but you can simulate this locally by running it on different ports and changing the appsetting configuration between boots. Settign IsPrimaryApi to false before running an intance of the API will run it as a query only API. 

e.g.
dotnet run --urls https://localhost:5001;http://localhost:5000
dotnet run --urls https://localhost:7244;http://localhost:5190