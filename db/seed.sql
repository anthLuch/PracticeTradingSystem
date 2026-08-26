-- seed.sql
-- Resets Orders/Trades to a clean, known state and loads sample data
-- to practice SELECT, JOIN, UPDATE/DELETE, and aggregates against.
-- OrderId/TradeId are supplied explicitly (not IDENTITY) since the real
-- app assigns these values itself in C#.

DELETE FROM Trades;
DELETE FROM Orders;

INSERT INTO Orders (OrderId, Symbol, Side, OrderType, Price, OriginalQuantity, Quantity, Status, CreatedAt)
VALUES
    (1,  'AAPL',  'Sell', 'Limit',  101.50,  100, 0,   'Filled',    '2026-08-18 09:31:00'),
    (2,  'AAPL',  'Buy',  'Limit',  101.50,  100, 0,   'Filled',    '2026-08-18 09:31:05'),
    (3,  'AAPL',  'Buy',  'Limit',  102.00,  200, 200, 'Open',      '2026-08-18 10:15:00'),
    (4,  'MSFT',  'Sell', 'Limit',  305.25,  150, 0,   'Filled',    '2026-08-18 11:00:00'),
    (5,  'MSFT',  'Buy',  'Market', 305.25,  150, 0,   'Filled',    '2026-08-18 11:00:02'),
    (6,  'MSFT',  'Buy',  'Limit',  304.00,  100, 100, 'Open',      '2026-08-19 09:05:00'),
    (7,  'TSLA',  'Sell', 'Limit',  245.75,  80,  0,   'Filled',    '2026-08-19 09:40:00'),
    (8,  'TSLA',  'Buy',  'Limit',  246.00,  80,  0,   'Filled',    '2026-08-19 09:40:10'),
    (9,  'TSLA',  'Sell', 'Limit',  247.00,  50,  50,  'Cancelled', '2026-08-19 13:22:00'),
    (10, 'AAPL',  'Sell', 'Limit',  103.00,  60,  60,  'Cancelled', '2026-08-20 10:10:00'),
    (11, 'AAPL',  'Buy',  'Limit',  100.75,  120, 40,  'Open',      '2026-08-20 14:05:00'),
    (12, 'AAPL',  'Sell', 'Limit',  100.75,  80,  0,   'Filled',    '2026-08-20 14:06:00'),
    (13, 'MSFT',  'Sell', 'Limit',  306.50,  90,  0,   'Filled',    '2026-08-20 15:30:00'),
    (14, 'MSFT',  'Buy',  'Limit',  306.50,  90,  0,   'Filled',    '2026-08-21 09:00:00'),
    (15, 'TSLA',  'Buy',  'FOK',    248.00,  200, 200, 'Cancelled', '2026-08-21 09:15:00'),
    (16, 'GOOGL', 'Buy',  'Limit', 2750.00,  10,  10,  'Open',      '2026-08-21 10:00:00'),
    (17, 'GOOGL', 'Sell', 'Limit', 2755.00,  15,  15,  'Open',      '2026-08-21 10:05:00'),
    (18, 'MSFT',  'Sell', 'IOC',   305.00,   40,  0,   'Filled',    '2026-08-19 09:10:00'),
    (19, 'MSFT',  'Buy',  'Limit', 305.00,   40,  0,   'Filled',    '2026-08-19 09:10:01');

INSERT INTO Trades (TradeId, BuyOrderId, SellOrderId, Symbol, Price, Quantity, ExecutedAt)
VALUES
    (1, 2,  1,  'AAPL', 101.50, 100, '2026-08-18 09:31:05'),
    (2, 5,  4,  'MSFT', 305.25, 150, '2026-08-18 11:00:02'),
    (3, 8,  7,  'TSLA', 245.75, 80,  '2026-08-19 09:40:10'),
    (4, 11, 12, 'AAPL', 100.75, 80,  '2026-08-20 14:06:00'),
    (5, 14, 13, 'MSFT', 306.50, 90,  '2026-08-21 09:00:00'),
    (6, 19, 18, 'MSFT', 305.00, 40,  '2026-08-19 09:10:01');
