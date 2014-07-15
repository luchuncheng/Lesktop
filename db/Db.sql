/****** Object:  Table [dbo].[Roles]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Roles](
		[ID] [int] NOT NULL PRIMARY KEY,
		[Name] [nvarchar](256) NOT NULL,
		UNIQUE(Name)
	)
	INSERT INTO Roles([ID], [Name]) VALUES (1, N'管理员');
	INSERT INTO Roles([ID], [Name]) VALUES (2, N'普通用户');
END
GO

/****** Object:  Table [dbo].[Users]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    /*
    Type:
        0 - 用户
        1 - 群组
    SubType: 
        0 - 注册用户, 
        1 - 公司内部人员
    IsTemp:
        Type = 0 时, 1 表示客服系统访客
        Type = 1 时, 1 表示多人会话（即临时群组）
    */
	CREATE TABLE [dbo].[Users](
		[ID] [int] IDENTITY(100000,1) NOT NULL PRIMARY KEY,
		[Name] [nvarchar](256) NOT NULL,
		[Nickname] [nvarchar](256) NOT NULL DEFAULT N'',
		[Type] [int] NOT NULL,
		[Password] [nvarchar](256) NOT NULL,
		[EMail] [nvarchar](256) NOT NULL DEFAULT N'',
		[UpperName] [nvarchar](256) NOT NULL,
		[MsgFileLimit] [int] NULL DEFAULT 0,
		[MsgImageLimit] [int] NULL DEFAULT 0,
		[AcceptStrangerIM] [int] NULL DEFAULT 1,
		[IsTemp] [int] NULL DEFAULT 0,
		[DiskSize] [int] NULL DEFAULT 0,
		[RegisterTime] [datetime] NOT NULL DEFAULT getdate(),
		[LastAccessTime] [datetime] NOT NULL DEFAULT getdate(),
		[HomePage] [nvarchar](256) NULL DEFAULT N'',
		[HeadIMG] [nvarchar](512) NOT NULL DEFAULT N'',
		[Remark] [text] NOT NULL DEFAULT N'',
		[Tel] [nvarchar](32) NOT NULL DEFAULT N'',
		[Mobile] [nvarchar](32) NOT NULL DEFAULT N'',
		[IsDeleted] [int] NOT NULL DEFAULT 0,
		[SubType] [int] NOT NULL DEFAULT 1, 
		UNIQUE(UpperName),
		UNIQUE(Name)
	)
	
	CREATE INDEX IDX_USERS_ID ON Users([ID]);
	CREATE INDEX IDX_USERS_UNAME ON Users(UpperName);
	CREATE INDEX IDX_USERS_NAME ON Users(Name);
	
	SET IDENTITY_INSERT Users ON
	INSERT INTO Users ([ID], [Name], Password, Type, Nickname, UpperName, RegisterTime)
	VALUES (2, N'administrator', N'', 0, N'系统管理员', N'ADMINISTRATOR', getdate());
	INSERT INTO Users ([ID], [Name], Password, Type, Nickname, UpperName, RegisterTime)
	VALUES (3, N'admin', N'D41D8CD98F00B204E9800998ECF8427E', 0, N'系统管理员', N'ADMIN', getdate());
	SET IDENTITY_INSERT Users OFF
END
GO

/****** Object:  Table [dbo].[Department]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Department]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Department](
		[ID] [int] IDENTITY(10,1) NOT NULL PRIMARY KEY,
		[Name] [nvarchar](256) NULL,
		[ParentID] [int] NOT NULL DEFAULT 0
	)
	SET IDENTITY_INSERT Department ON
	INSERT INTO Department (Name,[ID]) VALUES (N'XX公司', 1)
	SET IDENTITY_INSERT Department OFF
END

/****** Object:  Table [dbo].[Comment]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comment]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Comment](
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[SenderID] [int] NOT NULL,
		[ReceiverID] [int] NOT NULL,
		[Content] [text] NOT NULL,
		[CreatedTime] [datetime] NOT NULL,
		[IsRead] [int] NOT NULL,
		[IsImportant] [int] NOT NULL
	)
	
	CREATE INDEX [IDX_COMMENT_RECEIVERID] ON [dbo].[Comment] ([ReceiverID] ASC)
END
GO

/****** Object:  Table [dbo].[Message]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Message]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Message](
		[ID] [int] NOT NULL PRIMARY KEY,
		[CreatedTime] [datetime] NOT NULL,
		[Receiver] [int] NOT NULL,
		[Sender] [int] NOT NULL,
		[Content] [text] NOT NULL
	)
	CREATE INDEX IDX_MESSAGE_SRCT ON Message(Sender,Receiver,CreatedTime);
	CREATE INDEX IDX_MESSAGE_RSCT ON Message(Receiver,Sender,CreatedTime);
	CREATE INDEX IDX_MESSAGE_CTSR ON Message(CreatedTime,Sender,Receiver);
	CREATE INDEX IDX_MESSAGE_CTRS ON Message(CreatedTime,Receiver,Sender);
END
GO

/****** Object:  Table [dbo].[EmbedCode]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmbedCode]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[EmbedCode](
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[URL] [nvarchar](2048) NOT NULL,
		[EmbedConfig] [nvarchar](4000) NULL,
		[Users] [nvarchar](4000) NOT NULL
	)
END
GO

/****** Object:  Table [dbo].[IP]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ip]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ip](
		[ip1] [float] NULL,
		[ip2] [float] NULL,
		[ip3] [nvarchar](200) NULL,
		[ip4] [nvarchar](200) NULL
	)
	
	CREATE INDEX [IDX_IP_IP1] ON [dbo].[ip] ([ip1] ASC)
	CREATE INDEX [IDX_IP_IP2] ON [dbo].[ip] ([ip2] ASC)
END
GO

/****** Object:  Table [dbo].[Dept_User]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Dept_User]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Dept_User](
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[UserID] [int] NOT NULL,
		[DeptID] [int] NOT NULL
	)
	
	CREATE INDEX IDX_DEPTUSER_DU ON Dept_User([DeptID], [UserID]);
	CREATE INDEX IDX_DEPTUSER_UD ON Dept_User([UserID], [DeptID]);

	INSERT INTO Dept_User([UserID], [DeptID]) VALUES (2, 1);
	INSERT INTO Dept_User([UserID], [DeptID]) VALUES (3, 1);
END
GO

/****** Object:  Table [dbo].[User_Role]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_Role]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[User_Role](
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[UserID] [int] NOT NULL,
		[RoleID] [int] NOT NULL
	)
	INSERT INTO User_Role([UserID], [RoleID])
	SELECT [Users].[ID], Roles.[ID] FROM [Users], [Roles] WHERE UpperName='ADMIN';
	INSERT INTO User_Role([UserID], [RoleID])
	SELECT [Users].[ID], Roles.[ID] FROM [Users], [Roles] WHERE UpperName='ADMINISTRATOR';
END
GO

/****** Object:  Table [dbo].[UserRecvMessage]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRecvMessage]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[UserRecvMessage](
		[UserID] [int] NOT NULL,
		[MsgID] [int] NOT NULL
	)
	
	CREATE NONCLUSTERED INDEX [IDX_URM_MU] ON [dbo].[UserRecvMessage] ([MsgID] ASC, [UserID] ASC)
	CREATE NONCLUSTERED INDEX [IDX_URM_UM] ON [dbo].[UserRecvMessage] ([UserID] ASC, [MsgID] ASC)
END
GO

/****** Object:  Table [dbo].[UserRelationShip]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRelationship]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[UserRelationship](
		[HostID] [int] NOT NULL,
		[GuestID] [int] NOT NULL,
		[Relationship] [int] NOT NULL,
		[RenewTime] [datetime] NOT NULL,
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY
	)
	CREATE INDEX IDX_USERRELA_HG ON Userrelationship(HostID, GuestID);
	CREATE INDEX IDX_USERRELA_GH ON Userrelationship(GuestID, HostID);
END
GO

/****** Object:  Table [dbo].[DbInfo]    Script Date: 06/17/2011 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DbInfo]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[DbInfo](
		[DbVersion] [int] NOT NULL
	)
	INSERT INTO DbInfo (DbVersion) VALUES (130626);
END
GO

/*
DECLARE @DbVersion INT
SELECT @DbVersion = DbVersion FROM DbInfo
IF(@DbVersion = 130626)
BEGIN
	SET @DbVersion = 130626
	UPDATE DbInfo SET DbVersion = 130626
END
*/

/****** Object:  View [dbo].[IM_Message]    Script Date: 06/17/2011 ******/
/*聊天消息，不包括系统通知消息(即发送人为administrator的消息)*/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[IM_Message]'))
DROP VIEW [dbo].[IM_Message]
GO

CREATE VIEW [dbo].[IM_Message]
AS
select * from [Message] where Receiver > 3 and Sender > 3

GO

/****** Object:  View [dbo].[MessageList_Group]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MessageList_Group]'))
DROP VIEW [dbo].[MessageList_Group]
GO

CREATE VIEW [dbo].[MessageList_Group]
AS
select t.GroupID, max(t.CreatedTime) LatestTime, count(t.ID) MessagesCount
from(
	select Receiver GroupID, CreatedTime, IM_Message.[ID]
	from IM_Message,Users u
	where Receiver = u.[ID] and u.[Type] = 1
	union
	select Receiver GroupID, CreatedTime, IM_Message.[ID]
	from IM_Message, Users u
	where Receiver = u.[ID] and u.[Type] = 1
) t
group by t.GroupID

GO

/****** Object:  View [dbo].[MessageList_User]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[MessageList_User]'))
DROP VIEW [dbo].[MessageList_User]
GO

CREATE view [dbo].[MessageList_User]
as
select t.UserID, t.PeerID,max(t.CreatedTime) as LatestTime, count(t.[ID]) as MessagesCount
from(
	select Sender as UserID,Receiver as PeerID,CreatedTime,IM_Message.[ID] 
	from IM_Message, Users u, Users s 
	where Sender < Receiver and Receiver = u.[ID] and u.Type = 0
	union
	select Receiver as UserID,Sender as PeerID,CreatedTime,IM_Message.[ID]
	from IM_Message, Users u
	where Sender > Receiver and Receiver = u.[ID] and u.Type = 0
) t
group by t.UserID,t.PeerID

GO

/****** Object:  View [dbo].[UserGroupMessage]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[UserGroupMessage]'))
DROP VIEW [dbo].[UserGroupMessage]
GO

CREATE view [dbo].[UserGroupMessage]
as
select m.*, rm.UserID as UserID
from UserRecvMessage rm, IM_Message m, Users r
where rm.MsgID = m.[ID] and r.[ID] = m.Receiver and r.[Type] = 1 and m.Sender > 3

GO

/****** Object:  View [dbo].[UserMessage]    Script Date: 06/17/2011 00:26:09 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[UserMessage]'))
DROP VIEW [dbo].[UserMessage]
GO

CREATE view [dbo].[UserMessage]
as
select m.*, rm.UserID as UserID
from UserRecvMessage rm, IM_Message m, Users r, Users s
where rm.MsgID = m.[ID] 
	and r.[ID] = m.Receiver and r.Type = 0
	and s.[ID] = m.Sender and s.Type = 0
	
GO

/****** Object:  View [dbo].[VisibleUsers]    Script Date: 06/17/2011 00:26:09 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VisibleUsers]'))
DROP VIEW [dbo].VisibleUsers
GO

CREATE VIEW [dbo].VisibleUsers
AS
select 
	guest.*,
	host.[ID] as UserID
from 
	UserRelationship r,
	Users host,
	Users guest
where 
	guest.Type = 1 and
	r.HostID=host.[ID] and
	r.GuestID=guest.[ID] and
	host.IsDeleted = 0 and guest.IsDeleted = 0
union all
select 
	u.*,
	host.[ID] as UserID
from 
	Users u join Users host on host.SubType = 1
where 
	u.Type = 0 and u.IsTemp = 0 and 
	u.IsDeleted = 0 and u.SubType = 1

GO

/****** Object:  View [dbo].[UsersEx]    Script Date: 06/17/2011 00:26:09 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[UsersEx]'))
DROP VIEW [dbo].UsersEx
GO

CREATE VIEW UsersEx
AS
select u.*, isnull(ur.RoleID, 0) as IsAdmin, isnull(c.Name, N'') as Creator, isnull(c.ID, 0)  as CreatorID
from Users u 
	left join User_Role ur on u.[ID] = ur.UserID and ur.RoleID = 1 
	left join (
		UserRelationship r join Users c on r.GuestID = c.[ID]
	) on u.[ID] = r.HostID and r.Relationship = 3 and u.Type = 1
GO

/****** Object:  UserDefinedFunction [dbo].[GetAllSubDepts]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllSubDepts]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetAllSubDepts]
GO

CREATE function [dbo].[GetAllSubDepts](@deptID int)
returns @res table(
	[ID] int
)
as
begin
	insert @res([ID])
	select [ID] from Department where ParentID = @deptID;

	declare dept_cursor cursor
	for
	select [ID] from Department where ParentID = @deptID
	open dept_cursor
	declare @subDeptID int;
	fetch next from dept_cursor into @subDeptID;
	while(@@fetch_status = 0)
	begin
		insert into @res([ID])
		select [ID] from dbo.GetAllSubDepts(@subDeptID);
		fetch next from dept_cursor into @subDeptID;
	end

	return
end

GO

/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[SplitString]
GO

create function [dbo].[SplitString](
	@String nvarchar(4000),
	@SplitChar char
)
returns @res table(
	Value nvarchar(128),
	vindex int
)
as
begin
	declare @index int,@unit nvarchar(128),@inext int,@len int,@i int
	set @index=1
	set @i=1
	set @len=len(@String)
	while @index<=@len
	begin
		set @inext=charindex(@SplitChar,@String,@index)
		if @inext=0 set @inext=@len+1
		if @inext>@index
		begin
			set @unit=ltrim(rtrim(substring(@String,@index,@inext-@index)))
			if @unit<>''
			begin
				insert into @res (value,vindex) values (@unit,@i)
				set @i=@i+1
			end
		end
		set @index=@inext+1
	end
	return
end

GO

/****** Object:  UserDefinedFunction [dbo].[ParseIntArray] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id  =  OBJECT_ID(N'[dbo].[ParseIntArray]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].ParseIntArray
GO

CREATE FUNCTION [dbo].ParseIntArray(@data IMAGE)
RETURNS @res TABLE(
	[Value] INT,
	[Index] INT
)
AS
BEGIN
	declare @i int, @index int, @length int
	set @length = datalength(@data)
	set @i = 1
	set @index = 1
	while @i + 3 <= @length
	begin
		insert into @res ([Value], [Index]) values (convert(BIGINT, substring(@data, @i, 4)), @index)
		set @index = @index + 1
		set @i = @i + 4
	end
	return
END

GO


/****** Object:  UserDefinedFunction [dbo].[ParseBigIntArray] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id  =  OBJECT_ID(N'[dbo].[ParseBigIntArray]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].ParseBigIntArray
GO

CREATE FUNCTION [dbo].ParseBigIntArray(@data IMAGE)
RETURNS @res TABLE(
	[Value] BIGINT,
	[Index] INT
)
AS
BEGIN
	declare @i int, @index int, @length int
	set @length = datalength(@data)
	set @i = 1
	set @index = 1
	while @i + 7 <= @length
	begin
		insert into @res ([Value], [Index]) values (convert(BIGINT, substring(@data, @i, 8)), @index)
		set @index = @index + 1
		set @i = @i + 8
	end
	return
END

GO

/****** Object:  UserDefinedFunction [dbo].[ParseStringArray] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id  =  OBJECT_ID(N'[dbo].[ParseStringArray]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[ParseStringArray]
GO

CREATE FUNCTION [dbo].[ParseStringArray](
	@string NTEXT,
	@split_char NCHAR
)
RETURNS @res TABLE(
	[Value] NVARCHAR(128),
	[Index] INT
)
AS
BEGIN
	declare @index int,@unit nvarchar(128),@inext int,@len int,@i int
	set @index = 1
	set @i = 1
	set @len = datalength(@string)/2
	while @index <= @len
	begin
		set @inext = charindex(@split_char, @string, @index)
		if @inext = 0 set @inext = @len+1
		if @inext > @index
		begin
			set @unit = ltrim(rtrim(substring(@string, @index, @inext - @index)))
			if @unit <> ''
			begin
				insert into @res ([Value], [Index]) values (@unit, @i)
				set @i = @i + 1
			end
		end
		set @index = @inext + 1
	end
	return
END

GO

/****** Object:  UserDefinedFunction [dbo].[GetArea]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetArea]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetArea]
GO

create function [dbo].[GetArea](@ip nvarchar(100))
returns nvarchar(100)
begin
	declare @temp table([index] int,[value] nvarchar(50));
	insert @temp([index],[value])
	select vindex,value from dbo.SplitString(@ip,'.')

	declare @fip float;
	select @fip = sum(convert(float, t.value) * power(256,4-t.[index]))
	from @temp t

	declare @area nvarchar(100)
	set @area = ''
	select @area = ip3 from IP where ip1 <= @fip and @fip <= ip2
	return @area
end

GO

/****** Object:  UserDefinedFunction [dbo].[GetServiceUsersPreview]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetServiceUsersPreview]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetServiceUsersPreview]
GO

CREATE function [dbo].[GetServiceUsersPreview](@ids nvarchar(4000))
returns nvarchar(4000)
as
begin
	declare @tab_ids table([Index] int, ID int);
	insert into @tab_ids
	select vIndex, convert(int, [Value]) from dbo.SplitString(@ids,',');
	
	declare cs_cursor cursor
	for
	select u.[ID] as ID, d.[ID] as DeptID, u.Name as Name, u.Nickname as Nickname, d.Name as DeptName
	from 
		@tab_ids t1 join @tab_ids t2 on t1.[Index] % 2 = 1 and t1.[Index] = t2.[Index] - 1, 
		Users u, Department d, Dept_User du
	where 
		t1.ID = u.[ID] and t2.ID = d.[ID] and u.[ID] = du.UserID and d.[ID] = du.DeptID
	order by d.[ID]
	
	declare @res nvarchar(4000);
	set @res = N'';
	declare @PreDeptID int;
	set @PreDeptID = 0;
	declare @DeptName nvarchar(256), @DeptID int, @Name nvarchar(256), @Nickname nvarchar(256), @ID int;
	
	open cs_cursor
	fetch next from cs_cursor into @ID, @DeptID, @Name, @Nickname, @DeptName;
	while(@@fetch_status = 0)
	begin
		if(@DeptID <> @PreDeptID)
		begin
			set @res = @res + @DeptName + char(13) + char(10);
			set @PreDeptID = @DeptID;
		end
		set @res = @res + N'    '+@Nickname+N'('+@Name+N')' + char(13) +  char(10);
		fetch next from cs_cursor into @ID, @DeptID, @Name, @Nickname, @DeptName;
	end
	
	return @res;
end

GO

/****** Object:  UserDefinedFunction [dbo].[GetUserDepts]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserDepts]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetUserDepts]
GO

CREATE function [dbo].[GetUserDepts](@id int)
returns nvarchar(4000)
as
begin
	declare @res nvarchar(4000);
	set @res = N'';
	select @res = @res + ',' + d.Name
	from Dept_User du, Department d
	where du.DeptID = d.[ID] and du.UserID = @id;
	if(@res <> '') return substring(@res, 2, len(@res) - 1);
	return @res;
end

GO


/****** Object:  StoredProcedure [dbo].[AddFriend]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddFriend]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AddFriend]
GO

create proc [dbo].[AddFriend](@user int, @friend int)
as
begin
	insert into UserRelationship (HostID,GuestID,Relationship,RenewTime)
	select host.[ID] as HostID,guest.[ID] as GuestID,0,getdate()
	from Users host,Users guest
	where 
		(host.ID=@user or host.ID=@friend) and 
		(guest.ID=@friend or guest.ID=@user) and 
		host.[ID]<>guest.[ID]
end

GO

/****** Object:  StoredProcedure [dbo].[AddUsersToDept]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddUsersToDept]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AddUsersToDept]
GO

CREATE proc [dbo].[AddUsersToDept](@ids IMAGE, @deptId int)
as
begin
	declare @tab_ids_temp table(ID int);
	declare @tab_ids table(ID int);

	insert into @tab_ids_temp
	select distinct convert(int, [Value]) from dbo.ParseIntArray(@ids);

	insert into @tab_ids(ID)
	select ids.ID
	from @tab_ids_temp ids
	where not exists(select * from Dept_User du where ids.ID = du.UserID and du.[DeptID] = @deptId) and
		ids.ID not in (select du.UserID from Dept_User du, dbo.GetAllSubDepts(@deptId) d where du.DeptID = d.[ID])

	insert into Dept_User(DeptID, UserID)
	select @deptId, ids.ID
	from @tab_ids ids;
end

GO

/****** Object:  StoredProcedure [dbo].[AddUsersToGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AddUsersToGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AddUsersToGroup]
GO

CREATE proc [dbo].[AddUsersToGroup](@ids image, @groupId int)
as
begin
	declare @tab_ids_temp table(ID int);
	declare @tab_ids table(ID int);

	insert into @tab_ids_temp
	select distinct convert(int, [Value]) from dbo.ParseIntArray(@ids);

	insert into @tab_ids(ID)
	select ids.ID
	from @tab_ids_temp ids
	where not exists(select * from UserRelationship ur where ids.ID = ur.HostID and ur.GuestID = @groupId);

	insert into UserRelationship(HostID, GuestID, RenewTime, RelationShip)
	select @groupId, ids.ID, getDate(), 0
	from @tab_ids ids;

	insert into UserRelationship(GuestID, HostID, RenewTime, RelationShip)
	select @groupId, ids.ID, getDate(), 0
	from @tab_ids ids;
end

GO

/****** Object:  StoredProcedure [dbo].[CreateDept]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateDept]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateDept]
GO

CREATE proc [dbo].[CreateDept](@name nvarchar(256), @parentID int)
as
begin
	INSERT INTO Department (Name,ParentID) values (@name, @parentID);
end

GO

/****** Object:  StoredProcedure [dbo].[CreateEmbedCode]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateEmbedCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateEmbedCode]
GO

CREATE proc [dbo].[CreateEmbedCode](@Users nvarchar(4000), @config nvarchar(4000))
as
begin
	insert into EmbedCode (Url, EmbedConfig, Users) values (N'', @config, @Users);
	select @@IDENTITY;
end

GO

/****** Object:  StoredProcedure [dbo].[CreateGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateGroup]
GO

CREATE proc [dbo].[CreateGroup](
	@creator int, 
	@name nvarchar(256), 
	@nickname nvarchar(256), 
	@deptId int,
	@subType int,
	@remark nvarchar(1024)
)
as
begin
	if(exists(select [ID] from Users where UpperName=UPPER(@name)))
	begin
		raiserror(N'群"%s"已存在', 16, 1, @name)
		return
	end

	if(@name = '') select @name = '_g_' + convert(nvarchar(100),max([ID])) from Users;

	declare @now datetime;
	set @now=getdate();

	insert into Users (Name,UpperName,Password,Nickname,Type,EMail,IsTemp,RegisterTime,SubType, Remark) 
	values (@name,UPPER(@name),'',@nickname,1,'',0,getdate(), @subType, @remark)

	declare @id int
	set @id = @@identity

	insert into User_Role (UserID,RoleID)
	select [ID] as UserID,2 as RoleID from Users where [ID]=@id
					
	insert into UserRelationship (RenewTime,HostID,GuestID,Relationship)
	select getdate() as RenewTime,@creator as HostID, @id as GuestID,3 as Relationship
					
	insert into UserRelationship (RenewTime,HostID,GuestID,Relationship)
	select getdate() as RenewTime,@id as GuestID,@creator as HostID,3 as Relationship

	insert into Dept_User (DeptID,UserID) values (@deptId, @id);
	
	select @id;
end

GO

/****** Object:  StoredProcedure [dbo].[CreateTempGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateTempGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateTempGroup]
GO

CREATE proc [dbo].[CreateTempGroup](
	@creator int, 
	@members nvarchar(4000)
)
as
begin
	declare @tab_ids table(ID int);
	insert into @tab_ids
	select distinct convert(int, [Value]) from dbo.SplitString(@members,',');

	declare @gname nvarchar(100);
	select @gname = convert(nvarchar(100),max([ID])) from Users;
	declare @now datetime;
	set @now=getdate();

	insert into Users (Name,UpperName,Password,Nickname,Type,EMail,IsTemp,RegisterTime,SubType) 
	values (N'_tg_'+@gname,UPPER(N'_tg_'+@gname),'',N'多人会话'+@gname,1,'',1,@now,0)

	declare @id int
	set @id = @@identity

	insert into User_Role (UserID,RoleID)
	select [ID] as UserID,2 as RoleID from Users where [ID]=@id
					
	insert into UserRelationship (RenewTime,HostID,GuestID,Relationship)
	select getdate() as RenewTime,@creator as HostID, @id as GuestID, 3 as Relationship
					
	insert into UserRelationship (RenewTime,HostID,GuestID,Relationship)
	select getdate() as RenewTime,@id as GuestID, @creator as HostID, 3 as Relationship

	insert into Dept_User (DeptID,UserID) values (1, @id);
					
	insert into UserRelationship (RenewTime,HostID,GuestID,Relationship)
	select getdate() as RenewTime,@id,ids.ID,0 as Relationship
	from @tab_ids ids 
	where ids.ID <> @creator
					
	insert into UserRelationship (RenewTime,GuestID,HostID,Relationship)
	select getdate() as RenewTime,@id,ids.ID, 0 as Relationship
	from @tab_ids ids 
	where ids.ID <> @creator

	select @id;
end

GO

/****** Object:  StoredProcedure [dbo].[CreateTempUser]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateTempUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateTempUser]
GO

CREATE proc [dbo].[CreateTempUser](
	@ip nvarchar(256)
)
as
begin	
	declare @name nvarchar(256), @max_id int ;
	select @max_id = max([ID]) FROM Users;
	
	set @name = N'_t_'+CONVERT(nvarchar(256), @max_id+1);
	
	declare @nickname nvarchar(256);
	if(@ip = '') set @nickname = N'访客' + CONVERT(nvarchar(256), @max_id+1);
	else set @nickname = dbo.GetArea(@ip) + N'访客';
	
	declare @ip_name_index int;
	set @ip_name_index = charindex(N'.', @ip, 0);
	if(@ip_name_index <> 0) set @ip_name_index = charindex(N'.', @ip, @ip_name_index + 1);
	if(@ip_name_index <> 0) set @nickname = @nickname + N' [' + substring(@ip, 1, @ip_name_index) +  N'*.*]';

	insert into Users (Name,UpperName,Password,Nickname,Type,EMail,IsTemp,RegisterTime,SubType) 
	values (@name,UPPER(@name),N'',@nickname,0,N'',1,getdate(),0);

	declare @id int
	set @id = @@identity
	
	select @id;
	
end

GO

/****** Object:  StoredProcedure [dbo].[CreateUser]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreateUser]
GO

CREATE proc [dbo].[CreateUser](
	@name nvarchar(256), 
	@nickname nvarchar(256), 
	@password nvarchar(256), 
	@email nvarchar(256), 
	@deptId int,
	@subType int
)
as
begin	
	if exists (select [ID] from Users where UpperName=UPPER(@name))
	begin
		raiserror(N'用户"%s"已存在', 16 ,1, @name)
		return
	end

	insert into Users (Name,UpperName,Password,Nickname,Type,EMail,IsTemp,RegisterTime,SubType) 
	values (@name,UPPER(@name),@password,@nickname,0,@email,0,getdate(),@subType);

	declare @id int
	set @id = @@identity

	insert into User_Role (UserID,RoleID)
	select [ID] as UserID,2 as RoleID from Users where [ID]=@id;	

	if(@deptId > 0)
	begin
		insert into Dept_User (DeptID,UserID) values (@deptId, @id);
	end
	
	select @id;
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteComment]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteComment]
GO

create proc [dbo].[DeleteComment](@Id int)
as
begin
	delete from Comment where ID=@Id;
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteDept]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteDept]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteDept]
GO

CREATE proc [dbo].[DeleteDept](@id int)
as
begin
	if(exists(select * from Dept_User where DeptID=@id))
	begin
		raiserror(N'你不能删除存在员工的部门',16,1)
		return;
	end
	if(exists(select * from Department where ParentID=@id))
	begin
		raiserror(N'你不能删除存在子部门的部门',16,1)
		return;
	end
	DELETE FROM Department WHERE [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteEmbedCode]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteEmbedCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteEmbedCode]
GO

CREATE proc [dbo].[DeleteEmbedCode](@id int)
as
delete from EmbedCode where ID = @id;

GO

/****** Object:  StoredProcedure [dbo].[DeleteFriend]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteFriend]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteFriend]
GO

create proc [dbo].[DeleteFriend](@user int, @friend int)
as
begin
	delete from UserRelationship
	where (HostID=@user and GuestID=@friend) or (HostID=@friend and GuestID=@user)
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteGroup]
GO

CREATE proc [dbo].[DeleteGroup](@id int)
as
begin
	delete from User_Role where UserID=@id
	delete from UserRelationship where HostID=@id or GuestID=@id
	update Users set IsDeleted = 1, Name = N'_del_'+convert(nvarchar(20), [ID])+N'_'+Name, UpperName = N'_DEL_'+convert(nvarchar(20), [ID])+N'_'+UpperName where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteMessageGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteMessageGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteMessageGroup]
GO

CREATE proc [dbo].[DeleteMessageGroup](@user int, @peer int)
as
begin
	if(@peer <= 0) 
	begin
		delete from UserRecvMessage where MsgID in (select [ID] from Message where Receiver = @user)
		delete from Message where Receiver = @user
	end
	else
	begin
		delete from UserRecvMessage where MsgID in (select [ID] from Message where Sender = @user and Receiver = @peer)
		delete from UserRecvMessage where MsgID in (select [ID] from Message where Receiver = @user and Sender = @peer)
		delete from Message where Sender=@user and Receiver = @peer
		delete from Message where Receiver = @user and Sender = @peer
	end
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteMessages]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteMessages]
GO

CREATE proc [dbo].[DeleteMessages](
	@ids nvarchar(4000)
)
as
begin
	declare @ids_table table(id int)
	insert @ids_table(id)
	select convert(int,Value) from dbo.SplitString(@ids,',')
	delete from Message where [ID] in (select id from @ids_table);
	delete from UserRecvMessage where MsgID in (select id from @ids_table);
end

GO

/****** Object:  StoredProcedure [dbo].[DeleteUser]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteUser]
GO

CREATE proc [dbo].[DeleteUser](@id int)
as
begin
	delete from UserRelationship where HostID = @id or GuestID = @id;
	delete from User_Role where UserID = @id;
	delete from Dept_User where UserID = @id;
	update Users 
	set 
		IsDeleted = 1, 
		Name = N'_del_'+convert(nvarchar(20), [ID])+N'_'+Name, 
		UpperName = N'_DEL_'+convert(nvarchar(20), [ID])+N'_'+UpperName 
	where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[FindMessages]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FindMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[FindMessages]
GO

CREATE proc [dbo].[FindMessages](@user int, @peer int, @from datetime)
as
begin
	if(@peer > 0)
	begin
		select top 100 [ID],Receiver,Sender,Content,CreatedTime
		from Message 
		where Receiver = @user and Sender = @peer and CreatedTime > @from
		order by CreatedTime desc
	end
	else
	begin
		select top 100 temp.[ID], temp.Receiver, temp.Sender, temp.Content, temp.CreatedTime 
		from (
			select m.[ID], Receiver, Sender, Content, CreatedTime 
			from Message m, Users s, Users r
			where Receiver = @user and CreatedTime > @from 
				and s.[ID] = m.Sender and s.IsDeleted = 0
				and r.[ID] = m.Receiver and r.IsDeleted = 0
			union all
			select m.[ID], m.Receiver, m.Sender, m.Content, m.CreatedTime 
			from Message m, UserRelationShip ur, Users u, Users s
			where ur.HostID = @user and ur.GuestID = u.[ID] 
				and m.Sender = s.[ID] and s.IsDeleted = 0
				and u.Type = 1 and u.IsDeleted = 0 and ur.GuestID = m.Receiver 
				and m.CreatedTime > @from and m.CreatedTime > ur.RenewTime
				and u.SubType <> 3
		) temp
		order by CreatedTime desc
	end
end

GO

/****** Object:  StoredProcedure [dbo].[GetUserInfo]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserInfo]
GO

CREATE proc [dbo].[GetUserInfo](@id int)
as
begin
	select * from UsersEx where [ID] = @id
end

GO

/****** Object:  StoredProcedure [dbo].[GetUserID]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserID]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserID]
GO

CREATE proc [dbo].[GetUserID](@name nvarchar(256))
as
begin
	select ID from UsersEx where UpperName = UPPER(@name)
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllComment]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllComment]
GO

CREATE proc [dbo].[GetAllComment](@ReceiverID int)
as
begin
	if(@ReceiverID <>0)
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel
		from Comment join Users on Comment.[SenderID] = Users.[ID] 
		where ReceiverID = @ReceiverID
		order by CreatedTime desc
	else
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel 
		from Comment join Users on Comment.[SenderID] = Users.[ID]
		order by CreatedTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllDepts]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllDepts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllDepts]
GO

CREATE proc [dbo].[GetAllDepts](@deptID int)
as
begin
	if(@deptID < 0)
	begin
		select * from Department order by [ID] asc
	end
	else
	begin
		select * from Department where ParentID = @deptID order by [ID] asc
	end
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllEmbedCode]    Script Date: 06/17/2011 00:41:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllEmbedCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllEmbedCode]
GO

CREATE proc [dbo].[GetAllEmbedCode]
as
select *, [dbo].[GetServiceUsersPreview](Users) as UsersPreview from EmbedCode

GO

/****** Object:  StoredProcedure [dbo].[GetAllGroups]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllGroups]
GO

CREATE proc [dbo].[GetAllGroups](@deptID int)
as
begin
	if(@deptID <= 0)
	begin
		select u.[ID], u.Name, u.Nickname, u.EMail, u.RegisterTime, c.[ID] Creator
		from Users u, Users c, UserRelationShip ur 
		where u.IsTemp =0 and u.Type = 1 and u.[ID] = ur.HostID and c.[ID] = ur.GuestID and ur.Relationship = 3
		order by u.RegisterTime desc
	end
	else
	begin
		select u.[ID], u.Name, u.Nickname, u.EMail, u.RegisterTime, c.[ID] Creator
		from Users u, Users c, UserRelationShip ur, Dept_User du
		where u.IsTemp = 0 and u.Type = 1 and u.[ID] = ur.HostID and c.[ID] = ur.GuestID and ur.Relationship = 3 and u.[ID] = du.UserID and du.DeptID = @deptID
		order by u.RegisterTime desc
	end
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllRegisterGroups]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllRegisterGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllRegisterGroups]
GO

CREATE proc [dbo].[GetAllRegisterGroups]
as
begin
	select u.[ID] as ID, u.[ID], u.Name, u.Nickname, u.EMail, u.RegisterTime, c.[ID] Creator, u.Remark
	from Users u, Users c, UserRelationShip ur 
	where u.Type = 1 and u.[ID] = ur.HostID and c.[ID] = ur.GuestID and ur.Relationship = 3 and u.SubType=0 and u.IsTemp = 0
	order by u.RegisterTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllRegisterUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllRegisterUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllRegisterUsers]
GO

CREATE proc [dbo].[GetAllRegisterUsers]
as
begin
	select [ID], Name, Nickname, EMail, RegisterTime 
	from Users u 
	where u.Type = 0 and u.SubType=0 and u.IsDeleted = 0 and IsTemp = 0
	order by IsTemp asc, RegisterTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllUsers]
GO

CREATE proc [dbo].[GetAllUsers](@deptID int)
as
begin
	if(@deptID < 0)
	begin
		select [ID], Name, Nickname, EMail, RegisterTime 
		from Users u 
		where u.Type = 0
		order by RegisterTime desc
	end
	else
	begin
		select * from (
			select u.[ID], u.Name, u.Nickname, u.EMail, u.RegisterTime, d.[ID] as DeptID, d.Name as Depts
			from Users u, Dept_User du, Department d
			where u.Type = 0 and u.IsDeleted = 0 and u.[ID] = du.UserID and du.DeptID = @deptID and du.DeptID = d.[ID]
			union 
			select t.*, 0 as DeptID, dbo.GetUserDepts(t.[ID]) as Depts from (
				select distinct u.[ID], u.Name, u.Nickname, u.EMail, u.RegisterTime
				from Users u, Dept_User du, dbo.GetAllSubDepts(@deptID) subDepts
				where u.Type = 0 and u.[ID] = du.UserID and du.DeptID = subDepts.[ID] and u.IsDeleted = 0
			) t
		) users
		order by DeptID desc
	end
end

GO

/****** Object:  StoredProcedure [dbo].[GetAllCompanyUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllCompanyUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllCompanyUsers]
GO

CREATE proc [dbo].GetAllCompanyUsers
as
begin
	select u.*
	from Users u 
	where u.SubType = 1 and IsDeleted = 0
	order by RegisterTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetDeptInfo]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDeptInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDeptInfo]
GO

create proc [dbo].[GetDeptInfo](@id int)
as
begin
	select * from Department where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[GetDeptItems]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDeptItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDeptItems]
GO

create proc [dbo].[GetDeptItems](@user_id int, @dept_id int)
as
begin
	select UserID from Dept_User du, Users u 
	where du.UserID = u.[ID] and DeptID = @dept_id and u.SubType = 1 and u.IsTemp = 0 and u.[ID] > 3 and u.IsDeleted = 0 and u.[Type] = 0
	union 
	select UserID from Dept_User du, Users u, UserRelationship ur
	where du.UserID = u.[ID] and DeptID = @dept_id and u.SubType = 1 and u.IsTemp = 0 and u.[ID] > 3 and u.IsDeleted = 0 and u.[Type] = 1
		and ur.HostID = @user_id and ur.GuestID = u.[ID]
end

GO

/****** Object:  StoredProcedure [dbo].[GetTempGroups]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTempGroups]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTempGroups]
GO

create proc [dbo].[GetTempGroups](@user_id int)
as
begin
	select u.[ID] from Users u, UserRelationship ur
	where ur.HostID = @user_id and ur.GuestID = u.[ID]
		and u.SubType = 0 and u.IsTemp = 1 and u.[ID] > 3
end

GO

/****** Object:  StoredProcedure [dbo].[GetSubDepts]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSubDepts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSubDepts]
GO

create proc [dbo].[GetSubDepts](@dept_id int)
as
begin
	select * from Department where ParentID = @dept_id
end

GO

/****** Object:  StoredProcedure [dbo].[GetDeptUser]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDeptUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDeptUser]
GO

CREATE proc [dbo].[GetDeptUser]
as
begin
	select Dept_User.* from Dept_User, Users u, Department d 
	where DeptID = d.[ID] and UserID = u.[ID] and u.IsDeleted = 0 and u.IsTemp = 0 and u.SubType = 1
end

GO

/****** Object:  StoredProcedure [dbo].[GetCommFriends]    Script Date: 07/09/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCommFriends]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].GetCommFriends
GO

CREATE proc [dbo].GetCommFriends(@user_id int)
as
begin
	SELECT GuestID as CommFriendID, u.Name as CommFriendName
	FROM UserRelationship ur, Users u
	WHERE HostID = @user_id and ur.GuestID = u.[ID] and IsTemp = 0
end

GO

/****** Object:  StoredProcedure [dbo].[GetEmbedCode]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEmbedCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetEmbedCode]
GO

CREATE proc [dbo].[GetEmbedCode](@id int)
as
select * from EmbedCode where ID = @id;

GO

/****** Object:  StoredProcedure [dbo].[GetFriends]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFriends]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFriends]
GO

CREATE proc [dbo].[GetFriends](@name nvarchar(256))
as
begin
	select 
		guest.ID as ID,
		guest.Name as Name,
		guest.Type as Type,
		r.RenewTime as RenewTime,
		r.Relationship as Relationship
	from 
		UserRelationship r,
		Users host,
		Users guest
	where 
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.UpperName=upper(@name) and
		host.IsDeleted = 0 and guest.IsDeleted = 0
end

GO

/****** Object:  StoredProcedure [dbo].[GetGroupManagers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGroupManagers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetGroupManagers]
GO

create proc [dbo].[GetGroupManagers](@name nvarchar(256))
as
begin
	select 
		guest.Name as Name
	from 
		UserRelationship r,
		Users host,
		Users guest
	where 
		r.Relationship=2 and
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.UpperName=UPPER(@name)
end

GO

/****** Object:  StoredProcedure [dbo].[GetImportantComment]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetImportantComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetImportantComment]
GO

CREATE proc [dbo].[GetImportantComment](
	@ReceiverID int
)
as
begin
	if(@ReceiverID <>0)
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel
		from Comment join Users on Comment.[SenderID] = Users.[ID]  
		where ReceiverID = @ReceiverID and IsImportant = 1
		order by CreatedTime desc
	else
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel
		from Comment  join Users on Comment.[SenderID] = Users.[ID] 
		where IsImportant = 1
		order by CreatedTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetMessageList_Group]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessageList_Group]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessageList_Group]
GO

CREATE proc [dbo].[GetMessageList_Group](
	@IsTemp int,
	@PageSize int,
	@CurrentPage int, 
	@PageCount int OUTPUT
)
as
begin
	create table #temp(
		rowid int primary key identity(1,1),
		GroupID int
	);

	insert #temp(GroupID)
	select GroupID
	from MessageList_Group, Users u 
	where GroupID = u.[ID] and u.IsTemp = @IsTemp
	order by LatestTime desc


	declare @count int;
	select @count=count(*) from #temp
	if(@count % @PageSize = 0) set @PageCount = @count/@PageSize 
	else set @PageCount = @count/@PageSize + 1;
	if(@CurrentPage > @PageCount or @CurrentPage < 1) set @CurrentPage = @PageCount;

	declare @scount int;
	if(@CurrentPage=1) set @scount = 0;
	else set @scount = (@CurrentPage - 1) * @PageSize;

	set rowcount @PageSize
	select ml.*, u.Nickname as GroupNickname 
	from  #temp t, MessageList_Group ml, Users u 
	where ml.GroupID = u.[ID] and u.IsTemp = @IsTemp and t.GroupID=ml.GroupID and t.rowid > @scount
	order by LatestTime desc
end

GO

/****** Object:  StoredProcedure [dbo].[GetMessageList_User]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessageList_User]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessageList_User]
GO

CREATE proc [dbo].[GetMessageList_User](
	@User int,
	@Peer int,
	@PageSize int,
	@CurrentPage int, 
	@PageCount int OUTPUT
)
as
begin
	create table #temp(
		rowid int primary key identity(1,1),
		UserID int,
		PeerID int
	);

	if(@User = 0 and @Peer = 0)
	begin
		insert #temp(UserID,PeerID)
		select UserID,PeerID
		from MessageList_User 
		order by LatestTime desc
	end
	else if(@User <> 0 and @Peer = 0)
	begin 
		insert #temp(UserID,PeerID)
		select UserID,PeerID
		from (
			select UserID,PeerID,LatestTime from MessageList_User  where UserID = @User
			union
			select UserID,PeerID,LatestTime from MessageList_User  where PeerID = @User
		)t
		order by LatestTime desc
	end
	else if(@User = 0 and @Peer <> 0)
	begin
		insert #temp(UserID,PeerID)
		select UserID,PeerID
		from (
			select UserID,PeerID,LatestTime from MessageList_User  where UserID = @Peer
			union
			select UserID,PeerID,LatestTime from MessageList_User  where PeerID = @Peer
		)t
		order by LatestTime desc
	end
	else if(@User <> 0 and @Peer <> 0)
	begin
		insert #temp(UserID,PeerID)
		select UserID,PeerID
		from (
			select UserID,PeerID,LatestTime from MessageList_User  where UserID = @User and PeerID = @Peer
			union
			select UserID,PeerID,LatestTime from MessageList_User  where PeerID = @User and UserID = @Peer
		)t
		order by LatestTime desc
	end
	
	
	declare @count int;
	select @count=count(*) from #temp
	if(@count % @PageSize = 0) set @PageCount = @count/@PageSize 
	else set @PageCount = @count/@PageSize + 1;
	if(@CurrentPage > @PageCount or @CurrentPage < 1) set @CurrentPage = @PageCount;

	declare @scount int;
	if(@CurrentPage=1) set @scount = 0;
	else set @scount = (@CurrentPage - 1) * @PageSize;

	set rowcount @PageSize
	select 
		m.*,
		IsNull(u.Nickname,N'已删除用户') as UserNickname,
		IsNull(p.Nickname,N'已删除用户') as PeerNickname
	from (
		#temp t
		left join MessageList_User m on t.UserID = m.UserID and t.PeerID = m.PeerID
		left join Users u on m.UserID = u.[ID] 
		left join Users p on m.PeerID = p.[ID]
	)
	where t.rowid > @scount
	order by LatestTime desc

	drop table #temp
end

GO

/****** Object:  StoredProcedure [dbo].[GetMessageRecordUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessageRecordUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessageRecordUsers]
GO

CREATE proc [dbo].[GetMessageRecordUsers](@id int)
as
begin
	select distinct * 
	from ( 
		select r.[ID] as ID, r.Name, r.Nickname, r.HeadIMG, r.Type, r.IsTemp, r.IsDeleted
		from UserGroupMessage um, Users s, Users r 
		where @id = um.UserID and s.[ID] = um.Sender and r.[ID] = um.Receiver
		
		union all 
		
		select r.[ID] as ID, r.Name, r.Nickname, r.HeadIMG, r.Type, r.IsTemp, r.IsDeleted
		from UserMessage um, Users s, Users r 
		where @id = um.UserID and s.[ID] = um.Sender and r.[ID] = um.Receiver and um.Sender = @id
		
		union all 
		
		select s.[ID] as ID, s.Name, s.Nickname, s.HeadIMG, s.Type, s.IsTemp, s.IsDeleted
		from UserMessage um, Users s, Users r 
		where @id = um.UserID and s.[ID] = um.Sender and r.[ID] = um.Receiver and um.Receiver = @id
			
	)temp
	where temp.ID <> @id
	order by IsDeleted asc
end

GO

/****** Object:  StoredProcedure [dbo].[GetMessages_Group]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessages_Group]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessages_Group]
GO

CREATE proc [dbo].[GetMessages_Group](
	@Group int,
	@From datetime ,
	@To datetime,
	@PageSize int ,
	@CurrentPage int OUTPUT, 
	@PageCount int OUTPUT,
	@MsgID int = 0
)
as
begin
	create table #temp(
		rowid int primary key identity(1,1),
		MsgID int
	)

	insert #temp(MsgID)
	select t.[ID] 
	from(
		select [ID],CreatedTime from IM_Message where Receiver = @Group
	) t
	order by t.CreatedTime asc

	declare @count int;
	select @count=count(*) from #temp;
	if(@count % @PageSize = 0) set @PageCount = @count/@PageSize 
	else set @PageCount = @count/@PageSize + 1;
	if(@CurrentPage > @PageCount or @CurrentPage < 1) set @CurrentPage = @PageCount;
	
	if(@MsgID <> 0)
	begin
		declare @row_index int;
		select @row_index = rowid from #temp where MsgID = @MsgID;
		if(@count % @PageSize = 0) set @CurrentPage = @row_index/@PageSize 
		else set @CurrentPage = @row_index/@PageSize + 1;
	end
	
	declare @SplitCount int;
	if(@CurrentPage=1) set @SplitCount = 0;
	else set @SplitCount = @count - (@PageCount - @CurrentPage + 1) * @PageSize;

	set rowcount @PageSize
	select m.*, 
		s.Name as SenderName, r.Name as ReceiverName, 
		IsNull(s.Nickname,N'已删除用户') as SenderNickname, IsNull(r.Nickname,N'已删除用户') as ReceiverNickname 
	from 
		#temp t
		left join IM_Message m on t.MsgID = m.[ID]
		left join Users s on m.Sender = s.[ID]
		left join Users r on m.Receiver = r.[ID]
	where 
		t.rowid > @SplitCount
	order by CreatedTime asc

	drop table #temp
end

GO
/****** Object:  StoredProcedure [dbo].[GetMessages_User]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessages_User]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessages_User]
GO

CREATE proc [dbo].[GetMessages_User](
	@User int,
	@Peer int,
	@From datetime ,
	@To datetime,
	@PageSize int ,
	@CurrentPage int OUTPUT, 
	@PageCount int OUTPUT,
	@MsgID int = 0
)
as
begin
	create table #temp(
		rowid int primary key identity(1,1),
		MsgID int
	)

	insert #temp(MsgID)
	select t.[ID] 
	from(
		select [ID],CreatedTime from IM_Message where Sender = @User and Receiver = @Peer
		union 
		select [ID],CreatedTime from IM_Message where Sender = @Peer and Receiver = @User
	) t
	order by t.CreatedTime asc

	declare @count int;
	select @count=count(*) from #temp;
	if(@count % @PageSize = 0) set @PageCount = @count/@PageSize 
	else set @PageCount = @count/@PageSize + 1;
	if(@CurrentPage > @PageCount or @CurrentPage < 1) set @CurrentPage = @PageCount;
	
	if(@MsgID <> 0)
	begin
		declare @row_index int;
		select @row_index = rowid from #temp where MsgID = @MsgID;
		if(@count % @PageSize = 0) set @CurrentPage = @row_index/@PageSize 
		else set @CurrentPage = @row_index/@PageSize + 1;
	end

	declare @SplitCount int;
	if(@CurrentPage=1) set @SplitCount = 0;
	else set @SplitCount = @count - (@PageCount - @CurrentPage + 1) * @PageSize;

	set rowcount @PageSize
	select m.*, 
		s.Name as SenderName, r.Name as ReceiverName, 
		IsNull(s.Nickname,N'已删除用户') as SenderNickname, IsNull(r.Nickname,N'已删除用户') as ReceiverNickname 
	from 
		#temp t
		left join IM_Message m on t.MsgID = m.[ID]
		left join Users s on m.Sender = s.[ID]
		left join Users r on m.Receiver = r.[ID]
	where 
		t.rowid > @SplitCount
	order by CreatedTime asc

	drop table #temp
end

GO

/****** Object:  StoredProcedure [dbo].[GetRecvMsgMaxTime]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRecvMsgMaxTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetRecvMsgMaxTime]
GO

CREATE proc [dbo].[GetRecvMsgMaxTime](@id INT)
as
begin
	select LastAccessTime from Users where [ID] =@id;
end

GO

/****** Object:  StoredProcedure [dbo].[GetRelationship]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRelationship]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetRelationship]
GO

create proc [dbo].[GetRelationship](@account1 int, @account2 int)
as
begin
	select *
	from Users host,Users guest,UserRelationship r
	where host.ID=@account1 and guest.ID=@account2 and r.HostID=host.[ID] and r.GuestID=guest.[ID]
end

GO

/****** Object:  StoredProcedure [dbo].[GetServiceEmbedData]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetServiceEmbedData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetServiceEmbedData]
GO

CREATE proc [dbo].[GetServiceEmbedData](@ids nvarchar(4000))
as
begin
	declare @tab_ids table([Index] int, ID int);
	insert into @tab_ids
	select vIndex, convert(int, [Value]) from dbo.SplitString(@ids,',');
	
	select u.[ID] as ID, d.[ID] as DeptID, u.Name as Name, u.Nickname as Nickname, d.Name as DeptName
	from 
		@tab_ids t1 join @tab_ids t2 on t1.[Index] % 2 = 1 and t1.[Index] = t2.[Index] - 1, 
		Users u, Department d, Dept_User du
	where 
		t1.ID = u.[ID] and t2.ID = d.[ID] and u.[ID] = du.UserID and d.[ID] = du.DeptID
end

GO

/****** Object:  StoredProcedure [dbo].[GetUnreadComment]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUnreadComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUnreadComment]
GO

CREATE proc [dbo].[GetUnreadComment](
	@ReceiverID int,
	@Mark int
)
as
begin
	if(@ReceiverID<>0)
	begin
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel
		from Comment join Users on Comment.[SenderID] = Users.[ID] 
		where ReceiverID = @ReceiverID and IsRead = 0
		order by CreatedTime desc
		if(@Mark=1) Update Comment set IsRead = 1 where ReceiverID = @ReceiverID;
	end
	else
	begin
		select Comment.*, Users.Nickname as Name, Users.EMail as Mail, Users.Tel as Tel
		from Comment join Users on Comment.[SenderID] = Users.[ID] 
		where IsRead = 0
		order by CreatedTime desc
	end
end

GO

/****** Object:  StoredProcedure [dbo].[GetUserRoles]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserRoles]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserRoles]
GO

create proc [dbo].[GetUserRoles](@name nvarchar(256))
as
begin
	select r.Name as RoleName 
	from Users u,User_Role ur,Roles r 
	where u.UpperName=upper(@name) and u.[ID]=ur.UserID and ur.RoleID=r.[ID]
end

GO

/****** Object:  StoredProcedure [dbo].[GetVisibleUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetVisibleUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetVisibleUsers]
GO

CREATE proc [dbo].[GetVisibleUsers](@id int)
as
begin
	declare @subType int;
	select @subType = SubType from Users where ID=@id
	
	select 
		guest.ID as ID,
		guest.Name as Name,
		guest.Type as Type,
		r.RenewTime as RenewTime,
		r.Relationship as Relationship
	from 
		UserRelationship r,
		Users host,
		Users guest
	where 
		guest.Type = 1 and
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.ID=@id and
		host.IsDeleted = 0 and guest.IsDeleted = 0
	union all
	select 
		guest.ID as ID,
		guest.Name as Name,
		guest.Type as Type,
		r.RenewTime as RenewTime,
		r.Relationship as Relationship
	from 
		UserRelationship r,
		Users host,
		Users guest
	where 
		guest.Type = 0 and
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.ID=@id and
		host.IsDeleted = 0 and guest.IsDeleted = 0
	union all
	select 
		u.ID as ID,
		u.Name as Name,
		u.Type as Type,
		u.RegisterTime as RenewTime,
		0 as Relationship
	from 
		Users u
	where 
		@subType = 1 and u.Type = 0 and u.IsTemp = 0 and 
		u.IsDeleted = 0 and u.SubType = 1
end

GO

/****** Object:  StoredProcedure [dbo].[GetVisibleUsersDetails]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetVisibleUsersDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetVisibleUsersDetails]
GO

CREATE proc [dbo].[GetVisibleUsersDetails](@name nvarchar(256))
as
begin
	declare @subType int;
	select @subType = SubType from Users where UpperName=upper(@name);
	
	select 
		guest.*
	from 
		UserRelationship r,
		Users host,
		UsersEx guest
	where 
		guest.Type = 1 and
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.UpperName=upper(@name) and
		host.IsDeleted = 0 and guest.IsDeleted = 0
	union all
	select 
		guest.*
	from 
		UserRelationship r,
		Users host,
		UsersEx guest
	where 
		guest.Type = 0 and
		r.HostID=host.[ID] and
		r.GuestID=guest.[ID] and
		host.UpperName=upper(@name) and
		host.IsDeleted = 0 and guest.IsDeleted = 0
	union all
	select 
		u.*
	from 
		UsersEx u
	where 
		@subType = 1 and u.Type = 0 and u.IsTemp = 0 and 
		u.IsDeleted = 0 and u.SubType = 1
end

GO

/****** Object:  StoredProcedure [dbo].[HasUnreadComment]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HasUnreadComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[HasUnreadComment]
GO

create proc [dbo].[HasUnreadComment](@id int)
as
begin
	if(@id<>0)
	begin
		select count(*) from Comment where ReceiverID = @id and  IsRead=0;
	end
	else
	begin
		select count(*) from Comment where IsRead=0;
	end
end

GO

/****** Object:  StoredProcedure [dbo].[MarkAsImportant]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarkAsImportant]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MarkAsImportant]
GO

create proc [dbo].[MarkAsImportant](@Id int)
as
begin
	update comment set isimportant=1 where ID=@Id;
end

GO

/****** Object:  StoredProcedure [dbo].[MarkAsRead]    Script Date: 06/17/2011 *****/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarkAsRead]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MarkAsRead]
GO

create proc [dbo].[MarkAsRead](@Id int)
as
begin
	update comment set isread=1 where ID=@Id;
end

GO

/****** Object:  StoredProcedure [dbo].[MarkAsUnimportant]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarkAsUnimportant]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MarkAsUnimportant]
GO

create proc [dbo].[MarkAsUnimportant](@Id int)
as
begin
	update comment set isimportant=0 where ID=@Id;
end

GO

/****** Object:  StoredProcedure [dbo].[MarkAsUnread]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarkAsUnread]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MarkAsUnread]
GO

create proc [dbo].[MarkAsUnread](@Id int)
as
begin
	update comment set isread=0 where ID=@Id;
end

GO

/****** Object:  StoredProcedure [dbo].[NewComment]    Script Date: 06/17/2011 *****/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewComment]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[NewComment]
GO

CREATE proc [dbo].[NewComment](
	@SenderID int,
	@ReceiverID int,
	@Content text,
	@Tel nvarchar(50),
	@Mail nvarchar(50),
	@Name nvarchar(50)
)
as 
begin
	insert Comment (SenderID,ReceiverID,Content,CreatedTime,IsRead,IsImportant) 
	values (@SenderID,@ReceiverID,@Content,getdate(),0,0);
	
	declare @id int;
	set @id = @@identity;
	
	UPDATE Users SET Nickname=@name, EMail=@Mail, Tel = @Tel where [ID]=@SenderID and IsTemp = 1;
	
	select @id;
end

GO

/****** Object:  StoredProcedure [dbo].[NewMessage]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewMessage]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[NewMessage]
GO

CREATE proc [dbo].[NewMessage](@Receiver int,@Sender int,@Content text,@CreatedTime datetime,@ID int)
as
begin
	insert into Message (Receiver,Sender,Content,CreatedTime,[ID]) 
	values (@Receiver,@Sender,@Content,@CreatedTime,@ID)
	
	declare @receiver_type int;
	select @receiver_type = Type from Users where [ID] = @Receiver
	
	if(@receiver_type = 0)	
	begin
		insert into UserRecvMessage(UserID, MsgID) values (@Sender, @ID)
		insert into UserRecvMessage(UserID, MsgID) values (@Receiver, @ID)
	end
	else
	begin
		insert into UserRecvMessage(UserID, MsgID) 
		select u.[ID] UserID, @ID MsgID
		from UserRelationship r, Users u
		where u.[ID] = r.HostID and r.GuestID = @Receiver
	end
end

GO

/****** Object:  StoredProcedure [dbo].[RemoveFromDept]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RemoveFromDept]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RemoveFromDept]
GO

CREATE proc [dbo].[RemoveFromDept](@userId int, @deptId int)
as
begin
	declare @count int;
	select @count=count(*) from Dept_User where [UserID] = @userId;
	if(@count = 1)
	begin
		declare @name nvarchar(256);
		select @name = [Name] from Users where [ID]=@userId;
		raiserror (N'用户"%s"仅属于当前部门，请直接删除！',16,1,@name);
		return;
	end
	delete from Dept_User where UserID = @userId and DeptID = @deptId;
	delete from Dept_User where UserID = @userId and DeptID in (select [ID] from dbo.GetAllSubDepts(@deptId))
end

GO

/****** Object:  StoredProcedure [dbo].[RemoveFromGroup]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RemoveFromGroup]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RemoveFromGroup]
GO

CREATE proc [dbo].[RemoveFromGroup](@ids image, @groupId int)
as
begin
	declare @tab_ids_temp table(ID int);
	insert into @tab_ids_temp
	select convert(int, [Value]) from dbo.ParseIntArray(@ids);

	delete from UserRelationship where Relationship <> 3 and  HostID in (select ID from @tab_ids_temp) and GuestID = @groupId;
	delete from UserRelationship where Relationship <> 3 and GuestID in (select ID from @tab_ids_temp) and HostID = @groupId;
end

GO

/****** Object:  StoredProcedure [dbo].[ResetPassword]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetPassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetPassword]
GO

create proc [dbo].[ResetPassword](@id int, @password nvarchar(256))
as
begin
	Update Users Set Password = @password where [ID]=@id;
end

GO

/****** Object:  StoredProcedure [dbo].[ResetUserDepts]    Script Date: 06/17/2011 00:50:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetUserDepts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetUserDepts]
GO

CREATE proc [dbo].[ResetUserDepts](@userId int, @depts nvarchar(4000))
as
begin
	declare @tab_ids table(ID int);
	insert into @tab_ids
	select distinct convert(int, [Value]) from dbo.SplitString(@depts,',');

	declare @dept_ids table(ID int);
	insert into @dept_ids
	select * from @tab_ids d_ids
	where not exists (select * from @tab_ids ids, dbo.GetAllSubDepts(d_ids.[ID]) subs where ids.[ID] = subs.[ID])

	delete from Dept_User where UserID = @userId

	insert into Dept_User(DeptID,UserID)
	select ID, @userId from @dept_ids
end

GO

/****** Object:  StoredProcedure [dbo].[GetMessages]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMessages]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMessages]
GO

CREATE proc [dbo].[GetMessages](@user int, @peer int, @from datetime, @to datetime, @page int output, @pagesize int, @pagecount int output, @msgid int, @content nvarchar(1024))
as
begin
	create table #msgs(
		SN INT IDENTITY(1, 1) PRIMARY KEY,
		ID INT
	);
	
	declare @peer_type int;
	set @peer_type = 0;
	select @peer_type = [Type] from Users where ID = @peer;
	
	if(@user = 3)
	begin
		insert into #msgs(ID)
		select m.ID 
		from IM_Message m
		where (@peer = 0 or m.Receiver = @peer or m.Sender = @peer)
			and m.CreatedTime >= @from and m.CreatedTime <= @to
			and m.Content like @content
		order by m.CreatedTime asc
	end
	else if(@content = '')
	begin
		insert into #msgs(ID)
		select [MsgID] 
		from UserRecvMessage urm, IM_Message m
		where [UserID] = @user and urm.MsgID = m.ID and (@peer = 0 or (@peer_type = 1 and m.Receiver = @peer) or (@peer_type = 0 and (m.Sender = @peer or m.Receiver = @peer)))
			and m.CreatedTime >= @from and m.CreatedTime <= @to
		order by m.CreatedTime asc
	end
	else
	begin
		insert into #msgs(ID)
		select [MsgID] 
		from UserRecvMessage urm, IM_Message m
		where [UserID] = @user and urm.MsgID = m.ID and (@peer = 0 or (@peer_type = 1 and m.Receiver = @peer) or (@peer_type = 0 and (m.Sender = @peer or m.Receiver = @peer)))
			and m.CreatedTime >= @from and m.CreatedTime <= @to
			and m.Content like @content
		order by m.CreatedTime asc
	end
	declare @total int;
	select @total = COUNT(ID) from #msgs;
	
	if (@total % @pagesize = 0)
		set @pagecount = @total / @pagesize;
	else 
		set @pagecount = @total / @pagesize + 1;

	if(@msgid <> 0)
	begin
		declare @sn int
		select @sn = SN from #msgs where ID = @msgid;
		if (@@ROWCOUNT > 0)
		begin
			if (@sn % @pagesize = 0)
				set @page = @sn / @pagesize;
			else 
				set @page = @sn / @pagesize + 1;
		end
	end
	
	if(@msgid = 0)
	begin
		if(@page < 1 or @page > @pagecount) set @page = @pagecount;
	end
	
	declare @start int;
	set @start = (@page - 1) * @pagesize;
	
	if(@total > @pagesize and @start + @pagesize > @total) set @start = @total - @pagesize;
	
	set rowcount @pagesize
	select m.[ID], m.Receiver, m.Sender, m.Content, m.CreatedTime
	from #msgs msgs, IM_Message m
	where msgs.ID = m.ID and msgs.SN > @start
end
GO

/****** Object:  StoredProcedure [dbo].[SearchUsers]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SearchUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SearchUsers]
GO

CREATE proc [dbo].[SearchUsers](@keyword nvarchar(4000),@kw_type nvarchar(16))
as
begin
	if(@kw_type = 'MultiID')
	begin
		declare @tab_ids table(ID int);
		insert into @tab_ids
		select distinct convert(int, [Value]) from dbo.SplitString(@keyword,',');

		select u.*, dbo.GetUserDepts(u.[ID]) as DeptName
		from Users u, @tab_ids ids
		where u.Type=0 and u.[ID] = ids.ID and u.IsDeleted = 0;
	end
	else
	begin
		select u.*, dbo.GetUserDepts(u.[ID]) as DeptName
		from Users u
		where (Nickname like N'%'+@keyword+N'%' or u.Name like N'%'+@keyword+N'%') and u.Type=0 and u.[ID] > 3 and u.IsDeleted = 0
	end
end

GO

/****** Object:  StoredProcedure [dbo].[UpdateDeptInfo]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDeptInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateDeptInfo]
GO

create proc [dbo].[UpdateDeptInfo](@id int , @name nvarchar(256))
as
begin
	update Department set Name=@name where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[UpdateEmbedCode]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateEmbedCode]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateEmbedCode]
GO

CREATE proc [dbo].[UpdateEmbedCode](@id int, @Users nvarchar(4000), @config nvarchar(4000))
as
begin
	update EmbedCode set EmbedConfig = @config, Users=@Users where ID = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[Validate]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Validate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Validate]
GO

CREATE proc [dbo].[Validate](@name nvarchar(256), @password nvarchar(256))
as
begin
	select * from Users where UpperName=upper(@name) and Password=@password and IsDeleted = 0
end

GO

/****** Object:  StoredProcedure [dbo].[UpdateLastAccessTime]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLastAccessTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].UpdateLastAccessTime
GO

CREATE proc [dbo].UpdateLastAccessTime(@id int, @last_access_time datetime)
as
begin
	update Users set LastAccessTime = @last_access_time where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[GetLastAccessTime]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastAccessTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].GetLastAccessTime
GO

CREATE proc [dbo].GetLastAccessTime(@id int)
as
begin
	select LastAccessTime from Users where [ID] = @id;
end

GO

/****** Object:  StoredProcedure [dbo].[GetCompanyInfo]    Script Date: 06/17/2011 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].GetCompanyInfo
GO

CREATE proc [dbo].GetCompanyInfo
as
begin
	select * from Department where [ID] = 1;
end

GO
