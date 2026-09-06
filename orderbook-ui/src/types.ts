export type Order = {
    id: number
    orginalQauntity: number
    price: number
    orderType: 'Limit' | 'Market'
    side: 'Buy' | 'Sell'
}

export type Trade = {
    tradeId: number
    buyOrderId : number
    sellOrderId: number
    price: number
    quantity: number
}

export type DepthLevel = {
    price: number
    quantity: number
}

export type OrderRequest = {
    side: number
    symbol: string
    orginalQauntity: number
    price: number
    orderType: number
}

export type OrderHistoryRecord = {
    id: number
    symbol: string
    side: "Buy" | "Sell"
    orderType: "Limit" | "Market" | "IOC" | "FOK"
    price: number
    originalQuantity: number
    quantity: number
    status: string
    createdAt : string
}

export type TradeHistoryRecord = {
    tradeId : number,
    buyOrderId: number,
    sellOrderId: number,
    symbol: string,
    price: number,
    quantity: number,
    executedAt: string
}