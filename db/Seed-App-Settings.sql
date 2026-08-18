USE [KidNest]
GO

INSERT INTO [dbo].[SiteSettings]
           ([ContactEmail]
           ,[ContactPhone]
           ,[FacebookUrl]
           ,[InstagramUrl]
           ,[ContactWhatsapp]
           ,[LastUpdated])
     VALUES
           ('demo@kidnest.com',
           '+961 3 123 467',
           'https://facebook.com',
           'https://instagram.com',
           'https://whatsapp.com',
           GETUTCDATE());
GO
