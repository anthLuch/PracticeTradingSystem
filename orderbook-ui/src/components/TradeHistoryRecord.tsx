import type {TradeHistoryRecord} from "../types"

type TradeHistoryRecordProp = {
    historys : TradeHistoryRecord[]
}

function TradeHistory({historys} : TradeHistoryRecordProp)
{
    return(
        <div>
            <table className="data-table">
                <thead>
                    <tr>
                        <th>TradeId</th>
                        <th>BuyOrderId</th>
                        <th>SellOrderId</th>
                        <th>Symbol</th>
                        <th>Price</th>
                        <th>Quantity</th>
                        <th>ExecutedAt</th>
                    </tr>
                </thead>
                <tbody>
                    {historys.map((record) => (
                        <tr key={record.tradeId}>
                            <td>{record.tradeId}</td>
                            <td>{record.buyOrderId}</td>
                            <td>{record.sellOrderId}</td>
                            <td>{record.symbol}</td>
                            <td>{record.price}</td>
                            <td>{record.quantity}</td>
                            <td>{record.executedAt}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}

export default TradeHistory