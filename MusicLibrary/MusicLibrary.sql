DROP DATABASE IF EXISTS MusicLibrary;
CREATE DATABASE MusicLibrary;

GO

USE MusicLibrary;

DROP TABLE IF EXISTS artist;
CREATE TABLE artist 
(
  id int NOT NULL IDENTITY(1,1),
  name varchar(150) NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  PRIMARY KEY (id)
)

DROP TABLE IF EXISTS genre;
CREATE TABLE genre 
(
  id int NOT NULL IDENTITY(1,1),
  name varchar(150) NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  PRIMARY KEY (id)
)

DROP TABLE IF EXISTS album;
CREATE TABLE album 
(
  id int NOT NULL IDENTITY(1,1),
  title varchar(150) NOT NULL,
  artist_id int NULL,
  year int NULL,
  genre_id int NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  FOREIGN KEY (artist_id) REFERENCES artist(id),
  FOREIGN KEY (genre_id) REFERENCES genre(id)
)

DROP TABLE IF EXISTS path;
CREATE TABLE path 
(
  id int NOT NULL IDENTITY(1,1),
  location varchar(256) NOT NULL,
  last_scan_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
)

DROP TABLE IF EXISTS track_file;
CREATE TABLE track_file
(
  id INT NOT NULL IDENTITY(1,1),
  name VARCHAR(256) NOT NULL,
  track_id INT NOT NULL,
  data VARBINARY(MAX) NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
)

DROP TABLE IF EXISTS track;
CREATE TABLE track 
(
  id int NOT NULL IDENTITY(1,1),
  file_name varchar(256) NOT NULL,
  path_id int NULL,
  file_id INT NULL,
  title varchar(150) NOT NULL,
  album_id int NULL,
  genre_id int NULL,
  artist_id int NULL,
  position int NULL,
  year int NULL,
  duration decimal NOT NULL,
  play_count int NOT NULL DEFAULT 0,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  FOREIGN KEY (path_id) REFERENCES path(id),
  FOREIGN KEY (album_id) REFERENCES album(id),
  FOREIGN KEY (artist_id) REFERENCES artist(id),
  FOREIGN KEY (file_id) REFERENCES track_file(id)
)

DROP TABLE IF EXISTS playlist;
CREATE TABLE playlist
(
  id int NOT NULL IDENTITY(1,1),
  name varchar(150) NOT NULL,
  track_ids varchar(max) NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
) 

DROP TABLE IF EXISTS podcast;
CREATE TABLE podcast
(
  id int NOT NULL IDENTITY(1,1),
  title varchar(150) NOT NULL,
  url varchar(MAX) NOT NULL,
  content varchar(MAX) NOT NULL,
  last_update_date DATETIME NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
)

DROP TABLE IF EXISTS _transaction;
CREATE TABLE _transaction
(
  id INT NOT NULL IDENTITY(1,1),
  status INT NOT NULL,
  status_message TEXT NULL,
  error_message TEXT NULL,
  type INT NOT NULL,
  create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  modify_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
)