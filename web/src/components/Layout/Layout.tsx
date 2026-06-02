import { Outlet } from 'react-router-dom'
import DotField from '../DotField/DotField'
import Navbar from '../Navbar/Navbar'
import { Toaster } from '../ui/sonner'

export default function Layout() {
  return (
    <div className="relative min-h-screen">
      <div className="fixed inset-0 -z-10">
        <DotField
          dotRadius={1.5}
          dotSpacing={14}
          bulgeStrength={67}
          glowRadius={160}
          sparkle={false}
          waveAmplitude={0}
          cursorRadius={500}
          cursorForce={0.1}
          bulgeOnly
          gradientFrom="#A855F7"
          gradientTo="#B497CF"
          glowColor="#120F17"
        />
      </div>
      <Navbar />
      <main className="flex flex-col items-center pt-20 px-4">
        <Outlet />
      </main>
      <Toaster position="top-center" className="z-50" />
    </div>
  )
}
