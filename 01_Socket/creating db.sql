create database CitiesDB

use CitiesDB

create table Cities
(
	Id int primary key identity(1,1),
	Name nvarchar(100) not null check(Name <> ''),
	ZipCode int not null
)

select * from Cities

