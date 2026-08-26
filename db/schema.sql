-- schema.sql
-- Creates the base tables for TradingSystemDb

CREATE Table Orders (

    OrderId INT PRIMARY KEY,
    Symbol VARCHAR(50) NOT NULL,
    Side VARCHAR(4) NOT NULL CHECK (Side IN ('Buy', 'Sell')),
    OrderType VARCHAR(6) NOT NULL CHECK (OrderType IN ('Limit', 'Market', 'IOC', 'FOK')),
    Price DECIMAL(18,4) NOT NULL,
    OriginalQuantity INT NOT NULL,
    Quantity INT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);

CREATE Table Trades (

    TradeId INT PRIMARY KEY,
    BuyOrderId INT NOT NULL,
    SellOrderId INT NOT NULL,
    Symbol VARCHAR(50) NOT NULL,
    Price DECIMAL(18,4) NOT NULL,
    Quantity INT NOT NULL,
    ExecutedAt DATETIME2 NOT NULL,

    CONSTRAINT FK_Trades_BuyOrder FOREIGN KEY (BuyOrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_Trades_SellOrder FOREIGN KEY (SellOrderId) REFERENCES Orders(OrderId)
);