CREATE TABLE clientes (
    id VARCHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    lastname VARCHAR(255) NOT NULL
);

CREATE INDEX idx_name_lastname ON clientes(name, lastname);

CREATE INDEX idx_roupas_name ON roupas(name);


CREATE TABLE roupas (
    clienteId VARCHAR(36) NOT NULL,
    name VARCHAR(255) NOT NULL,
    FOREIGN KEY (clienteId) REFERENCES clientes(id)
);

