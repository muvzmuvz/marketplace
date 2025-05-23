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
              :src="product.images.path" 
              :alt="product.name" 
              class="product-image"
              loading="lazy"
            />
          </div>
          <div class="product-details">
            <div class="product-price">{{ formattedPrice(product.price) }}</div>
            <div class="product-name">{{ product.name }}</div>
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
        
        const data = await response.json();
        this.products = data;
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
        await fetch('http://localhost:8080/ProductViewHistory/clear', {
          method: 'POST',
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
}

.history-grid {
  display: inline-flex;
  gap: 15px;
  padding: 5px 0;
}

.product-card {
  position: relative;
  width: 200px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: transform 0.2s;
  cursor: pointer;
  flex-shrink: 0;
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
  object-fit: contain;
  mix-blend-mode: multiply;
}

.product-details {
  padding: 12px;
}

.product-price {
  color: #b10080;
  font-weight: 700;
  font-size: 16px;
  margin-bottom: 6px;
}

.product-name {
  font-size: 14px;
  color: #444;
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