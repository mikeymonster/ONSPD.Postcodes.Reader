ALTER PROCEDURE [dbo].[Postcode_Upsert] (
  @data [dbo].[PostcodeDataType] READONLY
)
AS
SET NOCOUNT ON;

MERGE INTO [dbo].[Postcode] AS t
USING @data AS s
ON
(
  t.[Postcode] = s.[Postcode]
)
WHEN MATCHED 
	 AND (t.[Latitude] <> s.[Latitude] 
		  OR t.[Longitude] <> s.[Longitude])
THEN UPDATE SET
  t.[Latitude]  = s.[Latitude],
  t.[Longitude] = s.[Longitude],
	  t.[Location] = CASE WHEN s.[Latitude] BETWEEN -90 AND 90 
						THEN geography::Point(s.[Latitude], s.[Longitude], 4326)
						ELSE NULL  
					END

WHEN NOT MATCHED BY TARGET THEN INSERT
(
  [Postcode],
  [Latitude],
  [Longitude],
  [Location]
)
VALUES
(
  s.[Postcode],
  s.[Latitude],
  s.[Longitude],
  CASE WHEN s.[Latitude] BETWEEN -90 AND 90 
						THEN geography::Point(s.[Latitude], s.[Longitude], 4326)
						ELSE NULL  
					END
);

GO