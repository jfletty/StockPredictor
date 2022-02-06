create procedure Stage.GetExchangesRequiringRefresh
	@frequency	int
as
	select
		Stage.Exchange.ExchangeKey
	from
		Stage.Exchange
	left join
		Stage.Refresh 
		on
			Stage.Exchange.ExchangeKey = Stage.Refresh.ExternalKey
	where 
		Stage.Refresh.StartDateTime is null or 
		(datediff(minute, Stage.Refresh.EndDateTime, getuctdate()) >= @frequency or Stage.Refresh.Status = 2)
		and Stage.Refresh.RefreshType = 2
	order by 
		newid()
RETURN 0
