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
          <div class="average-rating" v-if="comments.length">
            <span>Средняя оценка:</span>
            <span class="stars">{{ getStars(Math.round(averageRating)) }}</span>
            <span class="number">({{ averageRating }})</span>
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
<Button
  :class="[
    'add-to-cart flex items-center gap-2 px-4 py-2 rounded-md text-white font-medium transition-all duration-500',
    isAdded ? 'bg-green-500 hover:bg-green-600' : 'bg-blue-600 hover:bg-blue-700'
  ]"
  @click="addToCart"
>
  <component :is="isAdded ? CheckIcon : ShoppingCartIcon" class="w-5 h-5 transition-transform duration-300" />
  <span class="transition-all duration-300">
    {{ isAdded ? 'Добавлено' : 'Добавить в корзину' }}
  </span>
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
          <div class="rating-stars">
            <span
              v-for="n in 5"
              :key="n"
              class="star"
              :class="{ active: n <= productEvaluation }"
              @click="productEvaluation = n"
            >
              ★
            </span>
            <span class="rating-number">{{ productEvaluation }} / 5</span>
          </div>
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
            <div class="review-rating-stars">
              {{ getStars(comment.productEvaluation || 0) }}
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
  ShoppingCartIcon,
  CheckIcon
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const productId = route.params.id

const product = ref(null)
const isLoading = ref(true)
const comments = ref([])
const newComment = ref('')
const productEvaluation = ref(5) // по умолчанию 5 звёзд
const activeIndex = ref(0)
const userId = ref(null)

const isAdded = ref(false)

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
const fetchUser = async () => {
  try {
    const response = await fetch('http://localhost:8080/user/user', {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Ошибка загрузки пользователя')
    const data = await response.json()
    userId.value = data.id
  } catch (error) {
    console.error('Ошибка при получении пользователя:', error)
    alert('Не удалось получить данные пользователя. Отзывы не будут отправлены.')
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

  if (!userId.value) {
    alert('Вы не авторизованы. Нельзя отправить отзыв.')
    return
  }

  try {
    const review = {
      productId: Number(productId),
      description: newComment.value,
      productEvaluation: productEvaluation.value,
      rating: productEvaluation.value,
      dateCreated: new Date().toISOString(),
      userId: userId.value
    }

    const response = await fetch('http://localhost:8080/review/create_review', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*'
      },
      credentials: 'include',
      body: JSON.stringify(review)
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(JSON.stringify(errorData))
    }

    newComment.value = ''
    productEvaluation.value = 5
    await fetchComments()
  } catch (error) {
    console.error('Ошибка отправки отзыва:', error)
    alert('Не удалось отправить отзыв.')
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

    isAdded.value = true

    // Возвращаем в начальное состояние через 2 секунды
    setTimeout(() => {
      isAdded.value = false
    }, 2000)
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

const getStars = (count) => {
  return '★'.repeat(count) + '☆'.repeat(5 - count)
}

const averageRating = computed(() => {
  if (comments.value.length === 0) return 0
  const sum = comments.value.reduce((total, c) => total + (c.productEvaluation || 0), 0)
  return (sum / comments.value.length).toFixed(1)
})

onMounted(async () => {
  await fetchUser()
  await fetchProduct()
  await fetchComments()
})


const goBack = () => {
  router.go(-1)
}
</script>
