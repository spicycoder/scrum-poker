import { Routes, Route, useLocation, useParams } from 'react-router-dom'
import Layout from './components/Layout/Layout'
import HomePage from './components/HomePage/HomePage'
import GameRoom from './components/GameRoom'

function HomeRoute() {
  const location = useLocation()
  return <HomePage key={location.key} />
}

function JoinOrGameRoute() {
  const location = useLocation()
  const { roomId } = useParams<{ roomId: string }>()
  const storedName = localStorage.getItem('playerName')

  if (storedName && roomId && /^\d{4}$/.test(roomId)) {
    return <GameRoom key={location.key} />
  }

  return <HomePage key={location.key} defaultTab="join" roomId={roomId} />
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomeRoute />} />
        <Route path="/:roomId" element={<JoinOrGameRoute />} />
      </Route>
    </Routes>
  )
}

export default App
