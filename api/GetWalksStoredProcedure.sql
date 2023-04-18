use Track

create or alter function CalcDistance(@Lat1 real, @Lng1 real, @Lat2 real, @Lng2 real)
returns real
as
begin
	declare @lat1Radians as real = radians(@Lat1);
	declare @lat2Radians as real = radians(@Lat2);

	declare @lng1Radians as real = radians(@Lng1);
	declare @lng2Radians as real = radians(@Lng2);

    return 2 * 6371 * asin(sqrt(
		dbo.Hav(@lat1Radians - @lat2Radians) + cos(@lat1Radians) * cos(@lat2Radians) * dbo.Hav(@lng1Radians - @lng2Radians)
	));
end
go 

create function Hav(@A real)
returns real
as
begin
	return (1 - cos(@A)) / 2
end
go

create or alter view AnlyzedWalks as

	WITH CTEDiffs AS (
		SELECT *,
			DATEDIFF(MINUTE, LAG([date_track]) OVER (ORDER BY [date_track]), [date_track]) AS [TimeDiff]
		FROM [TrackLocation]
	),
	CTEWalks as (
		SELECT *, SUM(CASE WHEN [TimeDiff] > 30 OR [TimeDiff] IS NULL THEN 1 ELSE 0 END) OVER (ORDER BY [date_track]) AS [WalkNumber]
		FROM CTEDiffs
	)
	,
	CTEAnalyz as (
		select *,	
			DATEDIFF(SECOND, LAG([date_track]) OVER (ORDER BY [date_track]), [date_track]) AS [SecondsDiff],
			dbo.CalcDistance(LAG(latitude) over(ORDER BY [date_track]),  LAG(longitude) over(ORDER BY [date_track]), latitude, longitude) as [WalkDifferenceDistance],
			cast(CTEWalks.[date_track] as date) as [WalkDate]
		from CTEWalks
	)
	,
	CTETotal as (
		select *,
		SUM([SecondsDiff]) over(partition by [WalkNumber]) as PerWalkSeconds,
		SUM([SecondsDiff]) over(partition by [WalkDate]) as PerDayTotalSeconds,
		SUM([WalkDifferenceDistance] ) over (partition by [WalkNumber]) as PerWalkTotalDist,
		SUM([WalkDifferenceDistance]) over(partition by [WalkDate]) as PerDayTotalDist
		from CTEAnalyz
	)
	select
		[IMEI],
		[WalkDate],
		[WalkNumber],
		[PerWalkSeconds] / 60 as PerWalkMinutes,
		[PerDayTotalSeconds] / 60 as PerDayWalksMinutes,
		[PerWalkTotalDist],
		[PerDayTotalDist]
	from CTETotal
	--order by WalkNumber
	where WalkDifferenceDistance is not null
go

select * from TrackLocation
order by date_track
go

create or alter proc GetByIMEI @imei varchar(50)
as
begin
	select
		distinct [WalkNumber],
		[IMEI],
		[WalkDate],
		[PerWalkMinutes],
		[PerDayWalksMinutes],
		[PerWalkTotalDist],
		[PerDayTotalDist]
	from AnlyzedWalks
	where AnlyzedWalks.[IMEI]=@imei
	order by AnlyzedWalks.[PerWalkTotalDist] desc
end
go

select *
from AnlyzedWalks
group by AnlyzedWalks.WalkNumber

select * from AnlyzedWalks

exec dbo.GetByIMEI '359339077003915'
