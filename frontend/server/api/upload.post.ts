// server/api/upload.post.ts
import { IncomingForm } from 'formidable'
import fs from 'fs'
import path from 'path'

export const config = {
  api: {
    bodyParser: false,
  },
}

export default defineEventHandler(async (event) => {
  const form = new IncomingForm({ uploadDir: 'public/uploads', keepExtensions: true })

  // Убедимся, что папка существует
  const uploadPath = path.resolve('public/uploads')
  if (!fs.existsSync(uploadPath)) fs.mkdirSync(uploadPath, { recursive: true })

  return await new Promise((resolve, reject) => {
    form.parse(event.node.req, (err, fields, files) => {
      if (err) {
        reject({ error: 'Ошибка загрузки файла' })
        return
      }

      const file = files.file?.[0] || files.file
      const filename = path.basename(file.filepath)
      const imagePath = `/uploads/${filename}`

      resolve({ imagePath })
    })
  })
})
