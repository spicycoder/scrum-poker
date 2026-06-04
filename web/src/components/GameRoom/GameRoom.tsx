import { useParams } from 'react-router-dom'

export default function GameRoom() {
  const { roomId } = useParams<{ roomId: string }>()

  return (
    <div className="flex justify-center px-6 pt-8">
      <p className="text-muted-foreground">Room {roomId}</p>
    </div>
  )
}
