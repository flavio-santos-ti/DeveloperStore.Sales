CREATE TABLE cart_products (
    id SERIAL PRIMARY KEY, -- ID do item no carrinho
    cart_id INT NOT NULL, -- ID do carrinho associado
    product_id INT NOT NULL, -- ID do produto
    quantity INT NOT NULL, -- Quantidade do produto no carrinho
    FOREIGN KEY (cart_id) REFERENCES carts(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);