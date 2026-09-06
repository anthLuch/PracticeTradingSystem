import { useState } from 'react'

type OrderFormProps = {
    onOrderSubmitted: () => void
}

function OrderForm({onOrderSubmitted}: OrderFormProps) {
    const [side, setSide] = useState(0)
    const [symbol, setSymbol] = useState<string | undefined>(undefined)
    const [orderType, setOrderType] = useState(0)
    const [price, setPrice] = useState<number | undefined>(undefined)
    const [quantity, setQuantity] = useState<number | undefined>(undefined)

     const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
    
        await fetch('http://localhost:5133/api/order', {
          method: 'POST',
          headers:{'Content-Type' : 'application/json'},
          body: JSON.stringify({
            side,
            symbol,
            orderType,
            price,
            originalQuantity: quantity
          })
        })
        onOrderSubmitted()
      }

    return(
        <form className="order-form" onSubmit={handleSubmit}>
            <select value={side} onChange={e => setSide(Number(e.target.value))}>
                <option value={0}>Buy</option>
                <option value={1}>Sell</option>
            </select>

            <input 
            type="text"
            value={symbol ?? ''}
            onChange={e => setSymbol(String(e.target.value))}
            placeholder='Symbol' />

            <select value={orderType} onChange={e => setOrderType(Number(e.target.value))}>
                <option value={0}>Limit</option>
                <option value={1}>Market</option>
            </select>

            <input
            type='number'
            value={price ?? ''}
            onChange={e => setPrice(Number(e.target.value))}
            placeholder='Price'
            />

            <input
            type='number'
            value={quantity ?? ''}
            onChange={e => setQuantity(Number(e.target.value))}
            placeholder='Quantity'
            />

            <button type='submit'>Submit Order</button>
        </form>
    )

}

export default OrderForm