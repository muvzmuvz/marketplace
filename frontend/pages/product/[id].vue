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
