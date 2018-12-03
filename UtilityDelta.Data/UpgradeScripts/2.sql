﻿CREATE TABLE Tags ( 
Id INTEGER NOT NULL CONSTRAINT PK_Kids PRIMARY KEY AUTOINCREMENT, 
Name TEXT NOT NULL,  
BlogId INTEGER NOT NULL, 
CONSTRAINT FK_Tags_Blogs_BlogId FOREIGN KEY (BlogId) REFERENCES Blogs (Id) );

CREATE INDEX IX_Tags_BlogId ON Tags (BlogId)