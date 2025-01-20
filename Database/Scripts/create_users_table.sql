CREATE TABLE users (
    id SERIAL PRIMARY KEY, -- ID do usuário (chave primária)
    email VARCHAR(255) NOT NULL UNIQUE, -- E-mail do usuário (único e obrigatório)
    username VARCHAR(100) NOT NULL UNIQUE, -- Nome de usuário (único e obrigatório)
    password_hash TEXT NOT NULL, -- Hash da senha (obrigatório)
    firstname VARCHAR(100) NOT NULL, -- Primeiro nome
    lastname VARCHAR(100) NOT NULL, -- Último nome
    city VARCHAR(100), -- Cidade do endereço
    street VARCHAR(255), -- Rua do endereço
    address_number INT, -- Número da casa/endereço
    zipcode VARCHAR(20), -- Código postal
    geolocation_lat VARCHAR(50), -- Latitude da localização geográfica
    geolocation_long VARCHAR(50), -- Longitude da localização geográfica
    phone VARCHAR(20), -- Telefone do usuário
    status VARCHAR(20) NOT NULL DEFAULT 'Active', -- Status do usuário (enum: Active, Inactive, Suspended)
    role VARCHAR(20) NOT NULL DEFAULT 'Customer' -- Papel do usuário (enum: Customer, Manager, Admin)
);
