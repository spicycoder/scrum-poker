import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { HashRouter } from 'react-router-dom'
import { TooltipProvider } from '@/components/ui/tooltip'
import { ThemeProvider } from '@/contexts/ThemeContext'
import { ClarityScript } from '@/components/ClarityScript'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <HashRouter>
      <TooltipProvider>
        <ThemeProvider>
          <ClarityScript />
          <App />
        </ThemeProvider>
      </TooltipProvider>
    </HashRouter>
  </StrictMode>,
)
