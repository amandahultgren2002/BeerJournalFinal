CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    zip_code INTEGER,
    password_hash TEXT NOT NULL DEFAULT ''
);

CREATE TABLE beers (
    beer_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    brand TEXT,
    alcohol_pct NUMERIC,
    category TEXT
);

CREATE TABLE tasting_entries (
    entry_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    beer_id INTEGER NOT NULL,
    rating INTEGER,
    location VARCHAR(255),
    testing_date DATE,
    price NUMERIC,
    notes VARCHAR(1000),
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    CONSTRAINT fk_tasting_entries_user
        FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_tasting_entries_beer
        FOREIGN KEY (beer_id) REFERENCES beers(beer_id) ON DELETE RESTRICT
);

INSERT INTO beers (beer_id, name, brand, alcohol_pct, category) VALUES
(1, 'Tuborg Grøn', 'Tuborg', 4.6, 'Pilsner'),
(2, 'Carlsberg Pilsner', 'Carlsberg', 5.0, 'Pilsner'),
(3, 'Guinness Draught', 'Guinness', 4.2, 'Stout'),
(4, 'Pilsner Urquell', 'Plzeňský Prazdroj', 4.4, 'Pilsner'),
(5, 'Sierra Nevada Pale Ale', 'Sierra Nevada', 5.6, 'Pale Ale'),
(6, 'Duvel', 'Duvel Moortgat', 8.5, 'Belgian Strong Ale'),
(7, 'Hefeweissbier', 'Weihenstephaner', 5.4, 'Wheat'),
(8, 'Chimay Blue', 'Chimay', 9.0, 'Belgian Strong Ale'),
(9, 'Punk IPA', 'BrewDog', 5.4, 'IPA'),
(10, 'Beer Geek Breakfast', 'Mikkeller', 7.5, 'Stout'),
(11, '1664 Blanc', 'Kronenbourg', 5.0, 'Wheat'),
(12, 'Erdinger Weissbier', 'Erdinger', 5.3, 'Wheat'),
(13, 'Heineken', 'Heineken', 5.0, 'Lager'),
(14, 'Stella Artois', 'Stella Artois', 5.0, 'Pilsner'),
(15, 'Corona Extra', 'Corona', 4.5, 'Lager');

SELECT setval('beers_beer_id_seq', 15, true);