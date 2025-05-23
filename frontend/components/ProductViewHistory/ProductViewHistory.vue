<template>
  <div class="history">
    <h2 class="title">Вы смотрели</h2>

    <div v-if="loading" class="loading">Загрузка...</div>
    <div v-if="error" class="error">{{ error }}</div>

    <div class="grid" v-if="products.length">
      <div v-for="product in products" :key="product.id" class="card">
        <img :src="product.imagePath" :alt="product.name" class="card-img" />
        <div class="card-body">
          <div class="card-title">{{ product.name }}</div>
          <div class="card-price">{{ product.price.toLocaleString() }} ₽</div>
          <div class="card-desc">{{ truncate(product.description, 50) }}</div>
        </div>
      </div>
    </div>

    <div v-if="!loading && !products.length">История пуста</div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      products: [],
      loading: false,
      error: null,
    };
  },
  mounted() {
    this.fetchHistory();
  },
  methods: {
    async fetchHistory() {
      this.loading = true;
      this.error = null;
      try {
        const res = await fetch('http://localhost:8080/ProductViewHistory/history', {
          credentials: 'include', // отправляем куки
        });
        if (!res.ok) throw new Error('Ошибка сервера');
        const data = await res.json();
        this.products = data;
      } catch (e) {
        this.error = 'Не удалось загрузить историю.';
      } finally {
        this.loading = false;
      }
    },
    truncate(text, limit) {
      if (!text) return '';
      return text.length > limit ? text.slice(0, limit) + '…' : text;
    }
  }
};
</script>

<style scoped>
.history {
  padding: 20px;
  max-width: 1200px;
  margin: auto;
}

.title {
  font-size: 24px;
  margin-bottom: 20px;
}

.loading,
.error {
  font-size: 16px;
  margin-bottom: 10px;
  color: #a00;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 16px;
}

.card {
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
  overflow: hidden;
  transition: box-shadow 0.2s ease;
  cursor: pointer;
}

.card:hover {
  box-shadow: 0 0 10px rgba(0,0,0,0.1);
}

.card-img {
  width: 100%;
  height: 180px;
  object-fit: contain;
  background-color: #f9f9f9;
}

.card-body {
  padding: 10px;
}

.card-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 6px;
  min-height: 36px;
}

.card-price {
  font-size: 16px;
  color: #b10080;
  font-weight: bold;
  margin-bottom: 4px;
}

.card-desc {
  font-size: 12px;
  color: #777;
}
</style>
