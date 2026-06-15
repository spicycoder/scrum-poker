import { useEffect } from 'react'

export function ClarityScript() {
  useEffect(() => {
    const id = import.meta.env.VITE_CLARITY_ID
    if (!id) return

    const s = document.createElement('script')
    s.innerHTML = `(function(c,l,a,r,i,t,y){c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);})(window,document,"clarity","script","${id}");`
    document.head.appendChild(s)
    return () => s.remove()
  }, [])

  return null
}
