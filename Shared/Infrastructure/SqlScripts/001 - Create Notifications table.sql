CREATE TABLE [dbo].[Notifications](
[Id] [bigint] NOT NULL,
[Payload] [nvarchar](1000) NOT NULL,
[SendDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED([Id] ASC))