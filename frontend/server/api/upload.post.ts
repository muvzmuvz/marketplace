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
const uploadDir = path.resolve(process.cwd(), 'public/uploads')

  // Убедимся, что папка для загрузки существует
  if (!fs.existsSync(uploadDir)) {
    fs.mkdirSync(uploadDir, { recursive: true })
  }

  const form = new IncomingForm({
    uploadDir,
    keepExtensions: true,
    multiples: false,
  })

  return await new Promise((resolve, reject) => {
    form.parse(event.node.req, (err, fields, files) => {
      if (err) {
        console.error('Ошибка при загрузке файла:', err)
        reject({ error: 'Не удалось загрузить файл' })
        return
      }

      const uploadedFile = files.file?.[0] || files.file

      if (!uploadedFile || !uploadedFile.filepath) {
        reject({ error: 'Файл не найден' })
        return
      }

      const fileName = path.basename(uploadedFile.filepath)
      const publicPath = `uploads/${fileName}`

      resolve({ imagePath: publicPath })
    })
  })
})
