import { Routes, Route, useLocation, useParams } from 'react-router-dom'
import Layout from './components/Layout/Layout'
import HomePage from './components/HomePage/HomePage'
import GameRoom from './components/GameRoom'

function HomeRoute() {
  const location = useLocation()
  return <HomePage key={location.key} />
}

function JoinRoute() {
  const location = useLocation()
  const { roomId } = useParams<{ roomId: string }>()
  return <HomePage key={location.key} defaultTab="join" roomId={roomId} />
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomeRoute />} />
        <Route path="/:roomId" element={<JoinRoute />} />
        <Route path="/room/:roomId" element={<GameRoom />} />
      </Route>
    </Routes>
  )
}

export default App
