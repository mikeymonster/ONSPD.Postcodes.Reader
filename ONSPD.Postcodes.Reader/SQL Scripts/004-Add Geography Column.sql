
ALTER TABLE [dbo].[Postcode]
ADD Location GEOGRAPHY NULL
GO

CREATE SPATIAL INDEX [SPATIAL_Postcode_Location] 
   ON [dbo].[Postcode](Location);
GO