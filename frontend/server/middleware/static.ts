import { createReadStream, existsSync } from 'fs'
import { defineEventHandler, getRequestURL, sendStream } from 'h3'
import { join } from 'path'

export default defineEventHandler((event) => {
  const url = getRequestURL(event)

  if (url.pathname.startsWith('/uploads/')) {
    const filePath = join(process.cwd(), 'public', url.pathname)

    if (existsSync(filePath)) {
      return sendStream(event, createReadStream(filePath))
    }
  }
})