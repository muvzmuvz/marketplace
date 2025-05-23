<template>
  <div class="history">
    <h2>История просмотров</h2>

    <div v-if="loading">Загрузка истории...</div>

    <div v-if="error" class="error">{{ error }}</div>

    <ul v-if="products.length">
      <li v-for="product in products" :key="product.id" class="product-item">
        <img :src="product.imagePath" :alt="product.name" class="product-image" />
        <div class="product-info">
          <h3>{{ product.name }}</h3>
          <p>Цена: {{ product.price }} ₽</p>
          <p>{{ product.description }}</p>
        </div>
      </li>
    </ul>

    <div v-if="!loading && products.length === 0">История просмотров пуста.</div>
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
  async mounted() {
    await this.fetchHistory();
  },
  methods: {
    async fetchHistory() {
      this.loading = true;
      this.error = null;

      try {
        // Замените URL на свой API endpoint
        const response = await fetch('http://localhost:8080/ProductViewHistory/history',{
             method: 'GET',
             credentials: 'include',
        });

        if (!response.ok) {
          throw new Error(`Ошибка сервера: ${response.status}`);
        }

        const data = await response.json();

        this.products = data; // Предполагается, что API возвращает массив продуктов
      } catch (e) {
        this.error = 'Не удалось загрузить историю просмотров.';
        console.error(e);
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>

<style scoped>
.history {
  max-width: 600px;
  margin: 0 auto;
  font-family: Arial, sans-serif;
}

.product-item {
  display: flex;
  margin-bottom: 15px;
  border-bottom: 1px solid #ddd;
  padding-bottom: 10px;
}

.product-image {
  width: 80px;
  height: 80px;
  object-fit: contain;
  margin-right: 15px;
}

.product-info h3 {
  margin: 0;
  font-size: 1.1em;
}

.error {
  color: red;
  margin-bottom: 10px;
}
</style>
