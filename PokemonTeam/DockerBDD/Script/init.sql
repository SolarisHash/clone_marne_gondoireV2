-- 🧱 CRÉATION DES TABLES
CREATE DATABASE PokemonDb;
GO
USE PokemonDb;
GO

CREATE TABLE user_auth (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email VARCHAR(250) NOT NULL UNIQUE,
    password VARCHAR(128) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE player (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(25) NOT NULL,
    pokedollar INT DEFAULT 0,
    experience INT DEFAULT 0,
    fk_user_auth INT NOT NULL,
    game VARCHAR(10),
    CONSTRAINT fk_user FOREIGN KEY(fk_user_auth) REFERENCES user_auth(id) ON DELETE CASCADE
);
GO
CREATE TABLE pokemon (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    health_point SMALLINT NOT NULL,
    strength SMALLINT NOT NULL,
    defense SMALLINT NOT NULL,
    speed SMALLINT NOT NULL,
    max_health_point SMALLINT NOT NULL,
    unlocked_experience INT DEFAULT 0,
    fk_evolution INT NULL
);
GO
-- 🔁 Ajout de la contrainte auto-référencée avec NO ACTION (pour éviter cycles/cascade multiples)
ALTER TABLE pokemon
ADD CONSTRAINT fk_evolution FOREIGN KEY(fk_evolution) REFERENCES pokemon(id) ON DELETE NO ACTION;
GO
CREATE TABLE type (
    id INT IDENTITY(1,1) PRIMARY KEY,
    typeName VARCHAR(10) NOT NULL UNIQUE,
    fire DECIMAL(3,2) DEFAULT 1.00,
    water DECIMAL(3,2) DEFAULT 1.00,
    grass DECIMAL(3,2) DEFAULT 1.00,
    electric DECIMAL(3,2) DEFAULT 1.00,
    ice DECIMAL(3,2) DEFAULT 1.00,
    fighting DECIMAL(3,2) DEFAULT 1.00,
    poison DECIMAL(3,2) DEFAULT 1.00,
    ground DECIMAL(3,2) DEFAULT 1.00,
    flying DECIMAL(3,2) DEFAULT 1.00,
    psychic DECIMAL(3,2) DEFAULT 1.00,
    bug DECIMAL(3,2) DEFAULT 1.00,
    rock DECIMAL(3,2) DEFAULT 1.00,
    ghost DECIMAL(3,2) DEFAULT 1.00,
    dragon DECIMAL(3,2) DEFAULT 1.00,
    dark DECIMAL(3,2) DEFAULT 1.00,
    steel DECIMAL(3,2) DEFAULT 1.00,
    fairy DECIMAL(3,2) DEFAULT 1.00
);
GO
CREATE TABLE skill (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    damage SMALLINT NOT NULL,
    accuracy SMALLINT NOT NULL,
    power_point SMALLINT NOT NULL,
    fk_type INT NOT NULL,
    CONSTRAINT fk_skill_type FOREIGN KEY(fk_type) REFERENCES type(id) ON DELETE CASCADE
);
GO
CREATE TABLE object (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    price SMALLINT NOT NULL CHECK (price >= 0)
);
GO
CREATE TABLE log (
    id INT IDENTITY(1,1) PRIMARY KEY,
    message TEXT NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);
GO
CREATE TABLE player_pokemon (
    fk_player INT NOT NULL,
    fk_pokemon INT NOT NULL,
    PRIMARY KEY(fk_player, fk_pokemon),
    CONSTRAINT fk_player FOREIGN KEY(fk_player) REFERENCES player(id) ON DELETE CASCADE,
    CONSTRAINT fk_pokemon FOREIGN KEY(fk_pokemon) REFERENCES pokemon(id) ON DELETE CASCADE
);
GO
CREATE TABLE pokemon_skill (
    fk_pokemon INT NOT NULL,
    fk_skill INT NOT NULL,
    PRIMARY KEY(fk_pokemon, fk_skill),
    CONSTRAINT fk_pokemon_skill FOREIGN KEY(fk_pokemon) REFERENCES pokemon(id) ON DELETE CASCADE,
    CONSTRAINT fk_skill FOREIGN KEY(fk_skill) REFERENCES skill(id) ON DELETE CASCADE
);
GO
CREATE TABLE pokemon_type (
    fk_pokemon INT NOT NULL,
    fk_type INT NOT NULL,
    PRIMARY KEY(fk_pokemon, fk_type),
    CONSTRAINT fk_pokemon_type FOREIGN KEY(fk_pokemon) REFERENCES pokemon(id) ON DELETE CASCADE,
    CONSTRAINT fk_type FOREIGN KEY(fk_type) REFERENCES type(id) ON DELETE CASCADE
);
GO
CREATE TABLE player_object (
    fk_player INT NOT NULL,
    fk_object INT NOT NULL,
    PRIMARY KEY(fk_player, fk_object),
    CONSTRAINT fk_player_object FOREIGN KEY(fk_player) REFERENCES player(id) ON DELETE CASCADE,
    CONSTRAINT fk_object FOREIGN KEY(fk_object) REFERENCES object(id) ON DELETE CASCADE
);
GO
