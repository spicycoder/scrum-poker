import { useEffect } from 'react'
import { Routes, Route, Navigate, useParams, useNavigate } from 'react-router-dom'
import Layout from './components/Layout/Layout'
import HomePage from './components/HomePage/HomePage'
import GameRoom from './components/GameRoom'

function JoinPage() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()

  useEffect(() => {
    if (roomId && !/^\d{4}$/.test(roomId)) {
      navigate('/join', { replace: true })
    }
  }, [roomId, navigate])

  return <HomePage roomId={roomId?.match(/^\d{4}$/) ? roomId : undefined} />
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Navigate to="/create" replace />} />
        <Route path="/create" element={<HomePage />} />
        <Route path="/join" element={<HomePage />} />
        <Route path="/join/:roomId" element={<JoinPage />} />
        <Route path="/:roomId" element={<GameRoom />} />
      </Route>
    </Routes>
  )
}

export default App
