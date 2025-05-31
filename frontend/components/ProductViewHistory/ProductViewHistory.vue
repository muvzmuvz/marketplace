<template>
  <div class="history-container">
    <div class="history-header">
      <h2 class="history-title">Вы смотрели</h2>
      <button 
        v-if="products.length" 
        class="clear-all"
        @click="clearHistory"
      >
        Очистить всё
      </button>
    </div>

    <div v-if="loading" class="skeleton-grid">
      <div v-for="i in 6" :key="i" class="skeleton-card">
        <div class="skeleton-image"></div>
        <div class="skeleton-line short"></div>
        <div class="skeleton-line long"></div>
      </div>
    </div>

    <div v-if="error" class="error-message">
      ⚠️ {{ error }}
    </div>

    <div v-if="!loading && products.length" class="scroll-container">
      <div class="history-grid">
        <div 
          v-for="product in products" 
          :key="product.id" 
          class="product-card"
          @click="goToProduct(product.id)"
        >
          <button 
            class="remove-btn"
            @click.stop="removeFromHistory(product.id)"
          >
            ×
          </button>
          <div class="image-container">
            <img 
              :src="product.images[0].path" 
              :alt="product.name" 
              class="product-image"
              loading="lazy"
            />
          </div>
          <div class="product-details">
             <div class="product-name">{{ product.name }}</div>
            <div class="product-price text-gray-700">{{ formattedPrice(product.price) }}</div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="!loading && !products.length" class="empty-history">

      <p>Вы пока ничего не смотрели</p>
    </div>
  </div>
</template>

<script>
import '@/assets/css/productPage.css'

export default {
  props: {
    currentProductId: {
      type: Number,
      required: true
    }
  },
  data() {
    return {
      products: [],
      loading: true,
      error: null,
    };
  },
  mounted() {
    this.fetchHistory();
  },
  methods: {
    async fetchHistory() {
  try {
    const response = await fetch('http://localhost:8080/ProductViewHistory/history', {
      credentials: 'include'
    });

    if (!response.ok) throw new Error('Ошибка загрузки истории');

    let data = await response.json();

    // Сортируем по дате просмотра - новые первыми
    data.sort((a, b) => new Date(b.viewDate) - new Date(a.viewDate));

    // Извлекаем продукты
    let products = data.map(item => item.product);

    const currentId = this.currentProductId;

    if (currentId) {
      // Если текущий продукт есть в списке, помещаем его в начало
      const currentIndex = products.findIndex(p => p.id === currentId);
      if (currentIndex > -1) {
        const [currentProduct] = products.splice(currentIndex, 1);
        products.unshift(currentProduct);
      }
    }

    this.products = products;
  } catch (error) {
    this.error = 'Не удалось загрузить историю просмотров';
  } finally {
    this.loading = false;
  }
},
    async removeFromHistory(productId) {
      if (!confirm('Удалить товар из истории?')) return;

      try {
        await fetch(`http://localhost:8080/ProductViewHistory/${productId}`, {
          method: 'DELETE',
          credentials: 'include'
        });
        this.products = this.products.filter(p => p.id !== productId);
      } catch (error) {
        this.error = 'Ошибка при удалении товара';
      }
    },
    async clearHistory() {
      if (!confirm('Очистить всю историю просмотров?')) return;

      try {
        await fetch('http://localhost:8080/ProductViewHistory/allhistory', {
          method: 'DELETE',
          credentials: 'include'
        });
        this.products = [];
      } catch (error) {
        this.error = 'Ошибка при очистке истории';
      }
    },
    formattedPrice(price) {
      return new Intl.NumberFormat('ru-RU', {
        style: 'currency',
        currency: 'RUB',
        maximumFractionDigits: 0
      }).format(price);
    },
    goToProduct(id) {
      this.$router.push(`/product/${id}`);
    }
  }
};
</script>

<style scoped>
.history-container {
  display: grid;
  grid-template-rows: max-content;
  max-width: 100%;
  margin: 20px auto;
  padding: 0 15px;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.history-title {
  font-size: 24px;
  font-weight: 700;
  color: #333;
}

.clear-all {
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  font-size: 14px;
  padding: 5px 10px;
  transition: color 0.2s;
}

.clear-all:hover {
  color: #b10080;
}

.scroll-container {
  overflow-x: auto;
  padding-bottom: 15px;
  scroll-behavior: smooth; /* Плавный скролл */
  position: relative;

    scrollbar-width: thin; /* Firefox */
  scrollbar-color: rgba(131, 86, 62) transparent; /* Firefox */
}

/* Chrome, Edge, Safari */
.scroll-container::-webkit-scrollbar {
  height: 6px; 
}

.scroll-container::before,
.scroll-container::after {
  content: "";
  position: absolute;
  top: 0;
  bottom: 15px;
  width: 30px;
  pointer-events: none; /* Чтобы тени не мешали кликам */
  z-index: 10;
}

/* Chrome, Edge, Safari */
.scroll-container::-webkit-scrollbar {
  height: 6px; /* высота горизонтального скролла */
}

.scroll-container::-webkit-scrollbar-track {
  background: transparent;
}

.scroll-container::-webkit-scrollbar-thumb {
  background-color: #b10080; /* цвет скролла */
  border-radius: 3px;
  border: 1px solid transparent;
}

/* Добавим эффект при наведении на скролл */
.scroll-container::-webkit-scrollbar-thumb:hover {
  background-color: #870060;
}

.history-grid {
  display: flex; /* inline-flex можно оставить, но flex удобнее */
  gap: 15px;
  padding: 5px 0;
  /* Чтобы не было вертикальной прокрутки */
  overflow-y: hidden;
}

.product-card {
  flex-shrink: 0; /* Чтобы не сжимались */
  width: 200px;
  /* Остальное без изменений */
}

.product-card:hover {
  transform: translateY(-3px);
}

.image-container {
  position: relative;
  padding-top: 100%;
  background: #f8f8f8;
  border-radius: 8px 8px 0 0;
}

.product-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: fill;
  mix-blend-mode: multiply;
}

.product-details {
  padding: 12px;
}

.product-price {
  font-weight: 700;
  font-size: 16px;
  margin-bottom: 6px;
}

.product-name {
  font-size: 16px;
  color: black;
  font-weight: 600;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.3;
}

.remove-btn {
  position: absolute;
  top: 5px;
  right: 5px;
  width: 24px;
  height: 24px;
  border: none;
  border-radius: 50%;
  background: rgba(0,0,0,0.6);
  color: white;
  cursor: pointer;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
}

.product-card:hover .remove-btn {
  opacity: 1;
}

.empty-history {

  align-items: center;
  text-align: center;
  padding: 40px 0;
}

.empty-image {
  width: 100px;
  margin-bottom: 20px;
}

.skeleton-grid {
  display: flex;
  gap: 15px;
}

.skeleton-card {
  width: 200px;
  background: white;
  border-radius: 8px;
  padding: 12px;
}

.skeleton-image {
  background: #f0f0f0;
  border-radius: 6px;
  padding-top: 100%;
  margin-bottom: 12px;
}

.skeleton-line {
  height: 12px;
  background: #f0f0f0;
  border-radius: 4px;
  margin-bottom: 8px;
}

.skeleton-line.short {
  width: 60%;
}

.skeleton-line.long {
  width: 100%;
}

.error-message {
  color: #d32f2f;
  background: #ffebee;
  padding: 15px;
  border-radius: 4px;
  margin: 20px 0;
}
</style>