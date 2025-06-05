// /server/middleware/cors.ts

import { defineEventHandler, setHeader } from 'h3'

export default defineEventHandler((event) => {
    // Разрешаем доступ с любого домена (можно заменить на конкретный домен в продакшене)
    setHeader(event, 'Access-Control-Allow-Origin', 'http://37.252.19.136')

   // setHeader(event, 'Access-Control-Allow-Origin',  'http://localhost')

    // Разрешённые HTTP-методы
    setHeader(event, 'Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')

    // Разрешённые заголовки
    setHeader(event, 'Access-Control-Allow-Headers', 'Content-Type, Authorization')

    // Если это предварительный запрос (preflight), сразу отвечаем без тела
    if (event.node.req.method === 'OPTIONS') {
        event.node.res.statusCode = 204 // No Content
        event.node.res.end()
    }
})
