import { useState, useEffect } from 'react'
import { Monitor, Moon, Sun } from 'lucide-react'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '../ui/tooltip'

type Theme = 'system' | 'dark' | 'light'

const themes = [
  { value: 'system' as Theme, label: 'System', icon: Monitor },
  { value: 'dark' as Theme, label: 'Dark', icon: Moon },
  { value: 'light' as Theme, label: 'Light', icon: Sun },
]

function getStoredTheme(): Theme {
  const stored = localStorage.getItem('theme')
  if (stored === 'dark' || stored === 'light') return stored
  return 'system'
}

function applyTheme(theme: Theme) {
  const isDark =
    theme === 'dark' ||
    (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)
  document.documentElement.classList.toggle('dark', isDark)
}

export default function ThemeToggle() {
  const [theme, setTheme] = useState<Theme>(getStoredTheme)

  useEffect(() => {
    applyTheme(theme)
    localStorage.setItem('theme', theme)
  }, [theme])

  useEffect(() => {
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = () => {
      if (theme === 'system') applyTheme('system')
    }
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [theme])

  const current = themes.find((t) => t.value === theme)

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <ToggleGroup
          type="single"
          value={theme}
          onValueChange={(value) => {
            if (value) setTheme(value as Theme)
          }}
          size="sm"
        >
          {themes.map(({ value, label, icon: Icon }) => (
            <ToggleGroupItem key={value} value={value} aria-label={label}>
              <Icon className="h-4 w-4" />
            </ToggleGroupItem>
          ))}
        </ToggleGroup>
      </TooltipTrigger>
      <TooltipContent>
        <p>Theme: {current?.label}</p>
      </TooltipContent>
    </Tooltip>
  )
}
