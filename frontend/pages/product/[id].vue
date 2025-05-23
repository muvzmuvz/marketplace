<template>
  <NavMenu/>
  <div class="product-container">
    <button class="back-button" @click="goBack">
      <ArrowLeftIcon class="icon" />
      Назад
    </button>

    <div v-if="isLoading" class="loading-container">
      <p>Загрузка...</p>
    </div>

    <div v-else-if="product">
      <div class="gallery-info-grid">
        <!-- Основное изображение -->
        <div class="main-image-wrapper">
          <img
            :src="getImageUrl(product.images[activeIndex]?.path)"
            :alt="`Изображение товара ${activeIndex + 1}`"
            class="main-image"
            @error="handleImageError"
          />
        </div>

        <!-- Информация о товаре -->
        <div class="product-info">
          <h1 class="product-title">{{ product.name }}</h1>
          <div class="price-section">
            <span class="price">{{ formattedPrice }}</span>
          </div>
          <div class="details-section">
            <h2>Описание</h2>
            <p class="description">{{ product.description }}</p>
          </div>
          <div class="characteristics-section">
            <h2>Характеристики</h2>
            <pre class="characteristics">{{ product.characteristic }}</pre>
          </div>
          <div class="actions-section">
            <Button class="add-to-cart" @click="addToCart">
              <ShoppingCartIcon class="icon" />
              Добавить в корзину
            </Button>
          </div>
        </div>
      </div>

      <!-- Миниатюры -->
      <div class="thumbnails-grid">
        <div
          v-for="(image, index) in product.images"
          :key="image.id || index"
          :class="['thumb-wrapper', { active: index === activeIndex }]"
          @click="activeIndex = index"
        >
          <img
            :src="getImageUrl(image.path)"
            :alt="`Миниатюра ${index + 1}`"
            class="thumb-image"
            @error="handleImageError"
          />
        </div>
      </div>

      <!-- Отзывы -->
      <div class="reviews-section">
        <h2>Отзывы ({{ comments.length }})</h2>
        <div class="new-review">
          <textarea
            v-model="newComment"
            placeholder="Оставьте ваш отзыв..."
            class="review-input"
          ></textarea>
          <Button @click="postComment">Отправить отзыв</Button>
        </div>

        <div class="reviews-list">
          <div
            v-for="comment in comments"
            :key="comment.id"
            class="review-card"
          >
            <div class="review-header">
              <span class="user">Пользователь #{{ comment.userId }}</span>
              <span class="date">{{ formatDate(comment.dateCreated) }}</span>
            </div>
            <p class="review-text">{{ comment.description }}</p>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="not-found">
      <h2>Товар не найден</h2>
      <button @click="goBack" class="back-button">
        <ArrowLeftIcon class="icon" />
        Вернуться назад
      </button>
    </div>
  </div>
</template>

<script setup>
import '@/assets/css/productPage.css'
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeftIcon,
  ShoppingCartIcon
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const productId = route.params.id

const product = ref(null)
const isLoading = ref(true)
const comments = ref([])
const newComment = ref('')
const activeIndex = ref(0)

const fetchProduct = async () => {
  try {
    const response = await fetch(`http://localhost:8080/product/product/${productId}`)
    if (!response.ok) throw new Error('Товар не найден')
    const data = await response.json()
    product.value = {
      ...data,
      images: data.images && data.images.length > 0 ? data.images : []
    }
  } catch (error) {
    console.error('Ошибка загрузки товара:', error)
  } finally {
    isLoading.value = false
  }
}

const fetchComments = async () => {
  try {
    const response = await fetch(`http://localhost:8080/review/reviews_product/${productId}`)
    if (!response.ok) throw new Error('Ошибка загрузки отзывов')
    comments.value = await response.json()
  } catch (error) {
    console.error('Ошибка загрузки отзывов:', error)
  }
}

const postComment = async () => {
  if (!newComment.value.trim()) {
    alert('Отзыв не может быть пустым')
    return
  }
  try {
    const response = await fetch('http://localhost:8080/review/create_review', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({
        productId: Number(productId),
        description: newComment.value
      })
    })
    if (!response.ok) throw new Error('Ошибка отправки отзыва')
    newComment.value = ''
    await fetchComments()
  } catch (error) {
    console.error('Ошибка:', error)
    alert('Не удалось отправить отзыв')
  }
}

const addToCart = async () => {
  try {
    const response = await fetch(
      `http://localhost:8080/cart/add_pdouct_cart/${productId}`,
      {
        method: 'POST',
        credentials: 'include'
      }
    )
    if (!response.ok) throw new Error('Ошибка добавления в корзину')
    alert('Товар добавлен в корзину')
  } catch (error) {
    console.error('Ошибка:', error)
    alert('Не удалось добавить товар в корзину')
  }
}

const getImageUrl = (path) => {
  if (!path) return '/placeholder-image.jpg'
  if (path.startsWith('http')) return path
  return `http://localhost:3000${path}`
}

const handleImageError = (event) => {
  event.target.src = '/placeholder-image.jpg'
  event.target.style.objectFit = 'cover'
}

const formattedPrice = computed(() =>
  new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    maximumFractionDigits: 0
  }).format(product.value?.price || 0)
)

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric'
  })
}

onMounted(async () => {
  await fetchProduct()
  await fetchComments()
})

const goBack = () => {
  router.go(-1)
}
</script>

<style scoped>
.product-container {
  max-width: 70%;
  margin: 0 auto;
  margin-bottom: 50px;
  padding: 1rem;
  font-family: Arial, sans-serif;
  gap: 15px;
  @media (max-width:1024px) {
    max-width: 100%;
  }
}

/* Кнопка назад */
.back-button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  background: none;
  border: none;
  cursor: pointer;
  color: #666;
  font-weight: 500;
}
.back-button:hover {
  color: #333;
}
.icon {
  width: 20px;
  height: 20px;
}

/* Сетка для основной картинки и инфо */
.gallery-info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  margin-bottom: 1.5rem;
}
@media (max-width: 768px) {
  .gallery-info-grid {
    grid-template-columns: 1fr;
  }
}

/* Основное изображение */
.main-image-wrapper {
  width: 100%;
  aspect-ratio: 4 / 3;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 0 8px rgba(0, 0, 0, 0.1);
}
.main-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  user-select: none;
  pointer-events: none;
}

/* Информация о товаре */
.product-info {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.product-title {
  font-size: 2rem;
  margin: 0;
  color: #222;
}
.price-section {
  font-size: 1.5rem;
  font-weight: 700;
  color: black;
}
.details-section h2,
.characteristics-section h2 {
  margin-bottom: 0.5rem;
  color: #444;
}
.description,
.characteristics {
  color: #555;
  line-height: 1.4;
}
.characteristics {
  white-space: pre-wrap;
  background: #f5f5f5;
  padding: 1rem;
  border-radius: 6px;
  font-family: monospace;
}
.actions-section {
  margin-top: auto;
}
.add-to-cart {
  max-width: 100%;
  height: 48px;
  background-color: var(--caret-color);
  color: white;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  transition: background-color 0.3s;
}
.add-to-cart:hover {
  background-color: var(--caret-hover);
}

/* Сетка миниатюр */
.thumbnails-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
  gap: 12px;
  margin-bottom: 2rem;
}
.thumb-wrapper {
  width: 100%;
  aspect-ratio: 1 / 1;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border-color 0.3s ease;
}
.thumb-wrapper.active {
  border-color: #007bff;
}
.thumb-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  user-select: none;
  pointer-events: none;
}

/* Адаптация для мобильных */
@media (max-width: 600px) {
  .thumbnails-grid {
    grid-template-columns: repeat(auto-fill, minmax(60px, 1fr));
    gap: 8px;
  }
  .main-image-wrapper {
    aspect-ratio: 3 / 2;
  }
  .product-title {
    font-size: 1.5rem;
  }
  .price-section {
    font-size: 1.2rem;
  }
}

/* Отзывы */
.reviews-section {
  border-top: 1px solid #ddd;
  padding-top: 1rem;
}
.new-review {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.review-input {
  min-height: 80px;
  resize: vertical;
  padding: 0.5rem;
  font-size: 1rem;
  border-radius: 6px;
  border: 1px solid #ccc;
  font-family: Arial, sans-serif;
}
.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.review-card {
  border: 1px solid #eee;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  background: #fafafa;
}
.review-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.25rem;
  font-size: 0.85rem;
  color: #666;
  font-weight: 600;
}
.review-text {
  font-size: 1rem;
  color: #333;
}

/* Загрузка */
.loading-container {
  text-align: center;
  font-size: 1.2rem;
  color: #777;
}

/* Текст "Товар не найден" */
.not-found {
  text-align: center;
  color: #999;
  margin-top: 2rem;
  font-size: 1.5rem;
}
</style>
