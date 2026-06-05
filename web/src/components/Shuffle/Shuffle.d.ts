import type { ComponentType, CSSProperties } from 'react'

type ShuffleDirection = 'left' | 'right' | 'up' | 'down'
type ShuffleAnimationMode = 'evenodd' | 'random'

declare const Shuffle: ComponentType<{
  text: string
  className?: string
  style?: CSSProperties
  shuffleDirection?: ShuffleDirection
  duration?: number
  maxDelay?: number
  ease?: string
  threshold?: number
  rootMargin?: string
  tag?: keyof React.JSX.IntrinsicElements
  textAlign?: CSSProperties['textAlign']
  onShuffleComplete?: () => void
  shuffleTimes?: number
  animationMode?: ShuffleAnimationMode
  loop?: boolean
  loopDelay?: number
  stagger?: number
  scrambleCharset?: string
  colorFrom?: string
  colorTo?: string
  triggerOnce?: boolean
  respectReducedMotion?: boolean
  triggerOnHover?: boolean
}>

export default Shuffle
