CREATE TABLE [dbo].[WorkspaceShard]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL,
	[RegionId] BIGINT NOT NULL,
	[SqlServerName] NVARCHAR(100) NOT NULL,
	[DatabaseName] NVARCHAR(100),
	[IsDefaultForRegion] BIT NOT NULL DEFAULT 0,

	CONSTRAINT [PK_WorkspaceShard] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_WorkspaceShard_Region] FOREIGN KEY ([RegionId]) REFERENCES [Region]([Id])
)
