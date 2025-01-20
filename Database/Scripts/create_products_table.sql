CREATE TABLE products (
    id SERIAL PRIMARY KEY, -- ID do produto (chave primária)
    title VARCHAR(255) NOT NULL, -- Nome/título do produto
    price NUMERIC(10, 2) NOT NULL, -- Preço do produto com até 2 casas decimais
    description TEXT, -- Descrição detalhada do produto
    category VARCHAR(100), -- Categoria do produto
    image VARCHAR(255), -- URL da imagem do produto
    rating_rate NUMERIC(3, 2), -- Nota de avaliação (ex.: 4.5)
    rating_count INT -- Número de avaliações recebidas
);
