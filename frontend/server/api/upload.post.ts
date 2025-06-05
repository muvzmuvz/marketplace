// server/api/upload.post.ts

import { IncomingForm } from 'formidable'
import fs from 'fs'
import path from 'path'
import sharp from 'sharp'
import { v4 as uuidv4 } from 'uuid'

export const config = {
  api: {
    bodyParser: false,
  },
}

export default defineEventHandler(async (event) => {
  const uploadDir = path.resolve(process.cwd(), 'public/uploads')

  // Убедимся, что папка существует
  if (!fs.existsSync(uploadDir)) {
    fs.mkdirSync(uploadDir, { recursive: true })
  }

  const form = new IncomingForm({
    uploadDir,
    keepExtensions: true,
    multiples: false,
  })

  return await new Promise((resolve, reject) => {
    form.parse(event.node.req, async (err, fields, files) => {
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

      try {
        // Создаём уникальное имя
        const uniqueName = `${uuidv4()}.webp`
        const finalPath = path.join(uploadDir, uniqueName)

        // Сжимаем изображение
        await sharp(uploadedFile.filepath)
          .resize(864) // уменьшаем до 864px по ширине
          .webp({ quality: 60 }) // сохраняем как .webp
          .toFile(finalPath)

        // Удаляем оригинальный временный файл
        fs.unlinkSync(uploadedFile.filepath)

        // Возвращаем путь, по которому файл будет доступен
        resolve({ imagePath: `/uploads/${uniqueName}` })
      } catch (processingError) {
        console.error('Ошибка при обработке изображения:', processingError)
        reject({ error: 'Не удалось обработать изображение' })
      }
    })
  })
})
