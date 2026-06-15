import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { TooltipProvider } from '@/components/ui/tooltip'
import { ThemeProvider } from '@/contexts/ThemeContext'
import { ClarityScript } from '@/components/ClarityScript'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <TooltipProvider>
        <ThemeProvider>
          <ClarityScript />
          <App />
        </ThemeProvider>
      </TooltipProvider>
    </BrowserRouter>
  </StrictMode>,
)
