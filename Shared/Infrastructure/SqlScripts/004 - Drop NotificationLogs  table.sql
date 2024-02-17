IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationLogs]') AND type in (N'U'))
	DROP TABLE [dbo].[NotificationLogs]
GO