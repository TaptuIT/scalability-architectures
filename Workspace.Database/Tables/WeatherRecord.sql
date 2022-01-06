CREATE TABLE [dbo].[WeatherRecord]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [WorkspaceId] BIGINT NOT NULL, 
    [Summary] NVARCHAR(50) NULL, 
    [Temperature] FLOAT NULL, 
    [Date] DATETIME NULL,

    
	CONSTRAINT [FK_WeatherRecord_Workspace] FOREIGN KEY ([WorkspaceId]) REFERENCES [Workspace]([Id])
)
