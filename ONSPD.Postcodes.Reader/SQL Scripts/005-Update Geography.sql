UPDATE	[dbo].[Postcode]
SET		[Location] = geography::Point([Latitude], [Longitude], 4326)
WHERE	[Location] IS NULL
--Exclude invalid latitudes - terminated postcodes will have latitude 99
AND		[Latitude] BETWEEN -90 AND 90

GO