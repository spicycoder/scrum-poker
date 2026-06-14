import { Monitor, Moon, Sun } from 'lucide-react'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '../ui/tooltip'
import { useTheme, type Theme } from '../../contexts/ThemeContext'

const themes = [
  { value: 'system' as Theme, label: 'System', icon: Monitor },
  { value: 'dark' as Theme, label: 'Dark', icon: Moon },
  { value: 'light' as Theme, label: 'Light', icon: Sun },
]

export default function ThemeToggle() {
  const { selectedTheme, setTheme } = useTheme()

  const current = themes.find((t) => t.value === selectedTheme)

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <ToggleGroup
          type="single"
          value={selectedTheme}
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
