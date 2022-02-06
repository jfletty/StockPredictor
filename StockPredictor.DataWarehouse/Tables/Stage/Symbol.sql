create table Stage.Symbol
(
	SymbolKey			int identity(1,1)   not null,
	SymbolValue			nvarchar(25)	    not null,
	ExchangeKey			varchar(5)		    not null,
	ExchangeValue		nvarchar(5)		    null,
	IsDisabled			bit				    not null default (0),
	DisabledDate		datetime		    null,
    constraint PK_Symbol primary key (SymbolKey),
	constraint FK_Exchange_ExchangeKey foreign key (ExchangeKey) references Stage.Exchange(ExchangeKey),
	constraint UI_Symbol_ExchangeValueSymbolValue unique (SymbolValue, ExchangeValue)
)
go;

create index IX_Symbol_IsDisabled 
    on Stage.Symbol(IsDisabled);
go;