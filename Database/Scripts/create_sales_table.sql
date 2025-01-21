CREATE TABLE sales (
    id SERIAL PRIMARY KEY,
    sale_number VARCHAR(50) NOT NULL,
    sale_date TIMESTAMP NOT NULL,
    customer_id INT NOT NULL,
    branch VARCHAR(100),
    total_amount NUMERIC(10, 2) NOT NULL,
    is_cancelled BOOLEAN DEFAULT FALSE,
    CONSTRAINT fk_customer FOREIGN KEY (customer_id) REFERENCES users (id)
);
