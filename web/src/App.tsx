import { Routes, Route, Navigate, useParams } from 'react-router-dom'
import Layout from './components/Layout/Layout'
import HomePage from './components/HomePage/HomePage'
import GameRoom from './components/GameRoom'

function JoinPage() {
  const { roomId } = useParams<{ roomId: string }>()
  return <HomePage defaultTab="join" roomId={roomId} />
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Navigate to="/create" replace />} />
        <Route path="/create" element={<HomePage defaultTab="create" />} />
        <Route path="/join" element={<HomePage defaultTab="join" />} />
        <Route path="/join/:roomId" element={<JoinPage />} />
        <Route path="/:roomId" element={<GameRoom />} />
      </Route>
    </Routes>
  )
}

export default App
