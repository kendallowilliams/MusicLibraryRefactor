﻿CREATE TABLE [dbo].[Track] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [FileName]   VARCHAR (256) NOT NULL,
    [PathId]     INT           NULL,
    [Title]       VARCHAR (150) NOT NULL,
    [AlbumId]    INT           NULL,
    [GenreId]    INT           NULL,
    [ArtistId]   INT           NULL,
    [Position]    INT           NULL,
    [Year]        INT           NULL,
    [Duration]    DECIMAL (18)  NOT NULL,
    [PlayCount]  INT           DEFAULT ((0)) NOT NULL,
    [CreateDate] DATETIME2      DEFAULT (getdate()) NOT NULL,
    [ModifyDate] DATETIME2      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([Id] ASC),
    FOREIGN KEY ([AlbumId]) REFERENCES [dbo].[album] ([Id]),
    FOREIGN KEY ([ArtistId]) REFERENCES [dbo].[artist] ([Id]),
    FOREIGN KEY ([PathId]) REFERENCES [dbo].[TrackPath] ([Id])
);
