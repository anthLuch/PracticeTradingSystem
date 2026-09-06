import { useEffect, useState } from 'react'
import './App.css'
import OrderBook from './components/OrderBook'
import OrderHistory from './components/OrderHistoryRecord'
import TradeHistory from './components/TradeHistoryRecord'
import { type TradeHistoryRecord,type OrderHistoryRecord, type DepthLevel } from './types'
import OrderForm from './components/OrderForm'

function App() {
  const [bids, setBids] = useState<DepthLevel[]>([])

  const [asks, setAsks] = useState<DepthLevel[]>([])

  const [orderHistory, setOrderHistory] = useState<OrderHistoryRecord[]>([])

  const [tradeHistory, setTradeHistory] = useState<TradeHistoryRecord[]>([])

  const [symbolFilter, setSymbolFilter] = useState('')

  const uniqueSymbols = [...new Set(orderHistory.map( o => o.symbol))]

  useEffect(() =>{
    fetch(`http://localhost:5133/api/order/history?symbol=${symbolFilter}`)
        .then(res => res.json())
        .then(data => setOrderHistory(data))
  }, [symbolFilter])

  useEffect(() =>{
    fetch(`http://localhost:5133/api/trade/history?symbol=${symbolFilter}`)
        .then(res => res.json())
        .then(data => setTradeHistory(data))
  }, [symbolFilter])

  const fetchDepth = () => {
    fetch('http://localhost:5133/api/order/depth?side=0&levels=5')
        .then(res => res.json())
        .then(data => setBids(data))

    fetch('http://localhost:5133/api/order/depth?side=1&levels=5')
        .then(res => res.json())
        .then(data => setAsks(data))
    
  }

  useEffect(() => {
    fetchDepth()
  }, [])

  return (
    <div className="app">
      <h1>Order Book</h1>

      <div className="top-row">
        <div className="panel">
          <h2>Book</h2>
          <OrderBook bids={bids} asks={asks} />
        </div>

        <div className="panel submit-panel">
          <h2>Submit Order</h2>
          <OrderForm onOrderSubmitted={fetchDepth} />
        </div>
      </div>

      <div className="panel">
        <div className="filter-row">
          <label htmlFor="symbol-filter">Symbol</label>
          <select id="symbol-filter" value={symbolFilter} onChange={e => setSymbolFilter(e.target.value)}>
            <option value="">All</option>
            {uniqueSymbols.map(symbol => (
              <option key={symbol} value={symbol}>{symbol}</option>
            ))}
          </select>
        </div>

        <h2>Order History</h2>
        <div className="table-scroll">
          <OrderHistory history={orderHistory} />
        </div>

        <h2>Trade History</h2>
        <div className="table-scroll">
          <TradeHistory historys={tradeHistory} />
        </div>
      </div>
    </div>
  )
}

export default App