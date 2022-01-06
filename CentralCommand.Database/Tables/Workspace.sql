CREATE TABLE [dbo].[Workspace]
(
	[Id] BIGINT IDENTITY (1, 1) NOT NULL,
	
	[WorkspaceShardId] BIGINT NOT NULL,
	

	[Name] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_Workspace] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Workspace_WorkspaceShard] FOREIGN KEY ([WorkspaceShardId]) REFERENCES [WorkspaceShard]([Id])
)