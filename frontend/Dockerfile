FROM node:20 AS builder
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM node:20
WORKDIR /app

# Копируем основные артефакты сборки
COPY --from=builder /app/.output /app/.output

# Копируем дополнительные ресурсы (при необходимости)
COPY --from=builder /app/public /app/public
COPY --from=builder /app/server /app/server

EXPOSE 3000
CMD ["node", ".output/server/index.mjs"]