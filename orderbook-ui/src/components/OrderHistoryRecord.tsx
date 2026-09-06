import type { OrderHistoryRecord } from "../types";

type OrderHistoryRecordProp = {
    history : OrderHistoryRecord[]
}


function OrderHistory({history}: OrderHistoryRecordProp)
{
    return (
        <table className="data-table">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Side</th>
                    <th>Symbol</th>
                    <th>OrderType</th>
                    <th>Price</th>
                    <th>OrginalQuantity</th>
                    <th>Quantity</th>
                    <th>Status</th>
                    <th>CreatedAt</th>
                </tr>
            </thead>
            <tbody>
                {history.map((record) => (
                    <tr key={record.id}>
                        <td>{record.id}</td>
                        <td className={`side-${record.side.toLowerCase()}`}>{record.side}</td>
                        <td>{record.symbol}</td>
                        <td>{record.orderType}</td>
                        <td>{record.price}</td>
                        <td>{record.originalQuantity}</td>
                        <td>{record.quantity}</td>
                        <td className={`status-${record.status.toLowerCase()}`}>{record.status}</td>
                        <td>{record.createdAt}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    )
}

export default OrderHistory