# Preface

This is a mono repo containing demo/starter codebases for implementing 2 different scalability patterns: Command Query and Regional Data Sharding. 

# Command Query

There is only one project required for command query. In the CommandQuery.Api Project/folder there is a separate readme for how to run that demo and it can be adjusted to fit into a separate codebase. 

# Regional Data Sharding

There are 4 projects for this Demonstration/Starter codebase (2 API and 2 Database): A central command api+db to co-ordinate the shards, and a Workspace api+db which are the shards.

## Central Command

This is a central projec to hold the shards and be able to co-ordinate the running of this system. 
The base demo can create a workspace in an existing shard and retrun a list of all workspaces. However, shards thenselves must be configurd externally. 

### Central Command Setup

This is the co-ordinating project that stores (mostly) static information about the deployed shards. In a full implementation you will likely use a distrubuted pattern such as command query to have an instance of this in each region. 

Steps:
1. You will need to deploy the Database project and configure the central command connection string in appsettings.json (or appsettings.Local.json if this is in a repo)
2. Create the desired regions. These are intended to be used as a way to geographically track where the Shards are deployed to and to allow you to do automatic shard allocation by setting default shards (when these are being created). You need at least one region for the system to function as-is. 
3. Create the worksapce Shards. Each row in this table should correspond with a deployed workspace database (see Workspace Setup). Region Id links to the region table via FK, sql server name and database are used to tell the workspace API how to access the shard. Setting isDefaultForRegion will mean that this shard will be used when creating a new workspace for that region. This must be set for at least one workspace shard per region. If it is set for more, the first one will always be used as the default, If it is not set for any the create workpspace will not be able to determine which shard to create a workspace in and will fail. 

Once you have the databases deployed you can run the central command API and there are two endpoints. Get Workspaces and Create Workspace. Get returns a list of all current worksapces. Create will add a new one. 

### Workspace Setup

This is what contains the meaty transactional data of the system. This example just uses sample weather information, but you will likely have better things to store. 

Steps:
1. Each region specified in the command query setup typically should have its own SQL server (and Worksapce API running alongside it). This is not strictly necessary for the demo, but recommended for a realistic deployment. 
2. You should deploy 1 instance of the Workspace Database per shard specified in Orchestration. This should be in the appropriate SQL server for the region that the shard lives in. 
3. Run an instance of the workspace APi for each region with the appropriate Database connection straing for the command query database. 

Once this is deployed you can access worksapce data in particular shards. 