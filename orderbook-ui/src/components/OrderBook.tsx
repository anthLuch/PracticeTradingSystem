import type {DepthLevel} from '../types'

type OrderBookProps = {
    bids : DepthLevel[]
    asks : DepthLevel[]
}

function OrderBook({bids, asks} : OrderBookProps){
    const rowCount = Math.max(bids.length, asks.length)
    return (
        <div>
            <table className="data-table orderbook">
                <thead>
                    <tr>
                        <th>Bid Price</th>
                        <th>Bid Qty</th>
                        <th>Ask Price</th>
                        <th>Ask Qty</th>
                    </tr>
                </thead>
                <tbody>
                    {Array.from({ length: rowCount }).map((_, index) => (
                        <tr key={index}>
                            <td>{bids[index]?.price}</td>
                            <td>{bids[index]?.quantity}</td>
                            <td>{asks[index]?.price}</td>
                            <td>{asks[index]?.quantity}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default OrderBook