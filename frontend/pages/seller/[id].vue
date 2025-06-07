<script setup lang="ts">
import '@/assets/css/main.css';
import CardItem from '@/components/CardItem/CardItem.vue';
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

interface Product {
  id: number;
  name: string;
  price: number;
  description?: string;
  countProduct?: number;
  images: { path: string }[];
  user?: {
    id: number;
    name: string;
    email: string;
    dateRegistered: string;
    role: number;
  };
}

const route = useRoute();
const router = useRouter();
const sellerId = Number(route.params.id);

const products = ref<Product[]>([]);
const displayedProducts = ref<Product[]>([]);
const isLoading = ref(true);
const error = ref<string | null>(null);

const sellerName = ref<string | null>(null);

const currentPage = ref(1);
const itemsPerPage = ref(12);

const config = useRuntimeConfig();
const apiUrl = config.public.apiBaseUrl;

// Функция для исправления пути к картинке — чтобы начинался с '/uploads/'
const fixImagePath = (path: string): string => {
  if (!path) return '';

  // Убираем возможный префикс '/seller' и ведущие слэши
  let cleanPath = path.replace(/^\/?seller\/?/, ''); // удаляем '/seller' в начале пути
  cleanPath = cleanPath.replace(/^\/?/, '');        // удаляем ведущий слэш, если есть

  // Возвращаем путь с нужным префиксом
  return `/${cleanPath}`;
};

const fetchSellerProducts = async () => {
  try {
    isLoading.value = true;
    error.value = null;

    const response = await fetch(`${apiUrl}/product/${sellerName}/${sellerId}`);

    if (!response.ok) throw new Error('Ошибка загрузки товаров продавца');

    const data = await response.json();

    // Исправляем пути картинок каждого товара
    data.forEach((product: Product) => {
      if (product.images && product.images.length > 0) {
        product.images = product.images.map(image => ({
          path: fixImagePath(image.path)
        }));
      }
    });

    products.value = data;

    // Извлекаем имя продавца
    if (data.length > 0 && data[0].user) {
      sellerName.value = data[0].user.name;
    } else {
      sellerName.value = null;
    }

    updateDisplayedProducts();
  } catch (err) {
    error.value = (err as Error).message;
  } finally {
    isLoading.value = false;
  }
};

const updateDisplayedProducts = () => {
  const start = (currentPage.value - 1) * itemsPerPage.value;
  const end = start + itemsPerPage.value;
  displayedProducts.value = products.value.slice(start, end);
};

const totalPages = computed(() => Math.ceil(products.value.length / itemsPerPage.value));

const changePage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    updateDisplayedProducts();
  }
};

const goToProduct = (id: number) => {
  router.push(`/product/${id}`);
};

onMounted(fetchSellerProducts);
</script>

<template>
  <NavMenu />

  <div class="seller-page">
    <div class="seller-info">
      <h1 v-if="sellerName">Товары продавца: {{ sellerName }}</h1>
      <h1 v-else>Товары продавца №{{ sellerId }}</h1>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <div v-if="isLoading" class="loading">Загрузка товаров...</div>

    <div v-else>
      <div v-if="products.length === 0">
        У этого продавца пока нет товаров.
      </div>

      <div v-else class="card-grid">
        <CardItem
          v-for="product in displayedProducts"
          :key="product.id"
          :product="product"
          @click="goToProduct(product.id)"
        />
      </div>

      <div class="pagination" v-if="totalPages > 1">
        <button
          :disabled="currentPage === 1"
          @click="changePage(currentPage - 1)"
          class="pagination__button"
        >
          Назад
        </button>

        <span class="pagination__info">
          Страница {{ currentPage }} из {{ totalPages }}
        </span>

        <button
          :disabled="currentPage === totalPages"
          @click="changePage(currentPage + 1)"
          class="pagination__button"
        >
          Вперёд
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.seller-page {
  padding: 20px;
  max-width: 100%;
  margin: 0 auto;
}

.seller-info {
  background-color: #f6f6f9;
  border-radius: 16px;
  padding: 16px 28px;
  text-align: center;
  margin-bottom: 20px;
  font-weight: 600;
}

.loading {
  font-size: 18px;
  color: gray;
  text-align: center;
}

.error-message {
  color: red;
  text-align: center;
  margin-bottom: 10px;
}

.card-grid {
  display: grid;
  grid-template-columns: repeat(6, calc(16.6667% - 25px));
  justify-content: center;
  align-items: center;
  gap: 30px;
  padding: 20px 50px;

  @media (max-width: 1279px) {
    grid-template-columns: repeat(4, calc(25% - 4px));
    grid-auto-flow: dense;
  }

  @media (max-width: 767px) {
    grid-template-columns: repeat(2, calc(50% - 4px));
    grid-auto-flow: dense;
    padding: 10px 25px;
  }

  margin-bottom: 50px;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 20px;
  gap: 10px;
}

.pagination__button {
  padding: 8px 14px;
  background-color: var(--caret-color);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.pagination__button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background-color: var(--caret-hover);
}

.pagination__info {
  font-weight: bold;
}
</style>
